using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

using QArantine.Code.QArantineGUI.Services;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.QArantineGUI.Views
{
    public partial class VarTrackingWindow : Window
    {
        private readonly WindowSettingsService _windowSettingsService;

        public VarTrackingWindow()
        {
            InitializeComponent();
            DataContext = new VarTrackingWindowViewModel();

            _windowSettingsService = new WindowSettingsService();
            _windowSettingsService.LoadWindowSize(this);

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
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void VarTrackingWindow_PosChanged(object? sender, EventArgs e)
        {
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}
