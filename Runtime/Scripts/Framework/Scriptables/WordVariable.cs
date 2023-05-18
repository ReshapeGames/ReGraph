using System;
using Reshape.Unity;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [CreateAssetMenu(menuName = "Reshape/Word Variable", order = 12)]
    [Serializable]
    public class WordVariable : VariableScriptableObject
    {
        [MultiLineResizable(5)]
        [OnValueChanged("OnUpdatePreview")]
        [OnInspectorInit("OnUpdatePreview")]
        [HideLabel]
        [BoxGroup("Value")]
        public string value;

        [ListDrawerSettings(OnBeginListElementGUI = "OnUpdatePreview")]
        [OnValueChanged("OnUpdatePreview")]
        [BoxGroup("Value")]
        [InfoBox("Refer Preview to check the simulated value with parameters", "@param.Length > 0", InfoMessageType = InfoMessageType.Info)]
        public VariableScriptableObject[] param;

        [ReadOnly]
        [MultiLineResizable(5)]
        [HideInEditorMode]
        [BoxGroup("Runtime Value")]
        [HideLabel]
        public string runtimeValue;

        private bool inited;

        public string GetValue ()
        {
            if (Init())
                SetValue(value);
            return runtimeValue;
        }
        
        public override object GetObject ()
        {
            return GetValue();
        }
        
        public override void SetObject (object obj)
        {
            SetValue((string)obj);
        }

        public void SetValue (string value)
        {
            if (param != null)
            {
                string temp = value;
                if (param.Length > 0)
                {
                    try
                    {
                        temp = String.Format(value, param);
                    }
                    catch { }
                }

                if (!IsEqual(temp))
                {
                    runtimeValue = temp;
                    OnChanged();
                }
            }
            else
            {
                if (!IsEqual(value))
                {
                    runtimeValue = value;
                    OnChanged();
                }
            }

            Init();
        }

        public bool Init ()
        {
            if (inited == false)
            {
                inited = true;
                if (param != null)
                {
                    for (int i = 0; i < param.Length; i++)
                    {
                        param[i].onEarlyChange -= OnParamEarlyChanged;
                        param[i].onEarlyChange += OnParamEarlyChanged;
                    }
                }

                return true;
            }

            return false;
        }

        private void OnParamEarlyChanged ()
        {
            SetValue(value);
        }

        public bool IsEqual (string value)
        {
            return string.Equals(runtimeValue, value);
        }

        public bool Contains (string value)
        {
            return runtimeValue.Contains(value);
        }

        public static implicit operator string (WordVariable s)
        {
            return s.GetValue();
        }

        public override string ToString ()
        {
            return GetValue();
        }

        protected override void OnChanged ()
        {
            onReset -= OnReset;
            onReset += OnReset;
            base.OnEarlyChanged();
            base.OnChanged();
        }

        public override void OnReset ()
        {
            SetValue(value);
            inited = false;
            base.OnReset();
        }

#if UNITY_EDITOR
        private void OnUpdatePreview ()
        {
            if (!EditorApplication.isPlaying)
            {
                SetValue(value);
            }
        }

        public static VariableScriptableObject CreateNew (VariableScriptableObject variable)
        {
            if (variable == null)
            {
                return (VariableScriptableObject) CreateNew(null);
            }
            else if (variable.GetType() == typeof(WordVariable))
            {
                return (VariableScriptableObject) CreateNew((WordVariable) variable);
            }
            else
            {
                bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
                if (proceed)
                {
                    var word = CreateNew(null);
                    if (word != null)
                        return (VariableScriptableObject) word;
                }
            }
            return variable;
        }

        public static WordVariable CreateNew (WordVariable word)
        {
            if (word != null)
            {
                bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
                if (!proceed)
                    return word;
            }

            var path = EditorUtility.SaveFilePanelInProject("Graph Variable", "New Word Variable", "asset", "Select a location to create variable");
            if (path.Length == 0)
                return word;
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                return word;

            WordVariable variable = ScriptableObject.CreateInstance<WordVariable>();
            AssetDatabase.CreateAsset(variable, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return variable;
        }
#endif
    }

#if UNITY_EDITOR
    [CustomPreview(typeof(WordVariable))]
    public class WordVariablePreview : ObjectPreview
    {
        public override bool HasPreviewGUI ()
        {
            return true;
        }

        public override void OnPreviewGUI (Rect r, GUIStyle background)
        {
            var variable = (WordVariable) target;
            string previewValue;
            var value = variable.value;
            if (EditorApplication.isPlaying)
                value = variable.runtimeValue;
            if (variable.param != null && variable.param.Length > 0)
            {
                try
                {
                    previewValue = String.Format(value, variable.param);
                }
                catch
                {
                    previewValue = value;
                }
            }
            else
            {
                previewValue = value;
            }

            GUI.enabled = false;
            GUIStyle style = new GUIStyle(EditorStyles.textArea);
            style.wordWrap = true;
            EditorGUI.TextField(r, previewValue, style);
            GUI.enabled = true;
        }
    }

    [InitializeOnLoad]
    public static class WordVariableResetOnPlay
    {
        static WordVariableResetOnPlay ()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged (PlayModeStateChange state)
        {
            bool update = false;
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                    update = true;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                    update = true;
            }

            if (update)
            {
                string[] guids = AssetDatabase.FindAssets("t:WordVariable");
                if (guids.Length > 0)
                {
                    for (int i = 0; i < guids.Length; i++)
                    {
                        WordVariable variable = (WordVariable) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(UnityEngine.Object));
                        if (variable != null)
                        {
                            variable.OnReset();
                        }
                    }

                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
#endif
}