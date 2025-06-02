using System.Diagnostics;
using System.Text.Json;
using SharpHook;
using SharpHook.Data;
using SharpHook.Native;

namespace QArantine.Code.FrameworkModules.InputSimulation
{
    public class InputPlayer
    {
        private static readonly Lazy<InputPlayer> _instance = new(() => new InputPlayer());
        public static InputPlayer Instance => _instance.Value;

        private readonly List<IInputEvent> _events = [];
        private int _currentIndex = 0;
        private Thread? _playThread;
        private volatile bool _isPlaying = false;
        private volatile bool _isPaused = false;
        private volatile bool _stopRequested = false;
        private readonly EventSimulator _simulator = new();
        private long _startTimestamp = 0;
        private bool _isContinuousMode = true;
        private long? _firstPlaybackFrame = null;
        private long? _firstRecordedFrame = null;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            Converters = { new InputEventJsonConverter() }
        };
        public event Action? PlaybackStopped;

        private InputPlayer()
        {}

        public void LoadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                LogError("Invalid file path provided for input playback: " + filePath);
                return;
            }

            _events.Clear();
            _currentIndex = 0;
            string json = File.ReadAllText(filePath);

            List<IInputEvent>? events = JsonSerializer.Deserialize<List<IInputEvent>>(json, _serializerOptions);
            if (events != null)
                _events.AddRange(events);
        }

        public bool IsPaused => _isPaused;
        public bool IsPlaying => _isPlaying;

        public void StartDiscontinuousPlayback()
        {
            _isContinuousMode = false;
            _isPlaying = true;
            _firstPlaybackFrame = null;
            _firstRecordedFrame = _events.Count > 0 ? _events[0].FrameId : null;
        }
        public void PlayFrameInputs(long frameId)
        {
            if (!_isPlaying || _isContinuousMode)
                return;

            if (_events.Count == 0)
            {
                Stop();
                return;
            }

            // Sincroniza el primer frame de reproducción con el primero grabado
            if (_firstPlaybackFrame == null)
                _firstPlaybackFrame = frameId;

            if (_firstRecordedFrame == null)
                _firstRecordedFrame = _events[0].FrameId;

            // Calcula el frame relativo respecto al inicio de la grabación
            long relativeFrame = (frameId - _firstPlaybackFrame.Value) + _firstRecordedFrame!.Value;

            List<IInputEvent>? frameEvents = _events
                .Skip(_currentIndex)
                .Where(e => e.FrameId == relativeFrame)
                .ToList();

            if (frameEvents.Count == 0)
            {
                return;
            }

            foreach (IInputEvent? evt in frameEvents)
            {
                SimulateEvent(evt);
                _currentIndex++;
            }

            if (_currentIndex > 0)
            {
                _events.RemoveRange(0, _currentIndex);
                _currentIndex = 0;
            }

            if (_events.Count == 0)
            {
                Stop();
            }
        }

        public void StartContinuousPlayback()
        {
            _isContinuousMode = true;

            if (_isPlaying || !_isContinuousMode) return;

            if (_events.Count == 0)
            {
                LogWarning("Input playback did not start (no events)");
                return;
            }

            _isPlaying = true;
            _isPaused = false;
            _stopRequested = false;
            _playThread = new Thread(PlayLoop) { IsBackground = true };
            _playThread.Start();

            LogOK("Input playback started");
        }

        public void Pause() => _isPaused = true;

        public void Resume() => _isPaused = false;

        public void Reset() => _currentIndex = 0;

        public void Stop()
        {
            _stopRequested = true;
            _isPlaying = false;
            _playThread?.Join();
            _isContinuousMode = true;
            _firstPlaybackFrame = null;
            _firstRecordedFrame = null;
            _playThread = null;
            _currentIndex = 0;

            PlaybackStopped?.Invoke();

            LogOK("Input playback manually stopped.");
        }

        private void PlayLoop()
        {
            _startTimestamp = _events[0].Timestamp;
            Stopwatch sw = Stopwatch.StartNew();

            while (_currentIndex < _events.Count && !_stopRequested)
            {
                if (_isPaused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                IInputEvent evt = _events[_currentIndex];
                long elapsed = sw.ElapsedMilliseconds;
                long target = evt.Timestamp - _startTimestamp;

                if (elapsed < target)
                {
                    Thread.Sleep((int)Math.Min(5, (target - elapsed) / 1000)); // Sleep en pequeños intervalos
                    continue;
                }

                SimulateEvent(evt);
                _currentIndex++;
            }

            _isPlaying = false;

            PlaybackStopped?.Invoke();

            LogOK("Input playback ended.");
        }

        private void SimulateEvent(IInputEvent evt)
        {
            switch (evt)
            {
                case KeyboardEvent ke:
                    KeyCode keyCode = Enum.TryParse<KeyCode>(ke.Key, out var kc) ? kc : KeyCode.VcUndefined;
                    if (ke.IsPressed)
                    {
                        _simulator.SimulateKeyPress(keyCode);
                    }
                    else
                    {
                        _simulator.SimulateKeyRelease(keyCode);
                    }
                    break;

                case MouseEvent me:
                    short x = me.X, y = me.Y;
                    _simulator.SimulateMouseMovement(x, y);

                    // If no button is specified, just move the mouse
                    if (me.Button == null) return;

                    MouseButton button = Enum.TryParse<MouseButton>(me.Button, out var mb) ? mb : MouseButton.NoButton;
                    
                    if (me.IsPressed)
                    {
                        _simulator.SimulateMousePress(button);
                    }
                    else
                    {
                        _simulator.SimulateMouseRelease(button);
                    }
                    break;
            }
        }
    }
}