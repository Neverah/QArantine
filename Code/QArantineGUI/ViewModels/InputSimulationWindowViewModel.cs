using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using QArantine.Code.QArantineGUI.StaticData;
using QArantine.Code.FrameworkModules.InputSimulation;
using System.Diagnostics;

namespace QArantine.Code.QArantineGUI.ViewModels
{
    public class InputSimulationWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private InputRecorder inputRecorder;
        private InputPlayer inputPlayer;
        public ICommand StartRecordingCommand { get; }
        public ICommand StopRecordingCommand { get; }
        public ICommand TogglePlaybackCommand { get; }
        public ICommand ToggleContinuousRecordingModeCommand { get; }
        public ICommand ToggleContinuousPlaybackModeCommand { get; }
        public ICommand RefreshInputFilesCommand { get; }
        public ICommand OpenInputFilesDirectoryCommand { get; }
        private string startRecordingButtonIcon;
        private string togglePlaybackButtonIcon;
        private string? playbackSelectedInputFileName;
        public bool IsStartRecordingButtonEnabled => !IsRecordingActive;
        public bool IsStopRecordingButtonEnabled => IsRecordingActive;
        public string RecordingFileName { get; set; }
        public bool ContinuousRecordingModeEnabled { get; set; }
        public bool ContinuousPlaybackModeEnabled { get; set; }
        public string FilterText { get; set; } = string.Empty;
        public ObservableCollection<string> FoundInputFiles { get; }

        public InputSimulationWindowViewModel()
        {
            StartRecordingCommand = new RelayCommand(() => IsRecordingActive = true);
            StopRecordingCommand = new RelayCommand(() => IsRecordingActive = false);
            TogglePlaybackCommand = new RelayCommand(() => IsPlaybackActive = !IsPlaybackActive);
            ToggleContinuousRecordingModeCommand = new RelayCommand(() => ContinuousRecordingModeEnabled = !ContinuousRecordingModeEnabled);
            ToggleContinuousPlaybackModeCommand = new RelayCommand(() => ContinuousPlaybackModeEnabled = !ContinuousPlaybackModeEnabled);
            OpenInputFilesDirectoryCommand = new RelayCommand(OpenInputFilesDirectory);
            RefreshInputFilesCommand = new RelayCommand(RefreshInputFiles);

            inputRecorder = InputRecorder.Instance;
            inputPlayer = InputPlayer.Instance;

            inputRecorder.RecordingStopped += OnRecordingStopped;
            inputPlayer.PlaybackStopped += OnPlaybackStopped;

            FoundInputFiles = [];
            RefreshInputFiles();

            startRecordingButtonIcon = GetCurrentStartRecordingIcon();
            togglePlaybackButtonIcon = GetCurrentTogglePlaybackIcon();

            RecordingFileName = string.Empty;
            PlaybackSelectedInputFileName = string.Empty;
            FilterText = string.Empty;

            ContinuousRecordingModeEnabled = true;
            ContinuousPlaybackModeEnabled = true;
        }

        public bool IsRecordingActive
        {
            get { return inputRecorder.IsRecording; }
            set
            {
                if (inputRecorder.IsRecording != value)
                {
                    if (value)
                    {
                        inputRecorder.StartRecording(RecordingFileName, ContinuousRecordingModeEnabled);
                    }
                    else
                    {
                        inputRecorder.StopRecording();
                    }
                    RaisePropertyChanged(nameof(IsRecordingActive));
                    RaisePropertyChanged(nameof(IsStartRecordingButtonEnabled));
                    RaisePropertyChanged(nameof(IsStopRecordingButtonEnabled));
                    StartRecordingButtonIcon = GetCurrentStartRecordingIcon();
                }
            }
        }

        public bool IsPlaybackActive
        {
            get { return inputPlayer.IsPlaying; }
            set
            {
                if (inputPlayer.IsPlaying != value)
                {
                    if(value)
                    {
                        if (string.IsNullOrEmpty(PlaybackSelectedInputFileName))
                        {
                            LogWarning("No input file selected for input playback.");
                        }
                        else
                        {
                            inputPlayer.LoadFile(Path.Combine(inputRecorder.FilesDirectory ?? "", PlaybackSelectedInputFileName));

                            if(ContinuousPlaybackModeEnabled)
                            {
                                inputPlayer.StartContinuousPlayback();
                            }
                            else
                            {
                                inputPlayer.StartDiscontinuousPlayback();
                            }
                        }
                    }
                    else
                    {
                        inputPlayer.Stop();
                    }
                    RaisePropertyChanged(nameof(IsPlaybackActive));
                    TogglePlaybackButtonIcon = GetCurrentTogglePlaybackIcon();
                }
            }
        }

        public string? PlaybackSelectedInputFileName
        {
            get { return playbackSelectedInputFileName; }
            set
            {
                playbackSelectedInputFileName = value;
                RaisePropertyChanged(nameof(PlaybackSelectedInputFileName));
            }
        }

        public string StartRecordingButtonIcon
        {
            get { return startRecordingButtonIcon; }
            set
            {
                startRecordingButtonIcon = value;
                RaisePropertyChanged(nameof(StartRecordingButtonIcon));
            }
        }

        public string TogglePlaybackButtonIcon
        {
            get { return togglePlaybackButtonIcon; }
            set
            {
                togglePlaybackButtonIcon = value;
                RaisePropertyChanged(nameof(TogglePlaybackButtonIcon));
            }
        }

        private string GetCurrentStartRecordingIcon()
        {
            return IsRecordingActive ? IconsDictionary.QArantineIconsDictionary["Recording"] : IconsDictionary.QArantineIconsDictionary["Start"];
        }

        private string GetCurrentTogglePlaybackIcon()
        {
            return IsPlaybackActive ? IconsDictionary.QArantineIconsDictionary["Stop"] : IconsDictionary.QArantineIconsDictionary["Start"];
        }

        private void OnRecordingStopped()
        {
            RaisePropertyChanged(nameof(IsRecordingActive));
            StartRecordingButtonIcon = GetCurrentStartRecordingIcon();
        }

        private void OnPlaybackStopped()
        {
            RaisePropertyChanged(nameof(IsPlaybackActive));
            TogglePlaybackButtonIcon = GetCurrentTogglePlaybackIcon();
        }

        private void RefreshInputFiles()
        {
            FoundInputFiles.Clear();

            if (!Directory.Exists(inputRecorder.FilesDirectory))
            {
                LogWarning("Input files directory does not exist: " + inputRecorder.FilesDirectory);
                return;
            }

            string[]? files = Directory.GetFiles(inputRecorder.FilesDirectory, "*.json");
            foreach (string? file in files)
            {
                if (string.IsNullOrEmpty(FilterText) || file.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                    FoundInputFiles.Add(Path.GetFileName(file));
            }
            RaisePropertyChanged(nameof(FoundInputFiles));
        }

        private void OpenInputFilesDirectory()
        {
            string? directoryPath = inputRecorder.FilesDirectory;
            if (!string.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
            {
                Process.Start("explorer.exe", directoryPath);
            }
            else
            {
                LogWarning("Input files directory does not exist: " + directoryPath);
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
