using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private bool wasPlayingTransition;
        private IDictionary<int, IList<Area2D>> _checkpoints;

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
            Application.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            var connection = new Connection();
            _client = new Gw2Client(connection);
            LoadCheckpoints();
        }

        private void LoadCheckpoints()
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            //TODO: Load the checkpoints async
            var jsonString = File.ReadAllText("gw2sab_checkpoints.json");
            _checkpoints = JsonSerializer.Deserialize<Dictionary<int, IList<Area2D>>>(jsonString, options);
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

            var lastTick = _client.Mumble.Tick;
            _client.Mumble.Update();

            var mapCheckpoints = _checkpoints.GetValueOrDefault(_client.Mumble.MapId, null);
            if (mapCheckpoints == null)
            {
                return;
            }

            var playingTransition = lastTick == _client.Mumble.Tick;
            var avatarPosition = AvatarPosition;
            Log.Info($"\n[{avatarPosition.X}, {avatarPosition.Z}],");

            for (var i = _lastCheckpoint + 1; i < mapCheckpoints.Count; i++)
            {
                var checkpoint = mapCheckpoints[i];
                var isOnArea = checkpoint.IsPointInArea(avatarPosition);

                if (!isOnArea) continue;

                handleStandingOnArea(checkpoint, i, state, playingTransition);

                break;
            }

            wasPlayingTransition = playingTransition;
        }

        private void handleStandingOnArea(Area2D checkpoint, int checkpointIndex, LiveSplitState state,
            bool playingTransition)
        {
            switch (checkpoint.AreaType)
            {
                case AreaType.StartingArea:
                    if (wasPlayingTransition)
                    {
                        Log.Info(
                            $"Resetting timer because {checkpoint.Name} is a starting area and a transition was playing.");
                        _timer.Reset();
                    }

                    break;

                case AreaType.Checkpoint:
                    Log.Info($"Splitting because player is on {checkpoint.Name}");
                    _lastCheckpoint = checkpointIndex;
                    _timer.Split();
                    break;

                case AreaType.Boss:
                    if (playingTransition)
                    {
                        Log.Info($"Splitting because player is on boss {checkpoint.Name} and a transition happened");
                        var currentSplit = state.CurrentSplit;
                        _timer.Split();
                        currentSplit.SplitTime =
                            new Time(currentSplit.SplitTime.RealTime?.Subtract(
                                TimeSpan.FromMilliseconds(checkpoint.TimeSubtract)));
                    }

                    break;
            }
        }

        private void InitTimer(LiveSplitState state)
        {
            _timer = new TimerModel {CurrentState = state};
            _timer.OnReset += state_onReset;
            _client.Mumble.Update();
        }

        private void state_onReset(object sender, TimerPhase timerPhase)
        {
            _lastCheckpoint = 0;
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