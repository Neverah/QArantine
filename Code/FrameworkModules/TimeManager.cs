

using System.Diagnostics;

namespace TestFramework.Code.FrameworkModules
{
    public class TimeManager
    {
        public static Stopwatch AppClock = new Stopwatch();

        static TimeManager() => AppClock.Start();
    }
}