using RobotShared.Model;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;

namespace Server.AutoPilot
{
    /// <summary>
    /// Tracks autopilot state and sorrting progres for single robot.
    /// </summary>
    public class AutoPilotSession
    {
        public AutoPilotState State { get; private set; } = AutoPilotState.Running;
        public SortingSets sortingSets { get; private set; } = SortingSets.ScanEnvironment;

        public int ItemsSorted { get; private set; }
        public int CommandSent { get; private set; }
        public int ErrorEncountered { get; private set; }

        private List<float> _unsortedBinPosition = [];
        private List<float> _lebeledBinPosition = [];
        private int _currentUnsortedBinIndex = 0;
        private int _currentLabeledBinIndex = 0;
        private string? _heldItemlebel;
        private readonly Dictionary<float, string> _lebeledBinLebles = new();
        private readonly Dictionary<float, Fullness> _lebeledBinFullnes = new();
        private readonly HashSet<float> _scannedLebeledBin = [];
        private bool _beforeScanLabeledBins;

        /// <summary>
        ///  Get the next command for autopilot to execute based on current state and sorting progress or null if autopilotnot running.
        ///</summary>

        public CommandBase? GetNextCommand()
        {
            if (State != AutoPilotState.Running)
            {
                return null;
            }
            CommandSent++;
            return sortingSets switch
            {
                SortingSets.ScanEnvironment => new ScanEnvironmentCommand(),
                SortingSets.MoveToUnsortedBin => new MoveToZPositionCommand
                {
                    ZPosition = _unsortedBinPosition[_currentUnsortedBinIndex],
                },
                SortingSets.FaceUnsortedBin => new FaceDirectionCommand
                {
                    FacingDirection = FacingDirection.UnsortedBin,
                },
                SortingSets.ScanUnsortedBin => new ScanUnsortedBinCommand(),
                SortingSets.PickItem => new PickItemCommand(),
                SortingSets.MoveToLabeledBin => new MoveToZPositionCommand
                {
                    ZPosition = _lebeledBinPosition[_currentLabeledBinIndex],
                },
                SortingSets.FaceLabeledBin => new FaceDirectionCommand
                {
                    FacingDirection = FacingDirection.LabeledBin,
                },
                SortingSets.ScanLabeledBin => new ScanLabeledBinCommand(),
                SortingSets.PlaceItem => new PlaceItemCommand(),
                _ => null,
            };
        }
        ///<summary>
        /// Processthe rsult of command and advancing sorting phase
        /// Returns true if session should continue sending commands
        ///</summary>
        public bool ProcessResult(CommandResultBase commandResult)
        {
            if (State != AutoPilotState.Running)
            {
                return false;
            }
            if (!commandResult.Success)
            {
                ErrorEncountered++;
                sortingSets = SortingSets.ScanEnvironment;
                return true;
            }
            switch (sortingSets)
            {
                case SortingSets.ScanEnvironment:
                    ScanEnvironmentResultHandler(commandResult);
                    break;
                case SortingSets.MoveToUnsortedBin:
                    sortingSets = SortingSets.FaceUnsortedBin;
                    break;
                case SortingSets.FaceUnsortedBin:
                    sortingSets = SortingSets.ScanUnsortedBin;
                    break;
                case SortingSets.ScanUnsortedBin:
                    ScanUnSortedBinResultHandler(commandResult);
                    break;
                case SortingSets.PickItem:
                    sortingSets = SortingSets.MoveToLabeledBin;
                    FindMatchingLabeledBin();
                    break;
                case SortingSets.MoveToLabeledBin:
                    sortingSets = SortingSets.FaceLabeledBin;
                    break;
                case SortingSets.FaceLabeledBin:
                    FaceLebeledBinCompleteHandler();
                    break;
                case SortingSets.ScanLabeledBin:
                    ScanLabeledBinHandler(commandResult);
                    break;
                case SortingSets.PlaceItem:
                    PlaceItemResultHandler();
                    break;

            }
            return true;
        }

