namespace QArantine.Code.FrameworkModules.InputSimulation
{
    public static class InputSimulationExtensions
    {
        public static void RecordInputs(long frameId)
        {
#if !DISABLE_QARANTINE
            InputRecorder.Instance.RecordFrameInputs(frameId);
#endif
        }

        public static void PlayFrameInputs(long frameId)
        {
#if !DISABLE_QARANTINE
            InputPlayer.Instance.PlayFrameInputs(frameId);
#endif
        }
    }
}
