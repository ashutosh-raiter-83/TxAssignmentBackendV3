using RobotShared.Model;

namespace RobotSimulator.Model;

public class Environment
{
    public static readonly IReadOnlyList<string> Labels = [
        "Beverage",
        "Dairy",
        "Meat",
        "Seafood",
        "Cereal",
        "Produce",
        "Snack",
        "Canned Good",
        "Condiment",
        "Baked Good",
    ];

    public Robot Robot { get; set; }
    public List<UnsortedBin> UnsortedBins { get; set; }
    public List<LabeledBin> LabeledBins { get; set; }

    private readonly Random _rng = new();

    public Environment()
    {
        Robot = new Robot()
        {
            ZPosition = 0,
            FacingDirection = FacingDirection.Neutral,
            HeldItemLabel = null,
            FlagReason = null,
            FlaggedUnsortedBins = [],
            FlaggedLabeledBins = [],
            ServerCausedFlagCount = 0,
            CorrectSortCount = 0,
        };

        UnsortedBins = [];
        LabeledBins = [];

        var unsortedBinCount = _rng.Next(5, 10);
        for (var i=0; i<unsortedBinCount; ++i)
        {
            UnsortedBins.Add(new UnsortedBin()
            {
                ZPosition = i + _rng.NextSingle(),
                Items = Labels
                    .SelectMany(label => Enumerable.Repeat(label, _rng.Next(0, 5)))
                    .ToList(),
            });
        }

        var labeledBinCount = _rng.Next(5, 10);
        for (var i = 0; i < labeledBinCount; ++i)
        {
            LabeledBins.Add(new LabeledBin()
            {
                ZPosition = i + _rng.NextSingle(),
                Label = Labels[_rng.Next(0, Labels.Count)],
                CurrentCount = 0,
                MaxCount = _rng.Next(3, 7),
                LabelScanned = false,
            });
        }
    }

    public void ForceResolveFlags()
    {
        foreach (var flagged in Robot.FlaggedUnsortedBins)
        {
            switch (flagged.FlagReason)
            {
                case UnsortedBinFlagReason.BinEmpty:
                    UnsortedBins.RemoveAt(
                        UnsortedBins.FindIndex(bin => bin.ZPosition == flagged.ZPosition)
                    );
                    UnsortedBins.Add(new UnsortedBin()
                    {
                        ZPosition = flagged.ZPosition,
                        Items = Labels
                            .SelectMany(label => Enumerable.Repeat(label, _rng.Next(0, 5)))
                            .ToList(),
                    });
                    break;
                case UnsortedBinFlagReason.TopmostItemHasNoMatchingLabeledBin:
                    var bin = UnsortedBins.Find(bin => bin.ZPosition == flagged.ZPosition);
                    bin.Items.RemoveAt(0);
                    break;
                default:
                    throw new Exception($"Unknown UnsortedBinFlagReason={flagged.FlagReason}");
            }
        }

        foreach (var flagged in Robot.FlaggedLabeledBins)
        {
            switch (flagged.FlagReason)
            {
                case LabeledBinFlagReason.BinFull:
                    LabeledBins.RemoveAt(
                        LabeledBins.FindIndex(bin => bin.ZPosition == flagged.ZPosition)
                    );
                    LabeledBins.Add(new LabeledBin()
                    {
                        ZPosition = flagged.ZPosition,
                        Label = Labels[_rng.Next(0, Labels.Count)],
                        CurrentCount = 0,
                        MaxCount = _rng.Next(3, 7),
                        LabelScanned = false,
                    });
                    break;
                default:
                    throw new Exception($"Unknown LabeledBinFlagReason={flagged.FlagReason}");
            }
        }

        Robot.FlagReason = null;
        Robot.FlaggedUnsortedBins.Clear();
        Robot.FlaggedLabeledBins.Clear();
    }
}
