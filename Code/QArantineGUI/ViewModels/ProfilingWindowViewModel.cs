using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

using QArantine.Code.FrameworkModules.Profiling;
using QArantine.Code.QArantineGUI.Models;
using QArantine.Code.QArantineGUI.StaticData;
using static QArantine.Code.QArantineGUI.StaticData.ColorPalettes;

namespace QArantine.Code.QArantineGUI.ViewModels
{
    public class ProfilingWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<ObservableCollection<GUIFlagStats>> flagsProfilingData;
        private ObservableCollection<SysStats> memoryWorkingSetProfilingData;
        private ObservableCollection<SysStats> privateMemorySizeProfilingData;
        public LabelVisual FlagsChartTitle { get; private set; }
        public ISeries[] FlagsSeries { get; private set; }
        public List<Axis> FlagsXAxes { get; private set; }
        public List<Axis> FlagsYAxes { get; private set; }
        public List<RectangularSection> FlagsYSections { get; private set; }
        public LabelVisual MemoryChartTitle { get; private set; }
        public ISeries[] MemorySeries { get; private set; }
        public List<Axis> MemoryXAxes { get; private set; }
        public List<Axis> MemoryYAxes { get; private set; }
        public List<RectangularSection> MemoryYSections { get; private set; }
        public ICommand ToggleMeasurementsCommand { get; }
        public ICommand ToggleFileDumpCommand { get; }
        public ICommand ToggleQArantineFilterCommand { get; }
        public ICommand ClearProfilingDataCommand { get; }
        private bool isToggleMeasurementsEnabled;
        private bool isToggleFileDumpEnabled;
        private bool isQArantineFilterEnabled;
        private string toggleMeasurementsButtonIcon;
        private string toggleFileDumpButtonIcon;
        private string toggleQArantineFilterButtonIcon;
        private readonly Dictionary<string, int[]> flagColors;
        private readonly object flagsProfilingDataLock = new();
        private readonly object memoryDataLock = new();
        private string filterText;

