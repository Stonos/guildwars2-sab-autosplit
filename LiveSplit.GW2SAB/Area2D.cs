﻿using Gw2Sharp.Models;

namespace LiveSplit.GW2SAB
{
    /// <summary>
    /// Represents an area in 2D space
    /// </summary>
    public readonly struct Area2D
    {
        public Coordinates2[] Polygon { get; }

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
        public Area2D(Coordinates2 p1, Coordinates2 p2, Coordinates2 p3, Coordinates2 p4)
        {
            Polygon = new[] {p1, p2, p3, p4};
        }

        // https://stackoverflow.com/a/14998816/3210008
        public bool IsPointInArea(Coordinates2 testPoint)
        {
            var result = false;
            var j = Polygon.Length - 1;
            for (var i = 0; i < Polygon.Length; i++)
            {
                if (Polygon[i].Y < testPoint.Y && Polygon[j].Y >= testPoint.Y ||
                    Polygon[j].Y < testPoint.Y && Polygon[i].Y >= testPoint.Y)
                {
                    if (Polygon[i].X + (testPoint.Y - Polygon[i].Y) / (Polygon[j].Y - Polygon[i].Y) *
                        (Polygon[j].X - Polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }
    }
}