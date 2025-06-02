using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using QArantine.Code.QArantineGUI.Services;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.QArantineGUI.Views
{
    public partial class InputSimulationWindow : Window
    {
        public InputSimulationWindow()
        {
            InitializeComponent();
            DataContext = new InputSimulationWindowViewModel();

            WindowSettingsService.LoadWindowSize(this);

            // Suscripción a eventos
            this.Resized += InputSimulationWindow_Resized;
            this.PositionChanged += InputSimulationWindow_PosChanged;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputSimulationWindow_Resized(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void InputSimulationWindow_PosChanged(object? sender, EventArgs e)
        {
            WindowSettingsService.SaveWindowSizeAndPos(this);
        }
    }
}
