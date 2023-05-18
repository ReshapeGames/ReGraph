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
	[CreateAssetMenu(menuName="Reshape/Number Variable", order = 11)]
	[Serializable]
	public class NumberVariable : VariableScriptableObject
	{	
		[OnValueChanged("OnChangeValue")]
		public float value;
		[ReadOnly]
		[HideInEditorMode]
		public float runtimeValue;
		
		public float GetValue ()
		{
			return runtimeValue;
		}
		
		public override object GetObject ()
		{
			return GetValue();
		}
		
		public override void SetObject (object obj)
		{
			if (obj is int)
				SetValue((int)obj);
			else if (obj is long)
				SetValue((long)obj);
			else if (obj is double)
				SetValue((float)(double)obj);
			else if (obj is float)
				SetValue((float)obj);
		}

		public void SetValue (float value)
		{
			if (!IsEqual(value))
			{
				runtimeValue = value;
				OnChanged();
			}
		}

		public void AddValue (float value)
		{
			runtimeValue += value;
			OnChanged();
		}

		public void MinusValue (float value)
		{
			runtimeValue -= value;
			OnChanged();
		}

		public void MultiplyValue (float value)
		{
			runtimeValue *= value;
			OnChanged();
		}

		public void DivideValue (float value)
		{
			runtimeValue /= value;
			OnChanged();
		}
		
		public void RandomValue ()
		{
			runtimeValue = ReRandom.Range(1,101);
			OnChanged();
		}

		public bool IsEqual(float value)
		{
			return value.Equals(runtimeValue);
		}

		public static implicit operator float(NumberVariable f)
	    {
	        return f.runtimeValue;
	    }

	    public static implicit operator string(NumberVariable f)
	    {
	        return f.ToString();
	    }

	    public override string ToString()
	    {
		    return runtimeValue.DisplayString();
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
		    base.OnReset();
	    }
	    
#if UNITY_EDITOR
		private void OnChangeValue()
		{
			SetValue(value);
		}
		
		public static VariableScriptableObject CreateNew (VariableScriptableObject variable)
		{
			if (variable == null )
			{
				return (VariableScriptableObject) CreateNew(null);
			}
			else if (variable.GetType() == typeof(NumberVariable))
			{
				return (VariableScriptableObject) CreateNew((NumberVariable) variable);
			}
			else
			{
				bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
				if (proceed)
				{
					var number = CreateNew(null);
					if (number != null)
						return (VariableScriptableObject) number;
				}
			}
			return variable;
		}
		
		public static NumberVariable CreateNew (NumberVariable number)
		{
			if (number != null)
			{
				bool proceed = EditorUtility.DisplayDialog("Graph Variable", "Are you sure you want to create a new variable to replace the existing assigned variable ?", "OK", "Cancel");
				if (!proceed)
					return number;
			}

			var path = EditorUtility.SaveFilePanelInProject("Graph Variable", "New Number Variable", "asset", "Select a location to create variable");
			if (path.Length == 0)
				return number;
			if (!Directory.Exists(Path.GetDirectoryName(path)))
				return number;

			NumberVariable variable = ScriptableObject.CreateInstance<NumberVariable>();
			AssetDatabase.CreateAsset(variable, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return variable;
		}
#endif
	}
	
#if UNITY_EDITOR
	[InitializeOnLoad]
	public static class NumberVariableResetOnPlay
	{
	    static NumberVariableResetOnPlay ()
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
		        string[] guids = AssetDatabase.FindAssets("t:NumberVariable");
		        if ( guids.Length > 0 )
		        {
			        for ( int i=0; i<guids.Length; i++ )
			        {
				        NumberVariable variable = (NumberVariable)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(UnityEngine.Object));
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