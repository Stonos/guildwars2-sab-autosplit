using Gw2Sharp.Models;

namespace LiveSplit.GW2SAB.checkpoint
{
    /// <summary>
    /// Represents a checkpoint
    /// </summary>
    public class Checkpoint
    {
        public string Name { get; set; }
        public Area Area { get; set; }
        public CheckpointType CheckpointType { get; set; }
        public int TimeSubtract { get; set; }
        public CombatStatus CombatStatus { get; set; } = CombatStatus.Any;

        public bool IsPointInArea(Coordinates3 testPoint, bool isInCombat)
        {
            switch (CombatStatus)
            {
                case CombatStatus.InCombat when !isInCombat:
                case CombatStatus.OutOfCombat when isInCombat:
                    return false;
                default:
                    return Area.IsPointInArea(testPoint);
            }
        }
    }
}