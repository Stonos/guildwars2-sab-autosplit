using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.GW2SAB
{
    public class Component : IComponent
    {
        private readonly Gw2Sharp.Connection _connection;
        private readonly Gw2Sharp.Gw2Client _client;

        public string ComponentName => "Guild Wars 2 Super Adventure Box auto splitter";

        public float HorizontalWidth => 0;

        public float MinimumHeight => 0;

        public float VerticalHeight => 0;

        public float MinimumWidth => 0;

        public float PaddingTop => 0;

        public float PaddingBottom => 0;

        public float PaddingLeft => 0;

        public float PaddingRight => 0;

        public IDictionary<string, Action> ContextMenuControls => null;

        public Component()
        {
            _connection = new Gw2Sharp.Connection();
            _client = new Gw2Sharp.Gw2Client(_connection);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("x");
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public void SetSettings(XmlNode settings)
        {
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            _client.Mumble.Update();
            Log.Info(_client.Mumble.AvatarPosition.ToString());
        }
    }
}