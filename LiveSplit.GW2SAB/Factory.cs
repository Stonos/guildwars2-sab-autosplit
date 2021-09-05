using System;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace LiveSplit.GW2SAB
{
    internal class Factory : IComponentFactory
    {
        public string ComponentName => "Guild Wars 2 Super Adventure Box auto splitter";

        public string Description => "Splits when reaching a SAB checkpoint";

        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => null;

        public string XMLURL => null;

        public string UpdateURL => null;

        public Version Version => Version.Parse("1.0");

        public IComponent Create(LiveSplitState state)
        {
            return new Component();
        }
    }
}