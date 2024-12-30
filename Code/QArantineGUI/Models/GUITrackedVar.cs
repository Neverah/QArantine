using Avalonia.Media;
using QArantine.Code.FrameworkModules.VarTracking;
using QArantine.Code.QArantineGUI.StaticData;

namespace QArantine.Code.QArantineGUI.Models
{
    public class GUITrackedVar
    {
        public string Value { get; private set; }
        public IBrush ColorBrush { get; set; }

        public GUITrackedVar (string id, string value)
        {
            Value = $"{id}: {value}";
            ColorBrush = ColorPalettes.GetBrush([256, 256, 256]);
        }

        public GUITrackedVar (string id, string value, IBrush colorBrush)
        {
            Value = $"{id}: {value}";
            ColorBrush = colorBrush;
        }

        public GUITrackedVar (TrackedVar trackedVar)
        {
            Value = $"{trackedVar.ID}: {trackedVar.LastValue}";
            ColorBrush = ColorPalettes.GetBrush([256, 256, 256]);
        }

        public GUITrackedVar (TrackedVar trackedVar, IBrush colorBrush)
        {
            Value = $"{trackedVar.ID}: {trackedVar.LastValue}";
            ColorBrush = colorBrush;
        }
    }
}