using RobotShared.Hub;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using System.Collections.Concurrent;

namespace RobotSimulator;

public class RobotSimulator
{
    private readonly IRobotHub _client;
    private readonly float _randomHardwareFaultChance;
    private readonly Random _rng = new();

    public Model.Environment Environment { get; set; } = new();

    private readonly ConcurrentQueue<SentCommand> _commandQueue = new();

    public RobotSimulator(IRobotHub client, IRobotHubReceiverEvents events, float randomHardwareFaultChance)
    {
        _client = client;
        _randomHardwareFaultChance = randomHardwareFaultChance;

        events.OnCommandReceivedEvent += OnCommandReceived;
        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (!_commandQueue.TryPeek(out var sentCommand))
                {
                    await Task.Delay(1000);
                    continue;
                }
                try
                {
                    await DoCommand(sentCommand);
                    await Task.Delay(500);
                } catch
                {

                }
                _commandQueue.TryDequeue(out _);
            }
        });
    }

    public async Task WaitForCommandQueueEmpty()
    {
        while (!_commandQueue.IsEmpty)
        {
            await Task.Delay(1000);
        }
    }

    private void OnCommandReceived(SentCommand sentCommand)
    {
        Console.WriteLine($"Received command {sentCommand.CommandId}, {sentCommand.Command}");
        _commandQueue.Enqueue(sentCommand);
    }

    private async Task DoCommand(SentCommand sentCommand)
    {
        Console.WriteLine($"Processing command {sentCommand.CommandId}, {sentCommand.Command}");
        try
        {
            switch (sentCommand.Command)
            {
                case ClearFlagsCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case FaceDirectionCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case MoveToZPositionCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case PickItemCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case PlaceItemCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case QueryRobotStateCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case ScanEnvironmentCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case ScanLabeledBinCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                case ScanUnsortedBinCommand command:
                    await DoCommand(sentCommand.CommandId, command, _client.ReportCommandResult);
                    break;
                default:
                    throw new Exception($"Unknown command {sentCommand.Command.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Encountered exception {ex.Message}, {ex.StackTrace}");
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.SoftwareFault;
            _ = _client.ReportFlag(Environment.Robot.ToShared());
        }
        Console.WriteLine($"Processed command {sentCommand.CommandId}, {sentCommand.CommandId}");
    }

    private async Task DoCommand(string commandId, ClearFlagsCommand command, Func<ClearFlagsCommandResult, Task> reportCommandResult)
    {
        // The simulator assumes all flags have been resolved,
        // when the clear flags command has been sent.
        // In reality, the problem may still exist.
        try
        {
            if (Environment.Robot.FlagReason == RobotShared.Model.RobotFlagReason.SoftwareFault)
            {
                Console.WriteLine("Resetting environment due to software fault. If this issue persists, there is a serious bug in the simulator.");
                var newEnvironment = new Model.Environment();
                newEnvironment.Robot.ServerCausedFlagCount = Environment.Robot.ServerCausedFlagCount;
                newEnvironment.Robot.CorrectSortCount = Environment.Robot.CorrectSortCount;
                Environment = newEnvironment;
            } else
            {
                Console.WriteLine("Resolving flags");
                Environment.ForceResolveFlags();
            }
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = true,
                FailureReason = null,
            });
        }
        catch (Exception ex)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = ex.Message,
            });
            throw;
        }
    }

    private async Task DoCommand(string commandId, FaceDirectionCommand command, Func<FaceDirectionCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        Environment.Robot.FacingDirection = command.FacingDirection;
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
        });
    }

    private async Task DoCommand(string commandId, MoveToZPositionCommand command, Func<MoveToZPositionCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(Math.Abs(Environment.Robot.ZPosition - command.ZPosition)));

        if (command.ZPosition < 0)
        {
            Environment.Robot.ZPosition = 0;
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Attempted to move past start of rail",
            });
            ++Environment.Robot.ServerCausedFlagCount;
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        var max = Math.Max(
            Environment.UnsortedBins.Select(bin => bin.ZPosition).Max(),
            Environment.LabeledBins.Select(bin => bin.ZPosition).Max()
        ) + 0.5f;
        if (command.ZPosition > max)
        {
            Environment.Robot.ZPosition = max;
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Attempted to move past end of rail",
            });
            ++Environment.Robot.ServerCausedFlagCount;
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        Environment.Robot.ZPosition = command.ZPosition;
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
        });
    }

    private async Task DoCommand(string commandId, PickItemCommand command, Func<PickItemCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (Environment.Robot.IsHoldingItem)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Already holding item",
            });
            return;
        }

        if (Environment.Robot.FacingDirection != RobotShared.Model.FacingDirection.UnsortedBin)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not facing unsorted bin",
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        var unsortedBin = Environment.UnsortedBins.Find(bin => bin.ZPosition == Environment.Robot.ZPosition);
        if (unsortedBin == null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not at unsorted bin",
            });
            return;
        }

        if (unsortedBin.IsEmpty)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Attempted to pick from empty bin",
            });
            ++Environment.Robot.ServerCausedFlagCount;
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        Environment.Robot.HeldItemLabel = unsortedBin.RemoveTopmostItem();
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
        });
    }

    private async Task DoCommand(string commandId, PlaceItemCommand command, Func<PlaceItemCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (!Environment.Robot.IsHoldingItem)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not holding an item",
            });
            return;
        }

        if (Environment.Robot.FacingDirection != RobotShared.Model.FacingDirection.LabeledBin)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not facing labeled bin",
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        var labeledBin = Environment.LabeledBins.Find(bin => bin.ZPosition == Environment.Robot.ZPosition);
        if (labeledBin == null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not at labeled bin",
            });
            return;
        }

        if (labeledBin.IsFull)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Attempted to place to full bin",
            });
            ++Environment.Robot.ServerCausedFlagCount;
            // Assume the item fell to the floor
            Environment.Robot.HeldItemLabel = null;
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.UnsafeEnvironment;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (labeledBin.Label != Environment.Robot.HeldItemLabel)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Attempted to place to incorrect bin",
            });
            ++Environment.Robot.ServerCausedFlagCount;
            // Assume the item fell to the floor
            Environment.Robot.HeldItemLabel = null;
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.UnsafeEnvironment;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        ++Environment.Robot.CorrectSortCount;
        ++labeledBin.CurrentCount;
        Environment.Robot.HeldItemLabel = null;
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
        });
    }

    private async Task DoCommand(string commandId, QueryRobotStateCommand command, Func<QueryRobotStateCommandResult, Task> reportCommandResult)
    {
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
            Robot = Environment.Robot.ToShared(),
        });
    }

    private async Task DoCommand(string commandId, ScanEnvironmentCommand command, Func<ScanEnvironmentCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
                Robot = Environment.Robot.ToShared(),
                UnsortedBinZPositions = [],
                LabeledBinZPositions = [],
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
                Robot = Environment.Robot.ToShared(),
                UnsortedBinZPositions = [],
                LabeledBinZPositions = [],
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (Environment.Robot.IsHoldingItem)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Cannot scan while holding an item",
                Robot = Environment.Robot.ToShared(),
                UnsortedBinZPositions = [],
                LabeledBinZPositions = [],
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(Environment.Robot.ZPosition + 2));

        Environment.Robot.ZPosition = 0;
        Environment.Robot.FacingDirection = RobotShared.Model.FacingDirection.Neutral;
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
            Robot = Environment.Robot.ToShared(),
            UnsortedBinZPositions = Environment.UnsortedBins.Select(bin => bin.ZPosition).ToList(),
            LabeledBinZPositions = Environment.LabeledBins.Select(bin => bin.ZPosition).ToList(),
        });
    }

    private async Task DoCommand(string commandId, ScanLabeledBinCommand command, Func<ScanLabeledBinCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
                Label = "",
                Fullness = RobotShared.Model.Fullness.CompletelyFilled,
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
                Label = "",
                Fullness = RobotShared.Model.Fullness.CompletelyFilled,
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (Environment.Robot.IsHoldingItem)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Cannot scan while holding an item",
                Label = "",
                Fullness = RobotShared.Model.Fullness.CompletelyFilled,
            });
            return;
        }

        if (Environment.Robot.FacingDirection != RobotShared.Model.FacingDirection.LabeledBin)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not facing labeled bin",
                Label = "",
                Fullness = RobotShared.Model.Fullness.CompletelyFilled,
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        var labeledBin = Environment.LabeledBins.Find(bin => bin.ZPosition == Environment.Robot.ZPosition);
        if (labeledBin == null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not at labeled bin",
                Label = "",
                Fullness = RobotShared.Model.Fullness.CompletelyFilled,
            });
            return;
        }

        labeledBin.LabelScanned = true;
        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
            Label = labeledBin.Label,
            Fullness = labeledBin.Fullness,
        });

        if (labeledBin.IsFull)
        {
            Environment.Robot.FlaggedLabeledBins.RemoveAll(flagged => flagged.ZPosition == labeledBin.ZPosition);
            Environment.Robot.FlaggedLabeledBins.Add(new()
            {
                ZPosition = labeledBin.ZPosition,
                FlagReason = RobotShared.Model.LabeledBinFlagReason.BinFull,
            });
            await _client.ReportFlag(Environment.Robot.ToShared());
        }
    }

    private async Task DoCommand(string commandId, ScanUnsortedBinCommand command, Func<ScanUnsortedBinCommandResult, Task> reportCommandResult)
    {
        if (Environment.Robot.FlagReason != null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Robot is flagged",
                TopmostItemLabel = "",
            });
            return;
        }

        if (_rng.NextSingle() < _randomHardwareFaultChance)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Hardware fault",
                TopmostItemLabel = "",
            });
            Environment.Robot.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            await _client.ReportFlag(Environment.Robot.ToShared());
            return;
        }

        if (Environment.Robot.IsHoldingItem)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Cannot scan while holding an item",
                TopmostItemLabel = "",
            });
            return;
        }

        if (Environment.Robot.FacingDirection != RobotShared.Model.FacingDirection.UnsortedBin)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not facing unsorted bin",
                TopmostItemLabel = "",
            });
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        var unsortedBin = Environment.UnsortedBins.Find(bin => bin.ZPosition == Environment.Robot.ZPosition);
        if (unsortedBin == null)
        {
            await reportCommandResult(new()
            {
                CommandId = commandId,
                Success = false,
                FailureReason = "Not at unsorted bin",
                TopmostItemLabel = "",
            });
            return;
        }

        await reportCommandResult(new()
        {
            CommandId = commandId,
            Success = true,
            FailureReason = null,
            TopmostItemLabel = unsortedBin.TopmostItemLabel,
        });

        if (unsortedBin.IsEmpty)
        {
            Environment.Robot.FlaggedUnsortedBins.RemoveAll(flagged => flagged.ZPosition == unsortedBin.ZPosition);
            Environment.Robot.FlaggedUnsortedBins.Add(new()
            {
                ZPosition = unsortedBin.ZPosition,
                FlagReason = RobotShared.Model.UnsortedBinFlagReason.BinEmpty,
            });
            await _client.ReportFlag(Environment.Robot.ToShared());
        }
        else if (
            Environment.LabeledBins.All(bin => bin.LabelScanned) &&
            !Environment.LabeledBins.Any(bin => bin.Label == unsortedBin.TopmostItemLabel)
        )
        {
            Environment.Robot.FlaggedUnsortedBins.RemoveAll(flagged => flagged.ZPosition == unsortedBin.ZPosition);
            Environment.Robot.FlaggedUnsortedBins.Add(new()
            {
                ZPosition = unsortedBin.ZPosition,
                FlagReason = RobotShared.Model.UnsortedBinFlagReason.TopmostItemHasNoMatchingLabeledBin,
            });
            await _client.ReportFlag(Environment.Robot.ToShared());
        }
    }
}
