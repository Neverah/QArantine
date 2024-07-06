using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace QArantine.Code.FrameworkModules.VideoRecording
{
    public class ScreenRecorder
    {
        private string? ffmpegExeFilePath;
        private Process? ffmpegProcess;
        private string currRecordingVideoFile = "";
        private bool isRecording = false;
        private bool isOutputBeingRecorded = false;
        private string standardOutputLogFile = "ffmpeg_output.log";
        private string errorOutputLogFile = "ffmpeg_error.log";
        private Thread? outputThread;
        private Thread? errorThread;

        public ScreenRecorder()
        {
            ffmpegExeFilePath = ConfigManager.GetTFConfigParamAsString("FFmpegExeFilePath");

            Process.GetCurrentProcess().Exited += ParentProcess_Exited;
            Console.CancelKeyPress += Console_CancelKeyPress;
        }

        // Be careful with the Desktop version, the ffmpeg process won't stop by itself if you interrump the main process abruptly, it is recommended to use the window version for a smooth experience
        public void StartRecordingDesktop(string outputFilePath, bool createVideoRecordLogFiles, int framerate = 30)
        {
            currRecordingVideoFile = outputFilePath;
            var arguments = $"-y -f gdigrab -framerate {framerate} -i desktop -c:v libx264 -profile:v main -level 4.1 -pix_fmt yuv420p {outputFilePath}";
            CommonRecordingProcessStart(arguments, createVideoRecordLogFiles);
        }

        public void StartRecordingWindow(string outputFilePath, string windowName, bool createVideoRecordLogFiles, int framerate = 30)
        {
            currRecordingVideoFile = outputFilePath;
            var arguments = $"-y -f gdigrab -framerate {framerate} -i title=\"{windowName}\" -c:v libx264 -profile:v main -level 4.1 -pix_fmt yuv420p {outputFilePath}";
            CommonRecordingProcessStart(arguments, createVideoRecordLogFiles);
        }

        private void CommonRecordingProcessStart(string arguments, bool createVideoRecordLogFiles = false)
        {
            LogOK($"Starting to record video: {currRecordingVideoFile}");

            ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegExeFilePath,
                    Arguments = arguments,
                    RedirectStandardOutput = createVideoRecordLogFiles,
                    RedirectStandardError = createVideoRecordLogFiles,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Inicia el proceso de FFmpeg
            ffmpegProcess?.Start();
            isRecording = true;

            if (createVideoRecordLogFiles)
            {
                isOutputBeingRecorded = true;
                // Redirige la salida estándar y la salida de error a archivos en hilos separados
                if (ffmpegProcess?.StandardOutput != null)
                {
                    outputThread = new Thread(() => RedirectOutput(ffmpegProcess.StandardOutput, standardOutputLogFile));
                    outputThread.Start();
                }

                if (ffmpegProcess?.StandardError != null)
                {
                    errorThread = new Thread(() => RedirectOutput(ffmpegProcess.StandardError, errorOutputLogFile));
                    errorThread.Start();
                }
            }
        }

        private void RedirectOutput(StreamReader input, string outputFilePath)
        {
            using (var output = new StreamWriter(outputFilePath))
            {
                string? line;
                while ((line = input.ReadLine()) != null)
                {
                    output.WriteLine(line);
                }
            }
        }

        public void StopRecording()
        {
            if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            {
                try
                {
                    // Envía el comando "q" para detener la grabación
                    ffmpegProcess.StandardInput.WriteLine("q");

                    // Espera a que los hilos de salida terminen
                    if (isOutputBeingRecorded)
                    {
                        outputThread?.Join();
                        errorThread?.Join();
                    }

                    // Se espera a que el proceso se cierre
                    if (!ffmpegProcess.WaitForExit(10000))
                    {
                        throw new Exception("FFmpeg didn't stop correctly in the expected time frame (10s)");
                    }

                    LogOK($"FFmpeg video file created: {currRecordingVideoFile}");
                }
                catch (Exception ex)
                {
                    // Loguear la excepción si es necesario
                    LogFatalError($"Error when trying to stop FFmpeg lawfully: {ex.Message}");
                }
                finally
                {
                    // Forzar la terminación del proceso si aún está activo
                    if (!ffmpegProcess.HasExited)
                    {
                        ffmpegProcess.Kill(true); // Forzar la terminación inmediata
                        ffmpegProcess.WaitForExit();
                    }

                    ffmpegProcess.Dispose();
                    isRecording = false;
                }
            }
        }

        private void ParentProcess_Exited(object? sender, EventArgs e)
        {
            if (isRecording)
            {
                LogOK("Forced close of the ffmpeg process: main process exited");
                ffmpegProcess?.Kill(true);
            }
        }

        private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            // Detener la grabación cuando se detecta que se intenta cerrar la aplicación desde el IDE
            if (isRecording)
            {
                LogOK("Forced close of the ffmpeg process: cancel key press");
                ffmpegProcess?.Kill(true);
            }
        }
    }
}