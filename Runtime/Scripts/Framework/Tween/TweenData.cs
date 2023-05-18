using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
#if UNITY_EDITOR
using System;
using System.IO;
using Reshape.ReGraph;
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

namespace Reshape.ReFramework
{
    public class TweenData : ScriptableObject
    {
        [HideInInspector]
        public string id;

        [ValidateInput("ValidateDuration", "Value must be more than 0 and smaller than 10,000,000!", InfoMessageType.Warning)]
        [InlineProperty]
        [CustomContextMenu("Ping Origin Graph", "FindOriginGraph")]
        public FloatProperty duration = new FloatProperty(1f);

        [HideInInspector]
        [CustomContextMenu("Ping Origin Graph", "FindOriginGraph")]
        public bool customEase;

        [HideIf("customEase")]
        [InlineButton("SwitchCustomEase", "▼")]
        [CustomContextMenu("Ping Origin Graph", "FindOriginGraph")]
        public Ease ease;

        [ShowIf("customEase")]
        [InlineButton("SwitchCustomEase", "▼")]
        [CustomContextMenu("Ping Origin Graph", "FindOriginGraph")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public virtual Tween GetTween (Transform transform) { return default; }
        public virtual Tween GetTween (RectTransform transform) { return default; }
        public virtual Tween GetTween (Rigidbody transform) { return default; }
        public virtual Tween GetTween (Rigidbody2D transform) { return default; }
        public virtual Tween GetTween (Light transform) { return default; }
        public virtual Tween GetTween (Camera transform) { return default; }
        public virtual Tween GetTween (AudioMixer transform) { return default; }
        public virtual Tween GetTween (AudioSource transform) { return default; }
        public virtual Tween GetTween (ParticleSystem transform) { return default; }
        public virtual Tween GetTween (MeshRenderer renderer) { return default; }
        public virtual Tween GetTween (SpriteRenderer transform) { return default; }
        public virtual Tween GetTween (LineRenderer transform) { return default; }
        public virtual Tween GetTween (TrailRenderer transform) { return default; }
        public virtual Tween GetTween (Graphic transform) { return default; }
        public virtual Tween GetTween (Image transform) { return default; }
        public virtual Tween GetTween (Text transform) { return default; }
        public virtual Tween GetTween (Slider transform) { return default; }
        public virtual Tween GetTween (CanvasGroup transform) { return default; }
        public virtual Tween GetTween (LayoutElement transform) { return default; }
        public virtual Tween GetTween (ScrollRect transform) { return default; }
        public virtual Tween GetTween (TextMeshProUGUI transform) { return default; }
        public virtual Tween GetTween (TextMeshPro transform) { return default; }

#if UNITY_EDITOR
        private bool ValidateDuration (float value)
        {
            return value > 0f && value < 10000000;
        }

        private void SwitchCustomEase ()
        {
            customEase = !customEase;
        }
        
        public void FindOriginGraph ()
        {
            string graphId = name.Substring(0, name.IndexOf('.'));
            int.TryParse(graphId, out int id);
            var found = FindObjectsOfType(typeof(GraphRunner));
            int searchLength = found.Length;
            for (var i = 0; i < searchLength; i++)
            {
                GraphRunner runner = (GraphRunner) found[i];
                if (runner.graph.id == id)
                {
                    EditorGUIUtility.PingObject(runner.gameObject);
                    break;
                }
            }
        }

        [MenuItem("Tools/Reshape/Delete Unused TweenData")]
        public static void DeleteUnusedTweenData ()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            CleanTweenData(SceneManager.GetActiveScene(), activeScene.path);
        }

        public static TweenData CreateTweenData<T> (string id, string scenePath) where T : TweenData
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                EditorUtility.DisplayDialog("Graph Runner", "Please save your newly created scene before continue setup the tween node.", "OK");
                return default;
            }

