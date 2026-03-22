namespace Server.AutoPilot
{
    /// <summary>
    /// SortingSets would represent different sets that autopilot go through .
    /// </summary>
    public enum SortingSets
    {
        /// <summary>
        /// Scanning env to discover bin positions
        /// </summary>
        ScanEnvironment,

        /// <summary>
        /// Move to next unsorted bin to scan topmost item.
        /// </summary>
        /// 
        MoveToUnsortedBin,

        /// <summary>
        /// Face towwards the unsorted bin.
        /// </summary>
        /// 
        FaceUnsortedBin,

        /// <summary>
        /// Scan topmost item in the unsorted bin.
        /// </summary>
        /// 
        ScanUnsortedBin,


        /// <summary>
        /// Pick the topmost item from current the unsorted bin.
        /// </summary>
        /// 
        PickItem,

        /// <summary>
        /// Move to Labeled bin to scan orplace the item.
        /// </summary>
        MoveToLabeledBin,

        /// <summary>
        /// Facing towsrds lebeleled bin side.
        /// </summary>
        FaceLabeledBin,

        /// <summary>
        /// Scan current lebled bin to findits leble and fulletss
        /// </summary>
        ScanLabeledBin,

        /// <summary>
        /// Move to Labeled bin to scan orplace the item.
        /// </summary>
        PlaceItem,
        
    }
}
