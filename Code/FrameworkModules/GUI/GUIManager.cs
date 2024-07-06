using System;
using System.Threading;
using Avalonia.Threading;
using System.Collections.Generic;

using QArantine.Code.FrameworkModules.GUI.Logs;
using QArantine.Code.QArantineGUI;
using QArantine.Code.QArantineGUI.ViewModels;

namespace QArantine.Code.FrameworkModules.GUI
{
    public sealed class GUIManager
    {
        private static readonly GUIManager instance = new GUIManager();
        private Thread? _avaloniaThread;

        public LogBuffer GUILogBuffer { get; set; }
        public MainWindowViewModel? AvaloniaMainWindowViewModel { get; set; }

        // Constructor estático y constructor privado para aplicar el Singleton
        static GUIManager()
        {
        }
        private GUIManager()
        {
            GUILogBuffer = new();

            _avaloniaThread = new Thread(
                () => {
                    AvaloniaStarter.StartAvaloniaApp();
                }
            );
            _avaloniaThread.IsBackground = true;
        }

        public static GUIManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void StartQArantineGUI()
        {
            if (ConfigManager.GetTFConfigParamAsBool("GUIActive"))
            {
                _avaloniaThread?.Start();
            }
        }
    }
}