using Xunit;
using Moq;
using RobotShared.Model;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.AutoPilot;
using FluentAssertions;

namespace UnitTest.AutoPilot
{
    /// <summary>
    /// Task 3 :Feature Development - Adding newTest cases for AutoPilotSessions Testing
    /// </summary>
    public class AutoPilotSessions_Test
    {
        /// <summary>
        /// Task 3 :Feature Development - When created AutoPilotState should berunning
        /// </summary>
        [Fact]
        public void WhenCreated_ShouldbeRunning()
        {
            var session = new AutoPilotSession();
            session.State.Should().Be(AutoPilotState.Running);
        }
        /// <summary>
        /// Task 3 :Feature Development - When created AutoPilotState should scan with Environmen phase
        /// </summary>
        [Fact]
        public void WhenCreated_ShouldStartWithEnvironmentPhase()
        {
            var session = new AutoPilotSession();
            session.sortingSets.Should().Be(SortingSets.ScanEnvironment);
        }
        /// <summary>
        /// Task 3 :Feature Development - When created 1st command should be Scn Environment 
        /// </summary>
        [Fact]
        public void WhenCreated_ShouldBeFirstCOmmandScanEnvironment()
        {
            var session = new AutoPilotSession();
            var command = session.GetNextCommand();
            command.Should().BeOfType<ScanEnvironmentCommand>();
        }
        /// <summary>
        /// Task 3 :Feature Development - When DeActivated GetNextCommand shouldbe NULL
        /// </summary>
        [Fact]
        public void WhenCreated_GetNextCommandShouldbeNull()
        {
            var session = new AutoPilotSession();
            session.DeActivate();

            session.State.Should().Be(AutoPilotState.Deactivated);
            session.GetNextCommand().Should().BeNull();
        }

        /// <summary>
        /// Task 3 :Feature Development - When Paused thenresumed rsponse should be running
        /// </summary>
        [Fact]
        public void WhenPausedThenResumed_ShouldBeRunning()
        {
            var session = new AutoPilotSession();
            session.Pause();
            session.Resume();
            session.State.Should().Be(AutoPilotState.Running);
            session.sortingSets.Should().Be(SortingSets.ScanEnvironment);
        }
        /// <summary>
        /// Task 3 :Feature Development - When Resume  while no paused the should notchange
        /// </summary>
        [Fact]
        public void WhenResumeWhileNoPaused_ShouldNotChange()
        {
            var session = new AutoPilotSession();
            session.Resume();
            session.State.Should().Be(AutoPilotState.Running);
        }
        /// <summary>
        /// Task 3 :Feature Development - When Paused  while Deactivated Should not be Deactivated
        /// </summary>
        [Fact]
        public void WhenPausedWhileDeActivtivated_ShouldStayDeActivated()
        {
            var session = new AutoPilotSession();
            session.DeActivate();
            session.Pause();
            session.State.Should().Be(AutoPilotState.Deactivated);
        }

