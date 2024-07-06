using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

using QArantine.Code.FrameworkModules.Profiling;
using QArantine.Code.QArantineGUI.Models;
using static QArantine.Code.QArantineGUI.Models.ColorPalettes;

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
        private bool isToggleMeasurementsEnabled;
        private bool isToggleFileDumpEnabled;
        private IBrush toggleMeasurementsButtonBorderColor;
        private IBrush toggleFileDumpButtonBorderColor;
        private Dictionary<string, int[]> flagColors;
        private readonly object flagsProfilingDataLock = new object();
        private string filterText;

        public ProfilingWindowViewModel()
        {
            ToggleMeasurementsCommand = new RelayCommand(ToggleMeasurements);
            ToggleFileDumpCommand = new RelayCommand(ToggleFileDump);

            isToggleMeasurementsEnabled = TFProfiler.Instance.isMeasurementActive;
            isToggleFileDumpEnabled = TFProfiler.Instance.isFileDumpActive;
            toggleMeasurementsButtonBorderColor = TFProfiler.Instance.isMeasurementActive ? new SolidColorBrush(Color.Parse("#3CB93C")) : new SolidColorBrush(Color.Parse("#B44141"));
            toggleFileDumpButtonBorderColor = TFProfiler.Instance.isFileDumpActive ? new SolidColorBrush(Color.Parse("#3CB93C")) : new SolidColorBrush(Color.Parse("#B44141"));

            filterText = "";

            // Flags Profiling
            flagsProfilingData = new ObservableCollection<ObservableCollection<GUIFlagStats>>();

            flagColors = new Dictionary<string, int[]>();

            FlagsChartTitle = new LabelVisual
            {
                Text = "Flags duration",
                TextSize = 22,
                Padding = new LiveChartsCore.Drawing.Padding(14),
                Paint = new SolidColorPaint(SKColors.White)
            };

            FlagsSeries = new ISeries[]
            {
                new StackedColumnSeries<double>
                {
                    Values = new double[] {},
                    Name = "No data found yet"
                }
            };
            FlagsXAxes = new List<Axis>
            {
                new Axis { Labels = new string[] {} }
            };
            FlagsYAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Microseconds (µs)",
                    Labeler = value => $"{value} µs"
                }
            };
            FlagsYSections = new List<RectangularSection>
            {
                new RectangularSection
                {
                    Yi = 16667,
                    Yj = 16667,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Green.WithAlpha(150), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "60 FPS"
                },
                new RectangularSection
                {
                    Yi = 33333,
                    Yj = 33333,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Yellow.WithAlpha(90), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "30 FPS",
                }
            };

            // Memory Profiling
            memoryWorkingSetProfilingData = new ObservableCollection<SysStats>();
            privateMemorySizeProfilingData = new ObservableCollection<SysStats>();

            MemoryChartTitle = new LabelVisual
            {
                Text = "Memory usage",
                TextSize = 22,
                Padding = new LiveChartsCore.Drawing.Padding(14),
                Paint = new SolidColorPaint(SKColors.White)
            };

            MemorySeries = new ISeries[]
            {
                new StackedColumnSeries<double>
                {
                    Values = new double[] {},
                    Name = "No data found yet"
                }
            };
            MemoryXAxes = new List<Axis>
            {
                new Axis { Labels = new string[] {} }
            };
            MemoryYAxes = new List<Axis>
            {
                new Axis
                {
                    Name = "Bytes",
                    Labeler = value => $"{value / 1000000:0.##} MB"
                }
            };
            MemoryYSections = new List<RectangularSection>
            {
                new RectangularSection
                {
                    Yi = 500000000,
                    Yj = 500000000,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Green.WithAlpha(150), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "1 GB"
                },
                new RectangularSection
                {
                    Yi = 1000000000,
                    Yj = 1000000000,
                    Stroke = new SolidColorPaint { StrokeThickness = 3, Color = SKColors.Yellow.WithAlpha(90), PathEffect = new DashEffect(new float[] { 6, 6 }) },
                    Label = "500 MB",
                }
            };

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
                lock (flagsProfilingDataLock)
                {
                    FlagsProfilingData.Clear();
                }
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
                if (TFProfiler.Instance.isMeasurementActive != value)
                {
                    TFProfiler.Instance.SetMeasurementEnabledState(value);
                    isToggleMeasurementsEnabled = TFProfiler.Instance.isMeasurementActive;
                    RaisePropertyChanged(nameof(IsToggleMeasurementsEnabled));
                    ToggleMeasurementsButtonBorderColor = isToggleMeasurementsEnabled ? new SolidColorBrush(Color.Parse("#3CB93C")) : new SolidColorBrush(Color.Parse("#B44141"));
                    
                    if (isToggleMeasurementsEnabled) FlagsProfilingData.Clear();
                }
            }
        }

        public bool IsToggleFileDumpEnabled
        {
            get { return isToggleFileDumpEnabled; }
            set
            {
                if (TFProfiler.Instance.isFileDumpActive != value)
                {
                    TFProfiler.Instance.SetFileDumpEnabledState(value);
                    isToggleFileDumpEnabled = TFProfiler.Instance.isFileDumpActive;
                    RaisePropertyChanged(nameof(IsToggleFileDumpEnabled));
                    ToggleFileDumpButtonBorderColor = isToggleFileDumpEnabled ? new SolidColorBrush(Color.Parse("#3CB93C")) : new SolidColorBrush(Color.Parse("#B44141"));
                }
            }
        }

        public IBrush ToggleMeasurementsButtonBorderColor
        {
            get { return toggleMeasurementsButtonBorderColor; }
            set
            {
                toggleMeasurementsButtonBorderColor = value;
                RaisePropertyChanged(nameof(ToggleMeasurementsButtonBorderColor));
            }
        }

        public IBrush ToggleFileDumpButtonBorderColor
        {
            get { return toggleFileDumpButtonBorderColor; }
            set
            {
                toggleFileDumpButtonBorderColor = value;
                RaisePropertyChanged(nameof(ToggleFileDumpButtonBorderColor));
            }
        }

        private void ToggleMeasurements()
        {
            IsToggleMeasurementsEnabled = !IsToggleMeasurementsEnabled;
        }

        private void ToggleFileDump()
        {
            IsToggleFileDumpEnabled = !IsToggleFileDumpEnabled;
        }

        private void OnStatsUpdated(object? sender, ProfilingDataEventArgs eventStats)
        {
            // Memory
            if (memoryWorkingSetProfilingData.Count >= 10)
            {
                memoryWorkingSetProfilingData.RemoveAt(memoryWorkingSetProfilingData.Count - 1);
            }
            memoryWorkingSetProfilingData.Insert(0, eventStats.MemoryWorkingSetSysStats);
            RaisePropertyChanged(nameof(MemoryWorkingSetProfilingData));

            if (privateMemorySizeProfilingData.Count >= 10)
            {
                privateMemorySizeProfilingData.RemoveAt(privateMemorySizeProfilingData.Count - 1);
            }
            privateMemorySizeProfilingData.Insert(0, eventStats.PrivateMemorySizeSysStats);
            RaisePropertyChanged(nameof(PrivateMemorySizeProfilingData));

            UpdateMemoryChartSeries();

            // Flags
            if (flagsProfilingData.Count >= 10)
            {
                flagsProfilingData.RemoveAt(flagsProfilingData.Count - 1);
            }

            lock (flagsProfilingDataLock)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    flagsProfilingData.Insert(0, GetGUIFlagStatsListFromSourceFlagStats(eventStats.FlagsStats));
                    RaisePropertyChanged(nameof(FlagsProfilingData));
                    UpdateFlagsChartSeries();
                });
            }
        }

        private void UpdateMemoryChartSeries()
        {
            List<LineSeries<double>> chartSeries = new List<LineSeries<double>>();
            List<double> chartValues = new List<double>();

            foreach (SysStats stat in memoryWorkingSetProfilingData)
            {
                chartValues.Add(stat.Value);
            }

            chartSeries.Add(new LineSeries<double>
            {
                Name = "Memory Working Set",
                Values = chartValues.ToArray(),
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
                Values = chartValues.ToArray(),
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.LightGreen),
                GeometryFill = new SolidColorPaint(SKColors.Green),
                GeometryStroke = new SolidColorPaint(SKColors.LightGreen),
                LineSmoothness = 1 
            });

            MemorySeries = chartSeries.ToArray();
            RaisePropertyChanged(nameof(MemorySeries));
        }

        private ObservableCollection<GUIFlagStats> GetGUIFlagStatsListFromSourceFlagStats(List<FlagStats> sourceFlagStats)
        {
            ObservableCollection<GUIFlagStats> resultList = new ObservableCollection<GUIFlagStats>();
            foreach(FlagStats sourceStat in sourceFlagStats)
            {
                if (!flagColors.ContainsKey(sourceStat.ID))
                {
                    flagColors[sourceStat.ID] = BasicChartColorPalette[flagColors.Count % BasicChartColorPalette.Count];
                }

                if (string.IsNullOrWhiteSpace(FilterText) || sourceStat.ID.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                {
                    int[] brushColor = flagColors[sourceStat.ID];
                    resultList.Add(new GUIFlagStats(sourceStat, new SolidColorBrush(new Color(255, (byte)brushColor[0], (byte)brushColor[1], (byte)brushColor[2]))));
                }
            }
            return resultList;
        }

        private void UpdateFlagsChartSeries()
        {
            List<StackedColumnSeries<double>> chartSeries = new List<StackedColumnSeries<double>>();
            Dictionary<string, List<double>> flagsValues = new();

            for (int i = 0; i < flagsProfilingData.Count; i++)
            {
                foreach (GUIFlagStats stat in flagsProfilingData[i])
                {
                    if (!flagsValues.ContainsKey(stat.ID)) flagsValues[stat.ID] = new List<double>();
                    flagsValues[stat.ID].Add(stat.Average);

                    if (!flagColors.ContainsKey(stat.ID))
                    {
                        flagColors[stat.ID] = BasicChartColorPalette[flagColors.Count % BasicChartColorPalette.Count];
                    }
                }
            }

            foreach (var kvp in flagsValues.OrderBy(kvp => kvp.Value.Average()))
            {
                chartSeries.Add(new StackedColumnSeries<double>
                {
                    Name = $"{kvp.Key}",
                    Values = kvp.Value.ToArray(),
                    Fill = new SolidColorPaint(ColorPalettes.GetSKColor(flagColors[kvp.Key]))
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
