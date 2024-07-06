using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Avalonia.Interactivity;

using QArantine.Code.FrameworkModules;
using QArantine.Code.FrameworkModules.Logs;
using QArantine.Code.QArantineGUI.Services;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.QArantineGUI.Views
{
    public partial class MainWindow : Window
    {
        private readonly WindowSettingsService _windowSettingsService;

        public MainWindow()
        {
            InitializeComponent();

            _windowSettingsService = new WindowSettingsService();
            _windowSettingsService.LoadWindowSize(this);

            // Suscripción a eventos
            this.Resized += MainWindow_Resized;
            this.PositionChanged += MainWindow_PosChanged;
            this.Loaded += MainWindow_Loaded;

            // Verificar si la aplicación está en modo de aplicación de escritorio
            if (Application.Current != null && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Obtener la pantalla principal
                var primaryScreen = Screens.All.FirstOrDefault(s => s.IsPrimary);

                if (primaryScreen != null)
                {
                    // Obtener el ancho y alto del monitor principal
                    double screenWidth = primaryScreen.Bounds.Width;
                    double screenHeight = primaryScreen.Bounds.Height;

                    // Crear un nuevo objeto PixelPoint con las coordenadas para posicionar la ventana
                    var position = new PixelPoint((int)(screenWidth - this.Width), 0);
                    
                    // Establecer las coordenadas para que la ventana se posicione en la esquina superior derecha
                    this.Position = position;
                }

                // Asignar función de cambio de valor al ComboBox de DebugLvl
                var comboBox = this.FindControl<ComboBox>("DebugLvlComboBox");
                if (comboBox != null) comboBox.SelectionChanged += DebugLvlComboBox_SelectionChanged;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Resized(object? sender, EventArgs e)
        {
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void MainWindow_PosChanged(object? sender, EventArgs e)
        {
            _windowSettingsService.SaveWindowSizeAndPos(this);
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // Obtener una referencia al ScrollViewer
            ScrollViewer? logScrollViewer = this.FindControl<ScrollViewer>("LogScrollViewer");
            
            // Verificar si DataContext no es nulo antes de intentar pasar la referencia al ViewModel
            if (DataContext is MainWindowViewModel viewModel && logScrollViewer != null)
            {
                viewModel.SetScrollViewerReference(logScrollViewer);
            }
        }

        private void DebugLvlComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    viewModel.DebugLvlButtonBorderColor = new SolidColorBrush(Color.Parse(GUILogHandler.LogColorConsoleWindowMap[viewModel.SelectedLogLvl]));
                }
            }
        }

    }
}
