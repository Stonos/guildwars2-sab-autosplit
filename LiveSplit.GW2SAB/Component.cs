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
using LiveSplit.GW2SAB.checkpoint;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LiveSplit.GW2SAB
{
    public class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
    }
    public class Component : IComponent
    {
        private const int MaxSkippedTicks = 5;
        private readonly Gw2Client _client;
        private TimerModel _timer;
        private IDictionary<int, int> _lastCheckpoint = new Dictionary<int, int>();
        private bool wasPlayingTransition;
        private bool skipLoadingScreen;
        private bool _inloadingScreen;
        private IDictionary<int, IList<Checkpoint>> _checkpoints;
        private int noTickUpdateCount;

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
            //TODO: Make this a real Setting
            //var jsonConfig = File.ReadAllText("Components\\GW2SAB\\gw2sab_config.json");
            //var config = JsonSerializer.Deserialize<Dictionary<String, bool>>(jsonConfig, options);
            skipLoadingScreen = true;

            //TODO: Load the checkpoints async
            var jsonString = File.ReadAllText("Components\\GW2SAB\\gw2sab_checkpoints.json");
            _checkpoints = JsonSerializer.Deserialize<Dictionary<int, IList<Checkpoint>>>(jsonString, options);
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

        public Bitmap TakeScreenShot(double bottom_perc)
        {
            Process proc = Process.GetProcessById(((int)_client.Mumble.ProcessId));
            var rect = new User32.Rect();
            User32.GetWindowRect(proc.MainWindowHandle, ref rect);
            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;
            Bitmap bmp = new Bitmap(width, (int)(height * bottom_perc));    // smaller bitmap

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(rect.left, (int)(rect.top + (1- bottom_perc) * height), 0, 0, bmp.Size); // start lower
            }
            return bmp;
        }
        public static bool IsMostlyBlack(Bitmap a)
        {
            int counter = 0;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color c = a.GetPixel(x, y);
                    if (c.R == 0 && c.G == 0 && c.B == 0)
                    {
                        counter += 1;
                    }
                }
            }
            var i = (counter / (double)(a.Width * a.Height));
            return i >= 0.60;
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
                return;
            }

            var lastTick = _client.Mumble.Tick;
            _client.Mumble.Update();

            var mapId = _client.Mumble.MapId;
            var mapCheckpoints = _checkpoints.GetValueOrDefault(mapId, null);
            if (!skipLoadingScreen && mapCheckpoints == null)
            {
                return;
            }

            if (lastTick == _client.Mumble.Tick)
            {
                noTickUpdateCount++;
            }
            else
            {
                noTickUpdateCount = 0;
            }

            var playingTransition = noTickUpdateCount > MaxSkippedTicks;
            var avatarPosition = AvatarPosition;
            Log.Info($"\n[{avatarPosition.X}, {avatarPosition.Z}],");

            if (skipLoadingScreen)
            {
                // Pause for loading-screen
                if (playingTransition // must be transitioning
                    && !_inloadingScreen // only trigger once (also avoids skipping in charater creation)
                    //&& !wasPlayingTransition // only trigger once (take this to skip char creation)
                    && IsMostlyBlack(TakeScreenShot(0.1))) // check for black bar
                {
                    _timer.Pause();
                    Log.Info($"Pausing during a loading screen");
                    //wasPlayingTransition = playingTransition;   // update to avoid recheck
                    _inloadingScreen = true;
                }
                // Loading Screen Unpause
                if (!playingTransition && _inloadingScreen)
                {
                    _timer.Pause();
                    Log.Info($"Unpausing after a loading screen");
                    _inloadingScreen = false;
                }
            }

            // Redo Check to skip empty maps
            if (skipLoadingScreen && mapCheckpoints == null)
            {
                return;
            }

            for (var i = _lastCheckpoint.GetValueOrDefault(mapId, -1) + 1; i < mapCheckpoints.Count; i++)
            {
                var checkpoint = mapCheckpoints[i];
                var isOnArea = checkpoint.IsPointInArea(avatarPosition, _client.Mumble.IsInCombat);

                if (!isOnArea) continue;

                handleStandingOnArea(checkpoint, i, state, playingTransition, mapId);

                break;
            }

            wasPlayingTransition = playingTransition;
        }

        private void handleStandingOnArea(Checkpoint checkpoint, int checkpointIndex, LiveSplitState state,
            bool playingTransition, int mapId)
        {
            switch (checkpoint.CheckpointType)
            {
                case CheckpointType.StartingArea:
                    if (wasPlayingTransition && !playingTransition)
                    {
                        Log.Info(
                            $"Resetting timer because {checkpoint.Name} is a starting area and a transition was playing.");
                        _timer.Reset();
                    }

                    break;

                case CheckpointType.Checkpoint:
                    Log.Info($"Splitting because player is on {checkpoint.Name}");
                    _lastCheckpoint[mapId] = checkpointIndex;
                    _timer.Split();
                    break;

                case CheckpointType.Boss:
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
            _lastCheckpoint.Clear();
        }

        private void StartTimerIfNeeded()
        {
            var lastPosition = AvatarPosition;
            _client.Mumble.Update();
            var newPosition = AvatarPosition;

            if (newPosition.X != lastPosition.X || newPosition.Z != lastPosition.Z)
            {
                _lastCheckpoint.Clear();
                _timer.Start();
            }
        }
    }
}