namespace QArantine.Code.FrameworkModules
{
    public static class LogExtensions
    {
        public static void LogFatalError(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogFatalError(message);
#endif
        }

        public static void LogError(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogError(message);
#endif
        }

        public static void LogOK(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogOK(message);
#endif
        }

        public static void LogWarning(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogWarning(message);
#endif
        }

        public static void LogDebug(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogDebug(message);
#endif
        }

        public static void LogTestFatalError(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogTestFatalError(message);
#endif
        }

        public static void LogTestError(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogTestError(message);
#endif
        }

        public static void LogTestOK(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogTestOK(message);
#endif
        }

        public static void LogTestWarning(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogTestWarning(message);
#endif
        }

        public static void LogTestDebug(string message)
        {
#if !DISABLE_QARANTINE
            LogManager.LogTestDebug(message);
#endif
        }
    }
}
