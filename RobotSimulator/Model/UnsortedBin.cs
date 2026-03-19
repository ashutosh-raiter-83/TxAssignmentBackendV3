namespace RobotSimulator.Model;

public class UnsortedBin
{
    public float ZPosition { get; set; }

    public List<string> Items { get; set; }

    public bool IsEmpty => Items.Count == 0;
    public string TopmostItemLabel => IsEmpty ? "" : Items[0];
    public string RemoveTopmostItem()
    {
        var topmost = TopmostItemLabel;
        Items.RemoveAt(0);
        return topmost;
    }
}
