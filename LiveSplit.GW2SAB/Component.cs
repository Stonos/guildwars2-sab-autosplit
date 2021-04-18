using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Gw2Sharp;
using Gw2Sharp.Models;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.GW2SAB
{
    public class Component : IComponent
    {
        private readonly Gw2Client _client;
        private TimerModel _timer;
        private int _lastCheckpoint = -1;

        private Coordinates3 AvatarPosition => _client.Mumble.AvatarPosition;

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
            var connection = new Connection();
            _client = new Gw2Client(connection);
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
            if (_timer == null)
            {
                InitTimer(state);
            }

            if (state.CurrentPhase == TimerPhase.NotRunning)
            {
                StartTimerIfNeeded();
            }

            _client.Mumble.Update();
            var avatarPosition = AvatarPosition;
            var avatarPosition2D = new Coordinates2(AvatarPosition.X, AvatarPosition.Z);
            Log.Info($"\nnew Coordinates2({avatarPosition.X}, {avatarPosition.Z}),");

            for (var i = _lastCheckpoint + 1; i < Checkpoints.W1Checkpoints.Count; i++)
            {
                var checkpoint = Checkpoints.W1Checkpoints[i];
                var isOnCheckpoint = checkpoint.IsPointInArea(avatarPosition2D);
                if (isOnCheckpoint)
                {
                    _lastCheckpoint = i;
                    _timer.Split();
                    break;
                }
            }
        }

        private void InitTimer(LiveSplitState state)
        {
            _timer = new TimerModel {CurrentState = state};
            _client.Mumble.Update();
        }

        private void StartTimerIfNeeded()
        {
            var lastPosition = AvatarPosition;
            _client.Mumble.Update();
            var newPosition = AvatarPosition;

            if (newPosition != lastPosition)
            {
                _timer.Start();
            }
        }
    }
}