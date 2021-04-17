using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.GW2SAB
{
    class Factory : IComponentFactory
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
