using Gw2Sharp.Models;

namespace LiveSplit.GW2SAB
{
    /// <summary>
    /// Represents a checkpoint
    /// </summary>
    public struct Checkpoint
    {
        public string Name { get; set; }
        public Area Area { get; set; }
        public CheckpointType CheckpointType { get; set; }
        public int TimeSubtract { get; set; }

        public bool IsPointInArea(Coordinates3 testPoint)
        {
            return Area.IsPointInArea(testPoint);
        }
    }
}