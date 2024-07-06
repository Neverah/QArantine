using System;
using System.Diagnostics;

namespace QArantine.Code.FrameworkModules
{
    public class TimeManager
    {
        public static Stopwatch AppClock { get; }

        static TimeManager()
        {
            AppClock = Stopwatch.StartNew();
        }

        public static string GetAppElapsedSecondsAsString()
        {
            return AppClock.Elapsed.TotalSeconds.ToString("0.000");
        }

        public static string GetAppElapsedTimeAsFormatedString()
        {
            TimeSpan elapsedTime = AppClock.Elapsed;
            return $"{elapsedTime.Hours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}";;
        }

        public static long GetAppElapsedSecondsAsLong()
        {
            return AppClock.Elapsed.Ticks / TimeSpan.TicksPerMillisecond / 1000;
        }

        public static long GetAppElapsedMilisecondsAsLong()
        {
            return AppClock.Elapsed.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}