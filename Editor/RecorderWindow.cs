using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System;

namespace Prismify.Recorder
{
    public class RecorderWindow : OdinEditorWindow
    {
        private const string VIEWPOINT_TAG = "CameraViewpoint";

        [MenuItem("PRISMIFY/Recorder")]
        public static void ShowWindow()
        {
            GetWindow<RecorderWindow>("Primify Recorder").Show();
        }

#region Screenshot
        [Title("Settings")]
        [Space(10)]

        [TabGroup("Tabs", "Screenshot")]
        [FolderPath]
        [Delayed]
        [OnValueChanged("UpdateScreenshotCapture")]
        public string screenshotFolder = Application.dataPath + "/Screenshots";

        [TabGroup("Tabs", "Screenshot")]
        [Tooltip("Automatically create screenshot folder if not exist")]
        [Delayed]
        [OnValueChanged("UpdateScreenshotCapture")]
        public bool autoCreateFolders = true;

        [TabGroup("Tabs", "Screenshot")]
        [Delayed]
        [OnValueChanged("UpdateScreenshotCapture")]
        public string screenshotName = "Screenshot";

        private void UpdateScreenshotCapture()
        {
            ScreenshotCapture.Init(screenshotFolder, screenshotName, autoCreateFolders);
        }

        [Title("Simple Screenshot")]

        [TabGroup("Tabs", "Screenshot")]
        [Button("Take Screenshot")]
        public void TakeScreenshot()
        {
            currentAction = "Taking screenshot...";
            ScreenshotCapture.TakeScreenshot();    
            currentAction = "Waiting for action...";
        }

        [TabGroup("Tabs", "Screenshot")]
        [Button("Open Screenshots Folder")]
        public void OpenScreenshotFolder()
        {
            ScreenshotCapture.OpenScreenshotFolder();
        }

        [Title("Multiple Screenshots")]

        [TabGroup("Tabs", "Screenshot")]
        [Button("Find Viewpoints")]
        public void FindViewpoints()
        {
            cameraViewpoints.Clear();
            var viewpoints = GameObject.FindGameObjectsWithTag(VIEWPOINT_TAG);
            cameraViewpoints.AddRange(viewpoints);
           
            if (viewpoints.Length == 0)
                AddLog("❌ No viewpoints found!");
            else
                AddLog($"✅ Found {viewpoints.Length} viewpoints in scene");
        }

        [TabGroup("Tabs", "Screenshot")]
        [Button("Clear Viewpoints")]
        public void ClearViewpoints()
        {
            cameraViewpoints.Clear();
            AddLog("ℹ️ Viewpoints list cleared");
        }

        [TabGroup("Tabs", "Screenshot")]
        [LabelText("Camera Viewpoints")]
        public List<GameObject> cameraViewpoints = new List<GameObject>();

        [TabGroup("Tabs", "Screenshot")]
        [Button("Take All Screenshots")]
        public void TakeAllScreenshots()
        {
            if (cameraViewpoints.Count == 0)
            {
                AddLog("❌ No viewpoints defined!");
                return;
            }
            currentAction = "Taking multiple screenshots...";
            ScreenshotCapture.TakeAllPictures(cameraViewpoints, 1000);
           
            AddLog("✅ All screenshots completed!");
        }

        [TabGroup("Tabs", "Screenshot")]
        [Button("Stop Screenshot")]
        public void StopScreenshot()
        {
            ScreenshotCapture.StopScreenshot();
        }

        [TabGroup("Tabs", "Screenshot")]
        [LabelText("Current Viewpoint")]
        [ReadOnly]
        public GameObject currentViewpoint;

        [TabGroup("Tabs", "Screenshot")]
        [Button("Select Viewpoint")]
        public void SelectViewpoint(GameObject viewpoint)
        {
            if (viewpoint != null)
            {
                currentViewpoint = viewpoint;
                // Position camera on viewpoint
                SceneView.lastActiveSceneView.pivot = viewpoint.transform.position;
                SceneView.lastActiveSceneView.rotation = viewpoint.transform.rotation;
                SceneView.lastActiveSceneView.Repaint();
                AddLog($"Selected viewpoint: {viewpoint.name}");
            }
        }
#endregion

#region Video
        [TabGroup("Tabs", "Video")]
        [FolderPath]
        [Delayed]
        [OnValueChanged("UpdateVideoCapture")]
        public string videoFolder = Application.dataPath + "/Videos";

        [TabGroup("Tabs", "Video")]
        [LabelText("Auto Create Folders")]
        [Tooltip("Automatically create video folder if not exist")]
        [Delayed]
        [OnValueChanged("UpdateVideoCapture")]
        public bool autoCreateVideoFolders = true;

        [TabGroup("Tabs", "Video")]
        [LabelText("Allow recording audio")]
        public bool recordAudio = true;

        [TabGroup("Tabs", "Video")]
        [Delayed]
        [OnValueChanged("UpdateVideoCapture")]
        public string videoName = "Recording";

        private void UpdateVideoCapture()
        {
            VideoCapture.Init(videoFolder, videoName, autoCreateVideoFolders);
        }

        [TabGroup("Tabs", "Video")]
        [Button]
        public void StartRecording()
        {
            currentAction = "Recording in progress...";
            VideoCapture.StartRecording(recordAudio);
        }

        [TabGroup("Tabs", "Video")]
        [Button]
        public void StopRecording()
        {
            VideoCapture.StopRecording();
            currentAction = "Waiting for action...";
        }

        [TabGroup("Tabs", "Video")]
        [Button]
        public void OpenVideoFolder()
        {
            VideoCapture.OpenVideoFolder();
        }
#endregion

#region Logs
        [BoxGroup("Logs")]
        [ReadOnly]
        public static string currentAction = "Waiting for action...";

        [BoxGroup("Logs")]
        [ReadOnly]
        [ShowInInspector]
        private static List<string> logs = new List<string>();

        private const int MAX_LOG_MESSAGES = 1000;

        [BoxGroup("Logs")]
        [Button("Add Log")]
        public static void AddLog(string pCustomLogMessage)
        {
            if (!string.IsNullOrEmpty(pCustomLogMessage))
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                logs.Insert(0, $"[{timestamp}] {pCustomLogMessage}");
                
                // Limit the number of messages
                while (logs.Count > MAX_LOG_MESSAGES)
                {
                    logs.RemoveAt(logs.Count - 1);
                }
            }
        }

        [BoxGroup("Logs")]
        [Button("Clear Logs")]
        public void ClearLogs()
        {
            logs.Clear();
            currentAction = "Waiting for action...";
        }
#endregion
    }
}