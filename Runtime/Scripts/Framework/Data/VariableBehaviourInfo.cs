using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Reshape.ReFramework
{
    [System.Serializable]
    public struct VariableBehaviourInfo : IClone<VariableBehaviourInfo>
    {
        public enum Type
        {
            None = 0,
            SetValue = 1,
            AddValue = 2,
            MinusValue = 3,
            MultiplyValue = 4,
            DivideValue = 5,
            RandomValue = 6,
            CheckCondition = 20
        }

        public enum Condition
        {
            None = 0,
            Equal = 1,
            NotEqual = 2,
            LessThan = 31,
            MoreThan = 32,
            LessThanAndEqual = 33,
            MoreThanAndEqual = 34,
            Contains = 101
        }

        [OnValueChanged("OnChangeVariable")]
        [InlineButton("@VariableScriptableObject.OpenCreateVariableMenu(variable)", "✚")]
        public VariableScriptableObject variable;

        [ValueDropdown("TypeChoice")]
        [OnInspectorGUI("ShowTip")]
        [OnValueChanged("OnChangeType")]
        [ShowIf("ShowParamType")]
        public Type type;

        [ValueDropdown("ConditionChoice")]
        [ShowIf("ShowParamCondition")]
        public Condition condition;

        [LabelText("Value")]
        [ShowIf("ShowParamNumber")]
        [InlineProperty]
        public FloatProperty number;

        [LabelText("Value")]
        [ShowIf("ShowParamMessage")]
        [InlineProperty]
        public StringProperty message;
        
        public VariableBehaviourInfo ShallowCopy()
        {
            var info = new VariableBehaviourInfo();
            info.variable = variable;
            info.type = type;
            info.condition = condition;
            info.number = number.ShallowCopy();
            info.message = message.ShallowCopy();
            return info;
        }
        
        public bool Activate ()
        {
            if (variable == null)
                return false;
            if (variable is NumberVariable)
            {
                var fvar = (NumberVariable) variable;
                if (type == Type.SetValue)
                {
                    fvar.SetValue(number);
                }
                else if (type == Type.AddValue)
                {
                    fvar.AddValue(number);
                }
                else if (type == Type.MinusValue)
                {
                    fvar.MinusValue(number);
                }
                else if (type == Type.DivideValue)
                {
                    fvar.DivideValue(number);
                }
                else if (type == Type.MultiplyValue)
                {
                    fvar.MultiplyValue(number);
                }
                else if (type == Type.RandomValue)
                {
                    fvar.RandomValue();
                }
                else if (type == Type.CheckCondition)
                {
                    if (condition == Condition.Equal || condition == Condition.LessThanAndEqual || condition == Condition.MoreThanAndEqual)
                    {
                        if (fvar.IsEqual(number))
                            return true;
                    }

                    if (condition == Condition.NotEqual)
                    {
                        if (!fvar.IsEqual(number))
                            return true;
                    }

                    if (condition == Condition.LessThan || condition == Condition.LessThanAndEqual)
                    {
                        if (fvar < number)
                            return true;
                    }

                    if (condition == Condition.MoreThan || condition == Condition.MoreThanAndEqual)
                    {
                        if (fvar > number)
                            return true;
                    }
                }
            }
            else if (variable is WordVariable)
            {
                var svar = (WordVariable) variable;
                if (type == Type.SetValue)
                {
                    svar.SetValue(message);
                }
                else if (type == Type.CheckCondition)
                {
                    if (condition == Condition.Equal)
                    {
                        if (svar.IsEqual(message))
                            return true;
                    }
                    else if (condition == Condition.Contains)
                    {
                        if (svar.Contains(message))
                            return true;
                    }
                }
            }

            return false;
        }
        
        [HideInInspector]
        public bool typeChanged;

#if UNITY_EDITOR
        private void OnChangeType ()
        {
            typeChanged = true;
        }

        private void OnChangeVariable ()
        {
            type = Type.None;
            condition = Condition.None;
            number.Reset();
            message.Reset();
            typeChanged = true;
        }

        private bool ShowParamType ()
        {
            if (variable != null)
                return true;
            return false;
        }

        private bool ShowParamNumber ()
        {
            if (variable != null && variable is NumberVariable)
                if (type != Type.RandomValue)
                    return true;
            return false;
        }

        private bool ShowParamMessage ()
        {
            if (variable != null && variable is WordVariable)
                return true;
            return false;
        }

        private bool ShowParamCondition ()
        {
            if (variable != null && type == Type.CheckCondition)
                return true;
            return false;
        }

        public ValueDropdownList<Condition> ConditionChoice ()
        {
            var listDropdown = new ValueDropdownList<Condition>();
            if (variable is NumberVariable)
            {
                listDropdown.Add("Equal", Condition.Equal);
                listDropdown.Add("Not Equal", Condition.NotEqual);
                listDropdown.Add("Less Than", Condition.LessThan);
                listDropdown.Add("More Than", Condition.MoreThan);
                listDropdown.Add("Less Than And Equal", Condition.LessThanAndEqual);
                listDropdown.Add("More Than And Equal", Condition.MoreThanAndEqual);
            }
            else if (variable is WordVariable)
            {
                listDropdown.Add("Equal", Condition.Equal);
                listDropdown.Add("Contains", Condition.Contains);
            }

            return listDropdown;
        }

        public ValueDropdownList<Type> TypeChoice ()
        {
            var listDropdown = new ValueDropdownList<Type>();
            if (variable is NumberVariable)
            {
                listDropdown.Add("Set Value", Type.SetValue);
                listDropdown.Add("Add Value", Type.AddValue);
                listDropdown.Add("Minus Value", Type.MinusValue);
                listDropdown.Add("Multiply Value", Type.MultiplyValue);
                listDropdown.Add("Divide Value", Type.DivideValue);
                listDropdown.Add("Random Value", Type.RandomValue);
                listDropdown.Add("Check Condition", Type.CheckCondition);
            }
            else if (variable is WordVariable)
            {
                listDropdown.Add("Set Value", Type.SetValue);
                listDropdown.Add("Check Condition", Type.CheckCondition);
            }

            return listDropdown;
        }

        private void ShowTip ()
        {
            if (type == Type.RandomValue)
            {
                EditorGUILayout.HelpBox("Random between 1 to 100", MessageType.Info);
            }
        }
#endif
    }
}