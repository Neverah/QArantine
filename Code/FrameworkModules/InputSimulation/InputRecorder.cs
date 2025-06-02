using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using SharpHook;

namespace QArantine.Code.FrameworkModules.InputSimulation
{

    public class InputRecorder
    {
        private static readonly Lazy<InputRecorder> _instance = new(() => new InputRecorder());
        public static InputRecorder Instance => _instance.Value;

        private const int MouseMoveMinIntervalMs = 16; // 60 Hz
        private readonly ConcurrentQueue<IInputEvent> _events = new();
        private readonly HashSet<string> _pressedKeys = [];
        private readonly HashSet<string> _pressedMouseButtons = [];
        private short _lastMouseX = 0, _lastMouseY = 0;
        private long _lastMouseMoveTimestamp = 0;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private bool _isRecording = false;
        private bool _continuousMode = false;
        private SimpleGlobalHook? _hook;
        private readonly string? _liveFileDirectoryPath = ConfigManager.GetTFConfigParamAsString("InputSimulationOutputFolder");
        private string? _liveFilePath = null;
        private readonly int? _flushInterval = ConfigManager.GetTFConfigParamAsInt("FileFlushInterval");
        private readonly object _fileLock = new();
        private Thread? _flushThread;
        private readonly AutoResetEvent _flushSignal = new(false);
        private volatile bool _stopFlushThread = false;
        private bool _firstEventWritten = false;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new InputEventJsonConverter() }
        };
        public event Action? RecordingStopped;

        private InputRecorder()
        {
            if (_liveFileDirectoryPath != null && !Directory.Exists(_liveFileDirectoryPath))
            {
                Directory.CreateDirectory(_liveFileDirectoryPath);
            }
        }

        public bool IsRecording => _isRecording;
        public string? FilesDirectory => _liveFileDirectoryPath;

        // On demand recording for external projects requests
        public void RecordFrameInputs(long frameId)
        {
            if (!_isRecording || _continuousMode)
                return;

            long timestamp = _stopwatch.ElapsedMilliseconds;

            foreach (string? key in _pressedKeys)
            {
                KeyboardEvent evt = new()
                {
                    Timestamp = timestamp,
                    FrameId = frameId,
                    Key = key,
                    IsPressed = true
                };
                _events.Enqueue(evt);
            }

            foreach (string? button in _pressedMouseButtons)
            {
                MouseEvent evt = new()
                {
                    Timestamp = timestamp,
                    FrameId = frameId,
                    Button = button,
                    X = _lastMouseX,
                    Y = _lastMouseY,
                    IsPressed = true
                };
                _events.Enqueue(evt);
            }

            MouseEvent moveEvt = new()
            {
                Timestamp = timestamp,
                FrameId = frameId,
                Button = null,
                X = _lastMouseX,
                Y = _lastMouseY,
                IsPressed = false
            };
            _events.Enqueue(moveEvt);
        }

        // Continuous autonomus recording
        public void StartRecording(string fileName = "", bool continuous = false)
        {
            if (_liveFileDirectoryPath != null) Directory.CreateDirectory(_liveFileDirectoryPath);

            _liveFilePath = Path.Combine(_liveFileDirectoryPath ?? "",
                (!string.IsNullOrEmpty(fileName) ? fileName : (continuous ? "Cont-" : "Disc-") + $"input_recording_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}") + ".json");

            _isRecording = true;
            _continuousMode = continuous;
            _stopwatch.Restart();
            _events.Clear();
            _pressedKeys.Clear();
            _pressedMouseButtons.Clear();
            _firstEventWritten = false;

            if (_liveFilePath != null)
            {
                InitEventsFile();
                _stopFlushThread = false;
                _flushThread = new Thread(FlushLoop) { IsBackground = true };
                _flushThread.Start();
            }

            _hook = new SimpleGlobalHook();
            _hook.KeyPressed += OnKeyPressed;
            _hook.KeyReleased += OnKeyReleased;
            _hook.MousePressed += OnMousePressed;
            _hook.MouseReleased += OnMouseReleased;
            _hook.MouseMoved += OnMouseMoved;
            _hook.RunAsync();

            LogOK("Input recording started");
        }

        public void StopRecording()
        {
            _isRecording = false;
            _hook?.Dispose();
            _hook = null;

            _stopFlushThread = true;
            _flushSignal.Set();
            _flushThread?.Join();
            _flushThread = null;

            FlushEventsToFile();
            EndEventsFile();

            RecordingStopped?.Invoke();

            LogOK("Input recording stopped.");
        }

        private void FlushLoop()
        {
            while (!_stopFlushThread)
            {
                _flushSignal.WaitOne(_flushInterval ?? 5000);
                FlushEventsToFile();
            }
        }

        private void InitEventsFile()
        {
            if (_liveFilePath == null)
                return;

            lock (_fileLock)
            {
                File.WriteAllText(_liveFilePath, "[\n");
            }
        }

        private void EndEventsFile()
        {
            if (_liveFilePath == null)
                return;

            lock (_fileLock)
            {
                File.AppendAllText(_liveFilePath, "\n]");
            }
        }

        private void FlushEventsToFile()
        {
            List<IInputEvent> toWrite = [];
            while (_events.TryDequeue(out IInputEvent? evt))
                toWrite.Add(evt);

            if (toWrite.Count == 0 || _liveFilePath == null)
                return;

            lock (_fileLock)
            {
                using FileStream stream = new(_liveFilePath, File.Exists(_liveFilePath) ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read);
                using StreamWriter writer = new(stream);
                for (int i = 0; i < toWrite.Count; i++)
                {
                    if (_firstEventWritten)
                        writer.Write(",\n");

                    writer.Write(JsonSerializer.Serialize(toWrite[i], _serializerOptions));
                    _firstEventWritten = true;
                }
            }
        }

        public void Clear()
        {
            while (_events.TryDequeue(out _)) { }
            _pressedKeys.Clear();
            _pressedMouseButtons.Clear();
        }

        // SharpHook events
        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            string key = e.Data.KeyCode.ToString();
            OnKeyEvent(key, true);
        }

        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            string key = e.Data.KeyCode.ToString();
            OnKeyEvent(key, false);
        }

        private void OnMousePressed(object? sender, MouseHookEventArgs e)
        {
            string button = e.Data.Button.ToString();
            OnMouseEvent(button, e.Data.X, e.Data.Y, true);
        }

        private void OnMouseReleased(object? sender, MouseHookEventArgs e)
        {
            string button = e.Data.Button.ToString();
            OnMouseEvent(button, e.Data.X, e.Data.Y, false);
        }

        private void OnMouseMoved(object? sender, MouseHookEventArgs e)
        {
            _lastMouseX = e.Data.X;
            _lastMouseY = e.Data.Y;

            if (_isRecording && _continuousMode)
            {
                long now = _stopwatch.ElapsedMilliseconds;
                if (now - _lastMouseMoveTimestamp >= MouseMoveMinIntervalMs)
                {
                    MouseEvent evt = new()
                    {
                        Timestamp = now,
                        Button = null,
                        X = e.Data.X,
                        Y = e.Data.Y,
                        IsPressed = false
                    };
                    _events.Enqueue(evt);
                    _lastMouseMoveTimestamp = now;
                }
            }
        }

        // Autonomous recording (called by SharpHook events)
        public void OnKeyEvent(string key, bool isPressed)
        {
            if (isPressed)
                _pressedKeys.Add(key);
            else
                _pressedKeys.Remove(key);

            if (!_isRecording || !_continuousMode) return;

            KeyboardEvent evt = new()
            {
                Timestamp = _stopwatch.ElapsedMilliseconds,
                Key = key,
                IsPressed = isPressed
            };
            _events.Enqueue(evt);
        }

        public void OnMouseEvent(string button, short x, short y, bool isPressed)
        {
            if (isPressed)
                _pressedMouseButtons.Add(button);
            else
                _pressedMouseButtons.Remove(button);

            if (!_isRecording || !_continuousMode) return;

            MouseEvent evt = new()
            {
                Timestamp = _stopwatch.ElapsedMilliseconds,
                Button = button,
                X = x,
                Y = y,
                IsPressed = isPressed
            };
            _events.Enqueue(evt);
        }

        public IReadOnlyCollection<string> GetPressedKeys() => [.. _pressedKeys];
        public IReadOnlyCollection<string> GetPressedMouseButtons() => [.. _pressedMouseButtons];
    }
}