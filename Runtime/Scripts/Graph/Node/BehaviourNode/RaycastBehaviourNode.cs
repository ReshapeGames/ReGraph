using System.Collections;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class RaycastBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            RaycastEnableFromCameraToWorld = 10,
            RaycastEnableFromMouseToWorld = 11,
            RaycastEnableFromMouseToUi = 51,
            RaycastDisable = 1000
        }

        [SerializeField]
        [LabelText("Execution")]
        [OnValueChanged("MarkDirty")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideIf("HideGameObject")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(gameObject)")]
        [InlineButton("@gameObject.SetObjectValue(AssignGameObject())", "♺", ShowIf = "@gameObject.IsObjectValueType()")]
        private SceneObjectProperty gameObject = new SceneObjectProperty(SceneObject.ObjectType.GameObject);

        [SerializeField]
        [ValueDropdown("DrawActionNameListDropdown", ExpandAllMenuItems = true)]
        [OnValueChanged("MarkDirty")]
        [HideIf("HideActionName")]
        private ActionNameChoice actionName;

        [SerializeField]
        [HideIf("HideCamera")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(camera)")]
        [InlineButton("@camera.SetObjectValue(AssignCamera())", "♺", ShowIf = "@camera.IsObjectValueType()")]
        private SceneObjectProperty camera = new SceneObjectProperty(SceneObject.ObjectType.Camera);

        [SerializeField]
        [LabelText("@ParamFront1Name()")]
        [HideIf("HideParamFloat1")]
        [InlineProperty]
        [OnInspectorGUI("@MarkPropertyDirty(paramFloat1)")]
        private FloatProperty paramFloat1 = new FloatProperty(1000);

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None || gameObject.IsNull || actionName == null)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Raycast Behaviour node in " + context.gameObject.name);
            }
            else
            {
                GameObject go = gameObject;
                Camera cam = (Camera) camera;
                if (executionType is ExecutionType.RaycastEnableFromCameraToWorld or ExecutionType.RaycastEnableFromMouseToWorld)
                {
                    if (camera == null || paramFloat1 <= 0)
                    {
                        ReDebug.LogWarning("Graph Warning", "Found an empty Raycast Behaviour node in " + context.gameObject.name);
                    }
                    else
                    {
                        RayCastingController controller = go.GetComponent<RayCastingController>();
                        if (controller == null)
                            controller = go.AddComponent<RayCastingController>();
                        if (executionType == ExecutionType.RaycastEnableFromCameraToWorld)
                            controller.AddRay(RayCastingController.CastType.CastFromCameraToWorld, actionName, context.runner, cam, paramFloat1);
                        else if (executionType == ExecutionType.RaycastEnableFromMouseToWorld)
                            controller.AddRay(RayCastingController.CastType.CastFromMouseToWorld, actionName, context.runner, cam, paramFloat1);
                    }
                }
                else if (executionType == ExecutionType.RaycastEnableFromMouseToUi)
                {
                    
                    RayCastingController controller = go.GetComponent<RayCastingController>();
                    if (controller == null)
                        controller = go.AddComponent<RayCastingController>();
                    controller.AddRay(RayCastingController.CastType.CastFromMouseToUi, actionName, context.runner);
                }
                else if (executionType == ExecutionType.RaycastDisable)
                {
                    RayCastingController controller = go.GetComponent<RayCastingController>();
                    if (controller != null)
                        controller.RemoveRay(actionName);
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private bool HideGameObject ()
        {
            if (executionType != ExecutionType.None)
                return false;
            return true;
        }

        private bool HideCamera ()
        {
            if (executionType is ExecutionType.RaycastEnableFromCameraToWorld or ExecutionType.RaycastEnableFromMouseToWorld)
                return false;
            return true;
        }

        private bool HideActionName ()
        {
            if (executionType != ExecutionType.None)
                return false;
            return true;
        }

        private bool HideParamFloat1 ()
        {
            if (executionType is ExecutionType.RaycastEnableFromCameraToWorld or ExecutionType.RaycastEnableFromMouseToWorld)
                return false;
            return true;
        }

        private string ParamFront1Name ()
        {
            if (executionType is ExecutionType.RaycastEnableFromCameraToWorld or ExecutionType.RaycastEnableFromMouseToWorld)
                return "Ray Length";
            return string.Empty;
        }

        private static IEnumerable DrawActionNameListDropdown ()
        {
            return ActionNameChoice.GetActionNameListDropdown();
        }

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Enable World Cast From Camera", ExecutionType.RaycastEnableFromCameraToWorld},
            {"Enable World Cast From Mouse", ExecutionType.RaycastEnableFromMouseToWorld},
            {"Enable UI Cast From Mouse", ExecutionType.RaycastEnableFromMouseToUi},
            {"Disable Cast", ExecutionType.RaycastDisable},
        };

        public static string displayName = "Raycast Behaviour Node";
        public static string nodeName = "Raycast";

        public override string GetNodeInspectorTitle ()
        {
            return displayName;
        }

        public override string GetNodeViewTitle ()
        {
            return nodeName;
        }

        public override string GetNodeViewDescription ()
        {
            if (!gameObject.IsNull && actionName != null)
            {
                if (executionType == ExecutionType.RaycastEnableFromCameraToWorld)
                    if (!camera.IsNull && paramFloat1 > 0)
                        return "World Cast From Camera on " + gameObject.name + " at "+actionName+" action";
                if (executionType == ExecutionType.RaycastEnableFromMouseToWorld)
                    if (!camera.IsNull && paramFloat1 > 0)
                        return "World Cast From Mouse on " + gameObject.name + " at "+actionName+" action";
                if (executionType == ExecutionType.RaycastEnableFromMouseToUi)
                    return "UI Cast From Mouse on " + gameObject.name + " at "+actionName+" action";
                if (executionType == ExecutionType.RaycastDisable)
                    return "Disable Cast on " + gameObject.name + " at "+actionName+" action";
            }

            return string.Empty;
        }
#endif
    }
}