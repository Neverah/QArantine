using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using QArantine.Code.QArantineGUI.Services;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.QArantineGUI.Views
{
    public partial class VarTrackingWindow : Window
    {
        public VarTrackingWindow()
        {
            InitializeComponent();
            DataContext = new VarTrackingWindowViewModel();

            WindowSettingsService.LoadWindowSize(this);

            // Suscripción a eventos
            this.Resized += VarTrackingWindow_Resized;
            this.PositionChanged += VarTrackingWindow_PosChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void VarTrackingWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void VarTrackingWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}
