using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MyUnityPackage.Recorder
{
    public static class ScreenshotCapture
    {
        private static string screenshotFolder = Application.dataPath + "/Screenshots";
        private static string screenshotName = "Screenshot";
        private static bool autoCreateFolders = true;
        private static bool isTakingScreenshot = false;
        private static bool shouldStopScreenshot = false;

        [Space]
        [SerializeField] private static List<GameObject> outlinedObjects;
        private static bool outlineShowed = true;


        public static void Init(string folder, string name, bool autoCreate)
        {
            RecorderWindow.AddLog($"Initializing screenshot capture with folder: {folder}, name: {name}, autoCreate: {autoCreate}");
            screenshotFolder = folder;
            screenshotName = name;
            autoCreateFolders = autoCreate;
        }

        public static void TakeScreenshot(string fileName = null)
        {
            if (fileName == null)
            {
                fileName = screenshotName;
            }
            string fullPath = Path.Combine(screenshotFolder, fileName);
            ScreenCapture.CaptureScreenshot(fullPath);
            RecorderWindow.AddLog($"Screenshot taken: {screenshotName} saved in {screenshotFolder}");
        }

        public static void TakeAllPictures(List<GameObject> viewpoints, int delay = 1000)
        {
            if (isTakingScreenshot)
            {
                RecorderWindow.AddLog("❌ Screenshot capture already in progress!");
                return;
            }
            isTakingScreenshot = true;
            shouldStopScreenshot = false;

            // Save initial camera position
            Vector3 initPos = SceneView.lastActiveSceneView.pivot;
            Quaternion initRotation = SceneView.lastActiveSceneView.rotation;

            var task = TakeAllPicturesAsync(viewpoints, delay);
            
            // Handle completion
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    RecorderWindow.AddLog($"Error taking screenshots: {t.Exception}");
                }
                // Restore initial camera position  
                SceneView.lastActiveSceneView.pivot = initPos;
                SceneView.lastActiveSceneView.rotation = initRotation;
                SceneView.lastActiveSceneView.Repaint();
                RecorderWindow.currentAction = "Waiting for action...";
                isTakingScreenshot = false;
                shouldStopScreenshot = false;
            });
        }

        private static async Task TakeAllPicturesAsync(List<GameObject> viewpoints, int delay)
        {
            int i = 0;
            foreach (var viewpoint in viewpoints)
            {
                if (shouldStopScreenshot)
                {
                    RecorderWindow.AddLog("Screenshot capture stopped!");
                    return;
                }

                if (viewpoint == null) continue;

                // Position Main Camera on Viewpoint
                SceneView.lastActiveSceneView.pivot = viewpoint.transform.position;
                SceneView.lastActiveSceneView.rotation = viewpoint.transform.rotation;
                SceneView.lastActiveSceneView.Repaint();

                // Take screenshot
                TakeScreenshot(screenshotName + "_" + i + ".png");
                RecorderWindow.AddLog($"Screenshot taken from viewpoint: {viewpoint.name}");

                // Wait for delay
                await Task.Delay(delay);
                i++;
            }
            if(!shouldStopScreenshot)
            {
                RecorderWindow.AddLog("✅ All screenshots completed!");
            }
        }

        public static void StopScreenshot()
        {
            if (!isTakingScreenshot)
            {
                RecorderWindow.AddLog("❌ No screenshot capture in progress!");
                return;
            }

            shouldStopScreenshot = true;
            isTakingScreenshot = false;
            RecorderWindow.AddLog("✅ Screenshot capture stopped!");
        }

        private static void CreateFolderIfNotExists(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning("folder not found : " + folderPath);
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
                Debug.Log("Created folder: " + folderPath);
            }
        }

        public static void OpenScreenshotFolder()
        {
            if (Directory.Exists(screenshotFolder))
            {
                EditorUtility.RevealInFinder(screenshotFolder);
            }
            else if(autoCreateFolders)
            {
                CreateFolderIfNotExists(screenshotFolder);
                EditorUtility.RevealInFinder(screenshotFolder);
            }
            else
            {
                Debug.LogError($"Screenshot folder does not exist: {screenshotFolder}");
            }
        }


/*

        [Button]
        private static void ShowOutline()
        {
            outlineShowed = !outlineShowed;

            for (int i = 0; i < outlinedObjects.Count; i++)
            {
                if (outlinedObjects[i].GetComponent<Outlinable>() != null)
                {
                    outlinedObjects[i].GetComponent<Outlinable>().enabled = outlineShowed;
                }
                else
                {
                    var outline = outlinedObjects[i].AddComponent<Outlinable>();

                }
            }
        }
*/

        /*
        IEnumerator TakePictures()
        {
            yield return new WaitForSeconds(2.0f);
            
            ScreenCapture.CaptureScreenshot(path + ".png");
            int lastID = 0;
            Debug.Log("path : " + path + ".png");
            Debug.Log("outlinedObjects.Count : " + outlinedObjects.Count);
            yield return new WaitForSeconds(2.0f);

            for (int i = 0; i < outlinedObjects.Count; i++)
            {
                Debug.Log("take screen");
                outlinedObjects[lastID].GetComponent<Outlinable>().enabled = false;
                outlinedObjects[i].GetComponent<Outlinable>().enabled = true;
                lastID = i;
                Debug.Log("screen : " + outlinedObjects[i].name);
                ScreenCapture.CaptureScreenshot(path + "_" + outlinedObjects[i].name + ".png");
                yield return new WaitForSeconds(2.0f);
            }
            Debug.Log("finish");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
   
        }
*/


    }
}
