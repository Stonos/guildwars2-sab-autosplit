using System;
using Gw2Sharp.Models;

namespace LiveSplit.GW2SAB.checkpoint
{
    public struct Area
    {
        public AreaType AreaType { get; set; }

        /// <summary>
        /// <code>
        /// p1         p2
        /// *----------*
        /// |          |
        /// |          |
        /// *----------*
        /// p4         p3
        /// </code>
        /// </summary>
        public Coordinates2[] Polygon { get; set; }

        public Coordinates2 Center { get; set; }

        public double Radius { get; set; }

        public double MinimumHeight { get; set; }

        public bool IsPointInArea(Coordinates3 testPoint)
        {
            if (testPoint.Y < MinimumHeight)
            {
                return false;
            }

            //TODO: Use polymorphism instead of doing this
            switch (AreaType)
            {
                case AreaType.Polygon:
                    return IsPointInPolygon(testPoint);
                case AreaType.Circle:
                    return IsPointInCircle(testPoint);
                default:
                    throw new Exception($"Unsupported AreaType {AreaType}");
            }
        }

        // https://stackoverflow.com/a/14998816/3210008
        private bool IsPointInPolygon(Coordinates3 testPoint)
        {
            var result = false;
            var j = Polygon.Length - 1;
            for (var i = 0; i < Polygon.Length; i++)
            {
                if (Polygon[i].Y < testPoint.Z && Polygon[j].Y >= testPoint.Z ||
                    Polygon[j].Y < testPoint.Z && Polygon[i].Y >= testPoint.Z)
                {
                    if (Polygon[i].X + (testPoint.Z - Polygon[i].Y) / (Polygon[j].Y - Polygon[i].Y) *
                        (Polygon[j].X - Polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }

        private bool IsPointInCircle(Coordinates3 testPoint)
        {
            var dx = Math.Abs(testPoint.X - Center.X);
            if (dx > Radius) return false;
            var dy = Math.Abs(testPoint.Z - Center.Y);
            if (dy > Radius) return false;
            if (dx + dy <= Radius) return true;
            return dx * dx + dy * dy <= Radius * Radius;
        }
    }
}