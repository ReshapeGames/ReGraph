using System;
using Reshape.Unity;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace Reshape.ReFramework
{
	[CreateAssetMenu(menuName="Reshape/Word Variable", order = 12)]
	[Serializable]
	public class WordVariable : VariableScriptableObject
	{	
		[MultiLineResizable(5)]
		[OnValueChanged("OnUpdatePreview")]
		[OnInspectorInit("OnUpdatePreview")]
		[OnInspectorGUI("ShowPreview")]
		[HideLabel]
		[BoxGroup("Value")]
		public string value;
		[ListDrawerSettings(OnBeginListElementGUI = "OnUpdatePreview")]
		[OnValueChanged("OnUpdatePreview")]
		[BoxGroup("Value")]
		public VariableScriptableObject[] param;
		[ReadOnly]
		[MultiLineResizable(5)]
		[HideInEditorMode]
		[BoxGroup("Runtime Value")]
		[HideLabel]
		public string runtimeValue;

		[HideInInspector]
		public bool inited;

		public string GetValue ()
		{
			if (Init())
				SetValue(value);
			return runtimeValue;
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
					} catch { }
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

		public bool Init()
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

		private void OnParamEarlyChanged()
		{
			SetValue(value);
		}

		public bool IsEqual(string value)
		{
			return string.Equals(runtimeValue, value);
		}
		
		public bool Contains(string value)
		{
			return runtimeValue.Contains(value);
		}

		public static implicit operator string(WordVariable s)
	    {
	        return s.GetValue();
	    }
		
		public override string ToString()
		{
			return GetValue();
		}

		protected override void OnChanged()
		{
			onReset -= OnReset;
			onReset += OnReset;
			base.OnEarlyChanged();
			base.OnChanged();
		}

		public override void OnReset()
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
		private void ShowPreview()
		{
			if (!EditorApplication.isPlaying)
			{
				string previewValue;
				if (param != null && param.Length > 0)
				{
					try
					{
						previewValue = String.Format(value, param);
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
				EditorGUILayout.TextArea(previewValue);
				GUI.enabled = true;
			}
		}
#endif
	}
	
#if UNITY_EDITOR
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
		    if ( state == PlayModeStateChange.ExitingEditMode )
		    {
			    if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
				    update = true;
		    }
		    else if ( state == PlayModeStateChange.EnteredEditMode )
		    {
			    if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
				    update = true;
		    }

		    if (update)
		    {
			    string[] guids = AssetDatabase.FindAssets("t:WordVariable");
			    if ( guids.Length > 0 )
			    {
				    for ( int i=0; i<guids.Length; i++ )
				    {
					    WordVariable variable = (WordVariable)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(UnityEngine.Object));
					    if ( variable != null )
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