using System;
using System.Diagnostics;

using TestFramework.Code.FrameworkModules.VideoRecording;

namespace TestFramework.Code.FrameworkModules
{
    public sealed class RecordingManager
    {   
        private static RecordingManager? _instance;
        private ScreenRecorder screenRecorder;
        private bool IsAlreadyRecording = false;

        private RecordingManager()
        {
            screenRecorder = new();
        }

        public static RecordingManager Instance
        {
            get
            {
                _instance ??= new();
                return _instance;
            }
        }

        public void StartRecordingDesktop(string outputFilePath, int framerate = 30)
        {
            if (IsAlreadyRecording)
            {
                LogWarning($"Won't start recording the video '{outputFilePath}', the ScreenRecorder is already recording");
                return;
            }

            IsAlreadyRecording = true;
            screenRecorder.StartRecordingDesktop(outputFilePath, ConfigManager.GetTFConfigParamAsBool("CreateVideoRecordLogFiles"), framerate);
        }

        public void StartRecordingWindow(string outputFilePath, string windowName, int framerate = 30)
        {
            if (IsAlreadyRecording)
            {
                LogWarning($"Won't start recording the video '{outputFilePath}', the ScreenRecorder is already recording");
                return;
            }

            IsAlreadyRecording = true;
            screenRecorder.StartRecordingWindow(outputFilePath, windowName, ConfigManager.GetTFConfigParamAsBool("CreateVideoRecordLogFiles"), framerate);
        }

        public void StopRecording()
        {
            if (!IsAlreadyRecording)
            {
                LogWarning($"Can't stop recording, the ScreenRecorder wasn't recording anything");
                return;
            }

            IsAlreadyRecording = false;
            screenRecorder.StopRecording();
        }
    }
}