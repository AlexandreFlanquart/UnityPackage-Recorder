using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

namespace Prismify.Recorder
{
    public static class VideoCapture
    {
        private static string videoFolder = Application.dataPath + "/Videos";
        private static string videoName = "Recording";
        private static bool autoCreateFolders = true;
        private static RecorderController recorderController;

        public static void Init(string folder, string name, bool autoCreate)
        {
            videoFolder = folder;
            videoName = name;
            autoCreateFolders = autoCreate;
        }

        public static void PrepareRecording(bool audio = true)
        {
            // Create RecorderController if not exists
            if (recorderController == null)
            {
                var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
                recorderController = new RecorderController(settings);
            }

            // Configure recorder for video
            var movieRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieRecorder.name = "Video Recorder";
            movieRecorder.Enabled = true;
            
            // Configure input settings
            movieRecorder.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = 1920,
                OutputHeight = 1080
            };
            movieRecorder.AudioInputSettings.PreserveAudio = audio;

            // Set output path
            string fileName = $"{videoName}.mp4";
            string fullPath = Path.Combine(videoFolder, fileName);
            movieRecorder.OutputFile = fullPath;

            // Add recorder to controller
            var controllerSettings = recorderController.Settings;
            controllerSettings.AddRecorderSettings(movieRecorder);

            recorderController.PrepareRecording();
        }

        public static void StartRecording(bool audio = true)
        {
            if (!EditorApplication.isPlaying)
            {
                RecorderWindow.AddLog("❌ Cannot start recording: Play mode must be active!");
                return;
            }

            PrepareRecording(audio);
            RecorderWindow.AddLog("Preparing recording...");
            
            recorderController.StartRecording();
            RecorderWindow.AddLog($"Started recording: {videoName}");

        }

        public static void StopRecording()
        {
            if (recorderController == null)
            {
                RecorderWindow.AddLog("❌ No active recording!");
                return;
            }

            if (!recorderController.IsRecording())
            {
                RecorderWindow.AddLog("❌ No active recording!");
                return;
            }

            try
            {
                recorderController.StopRecording();
                RecorderWindow.AddLog($"Stopped recording: {videoName}");
            }
            catch (System.Exception e)
            {
                RecorderWindow.AddLog($"❌ Error stopping recording: {e.Message}");
            }
            finally
            {
                recorderController = null;
            }
        }

        public static void OpenVideoFolder()
        {
            if (Directory.Exists(videoFolder))
            {
                EditorUtility.RevealInFinder(videoFolder);
            }
            else if (autoCreateFolders)
            {
                CreateFolderIfNotExists(videoFolder);
                EditorUtility.RevealInFinder(videoFolder);
            }
            else
            {
                Debug.LogError($"Video folder does not exist: {videoFolder}");
            }
        }

        private static void CreateFolderIfNotExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
        }
    }
} 