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

using QArantine.Code.FrameworkModules;
using QArantine.Code.FrameworkModules.GUI;
using QArantine.Code.FrameworkModules.GUI.Logs;
using QArantine.Code.QArantineGUI.Views;

namespace QArantine.Code.QArantineGUI.ViewModels
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
        public ICommand OpenProfilingCommand { get; }
        private ProfilingWindow? profilingWindow;
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
            OpenProfilingCommand = new RelayCommand(OpenProfilingWindow);
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

        private void OpenProfilingWindow()
        {
            if (profilingWindow == null)
            {
                profilingWindow = new ProfilingWindow();
                profilingWindow.Closed += (sender, e) => profilingWindow = null;
                profilingWindow.Show();
            }
            else
            {
                profilingWindow.Activate();
            }
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
        private string _timestamp;
        private IBrush _timestampForeground;
        private string _testTag;
        private IBrush _testTagForeground;
        private string _logBody;
        private IBrush _logBodyForeground;

        public string Timestamp
        {
            get { return _timestamp; }
            set  { _timestamp = value; }
        }

        public IBrush TimestampForeground
        {
            get { return _timestampForeground; }
            set { _timestampForeground = value; }
        }

        public string TestTag
        {
            get { return _testTag; }
            set { _testTag = value; }
        }

        public IBrush TestTagForeground
        {
            get { return _testTagForeground; }
            set {  _testTagForeground = value; }
        }

        public string LogBody
        {
            get { return _logBody; }
            set { _logBody = value; }
        }

        public IBrush LogBodyForeground
        {
            get { return _logBodyForeground; }
            set { _logBodyForeground = value; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public LogLine(string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            _timestamp = timestamp ?? "!NullTextFound!";
            _timestampForeground = timestampForeground ?? Brushes.Blue;
            _testTag = testTag ?? "!NullTextFound!";
            _testTagForeground = testTagForeground ?? Brushes.Blue;
            _logBody = logBody ?? "!NullTextFound!";
            _logBodyForeground = logBodyForeground ?? Brushes.Blue;
            RaisePropertyChanged("");
        }

        public LogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            _timestamp = timestamp ?? "!NullTextFound!";
            _timestampForeground = !string.IsNullOrEmpty(timestampForegroundHexCode) ? new SolidColorBrush(Color.Parse(timestampForegroundHexCode)) : Brushes.Blue;
            _testTag = testTag ?? "!NullTextFound!";
            _testTagForeground = !string.IsNullOrEmpty(testTagForegroundHexCode) ? new SolidColorBrush(Color.Parse(testTagForegroundHexCode)) : Brushes.Blue;
            _logBody = logBody ?? "!NullTextFound!";
            _logBodyForeground = !string.IsNullOrEmpty(logBodyForegroundHexCode) ? new SolidColorBrush(Color.Parse(logBodyForegroundHexCode)) : Brushes.Blue;
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
