using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
	[Serializable]
	public class ActionNameChoice : ScriptableObject
	{
		[ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
		public string actionName;
		
		public static implicit operator string(ActionNameChoice choice)
		{
			return choice.actionName;
		}

		public override string ToString()
		{
			return actionName;
		}

#if UNITY_EDITOR
	    private static IEnumerable DrawActionNameListDropdown()
		{
			return GetActionNameListDropdown();
		}

	    public static ValueDropdownList<ActionNameChoice> GetActionNameListDropdown ()
	    {
		    ValueDropdownList<ActionNameChoice> actionNameListDropdown = new ValueDropdownList<ActionNameChoice>();
                
		    string[] assets = AssetDatabase.FindAssets("t:ActionNameChoice");
		    if ( assets.Length > 0 )
		    {
			    for ( int i=0; i<assets.Length; i++ )
			    {
				    string path = AssetDatabase.GUIDToAssetPath(assets[i]);
				    ActionNameChoice actionNameChoice = AssetDatabase.LoadAssetAtPath<ActionNameChoice>(path);
				    actionNameListDropdown.Add(actionNameChoice.name, actionNameChoice);
			    }
		    }

		    return actionNameListDropdown;
	    }
#endif
	}
}
