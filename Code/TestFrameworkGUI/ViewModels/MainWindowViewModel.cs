using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

using TestFramework.Code.FrameworkModules;
using TestFramework.Code.FrameworkModules.GUI;
using TestFramework.Code.FrameworkModules.GUI.Logs;

namespace TestFramework.Code.TestFrameworkGUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LogLine> _logLines;
        private LogBuffer tfLogBuffer;
        private ScrollViewer? _logScrollViewer;

        public LogManager.LogLevel SelectedLogLvl
        {
            get { return LogManager.LogLvl; }
            set 
            {
                LogManager.LogLvl = value;
                RaisePropertyChanged(nameof(SelectedLogLvl));
                LogOK($"Log level changed to: {SelectedLogLvl.ToString()}");
            }
        }
        public List<string> AvailableDebugLvls { get; }

        private bool _isAutoScrollEnabled = true;
        public bool IsAutoScrollEnabled
        {
            get { return _isAutoScrollEnabled; }
            set
            {
                if (_isAutoScrollEnabled != value)
                {
                    _isAutoScrollEnabled = value;
                    RaisePropertyChanged(nameof(IsAutoScrollEnabled));
                    AutoScrollButtonBorderColor = IsAutoScrollEnabled ? Brushes.White : new SolidColorBrush(Color.Parse("#B44141"));
                    if (_isAutoScrollEnabled)
                    {
                        ScrollToBottom();
                    }
                }
            }
        }

        private IBrush _debugLvlButtonBorderColor = Brushes.White;
        public IBrush DebugLvlButtonBorderColor
        {
            get { return _debugLvlButtonBorderColor; }
            set
            {
                _debugLvlButtonBorderColor = value;
                RaisePropertyChanged(nameof(DebugLvlButtonBorderColor));
            }
        }

        private IBrush _autoScrollButtonBorderColor = Brushes.White;
        public IBrush AutoScrollButtonBorderColor
        {
            get { return _autoScrollButtonBorderColor; }
            set
            {
                _autoScrollButtonBorderColor = value;
                RaisePropertyChanged(nameof(AutoScrollButtonBorderColor));
            }
        }

        public ICommand ToggleAutoScrollCommand { get; }
        public ICommand ClearLogScrollCommand { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<LogLine> LogLines
        {
            get { return _logLines; }
            set
            {
                _logLines = value;
                RaisePropertyChanged(nameof(LogLines));
            }
        }

        public MainWindowViewModel()
        {
            AvailableDebugLvls = Enum.GetValues(typeof(LogManager.LogLevel)).Cast<LogManager.LogLevel>().Select(e => e.ToString()).ToList();

            ClearLogScrollCommand = new RelayCommand(ClearLogLines);
            ToggleAutoScrollCommand = new RelayCommand(ToggleAutoScroll);
            // Se añade la referencia a esta ventana en el GUIManager
            GUIManager.Instance.AvaloniaMainWindowViewModel = this;
            // Se obtiene la referencia al buffer de Logs del TF
            tfLogBuffer = GUIManager.Instance.GUILogBuffer;
            SuscribeToTFLogBuffer(tfLogBuffer);

            _logLines = new ObservableCollection<LogLine>();
            LogBuffer_LogLinesAdded(null, EventArgs.Empty);
        }

        public void ClearLogLines()
        {
            _logLines.Clear();
            RaisePropertyChanged(nameof(LogLines));
        }

        private void ToggleAutoScroll()
        {
            IsAutoScrollEnabled = !IsAutoScrollEnabled;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SuscribeToTFLogBuffer(LogBuffer logBuffer)
        {
            logBuffer.LogLinesAdded += LogBuffer_LogLinesAdded;
        }

        private void LogBuffer_LogLinesAdded(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                tfLogBuffer.LogMutex.WaitOne();

                try
                {
                    ObservableCollection<BufferLogLine> logLines = tfLogBuffer.LogLinesBuffer;
                    foreach (BufferLogLine logLine in logLines)
                    {
                        AddLogLine(GetFinalLogLineFromBufferLogLine(logLine));
                    }
                    logLines.Clear();
                }
                finally
                {
                    tfLogBuffer.LogMutex.ReleaseMutex();
                }
                ScrollToBottom();
            });
        }

        private void ScrollToBottom()
        {
            // Verificar si el ScrollViewer está disponible
            if (_isAutoScrollEnabled && _logScrollViewer != null)
            {
                // Realizar autoscroll
                _logScrollViewer.Offset = new Vector(_logScrollViewer.Offset.X, _logScrollViewer.Extent.Height);
            }
        }

        public void SetScrollViewerReference(ScrollViewer scrollViewer)
        {
            _logScrollViewer = scrollViewer;
        }

        private LogLine GetFinalLogLineFromBufferLogLine(BufferLogLine bufferLogLine)
        {
            return new LogLine(bufferLogLine.Timestamp, bufferLogLine.TimestampForegroundHexCode, bufferLogLine.TestTag, bufferLogLine.TestTagForegroundHexCode, bufferLogLine.LogBody, bufferLogLine.LogBodyForegroundHexCode);
        }

        public void AddLogLine(LogLine logLine)
        {
            LogLines.Add(logLine);
        }

        public void AddLogLine(string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            LogLines.Add(new LogLine(timestamp, timestampForeground, testTag, testTagForeground, logBody, logBodyForeground ));
        }

        public void AddLogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            LogLines.Add(new LogLine(timestamp, timestampForegroundHexCode, testTag, testTagForegroundHexCode, logBody, logBodyForegroundHexCode));
        }
    }

    public class LogLine : INotifyPropertyChanged
    {
        private string? _timestamp;
        private IBrush? _timestampForeground;
        private string? _testTag;
        private IBrush? _testTagForeground;
        private string? _logBody;
        private IBrush? _logBodyForeground;

        public string Timestamp
        {
            get { return _timestamp!; }
            set  { _timestamp = value; }
        }

        public IBrush TimestampForeground
        {
            get { return _timestampForeground!; }
            set { _timestampForeground = value; }
        }

        public string TestTag
        {
            get { return _testTag!; }
            set { _testTag = value; }
        }

        public IBrush TestTagForeground
        {
            get { return _testTagForeground!; }
            set {  _testTagForeground = value; }
        }

        public string LogBody
        {
            get { return _logBody!; }
            set { _logBody = value; }
        }

        public IBrush LogBodyForeground
        {
            get { return _logBodyForeground!; }
            set { _logBodyForeground = value; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public LogLine(string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            Timestamp = timestamp ?? "";
            TimestampForeground = timestampForeground ?? Brushes.White;
            TestTag = testTag ?? "";
            TestTagForeground = testTagForeground ?? Brushes.White;
            LogBody = logBody ?? "";
            LogBodyForeground = logBodyForeground ?? Brushes.White;
            RaisePropertyChanged("");
        }

        public LogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            Timestamp = timestamp ?? "";
            TimestampForeground = timestampForegroundHexCode != null ? new SolidColorBrush(Color.Parse(timestampForegroundHexCode)) : Brushes.White;
            TestTag = testTag ?? "";
            TestTagForeground = testTagForegroundHexCode != null ? new SolidColorBrush(Color.Parse(testTagForegroundHexCode)) : Brushes.White;
            LogBody = logBody ?? "";
            LogBodyForeground = logBodyForegroundHexCode != null ? new SolidColorBrush(Color.Parse(logBodyForegroundHexCode)) : Brushes.White;
            RaisePropertyChanged("");
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return _timestamp + " " + _testTag + " " + _logBody;
        }
    }
}
