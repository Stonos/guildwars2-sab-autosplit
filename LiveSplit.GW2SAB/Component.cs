using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
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
        private int _maxSkippedTicks;
        private readonly Gw2Client _client;
        private TimerModel _timer;
        private readonly IDictionary<int, int> _lastCheckpoint = new Dictionary<int, int>();
        private bool _wasPlayingTransition;
        private bool _wasAutoPaused;
        private IDictionary<int, IList<Checkpoint>> _checkpoints;
        private int _noTickUpdateCount;
        private bool _cachedScreenshotLoadingScreenResult;
        private LoadingScreen _loadingScreen;
        private StartCondition _startCondition;
        private bool _pauseOnExit;
        private double _blackBarSize;
        private double _blackPixelPercentage;
        private Checkpoint _lastResetCheckpoint;

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
            LoadSettings();
        }

        private void LoadSettings()
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, true)
                },
                AllowTrailingCommas = true
            };

            //TODO: Make this real Settings
            var jsonConfig = File.ReadAllText("Components\\GW2SAB\\gw2sab_config.json");
            var rawConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonConfig, options);
            var rawLoadingScreen = rawConfig.GetValueOrDefault("LoadingScreens", "Include");
            var rawStartCondition = rawConfig.GetValueOrDefault("StartCondition", "Moving");
            var rawPauseOnExit = rawConfig.GetValueOrDefault("PauseOnExit", "true");
            var rawMaxSkippedTicks = rawConfig.GetValueOrDefault("MaxSkippedTicks", "3");
            var rawBlackBarSize = rawConfig.GetValueOrDefault("BlackBarSize", "0.1");
            var rawBlackPixelPercentage = rawConfig.GetValueOrDefault("BlackPixelPercentage", "0.8");

            if (!Enum.TryParse(rawLoadingScreen, true, out _loadingScreen))
            {
                _loadingScreen = LoadingScreen.Include;
            }

            if (!Enum.TryParse(rawStartCondition, true, out _startCondition))
            {
                _startCondition = StartCondition.Moving;
            }

            if (!bool.TryParse(rawPauseOnExit, out _pauseOnExit))
            {
                _pauseOnExit = true;
            }

            if (!int.TryParse(rawMaxSkippedTicks, out _maxSkippedTicks))
            {
                _maxSkippedTicks = 3;
            }

            if (!double.TryParse(rawBlackBarSize, out _blackBarSize))
            {
                _blackBarSize = 0.1f;
            }

            if (!double.TryParse(rawBlackPixelPercentage, out _blackPixelPercentage))
            {
                _blackPixelPercentage = 0.8f;
            }
        }

        private void LoadCheckpoints()
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                },
                AllowTrailingCommas = true
            };

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

        private Process GetGw2Process()
        {
            Process proc = null;
            var mumbleId = (int)_client.Mumble.ProcessId;
            if (mumbleId != 0) // mumble has data, may or may not be correct
            {
                try
                {
                    proc = Process.GetProcessById(mumbleId);
                }
                catch (ArgumentException)
                {
                }
            }

            if (proc == null) // mumble had no / wrong id, try by name
            {
                var tmp = Process.GetProcessesByName("Gw2-64");
                if (tmp.Length > 0) proc = tmp[0];
            }

            return proc;
        }

        private Bitmap TakeScreenShot(double bottomPercent)
        {
            var proc = GetGw2Process();
            var rect = new User32.Rect();
            User32.GetWindowRect(proc.MainWindowHandle, ref rect);
            var width = rect.right - rect.left;
            var height = rect.bottom - rect.top;
            var bmp = new Bitmap(width, (int)(height * bottomPercent)); // smaller bitmap

            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(
                    rect.left,
                    (int)(rect.top + (1 - bottomPercent) * height),
                    0,
                    0,
                    bmp.Size
                ); // start lower
            }

            return bmp;
        }

        private bool IsMostlyBlack(Bitmap a)
        {
            var counter = 0;
            for (var x = 0; x < a.Width; x++)
            {
                for (var y = 0; y < a.Height; y++)
                {
                    var c = a.GetPixel(x, y);
                    if (c.R == 0 && c.G == 0 && c.B == 0)
                    {
                        counter += 1;
                    }
                }
            }

            var i = counter / (double)(a.Width * a.Height);
            return i >= _blackPixelPercentage && !(i == 1);
        }

        private bool IsLoading(bool forceRecheck = false)
        {
            if (!IsTransitioning()) // not transitioning, will invalidate cache
            {
                _cachedScreenshotLoadingScreenResult = false;
                return false;
            }

            if (_cachedScreenshotLoadingScreenResult && !forceRecheck)
            {
                return true; // transitioning and screenshot from cache approved
            }

            _cachedScreenshotLoadingScreenResult = IsMostlyBlack(TakeScreenShot(_blackBarSize));
            return _cachedScreenshotLoadingScreenResult;
        }

        private bool IsTransitioning()
        {
            return _noTickUpdateCount > _maxSkippedTicks;
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (_timer == null)
            {
                InitTimer(state);
            }

            // Check if the Game has closed, but only if it was running before.
            // this check should save the overhead of a syscall when not transitioning
            if (_pauseOnExit && IsTransitioning() && GetGw2Process() == null)
            {
                if (state.CurrentPhase == TimerPhase.Running)
                {
                    _timer.Pause();
                    _wasAutoPaused = true;
                }

                return;
            }

            // need to remember transition state before update, since position will not be updated
            if (state.CurrentPhase == TimerPhase.NotRunning)
                _wasPlayingTransition = IsTransitioning();

            // accumulate ticks
            var lastTick = _client.Mumble.Tick;
            var lastPosition = AvatarPosition;
            _client.Mumble.Update();

            var mapId = _client.Mumble.MapId;
            var mapCheckpoints = _checkpoints.GetValueOrDefault(mapId, null);

            if (lastTick == _client.Mumble.Tick)
            {
                _noTickUpdateCount++;
            }
            else
            {
                _noTickUpdateCount = 0;
            }

            if (state.CurrentPhase == TimerPhase.NotRunning) // From here the timer has to be started
            {
                StartTimerIfNeeded(lastPosition, _wasPlayingTransition);
                return;
            }

            if (_lastResetCheckpoint != null)
            {
                if (!_lastResetCheckpoint.IsPointInArea(lastPosition, false))
                {
                    _lastResetCheckpoint = null;
                }
            }

            var playingTransition = IsTransitioning();
            var avatarPosition = AvatarPosition;
            Log.Info($"\n[{avatarPosition.X}, {avatarPosition.Z}],");

            if (_loadingScreen != LoadingScreen.Include)
            {
                // Loading-Screen Start
                if (IsLoading())
                {
                    switch (state.CurrentPhase)
                    {
                        case TimerPhase.Running when _loadingScreen == LoadingScreen.Exclude:
                            _timer.Pause();
                            _wasAutoPaused = true;
                            Log.Info("Pausing during a loading screen");
                            break;
                        case TimerPhase.Paused when _loadingScreen == LoadingScreen.Only && _wasAutoPaused:
                            _timer.Pause();
                            _wasAutoPaused = false;
                            Log.Info("Starting to time loading screen");
                            break;
                    }
                }
                // Not Loading-Screen
                else
                {
                    switch (state.CurrentPhase)
                    {
                        case TimerPhase.Running when _loadingScreen == LoadingScreen.Only:
                            _timer.Pause();
                            _wasAutoPaused = true;
                            Log.Info("Pausing after a loading screen");
                            break;

                        // if paused in transition, which is not a loading screen, do not unpause, probably character select after restart
                        case TimerPhase.Paused when _loadingScreen == LoadingScreen.Exclude && _wasAutoPaused &&
                                                    !IsTransitioning():
                            _timer.Pause();
                            _wasAutoPaused = false;
                            Log.Info("Starting after a loading screen");
                            break;
                    }
                }
            }

            // Redo Check to skip empty maps
            if (mapCheckpoints == null)
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

            _wasPlayingTransition = playingTransition;
        }

        private void handleStandingOnArea(Checkpoint checkpoint, int checkpointIndex, LiveSplitState state,
            bool playingTransition, int mapId)
        {
            switch (checkpoint.CheckpointType)
            {
                case CheckpointType.StartingArea:
                    if (_wasPlayingTransition && !playingTransition)
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

                case CheckpointType.PauseTimer:
                    Log.Info($"Pausing timer because player is on {checkpoint.Name}");
                    if (_timer.CurrentState.CurrentPhase != TimerPhase.Paused)
                    {
                        _timer.Pause();
                    }

                    break;

                case CheckpointType.ResetTimer:
                    if (_lastResetCheckpoint == null)
                    {
                        Log.Info($"Resetting timer because player is on {checkpoint.Name}");
                        _timer.ResetAndSetAttemptAsPB();
                        _timer.Start();
                        _lastResetCheckpoint = checkpoint;
                    }

                    break;
            }
        }

        private void InitTimer(LiveSplitState state)
        {
            _timer = new TimerModel { CurrentState = state };
            _timer.OnReset += state_onReset;
            _client.Mumble.Update();
            _wasAutoPaused = false;
        }

        private void state_onReset(object sender, TimerPhase timerPhase)
        {
            _lastCheckpoint.Clear();
            _wasAutoPaused = false;
        }

        private void StartTimerIfNeeded(Coordinates3 lastPosition, bool wasPlayingTransition)
        {
            _client.Mumble.Update();
            var newPosition = AvatarPosition;

            switch (_startCondition)
            {
                case StartCondition.Moving: // character moves (and was not transitioning, which would be a move too)
                    if (!wasPlayingTransition && (newPosition.X != lastPosition.X || newPosition.Z != lastPosition.Z))
                    {
                        _lastCheckpoint.Clear();
                        _timer.Start();
                        if (_loadingScreen == LoadingScreen.Only)
                        {
                            _timer.Pause(); // immediately pause until loading screen starts
                            _wasAutoPaused = true;
                        }
                    }

                    break;

                case StartCondition.Loading: // loading screen is visible
                    if (IsLoading(true))
                    {
                        _lastCheckpoint.Clear();
                        _timer.Start();
                        if (_loadingScreen == LoadingScreen.Exclude)
                        {
                            _timer.Pause(); // immediately pause until loading screen ends
                            _wasAutoPaused = true;
                        }
                    }

                    break;

                case StartCondition.NotLoading: // anything where the black bar is not visible
                    if (!IsLoading(true))
                    {
                        _lastCheckpoint.Clear();
                        _timer.Start();
                        if (_loadingScreen == LoadingScreen.Only)
                        {
                            _timer.Pause(); // immediately pause until next loading screen starts
                            _wasAutoPaused = true;
                        }
                    }

                    break;

                case StartCondition.NotTransitioning: // no loading screen or character select
                    if (!IsTransitioning())
                    {
                        _lastCheckpoint.Clear();
                        _timer.Start();
                        if (_loadingScreen == LoadingScreen.Only)
                        {
                            _timer.Pause(); // immediately pause until next loading screen starts
                            _wasAutoPaused = true;
                        }
                    }

                    break;
            }
        }
    }
}