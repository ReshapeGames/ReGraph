using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Reshape.ReFramework
{
    [InitializeOnLoad]
    public class SceneSaveEvents
    {
        static SceneSaveEvents ()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        static void OnSceneSaving (Scene scene, string path)
        {
            TweenData.CleanTweenData(scene, path);
        }
    }
}