using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reshape.ReFramework
{
    [HideMonoScript]
    public class MouseRotationController : BaseBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        private Vector2 rotateRate;
        private InputActionAsset actionAsset;
        private string actionName;
        private Camera facingCamera;
        private InputAction action;
        private Vector3 rotation;

        public void Initial (Vector2 rate, InputActionAsset input, string inputName, Camera cam)
        {
            rotateRate = rate;
            actionAsset = input;
            actionName = inputName;
            facingCamera = cam;
        }

        public void Terminate ()
        {
            Destroy(this);
        }

        protected override void Start ()
        {
            if (actionAsset != null)
            {
                action = actionAsset.FindAction(actionName);
                if (action != null)
                {
                    rotation = transform.localEulerAngles;
                    return;
                }
            }

            Terminate();
        }

        protected void Update ()
        {
            var movement = action.ReadValue<Vector2>();
            movement.x = -movement.x * (Time.deltaTime * rotateRate.x);
            movement.y = movement.y * (Time.deltaTime * rotateRate.y);
            if (facingCamera != null)
            {
                transform.Rotate(facingCamera.transform.right, movement.y, Space.World);
                transform.Rotate(facingCamera.transform.up, movement.x, Space.World);
            }
            else
            {
                rotation += new Vector3(movement.y, movement.x, 0);
                transform.localEulerAngles = rotation;
            }
        }
#endif
    }
}