            string sceneFolder = Path.GetDirectoryName(scenePath);
            string folderName = Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR_WIN
            string dataFolder = sceneFolder + $"\\{folderName}\\";
#elif UNITY_EDITOR_OSX
            string dataFolder = sceneFolder + $"/{folderName}/";
#endif
            string fileName = id;
            string filePath = dataFolder + fileName + ".asset";
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);
            if (File.Exists(filePath))
            {
                TweenData existingTween = AssetDatabase.LoadAssetAtPath<TweenData>(filePath);
                if (existingTween is T)
                    return existingTween;
                AssetDatabase.DeleteAsset(filePath);
            }

            TweenData tweenData = ScriptableObject.CreateInstance<T>();
            tweenData.id = fileName;
            AssetDatabase.CreateAsset(tweenData, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return tweenData;
        }

        public static TweenData GetTweenData<T> (string id, string scenePath) where T : TweenData
        {
            if (string.IsNullOrEmpty(scenePath))
                return default;
            string sceneFolder = Path.GetDirectoryName(scenePath);
            string folderName = Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR_WIN
            string dataFolder = sceneFolder + $"\\{folderName}\\";
#elif UNITY_EDITOR_OSX
            string dataFolder = sceneFolder + $"/{folderName}/";
#endif
            if (!Directory.Exists(dataFolder))
                return default;
            string fileName = id;
            string filePath = dataFolder + fileName + ".asset";
            if (!File.Exists(filePath))
                return default;
            TweenData existingTween = AssetDatabase.LoadAssetAtPath<TweenData>(filePath);
            if (existingTween is T)
                return existingTween;
            return default;
        }

        public static TweenData CloneTweenData (string id, string scenePath, TweenData origin)
        {
            string sceneFolder = Path.GetDirectoryName(scenePath);
            string folderName = Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR_WIN
            string dataFolder = sceneFolder + $"\\{folderName}\\";
#elif UNITY_EDITOR_OSX
            string dataFolder = sceneFolder + $"/{folderName}/";
#endif
            string fileName = id;
            string filePath = dataFolder + fileName + ".asset";
            if (!Directory.Exists(dataFolder))
                return default;
            if (File.Exists(filePath))
                return default;
            var tweenData = Instantiate(origin);
            tweenData.id = fileName;
            AssetDatabase.CreateAsset(tweenData, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return tweenData;
        }
        
        public static void CleanTweenData (Scene scene, string path)
        {
            var settings = FrameworkSettings.GetSettings();
            if (settings == null)
                return;
            if (!settings.removeAtSaveScene)
                return;

            string sceneFolder = Path.GetDirectoryName(path);
            string folderName = Path.GetFileNameWithoutExtension(path);
#if UNITY_EDITOR_WIN
            string dataFolder = sceneFolder + $"\\{folderName}\\";
#elif UNITY_EDITOR_OSX
            string dataFolder = sceneFolder + $"/{folderName}/";
#endif
            if (!Directory.Exists(dataFolder))
                return;
            EditorUtility.DisplayProgressBar("Graph Saving", "Searching tween data files", 0);
            var foundAllGraphRunner = UnityEngine.Object.FindObjectsOfType(typeof(GraphRunner));
            var graphRunnerCount = foundAllGraphRunner.Length;
            var folderInfo = new DirectoryInfo(dataFolder);
            var folderFilesInfo = folderInfo.GetFiles("*.asset");
            var folderFilesInfoCount = folderFilesInfo.Length;
            var currentProcessing = 0f;
            foreach (var fileInfo in folderFilesInfo)
            {
                currentProcessing++;
                EditorUtility.DisplayProgressBar("Graph Saving", "Processing tween data files", currentProcessing/folderFilesInfoCount);
                string graphId = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf('.'));
                int.TryParse(graphId, out int id);
                if (id > 0)
                {
                    bool found = false;
                    for (var i = 0; i < graphRunnerCount; i++)
                    {
                        GraphRunner runner = (GraphRunner) foundAllGraphRunner[i];
                        if (runner.graph.id == id)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        continue;
                    string filePath = dataFolder + fileInfo.Name;
                    if (!File.Exists(filePath))
                        continue;
                    AssetDatabase.DeleteAsset(filePath);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
#endif
    }
}