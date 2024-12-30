using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls;

using QArantine.Code.FrameworkModules;
using QArantine.Code.FrameworkModules.Logs;
using QArantine.Code.FrameworkModules.GUI;
using QArantine.Code.FrameworkModules.GUI.Logs;
using QArantine.Code.QArantineGUI.Views;
using QArantine.Code.QArantineGUI.Models;
using QArantine.Code.QArantineGUI.StaticData;

namespace QArantine.Code.QArantineGUI.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<GUILogLine> _logLines;
        private LogBuffer tfLogBuffer;
        private ScrollViewer? _logScrollViewer;

        public LogManager.LogLevel SelectedLogLvl
        {
            get { return LogManager.LogLvl; }
            set 
            {
                LogManager.LogLvl = value;
                RaisePropertyChanged(nameof(SelectedLogLvl));
                LogOK($"Log level changed to: {SelectedLogLvl}");
            }
        }
        public List<string> AvailableLogLvls { get; }
        private IBrush _logLvlButtonForegroundColor = Brushes.White;
        public IBrush LogLvlButtonForegroundColor
        {
            get { return _logLvlButtonForegroundColor; }
            set
            {
                _logLvlButtonForegroundColor = value;
                RaisePropertyChanged(nameof(LogLvlButtonForegroundColor));
            }
        }

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
                    AutoScrollButtonIcon = IsAutoScrollEnabled ? IconsDictionary.QArantineIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.QArantineIconsDictionary["AutoScroll_Disabled"];
                    if (_isAutoScrollEnabled)
                    {
                        ScrollToBottom();
                    }
                }
            }
        }

        private string _autoScrollButtonIcon = IconsDictionary.QArantineIconsDictionary["No_Image"];
        public string AutoScrollButtonIcon
        {
            get { return _autoScrollButtonIcon; }
            set
            {
                _autoScrollButtonIcon = value;
                RaisePropertyChanged(nameof(AutoScrollButtonIcon));
            }
        }

        public ICommand ToggleAutoScrollCommand { get; }
        public ICommand ClearLogScrollCommand { get; }
        public ICommand OpenVarTrackingCommand { get; }
        public ICommand OpenProfilingCommand { get; }
        private VarTrackingWindow? varTrackingWindow;
        private ProfilingWindow? profilingWindow;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<GUILogLine> LogLines
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
            AvailableLogLvls = Enum.GetValues(typeof(LogManager.LogLevel)).Cast<LogManager.LogLevel>().Select(e => e.ToString()).ToList();
            LogLvlButtonForegroundColor = new SolidColorBrush(Color.Parse(GUILogHandler.LogColorConsoleWindowMap[SelectedLogLvl]));
            AutoScrollButtonIcon = IsAutoScrollEnabled ? IconsDictionary.QArantineIconsDictionary["AutoScroll_Enabled"] : IconsDictionary.QArantineIconsDictionary["AutoScroll_Disabled"];

            ClearLogScrollCommand = new RelayCommand(ClearLogLines);
            ToggleAutoScrollCommand = new RelayCommand(ToggleAutoScroll);
            OpenVarTrackingCommand = new RelayCommand(OpenVarTrackingWindow);
            OpenProfilingCommand = new RelayCommand(OpenProfilingWindow);
            // Se añade la referencia a esta ventana en el GUIManager
            GUIManager.Instance.AvaloniaMainWindowViewModel = this;
            // Se obtiene la referencia al buffer de Logs del TF
            tfLogBuffer = GUIManager.Instance.GUILogBuffer;
            SuscribeToTFLogBuffer(tfLogBuffer);

            _logLines = [];
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

        private void OpenVarTrackingWindow()
        {
            if (varTrackingWindow == null)
            {
                varTrackingWindow = new VarTrackingWindow();
                varTrackingWindow.Closed += (sender, e) => varTrackingWindow = null;
                varTrackingWindow.Show();
            }
            else
            {
                varTrackingWindow.Activate();
            }
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

        private GUILogLine GetFinalLogLineFromBufferLogLine(BufferLogLine bufferLogLine)
        {
            return new GUILogLine(bufferLogLine.Timestamp, bufferLogLine.TimestampForegroundHexCode, bufferLogLine.TestTag, bufferLogLine.TestTagForegroundHexCode, bufferLogLine.LogBody, bufferLogLine.LogBodyForegroundHexCode);
        }

        public void AddLogLine(GUILogLine logLine)
        {
            LogLines.Add(logLine);
        }

        public void AddLogLine(string timestamp, IBrush timestampForeground, string testTag, IBrush testTagForeground, string logBody, IBrush logBodyForeground)
        {
            LogLines.Add(new GUILogLine(timestamp, timestampForeground, testTag, testTagForeground, logBody, logBodyForeground ));
        }

        public void AddLogLine(string timestamp, string timestampForegroundHexCode, string testTag, string testTagForegroundHexCode, string logBody, string logBodyForegroundHexCode)
        {
            LogLines.Add(new GUILogLine(timestamp, timestampForegroundHexCode, testTag, testTagForegroundHexCode, logBody, logBodyForegroundHexCode));
        }
    }
}
