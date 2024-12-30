using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

using QArantine.Code.QArantineGUI.Services;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.QArantineGUI.Views
{
    public partial class ProfilingWindow : Window
    {
        private readonly WindowSettingsService _windowSettingsService;

        public ProfilingWindow()
        {
            InitializeComponent();
            DataContext = new ProfilingWindowViewModel();

            _windowSettingsService = new WindowSettingsService();
            _windowSettingsService.LoadWindowSize(this);

            // Suscripción a eventos
            this.Resized += ProfilingWindow_Resized;
            this.PositionChanged += ProfilingWindow_PosChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ProfilingWindow_Resized(object? sender, EventArgs e)
        {
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void ProfilingWindow_PosChanged(object? sender, EventArgs e)
        {
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}
