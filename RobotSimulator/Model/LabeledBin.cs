using RobotShared.Model;

namespace RobotSimulator.Model;

public class LabeledBin
{
    public float ZPosition { get; set; }

    public string Label { get; set; }

    public int CurrentCount { get; set; }
    public int MaxCount { get; set; }

    public bool IsFull => CurrentCount >= MaxCount;

    public bool LabelScanned { get; set; }

    public Fullness Fullness => CurrentCount >= MaxCount ?
        Fullness.CompletelyFilled :
        CurrentCount > 0 ?
        Fullness.PartiallyFilled :
        Fullness.Empty;
}
