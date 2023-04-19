#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    public class GraphEditorVariable
    {
#if UNITY_EDITOR
        public const string OutputBNPreviousParentGuid = "_ReGraph.OutputBehaviourNode.PreviousParentGuid";
        
        public static string GetString (string id, string variable)
        {
            return EditorPrefs.GetString(id+variable, string.Empty);
        }
        
        public static void SetString (string id, string variable, string value)
        {
            EditorPrefs.SetString(id+variable, value);
        }
        
        public static bool GetBool (string id, string variable)
        {
            return EditorPrefs.GetBool(id+variable, false);
        }
        
        public static void SetBool (string id, string variable, bool value)
        {
            EditorPrefs.SetBool(id+variable, value);
        }
#endif
    }
}