        /// <summary>
        /// Pause auto pilot due to a flag on therobtor bins
        /// </summary>
        /// <param name="result"></param>
        public void Pause()
        {
            if(State == AutoPilotState.Running)
            {
                State = AutoPilotState.Paused;
            }
        }
        /// <summary>
        /// Resume auto pilot after flags are cleared. Restarts from environment scan forsafety.
        /// </summary>
        public void Resume()
        {
            if(State == AutoPilotState.Paused)
            {
                State = AutoPilotState.Running;
                sortingSets = SortingSets.ScanEnvironment;
                _scannedLebeledBin.Clear();
                _beforeScanLabeledBins = false;
                _lebeledBinLebles.Clear();
                _lebeledBinFullnes.Clear();
            }
        }
        /// <summary>
        /// Deactivate auto pilot session.
        /// </summary>
        public void DeActivate()
        {
            State = AutoPilotState.Deactivated;
        }
        private void ScanEnvironmentResultHandler(CommandResultBase result)
        {
            if (result is ScanEnvironmentCommandResult scanResult)
            {
                _unsortedBinPosition = scanResult.UnsortedBinZPositions ?? [];
                _lebeledBinPosition = scanResult.LabeledBinZPositions ?? [];
                _currentUnsortedBinIndex = 0;
                _currentLabeledBinIndex = 0;
                _scannedLebeledBin.Clear();
                _lebeledBinLebles.Clear();
                _lebeledBinFullnes.Clear();
                _beforeScanLabeledBins = false;

                if (_unsortedBinPosition.Count == 0)
                {
                    // Here no unsorted bins so need to rescan after cycle
                    sortingSets = SortingSets.ScanEnvironment;
                    return;
                }
                if (_lebeledBinPosition.Count > 0)
                {
                    _beforeScanLabeledBins = true;
                    _currentLabeledBinIndex = 0;
                    sortingSets = SortingSets.MoveToLabeledBin;
                }
                else
                {
                    //Non sorted bins rescan after a cyle
                    sortingSets = SortingSets.MoveToUnsortedBin;
                }
            }
        }
        private void FindMatchingLabeledBin()
        {
            // Try to find a labeled bin that matches the held item label
            foreach (var lbl in _lebeledBinLebles)
            {
                if (lbl.Value==_heldItemlebel && 
                    _lebeledBinFullnes.TryGetValue(lbl.Key,out var fullness) &&
                    fullness != Fullness.CompletelyFilled)
                {
                    _currentLabeledBinIndex = _lebeledBinPosition.IndexOf(lbl.Key);
                    if(_currentLabeledBinIndex >= 0)
                    {
                        return;
                    }
                }
            }
            // need no scan for matching
            _currentLabeledBinIndex = 0;
            for(int x=0; x < _lebeledBinPosition.Count; x++)
            {
                if (!_scannedLebeledBin.Contains(_lebeledBinPosition[x]))
                {
                    _currentLabeledBinIndex = x;
                    return;
                }
            }
            //if no match go  tofirrst
            _currentLabeledBinIndex = 0;

        }
        private void ScanUnSortedBinResultHandler(CommandResultBase result)
        {
            if (result is ScanUnsortedBinCommandResult scanResult)
            {
                if (string.IsNullOrEmpty(scanResult.TopmostItemLabel))
                {
                    // Bin is empty move to Next unsorted bin
                    MovementToNextUnsortedBin();
                }
                else
                {
                    _heldItemlebel = scanResult.TopmostItemLabel;
                    sortingSets = SortingSets.PickItem;
                }
            }
        }
        private void MovementToNextUnsortedBin()
        {
            _currentUnsortedBinIndex++;
            if (_currentUnsortedBinIndex < _unsortedBinPosition.Count)
            {
                sortingSets = SortingSets.MoveToUnsortedBin;
            }
            else
            {
                // No more unsorted bins to try, rescan environment
                sortingSets = SortingSets.ScanEnvironment;
            }
        }
        private void ScanLabeledBinHandler(CommandResultBase result)
        {
            if (result is ScanLabeledBinCommandResult scanResult)
            {
                var current = _lebeledBinPosition[_currentLabeledBinIndex];
                _scannedLebeledBin.Add(current);
                _lebeledBinLebles[current] = scanResult.Label;
                _lebeledBinFullnes[current] = scanResult.Fullness;

                if (_beforeScanLabeledBins) 
                {
                    //Contiue to prescan remaining lbeled bins
                    _currentLabeledBinIndex++;
                    if(_currentLabeledBinIndex < _lebeledBinPosition.Count)
                    {
                        sortingSets = SortingSets.MoveToLabeledBin;
                    }
                    else
                    {
                        // All lebeled bins to be scanned,  start sorting unsorted bins
                        _beforeScanLabeledBins = false;
                        _currentUnsortedBinIndex = 0;
                        sortingSets = SortingSets.MoveToUnsortedBin;
                    }
                }
                else
                {
                    if(scanResult.Label == _heldItemlebel && 
                        scanResult.Fullness != Fullness.CompletelyFilled)
                    {
                        sortingSets = SortingSets.PlaceItem;
                    }
                    else
                    {
                        //Nomatching or if ful; cjeckor next leblebins
                        _currentLabeledBinIndex++;
                        if (_currentLabeledBinIndex < _lebeledBinPosition.Count)
                        {
                            sortingSets = SortingSets.MoveToLabeledBin;
                        }
                        else
                        {
                            //No match binl item can be placed
                            //rescan envirnment
                            sortingSets = SortingSets.ScanEnvironment;
                        }
                    }
                }
                
            }
        }
        private void FaceLebeledBinCompleteHandler()
        {
            if (_beforeScanLabeledBins)
            {
                sortingSets =SortingSets.ScanLabeledBin;
                return;
            }
            sortingSets = SortingSets.PlaceItem;
            //var currentBinZ = _lebeledBinPosition[_currentLabeledBinIndex];
            //if(_scannedLebeledBin.Contains(currentBinZ) && _lebeledBinLebles.TryGetValue(currentBinZ, out var label) 
            //    && label == _heldItemlebel && _lebeledBinFullnes.TryGetValue(currentBinZ,out var fullness) &&
            //    fullness!=Fullness.CompletelyFilled)
            //{
            //    // Already scanned this bin, just place item
            //    sortingSets = SortingSets.PlaceItem;
            //}
            //else
            //{
            //    // Need to scan this bin to check if it's the right one
            //    sortingSets = SortingSets.ScanLabeledBin;
            //}
        }
        private void PlaceItemResultHandler()
        {
            ItemsSorted++;
            _heldItemlebel= null;

            //After placing item, go back to thesame unsorted bin for next item.
            sortingSets = SortingSets.MoveToUnsortedBin;
        }
    }
}
