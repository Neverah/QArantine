using Avalonia.Media;
using QArantine.Code.FrameworkModules.Profiling;

namespace QArantine.Code.QArantineGUI.Models
{
    public class GUIFlagStats
    {
        public string ID { get; private set; }
        public double Average { get; private set; }
        public double Max { get; private set; }
        public double Min { get; private set; }
        public double Sum  { get; private set; }
        public int Count { get; private set; }
        public IBrush ColorBrush { get; set; }

        public GUIFlagStats (string id, double average, double max, double min, double sum, int count)
        {
            ID = id;
            Average = average;
            Max = max;
            Min = min;
            Sum = sum;
            Count = count;
            ColorBrush = ColorPalettes.GetBrush(new int[] {256, 256, 256});
        }

        public GUIFlagStats (string id, double average, double max, double min, double sum, int count, IBrush colorBrush)
        {
            ID = id;
            Average = average;
            Max = max;
            Min = min;
            Sum = sum;
            Count = count;
            ColorBrush = colorBrush;
        }

        public GUIFlagStats (FlagStats flagStats)
        {
            ID = flagStats.ID;
            Average = flagStats.Average;
            Max = flagStats.Max;
            Min = flagStats.Min;
            Sum = flagStats.Sum;
            Count = flagStats.Count;
            ColorBrush = ColorPalettes.GetBrush(new int[] {256, 256, 256});
        }

        public GUIFlagStats (FlagStats flagStats, IBrush colorBrush)
        {
            ID = flagStats.ID;
            Average = flagStats.Average;
            Max = flagStats.Max;
            Min = flagStats.Min;
            Sum = flagStats.Sum;
            Count = flagStats.Count;
            ColorBrush = colorBrush;
        }
    }
}