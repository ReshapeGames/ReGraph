using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Reshape.ReGraph;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reshape.ReFramework
{
    [DisallowMultipleComponent]
    public class RayCastingController : BaseBehaviour
    {
        public const int INSTANCE = 5941;

        [Serializable]
        public class RayReceiver
        {
            public GraphRunner runner;
            public bool lastUpdateHit;

            public RayReceiver (GraphRunner runner, bool lastUpdateHit)
            {
                this.runner = runner;
                this.lastUpdateHit = lastUpdateHit;
            }

            public void SetLastUpdateHit (bool value)
            {
                lastUpdateHit = value;
            }

            public void HandleCancelRay (string ray)
            {
                runner.TriggerRay(TriggerNode.Type.RayLeave, ray);
            }
        }

        [Serializable]
        public class RayInfo
        {
            public GraphRunner activator;
            public CastType type;
            public string rayName;
            public Camera cameraView;
            public float rayDistance;
            public List<RayReceiver> receivers;

            public RayInfo (CastType cast, string actionName, GraphRunner runner, Camera cam, float distance)
            {
                type = cast;
                rayName = actionName;
                activator = runner;
                cameraView = cam;
                rayDistance = distance;
                receivers = new List<RayReceiver>();
            }

            public RayInfo (CastType cast, string actionName, GraphRunner runner)
            {
                type = cast;
                rayName = actionName;
                activator = runner;
                cameraView = null;
                rayDistance = 0;
                receivers = new List<RayReceiver>();
            }

            public void SetReceiverLastUpdateHit (bool value)
            {
                for (int i = 0; i < receivers.Count; i++)
                    receivers[i].SetLastUpdateHit(value);
            }

            public void HandleReceiverCancelRay ()
            {
                for (int i = 0; i < receivers.Count; i++)
                {
                    if (!receivers[i].lastUpdateHit)
                    {
                        receivers[i].HandleCancelRay(rayName);
                        receivers.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public enum CastType
        {
            None,
            CastFromCameraToWorld = 10,
            CastFromMouseToWorld = 11,
            CastFromMouseToUi = 51,
        }

        private GraphRunner caller;

        [SerializeField, ReadOnly]
        private List<RayInfo> rays;

        public void AddRay (CastType cast, string actionName, GraphRunner runner, Camera cam, float distance)
        {
            if (cast is CastType.CastFromCameraToWorld or CastType.CastFromMouseToWorld)
                if (string.IsNullOrEmpty(actionName) || cam == null || distance <= 0)
                    return;
            if (rays == null)
                rays = new List<RayInfo>();
            rays.Add(new RayInfo(cast, actionName, runner, cam, distance));
        }

        public void AddRay (CastType cast, string actionName, GraphRunner runner)
        {
            if (cast == CastType.CastFromMouseToUi && string.IsNullOrEmpty(actionName))
                return;
            if (rays == null)
                rays = new List<RayInfo>();
            rays.Add(new RayInfo(cast, actionName, runner));
        }

        [SpecialName]
        public void RemoveRay (string actionName)
        {
            if (rays != null)
            {
                for (int i = 0; i < rays.Count; i++)
                {
                    RayInfo ray = rays[i];
                    if (ray.rayName == actionName)
                    {
                        ray.SetReceiverLastUpdateHit(false);
                        ray.HandleReceiverCancelRay();
                        rays.RemoveAt(i);
                        i--;
                    }
                }

                if (rays.Count == 0)
                    Terminate();
            }
        }

        [SpecialName]
        public void Terminate ()
        {
            Destroy(this);
        }

        [SpecialName]
        public void AddReceiver (GraphRunner receiver, string rayName)
        {
            for (int i = 0; i < rays.Count; i++)
            {
                RayInfo ray = rays[i];
                if (ray.rayName == rayName)
                {
                    bool found = false;
                    for (int j = 0; j < ray.receivers.Count; j++)
                    {
                        if (ray.receivers[j].runner == receiver)
                        {
                            RayReceiver temp = ray.receivers[j];
                            temp.lastUpdateHit = true;
                            ray.receivers[j] = temp;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        ray.receivers.Add(new RayReceiver(receiver, true));
                    }
                    break;
                }
            }
        }

        protected override void Start ()
        {
            if (rays == null || rays.Count == 0)
            {
                Terminate();
                return;
            }

            caller = GetComponent<GraphRunner>();
            Remember(INSTANCE, this);
        }

        protected void Update ()
        {
            for (int i = 0; i < rays.Count; i++)
            {
                rays[i].SetReceiverLastUpdateHit(false);
                RayInfo ray = rays[i];
                if (ray.type == CastType.CastFromCameraToWorld)
                {
                    RaycastHit? hit = RayCastFromScreenPointToWorld(ray.rayName, ray.cameraView, ReInput.mousePositionAtCenterScreen, ray.rayDistance);
                    if (hit != null)
                    {
                        RaycastHit theHit = (RaycastHit) hit;
                        if (theHit.distance > ray.rayDistance)
                        {
                            TriggerRayHit(ray, theHit);
                            TriggerRayMissed(ray);
                        }
                        else
                        {
                            TriggerRayReceived(ray, theHit);
                        }
                    }
                    else
                    {
                        TriggerRayMissed(ray);
                    }
                }
                else if (ray.type == CastType.CastFromMouseToWorld)
                {
                    RaycastHit? hit = RayCastFromScreenPointToWorld(ray.rayName, ray.cameraView, ReInput.mousePosition, ray.rayDistance);
                    if (hit != null)
                    {
                        RaycastHit theHit = (RaycastHit) hit;
                        if (theHit.distance > ray.rayDistance)
                        {
                            TriggerRayHit(ray, theHit);
                            TriggerRayMissed(ray);
                        }
                        else
                        {
                            TriggerRayReceived(ray, theHit);
                        }
                    }
                    else
                    {
                        TriggerRayMissed(ray);
                    }
                }
                else if (ray.type == CastType.CastFromMouseToUi)
                {
                    bool somethingHit = false;
                    bool somethingReceived = false;
                    GameObject hitGo = null;
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    {
                        PointerEventData pointerData = new PointerEventData(EventSystem.current) {pointerId = -1};
                        pointerData.position = ReInput.mousePosition;
                        List<RaycastResult> results = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(pointerData, results);
                        for (int j = 0; j < results.Count; j++)
                        {
                            if (j == 0)
                            {
                                if (results[j].gameObject.GetComponentInParent<Canvas>().renderMode != RenderMode.WorldSpace)
                                {
                                    hitGo = results[j].gameObject;
                                    somethingHit = true;
                                }
                            }

                            if (RayCastFromScreenPointToUi(ray.rayName, results[j].gameObject))
                            {
                                hitGo = results[j].gameObject;
                                somethingReceived = true;
                                break;
                            }
                        }
                    }

                    if (somethingReceived)
                    {
                        TriggerRayReceived(ray, hitGo);
                    }
                    else
                    {
                        if (somethingHit)
                        {
                            TriggerRayHit(ray, hitGo);
                        }

                        TriggerRayMissed(ray);
                    }
                }

                if (i < rays.Count)
                    rays[i].HandleReceiverCancelRay();
            }
        }

        public void TriggerRayReceived (RayInfo ray, RaycastHit hit)
        {
            if (ray.activator != null)
            {
                ray.activator.TriggerRay(TriggerNode.Type.RayAccepted, ray.rayName);
            }

            if (caller != null && caller != ray.activator)
            {
                caller.TriggerRay(TriggerNode.Type.RayAccepted, ray.rayName);
            }
        }

        public void TriggerRayReceived (RayInfo ray, GameObject hit)
        {
            if (ray.activator != null)
            {
                ray.activator.TriggerRay(TriggerNode.Type.RayAccepted, ray.rayName);
            }

            if (caller != null && caller != ray.activator)
            {
                caller.TriggerRay(TriggerNode.Type.RayAccepted, ray.rayName);
            }
        }


        public void TriggerRayMissed (RayInfo ray)
        {
            if (ray.activator != null)
            {
                ray.activator.TriggerRay(TriggerNode.Type.RayMissed, ray.rayName);
            }

            if (caller != null && caller != ray.activator)
            {
                caller.TriggerRay(TriggerNode.Type.RayMissed, ray.rayName);
            }
        }

        public void TriggerRayHit (RayInfo ray, RaycastHit hit)
        {
            if (ray.activator != null)
            {
                ray.activator.TriggerRay(TriggerNode.Type.RayHit, ray.rayName);
            }

            if (caller != null && caller != ray.activator)
            {
                caller.TriggerRay(TriggerNode.Type.RayHit, ray.rayName);
            }
        }

        public void TriggerRayHit (RayInfo ray, GameObject hit)
        {
            if (ray.activator != null)
            {
                ray.activator.TriggerRay(TriggerNode.Type.RayHit, ray.rayName);
            }

            if (caller != null && caller != ray.activator)
            {
                caller.TriggerRay(TriggerNode.Type.RayHit, ray.rayName);
            }
        }
    }
}