        /// <summary>
        /// Task 3 :Feature Development - When Scam Env Success should advanceto Move to unsorted Bin
        /// </summary>
        [Fact]
        public void WhenScanEnvironmentSucceed_ShouldAdvanceToMovetoUnsortedBin()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmdA",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f, 2.0f],
                LabeledBinZPositions = [],
            });

            session.sortingSets.Should().Be(SortingSets.MoveToUnsortedBin);
            var command = session.GetNextCommand();
            command.Should().BeOfType<MoveToZPositionCommand>();
            ((MoveToZPositionCommand)command!).ZPosition.Should().Be(1.0f);
        }

        [Fact]
        public void WhenScanEnvironmentReturnNoUnsrotedBins_ShouldStayAtAtScanEnvironment()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmdA",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [],
                LabeledBinZPositions = [1.0f, 2.0f],
            });

            session.sortingSets.Should().Be(SortingSets.ScanEnvironment);

        }

        [Fact]
        public void WhenScanUnSortedBinEmpty_ShouldAdvancetoNxtUnsrotedBin()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmd1",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f, 2.0f],
                LabeledBinZPositions = [],
            });

            //MOve tofisrst unsorted bib
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd2",
                Success = true,
                FailureReason = null,
            });
            //Face to unsorted bin
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd3",
                Success = true,
                FailureReason = null,
            });
            // Scan unsorted bin -emoty
            session.ProcessResult(new ScanUnsortedBinCommandResult
            {
                CommandId = "cmd4",
                Success = true,
                FailureReason = null,
                TopmostItemLabel ="",
            });

            //Should move tosecond unsorted bin
            session.sortingSets.Should().Be(SortingSets.MoveToUnsortedBin);

            var command = session.GetNextCommand();
            command.Should().BeOfType<MoveToZPositionCommand>();
            ((MoveToZPositionCommand)command!).ZPosition.Should().Be(2.0f);
        }
        [Fact]
        public void WhenScanUnSortedBinHasItemsIn_ShouldPickItem()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmd1",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f],
                LabeledBinZPositions = [],
            });

            //MOve to  unsorted bib
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd2",
                Success = true,
                FailureReason = null,
            });
            //Face to unsorted bin
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd3",
                Success = true,
                FailureReason = null,
            });
            // Scan unsorted bin -emoty
            session.ProcessResult(new ScanUnsortedBinCommandResult
            {
                CommandId = "cmd4",
                Success = true,
                FailureReason = null,
                TopmostItemLabel = "Beverage",
            });

            session.sortingSets.Should().Be(SortingSets.PickItem);


        }
        [Fact]
        public void WhenCommandFails_ShouldIncrementErrorCountandRestartScan()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmdA",
                Success = false,
                FailureReason = "error occurred",
                Robot = new Robot(),
                UnsortedBinZPositions = [],
                LabeledBinZPositions = [],
            });

            session.ErrorEncountered.Should().Be(1);
            session.sortingSets.Should().Be(SortingSets.ScanEnvironment);

        }
        [Fact]
        public void WhenLebelledBInNotMatch_ShouldSkiptoMatchingBin()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmd1",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f],
                LabeledBinZPositions = [2.0f,3.0f],
            });

            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd2",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd3",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new ScanLabeledBinCommandResult
            {
                CommandId = "cmd4",
                Success = true,
                FailureReason = null,
                Label = "Meat",
                Fullness = Fullness.Empty,
            });

            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd5",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd6",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new ScanLabeledBinCommandResult
            {
                CommandId = "cmd7",
                Success = true,
                FailureReason = null,
                Label = "Dairy",
                Fullness = Fullness.Empty,
            });
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd8",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd9",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new ScanUnsortedBinCommandResult
            {
                CommandId = "cmd10",
                Success = true,
                FailureReason = null,
                TopmostItemLabel = "Dairy",
            });

            session.ProcessResult(new PickItemCommandResult
            {
                CommandId = "cmd11",
                Success = true,
                FailureReason = null,
            });


            session.sortingSets.Should().Be(SortingSets.MoveToLabeledBin);
            var command = session.GetNextCommand();
            command.Should().BeOfType<MoveToZPositionCommand>();
            ((MoveToZPositionCommand)command!).ZPosition.Should().Be(3.0f);


        }
        [Fact]
        public void WhenFullSortCycleCompleted_ShouldIncrementSorted()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmd1",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f],
                LabeledBinZPositions = [2.0f],
            });

            //PRescan MOve to  unsorted bin
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd2",
                Success = true,
                FailureReason = null,
            });
            //PRescan  Face to unsorted bin
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd3",
                Success = true,
                FailureReason = null,
            });
            //PRescan  Face lebeled bin
            session.ProcessResult(new ScanLabeledBinCommandResult
            {
                CommandId = "cmd4",
                Success = true,
                FailureReason = null,
                Label = "Beverage",
                Fullness = Fullness.Empty,
            });
            // Sorting now : Move to Unsorted bin
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd5",
                Success = true,
                FailureReason = null,
            });
            // Face unsrorted Bin
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd6",
                Success = true,
                FailureReason = null,
            });
            // Scan unsorted bin -emoty
            session.ProcessResult(new ScanUnsortedBinCommandResult
            {
                CommandId = "cmd7",
                Success = true,
                FailureReason = null,
                TopmostItemLabel = "Beverage",
            });
            session.ProcessResult(new PickItemCommandResult
            {
                CommandId = "cmd8",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd9",
                Success = true,
                FailureReason = null,
            });

            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd10",
                Success = true,
                FailureReason = null,
            });
            
            session.ProcessResult(new PlaceItemCommandResult
            {
                CommandId = "cmd11",
                Success = true,
                FailureReason = null,
            });

            session.ItemsSorted.Should().Be(1);
            //Afterplacing, should go bcak to same unsorted bin
            session.sortingSets.Should().Be(SortingSets.MoveToUnsortedBin);


        }
        [Fact]
        public void WhenProcessResultWhilePause_ShouldReturnFalse()
        {
            var session = new AutoPilotSession();
            session.Pause();

            var response = session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmdA",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f],
                LabeledBinZPositions = [1.0f],
            });
            response.Should().BeFalse();
        }
        [Fact]
        public void WhenStatisticsIninialized_ShouldBeZero()
        {
            var session = new AutoPilotSession();
            session.ItemsSorted.Should().Be(0);
            session.CommandSent.Should().Be(0);
            session.ErrorEncountered.Should().Be(0);
        }

        [Fact]
        public void WhenAllUnsortedBinAreEmptry_ShouldRestartScanEnvironment()
        {
            var session = new AutoPilotSession();
            session.ProcessResult(new ScanEnvironmentCommandResult
            {
                CommandId = "cmd1",
                Success = true,
                FailureReason = null,
                Robot = new Robot(),
                UnsortedBinZPositions = [1.0f],
                LabeledBinZPositions = [],
            });

            session.ProcessResult(new MoveToZPositionCommandResult
            {
                CommandId = "cmd2",
                Success = true,
                FailureReason = null,
            });
            session.ProcessResult(new FaceDirectionCommandResult
            {
                CommandId = "cmd3",
                Success = true,
                FailureReason = null,
            });

            session.ProcessResult(new ScanUnsortedBinCommandResult
            {
                CommandId = "cmd4",
                Success = true,
                FailureReason = null,
                TopmostItemLabel = "",
            });

            //Only 1 Bin and its empty Should Restrt
            session.sortingSets.Should().Be(SortingSets.ScanEnvironment);
        }
    }
    }