        public ProfilingWindowViewModel()
        {
            ToggleMeasurementsCommand = new RelayCommand(ToggleMeasurements);
            ToggleFileDumpCommand = new RelayCommand(ToggleFileDump);
            ToggleQArantineFilterCommand = new RelayCommand(ToggleQArantineFilter);
            ClearProfilingDataCommand = new RelayCommand(ClearProfilingData);

            isToggleMeasurementsEnabled = TFProfiler.Instance.IsMeasurementActive;
            isToggleFileDumpEnabled = TFProfiler.Instance.IsFileDumpActive;
            isQArantineFilterEnabled = true;
            toggleMeasurementsButtonIcon = GetCurrentToggleMeasurementsIcon();
            toggleFileDumpButtonIcon = GetCurrentToggleFileDumpIcon();
            toggleQArantineFilterButtonIcon = GetCurrentQArantineFilterButtonIcon();

            filterText = "";

            // Flags Profiling
            flagsProfilingData = [];

            flagColors = [];

            FlagsChartTitle = new LabelVisual
            {
                Text = "Flags duration",
                TextSize = 22,
                Padding = new LiveChartsCore.Drawing.Padding(14),
                Paint = new SolidColorPaint(SKColors.White)
            };

            FlagsSeries =
            [
                new StackedColumnSeries<double>
                {
                    Values = [],
                    Name = "No data found yet"
                }
            ];
            FlagsXAxes =
            [
                new Axis { Labels = [] }
            ];
            FlagsYAxes =
            [
                new Axis
                {
                    Name = "Microseconds (µs)",
                    Labeler = value => $"{value} µs"
                }
            ];
            FlagsYSections =
            [
                new() {
                    Yi = 16667,
                    Yj = 16667,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Green.WithAlpha(150), PathEffect = new DashEffect([6, 6]) },
                    Label = "60 FPS"
                },
                new() {
                    Yi = 33333,
                    Yj = 33333,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Yellow.WithAlpha(90), PathEffect = new DashEffect([6, 6]) },
                    Label = "30 FPS",
                }
            ];

            // Memory Profiling
            memoryWorkingSetProfilingData = [];
            privateMemorySizeProfilingData = [];

            MemoryChartTitle = new LabelVisual
            {
                Text = "Memory usage",
                TextSize = 22,
                Padding = new LiveChartsCore.Drawing.Padding(14),
                Paint = new SolidColorPaint(SKColors.White)
            };

            MemorySeries =
            [
                new StackedColumnSeries<double>
                {
                    Values = [],
                    Name = "No data found yet"
                }
            ];
            MemoryXAxes =
            [
                new Axis { Labels = [] }
            ];
            MemoryYAxes =
            [
                new Axis
                {
                    Name = "Bytes",
                    Labeler = value => $"{value / 1000000:0.##} MB"
                }
            ];
            MemoryYSections =
            [
                new() {
                    Yi = 500000000,
                    Yj = 500000000,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Green.WithAlpha(150), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "1 GB"
                },
                new() {
                    Yi = 1000000000,
                    Yj = 1000000000,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Yellow.WithAlpha(90), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "500 MB",
                }
            ];

            // Events
            TFProfiler.Instance.StatsUpdated += OnStatsUpdated;
        }

        public string FilterText
        {
            get { return filterText; }
            set
            {
                filterText = value;
                RaisePropertyChanged(nameof(FilterText));
            }
        }

        public ObservableCollection<ObservableCollection<GUIFlagStats>> FlagsProfilingData
        {
            get => flagsProfilingData;
            set
            {
                flagsProfilingData = value;
                RaisePropertyChanged(nameof(FlagsProfilingData));
            }
        }

        public ObservableCollection<SysStats> MemoryWorkingSetProfilingData
        {
            get => memoryWorkingSetProfilingData;
            set
            {
                memoryWorkingSetProfilingData = value;
                RaisePropertyChanged(nameof(MemoryWorkingSetProfilingData));
            }
        }

        public ObservableCollection<SysStats> PrivateMemorySizeProfilingData
        {
            get => privateMemorySizeProfilingData;
            set
            {
                privateMemorySizeProfilingData = value;
                RaisePropertyChanged(nameof(PrivateMemorySizeProfilingData));
            }
        }

        public bool IsToggleMeasurementsEnabled
        {
            get { return isToggleMeasurementsEnabled; }
            set
            {
                if (TFProfiler.Instance.IsMeasurementActive != value)
                {
                    TFProfiler.Instance.SetMeasurementEnabledState(value);
                    isToggleMeasurementsEnabled = TFProfiler.Instance.IsMeasurementActive;
                    RaisePropertyChanged(nameof(IsToggleMeasurementsEnabled));

                    if (!isToggleMeasurementsEnabled)
                    {
                        isToggleFileDumpEnabled = false;
                        IsToggleFileDumpEnabled = false;
                    }
                }
                ToggleMeasurementsButtonIcon = GetCurrentToggleMeasurementsIcon();
            }
        }

        public bool IsToggleFileDumpEnabled
        {
            get { return isToggleFileDumpEnabled; }
            set
            {
                if (TFProfiler.Instance.IsFileDumpActive != value)
                {
                    TFProfiler.Instance.SetFileDumpEnabledState(value);
                    isToggleFileDumpEnabled = TFProfiler.Instance.IsFileDumpActive;
                    RaisePropertyChanged(nameof(IsToggleFileDumpEnabled));
                }
                ToggleFileDumpButtonIcon = GetCurrentToggleFileDumpIcon();
            }
        }

        public bool IsQArantineFilterEnabled
        {
            get { return isQArantineFilterEnabled; }
            set 
            { 
                isQArantineFilterEnabled = value;
                ToggleQArantineFilterButtonIcon = GetCurrentQArantineFilterButtonIcon();
            }
        }

        public string ToggleMeasurementsButtonIcon
        {
            get { return toggleMeasurementsButtonIcon; }
            set
            {
                toggleMeasurementsButtonIcon = value;
                RaisePropertyChanged(nameof(ToggleMeasurementsButtonIcon));
            }
        }

        public string ToggleFileDumpButtonIcon
        {
            get { return toggleFileDumpButtonIcon; }
            set
            {
                toggleFileDumpButtonIcon = value;
                RaisePropertyChanged(nameof(ToggleFileDumpButtonIcon));
            }
        }

        public string ToggleQArantineFilterButtonIcon
        {
            get { return toggleQArantineFilterButtonIcon; }
            set
            {
                toggleQArantineFilterButtonIcon = value;
                RaisePropertyChanged(nameof(ToggleQArantineFilterButtonIcon));
            }
        }

        private string GetCurrentToggleMeasurementsIcon()
        {
            return TFProfiler.Instance.IsMeasurementActive ? IconsDictionary.QArantineIconsDictionary["ProfMeasurements_Enabled"] : IconsDictionary.QArantineIconsDictionary["ProfMeasurements_Disabled"];
        }

        private string GetCurrentToggleFileDumpIcon()
        {
            return TFProfiler.Instance.IsFileDumpActive ? IconsDictionary.QArantineIconsDictionary["ProfFileDump_Enabled"] : IconsDictionary.QArantineIconsDictionary["ProfFileDump_Disabled"];
        }

        private string GetCurrentQArantineFilterButtonIcon()
        {
            return isQArantineFilterEnabled ? IconsDictionary.QArantineIconsDictionary["Filter_Enabled"] : IconsDictionary.QArantineIconsDictionary["Filter_Disabled"];
        }

        private void ToggleMeasurements()
        {
            IsToggleMeasurementsEnabled = !IsToggleMeasurementsEnabled;
        }

        private void ToggleFileDump()
        {
            IsToggleFileDumpEnabled = !IsToggleFileDumpEnabled;
        }

        private void ToggleQArantineFilter()
        {
            IsQArantineFilterEnabled = !IsQArantineFilterEnabled;
        }

        private void ClearProfilingData()
        {
            lock (flagsProfilingDataLock)
            {
                FlagsProfilingData.Clear();
            }
            lock (memoryDataLock)
            {
                MemoryWorkingSetProfilingData.Clear();
                PrivateMemorySizeProfilingData.Clear();
            }
        }

        private void OnStatsUpdated(object? sender, ProfilingDataEventArgs eventStats)
        {
            // Memory
            lock (memoryDataLock)
            {
                if (memoryWorkingSetProfilingData.Count >= 10)
                {
                    memoryWorkingSetProfilingData.RemoveAt(memoryWorkingSetProfilingData.Count - 1);
                }
                memoryWorkingSetProfilingData.Insert(0, eventStats.MemoryWorkingSetSysStats);
            }
            RaisePropertyChanged(nameof(MemoryWorkingSetProfilingData));

            lock (memoryDataLock)
            {
                if (privateMemorySizeProfilingData.Count >= 10)
                {
                    privateMemorySizeProfilingData.RemoveAt(privateMemorySizeProfilingData.Count - 1);
                }
                privateMemorySizeProfilingData.Insert(0, eventStats.PrivateMemorySizeSysStats);
            }
            RaisePropertyChanged(nameof(PrivateMemorySizeProfilingData));

            UpdateMemoryChartSeries();

            // Flags
            lock (flagsProfilingDataLock)
            {
                if (flagsProfilingData.Count >= 10)
                {
                    flagsProfilingData.RemoveAt(flagsProfilingData.Count - 1);
                }
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                lock (flagsProfilingDataLock)
                {
                    flagsProfilingData.Insert(0, GetGUIFlagStatsListFromSourceFlagStats(eventStats.FlagsStats));
                }
                RaisePropertyChanged(nameof(FlagsProfilingData));
                UpdateFlagsChartSeries();
            });
        }

        private void UpdateMemoryChartSeries()
        {
            List<LineSeries<double>> chartSeries = [];
            List<double> chartValues = [];

            foreach (SysStats stat in memoryWorkingSetProfilingData)
            {
                chartValues.Add(stat.Value);
            }

            chartSeries.Add(new LineSeries<double>
            {
                Name = "Memory Working Set",
                Values = [.. chartValues],
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightBlue),
                GeometryFill = new SolidColorPaint(SKColors.Cyan),
                GeometryStroke = new SolidColorPaint(SKColors.LightBlue),
                LineSmoothness = 1 
            });

            chartValues.Clear();

            foreach (SysStats stat in privateMemorySizeProfilingData)
            {
                chartValues.Add(stat.Value);
            }

            chartSeries.Add(new LineSeries<double>
            {
                Name = "Private Memory Size",
                Values = [.. chartValues],
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightGreen),
                GeometryFill = new SolidColorPaint(SKColors.Green),
                GeometryStroke = new SolidColorPaint(SKColors.LightGreen),
                LineSmoothness = 1 
            });

            MemorySeries = [.. chartSeries];
            RaisePropertyChanged(nameof(MemorySeries));
        }

        private ObservableCollection<GUIFlagStats> GetGUIFlagStatsListFromSourceFlagStats(List<FlagStats> sourceFlagStats)
        {
            ObservableCollection<GUIFlagStats> resultList = [];
            foreach(FlagStats sourceStat in sourceFlagStats)
            {
                if (!flagColors.TryGetValue(sourceStat.ID, out int[]? value))
                {
                    value = BasicChartColorPalette[flagColors.Count % BasicChartColorPalette.Count];
                    flagColors[sourceStat.ID] = value;
                }

                if ((string.IsNullOrWhiteSpace(FilterText) || sourceStat.ID.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) && (!IsQArantineFilterEnabled || !sourceStat.ID.Contains("QArantine", StringComparison.OrdinalIgnoreCase)))
                {
                    int[] brushColor = value;
                    resultList.Add(new GUIFlagStats(sourceStat, new SolidColorBrush(new Color(255, (byte)brushColor[0], (byte)brushColor[1], (byte)brushColor[2]))));
                }
            }
            return resultList;
        }

        private void UpdateFlagsChartSeries()
        {
            List<StackedColumnSeries<double>> chartSeries = [];
            Dictionary<string, List<double>> flagsValues = [];

            // Inicializa las listas de valores para cada ID de flag
            foreach (ObservableCollection<GUIFlagStats> statCollection in flagsProfilingData)
            {
                foreach (GUIFlagStats stat in statCollection)
                {
                    if (!flagsValues.ContainsKey(stat.ID))
                    {
                        flagsValues[stat.ID] = new List<double>(new double[flagsProfilingData.Count]);
                    }
                }
            }

            // Rellena las listas de valores con los datos actuales
            for (int i = 0; i < flagsProfilingData.Count; i++)
            {
                foreach (GUIFlagStats stat in flagsProfilingData[i])
                {
                    flagsValues[stat.ID][i] = stat.Average;
                }
            }

            // Crea las series de la gráfica a partir de las listas de valores
            foreach (var kvp in flagsValues.OrderBy(kvp => kvp.Value.Average()))
            {
                chartSeries.Add(new StackedColumnSeries<double>
                {
                    Name = $"{kvp.Key}",
                    Values = [.. kvp.Value],
                    Fill = new SolidColorPaint(GetSKColor(flagColors[kvp.Key]))
                });
            }

            FlagsSeries = chartSeries.ToArray();
            RaisePropertyChanged(nameof(FlagsSeries));
        }


        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
