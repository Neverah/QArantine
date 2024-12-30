namespace QArantine.Code.FrameworkModules.VarTracking
{
    public class TrackedVar
    {
        public string ID { get; private set; }
        public Func<object> UpdateFunc { get; private set; }
        public string? LastValue { get; private set; }
        public string CurrentValue { get { UpdateLastValue(); return LastValue!; } }

        public TrackedVar(string id, Func<object> updateFunc)
        {
            ID = id;
            UpdateFunc = updateFunc;
            UpdateLastValue();
        }

        private void UpdateLastValue()
        {
            if (UpdateFunc.Target != null) LastValue = UpdateFunc()?.ToString() ?? "NO_VALUE";
            else LastValue = "NULL_OBJECT_INSTANCE";
        }
    }
}