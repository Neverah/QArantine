

using System.Diagnostics;

namespace TestFramework.Code.FrameworkModules
{
    public class TimeManager
    {
        public static Stopwatch AppClock { get; }

        static TimeManager()
        {
            AppClock = new();
            AppClock.Start();
        }

        public static string GetAppElapsedTimeAsString()
        {
            return AppClock.Elapsed.TotalSeconds.ToString("0.000");
        }
    }
}