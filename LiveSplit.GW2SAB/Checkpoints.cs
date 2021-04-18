using System.Collections.Generic;
using Gw2Sharp.Models;

namespace LiveSplit.GW2SAB
{
    public static class Checkpoints
    {
        public static readonly IList<Area2D> W1Checkpoints = new List<Area2D>
        {
            new Area2D( // W1Z1 starting area
                new Coordinates2(-1419.16613769531, 485.513458251953),
                new Coordinates2(-1417.16613769531, 485.513458251953),
                new Coordinates2(-1417.16613769531, 483.513458251953),
                new Coordinates2(-1419.16613769531, 483.513458251953),
                AreaType.StartingArea
            ),
            new Area2D( // W1Z1 checkpoint 1
                new Coordinates2(-1249.08032226563, 491.485504150391),
                new Coordinates2(-1247.06164550781, 488.676849365234),
                new Coordinates2(-1249.93151855469, 485.890991210938),
                new Coordinates2(-1252.90014648438, 487.798675537109)
            ),
            new Area2D( // W1Z1 checkpoint 2
                new Coordinates2(-1117.25170898438, 489.855133056641),
                new Coordinates2(-1113.91662597656, 489.345672607422),
                new Coordinates2(-1113.81884765625, 485.189910888672),
                new Coordinates2(-1116.99572753906, 484.983428955078)
            ),
            new Area2D( // W1Z1 checkpoint 3
                new Coordinates2(-1029.44836425781, 492.549621582031),
                new Coordinates2(-1025.85180664063, 492.487457275391),
                new Coordinates2(-1026.05004882813, 488.389068603516),
                new Coordinates2(-1029.44787597656, 487.663024902344)
            ),
            new Area2D( // W1Z1 checkpoint 4
                new Coordinates2(-933.779541015625, 516.104309082031),
                new Coordinates2(-930.035217285156, 515.939514160156),
                new Coordinates2(-930.363891601563, 512.101196289063),
                new Coordinates2(-933.898864746094, 510.876007080078)
            ),
            new Area2D( // W1Z1 checkpoint 5
                new Coordinates2(-843.289123535156, 460.69140625),
                new Coordinates2(-840.271728515625, 462.517211914063),
                new Coordinates2(-837.490417480469, 459.593444824219),
                new Coordinates2(-839.508239746094, 456.984771728516)
            ),
            new Area2D( // W1Z1 boss area
                new Coordinates2(-843.380615234375, 487.674133300781),
                new Coordinates2(-831.083557128906, 472.828460693359),
                new Coordinates2(-842.870361328125, 461.533081054688),
                new Coordinates2(-855.393859863281, 473.586303710938),
                AreaType.Boss
            ),
            new Area2D( // W1Z2 starting area
                new Coordinates2(-701.850891113281, 506.692169189453),
                new Coordinates2(-699.850891113281, 506.692169189453),
                new Coordinates2(-699.850891113281, 504.692169189453),
                new Coordinates2(-701.850891113281, 504.692169189453),
                AreaType.StartingArea
            ),
        }.AsReadOnly();
    }
}