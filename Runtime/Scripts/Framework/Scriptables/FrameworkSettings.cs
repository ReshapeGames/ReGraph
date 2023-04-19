using System.Collections.Generic;
using Reshape.Unity;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
    using Reshape.Unity.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEditor.SceneManagement;
#endif

namespace Reshape.ReFramework
{
    //[CreateAssetMenu(menuName = "Reshape/ReFramework Settings", order = 23)]
    [HideMonoScript]
    public class FrameworkSettings : ScriptableObject
    {
        [BoxGroup("Graph")]
        public GameObject graphManager;
        [BoxGroup("Graph")]
        [PropertyTooltip("Skip execute all debug behaviour node")]
        [LabelText("Skip Debug Node")]
        public bool skipDebugNode;
        [BoxGroup("Character Control")]
        [LabelText("1st Person Controller")]
        public GameObject fpPlayerController;
        [BoxGroup("Character Control")]
        [LabelText("3rd Person Controller")]
        public GameObject tpPlayerController;
        [BoxGroup("OutLine")]
        public Shader outlineShader;
        [BoxGroup("OutLine")]
        public Shader outlineBufferShader;
        [BoxGroup("UI")]
        public GameObject dialogCanvas;

#if UNITY_EDITOR
        public static FrameworkSettings GetSettings ()
        {
            var settings = FindSettings();
            return settings;
        }

        internal static SerializedObject GetSerializedSettings ()
        {
            return new SerializedObject(GetSettings());
        }
        
        static FrameworkSettings FindSettings ()
        {
            var guids = AssetDatabase.FindAssets("t:FrameworkSettings");
            if (guids.Length > 1)
            {
                ReDebug.LogWarning("Framework Editor",$"Found multiple settings files, currently is using the first found settings file.", false);
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<FrameworkSettings>(path);
            }
        }
        
        public static List<T> LoadAssets<T> () where T : UnityEngine.Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets;
        }
        
        public static List<string> GetAssetPaths<T> () where T : UnityEngine.Object
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<string> paths = new List<string>();
            foreach (var assetId in assetIds)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                paths.Add(path);
            }

            return paths;
        }
        
        [MenuItem("GameObject/Reshape/1st Person Controller", false, 101)]
        public static void AddFpPlayerController()
        {
            if ( ReEditorHelper.IsInPrefabStage() )
            {
                ReDebug.LogWarning("Not able to do add 1st Person Controller when you are editing a prefab!");
                EditorUtility.DisplayDialog("1st Person Controller", "Not able to do add controller when you are editing a prefab!", "OK");
                return;
            }
            
            var controller = FindObjectOfType<FirstPersonController>();
            if (controller != null)
            {
                ReDebug.LogWarning("Not able to do add 1st Person Controller since there is already have one in the scene!");
                EditorUtility.DisplayDialog("1st Person Controller", "Not able to do add controller since there is already have one in the scene!", "OK");
                return;
            }

            if (Camera.main != null)
            {
                ReDebug.LogWarning("Please aware there is a camera in 1st Person Controller while your scene have another MainCamera!");
                EditorUtility.DisplayDialog("1st Person Controller", "Please aware there is a camera in 1st Person Controller while your scene have another MainCamera!", "OK");
            }

            var settings = FrameworkSettings.GetSettings();
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(settings.fpPlayerController);
            go.name = "First Person Controller";
            ReDebug.Log("Created First Person Controller GameObject!");
            EditorSceneManager.MarkAllScenesDirty();
        }
        
        [MenuItem("GameObject/Reshape/3rd Person Controller", false, 101)]
        public static void AddTpPlayerController()
        {
            if ( ReEditorHelper.IsInPrefabStage() )
            {
                ReDebug.LogWarning("Not able to do add 3rd Person Controller when you are editing a prefab!");
                EditorUtility.DisplayDialog("3rd Person Controller", "Not able to do add controller when you are editing a prefab!", "OK");
                return;
            }
            
            var controller = FindObjectOfType<ThirdPersonController>();
            if (controller != null)
            {
                ReDebug.LogWarning("Not able to do add 3rd Person Controller since there is already have one in the scene!");
                EditorUtility.DisplayDialog("3rd Person Controller", "Not able to do add controller since there is already have one in the scene!", "OK");
                return;
            }
            
            if (Camera.main != null)
            {
                ReDebug.LogWarning("Please aware there is a camera in 3rd Person Controller while your scene have another MainCamera!");
                EditorUtility.DisplayDialog("3rd Person Controller", "Please aware there is a camera in 3rd Person Controller while your scene have another MainCamera!", "OK");
            }

            var settings = FrameworkSettings.GetSettings();
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(settings.tpPlayerController);
            go.name = "Third Person Controller";
            ReDebug.Log("Created Third Person Controller GameObject!");
            EditorSceneManager.MarkAllScenesDirty();
        }
#endif
    }
}