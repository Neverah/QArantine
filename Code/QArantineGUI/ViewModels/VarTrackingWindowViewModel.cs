using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Avalonia.Media;

using QArantine.Code.QArantineGUI.Models;
using QArantine.Code.QArantineGUI.StaticData;
using QArantine.Code.FrameworkModules.VarTracking;
using static QArantine.Code.QArantineGUI.StaticData.ColorPalettes;

namespace QArantine.Code.QArantineGUI.ViewModels
{
    public class VarTrackingWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<GUITrackedVar> varTrackingData;
        public ICommand ToggleTrackingCommand { get; }
        private bool isToggleTrackingEnabled;
        private string toggleTrackingButtonIcon;
        private Dictionary<string, int[]> trackedVarsColors;
        private readonly object varTrackingDataLock = new object();
        private string filterText;

        public VarTrackingWindowViewModel()
        {
            ToggleTrackingCommand = new RelayCommand(ToggleTracking);

            isToggleTrackingEnabled = VarTracker.Instance.IsTrackingActive;
            toggleTrackingButtonIcon = GetCurrentToggleTrackingIcon();

            filterText = "";

            // VarTracking
            varTrackingData = [];
            trackedVarsColors = [];

            // Events
            VarTracker.Instance.VarValuesUpdated += OnVarValuesUpdated;
        }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                RaisePropertyChanged(nameof(FilterText));
                lock (varTrackingDataLock)
                {
                    VarTrackingData.Clear();
                }
            }
        }

        public ObservableCollection<GUITrackedVar> VarTrackingData
        {
            get => varTrackingData;
            set
            {
                varTrackingData = value;
                RaisePropertyChanged(nameof(VarTrackingData));
            }
        }

        public bool IsToggleTrackingEnabled
        {
            get { return isToggleTrackingEnabled; }
            set
            {
                if (VarTracker.Instance.IsTrackingActive != value)
                {
                    VarTracker.Instance.SetTrackingEnabledState(value);
                    isToggleTrackingEnabled = VarTracker.Instance.IsTrackingActive;
                    RaisePropertyChanged(nameof(IsToggleTrackingEnabled));

                    if (isToggleTrackingEnabled)
                    {
                        VarTrackingData.Clear();
                    }
                }
                ToggleTrackingButtonIcon = GetCurrentToggleTrackingIcon();
            }
        }

        public string ToggleTrackingButtonIcon
        {
            get { return toggleTrackingButtonIcon; }
            set
            {
                toggleTrackingButtonIcon = value;
                RaisePropertyChanged(nameof(ToggleTrackingButtonIcon));
            }
        }

        private string GetCurrentToggleTrackingIcon()
        {
            return VarTracker.Instance.IsTrackingActive ? IconsDictionary.QArantineIconsDictionary["Pause"] : IconsDictionary.QArantineIconsDictionary["Start"];
        }

        private void ToggleTracking()
        {
            IsToggleTrackingEnabled = !IsToggleTrackingEnabled;
        }

        private void OnVarValuesUpdated(object? sender, VarTrackerUpdateEventArgs eventTrackedVars)
        {
            lock (varTrackingDataLock)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    VarTrackingData = GetGUITrackedVarsListFromSourceTrackedVars(eventTrackedVars.TrackedVars);
                    RaisePropertyChanged(nameof(VarTrackingData));
                });
            }
        }

        private ObservableCollection<GUITrackedVar> GetGUITrackedVarsListFromSourceTrackedVars(List<TrackedVar> sourceTrackedVars)
        {
            ObservableCollection<GUITrackedVar> resultList = [];
            foreach(TrackedVar sourceTrackedVar in sourceTrackedVars)
            {
                if (!trackedVarsColors.TryGetValue(sourceTrackedVar.ID, out int[]? value))
                {
                    value = BasicChartColorPalette[trackedVarsColors.Count % BasicChartColorPalette.Count];
                    trackedVarsColors[sourceTrackedVar.ID] = value;
                }

                if (string.IsNullOrWhiteSpace(FilterText) || sourceTrackedVar.ID.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                {
                    int[] brushColor = value;
                    resultList.Add(new GUITrackedVar(sourceTrackedVar, new SolidColorBrush(new Color(255, (byte)brushColor[0], (byte)brushColor[1], (byte)brushColor[2]))));
                }
            }
            return resultList;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
