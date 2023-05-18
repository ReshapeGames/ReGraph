using Reshape.ReFramework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Reshape.ReGraph
{
    public abstract class Node
    {
        public enum State
        {
            Running = 1,
            Failure = 50,
            Stop = 60,
            Success = 100
        }

        [HideIf("@GetType().ToString().Contains(\"RootNode\")")]
        [OnValueChanged("OnEnableChange")]
        [InlineButton("ShowAdvanceSettings", "≡")]
        public bool enabled = true;
        
        [ShowIf("showAdvanceSettings"), BoxGroup("Show Debug Info")]
        [ReadOnly]
        [PropertyOrder(-50)]
        public string guid = System.Guid.NewGuid().ToString();
        
        [HideInInspector]
        public Vector2 position;
        [HideInInspector]
        public GraphRunner runner;
        [HideInInspector]
        public bool dirty;
        [HideInInspector]
        public bool forceRepaint;

#if UNITY_EDITOR
        protected void MarkDirty ()
        {
            dirty = true;
        }
        
        protected void MarkPropertyDirty (FloatProperty p)
        {
            if (p.dirty)
            {
                p.dirty = false;
                MarkDirty();
            }
        }
        
        protected void MarkPropertyDirty (StringProperty p)
        {
            if (p.dirty)
            {
                p.dirty = false;
                MarkDirty();
            }
        }
        
        protected void MarkPropertyDirty (SceneObjectProperty p)
        {
            if (p.dirty)
            {
                p.dirty = false;
                MarkDirty();
            }
        }
        
        protected void MarkRepaint ()
        {
            forceRepaint = true;
        }
        
        protected T AssignComponent<T>()
        {
            MarkDirty();
            return PropertyLinking.GetComponent<T>(runner);
        }
        
        protected GameObject AssignGameObject()
        {
            MarkDirty();
            return PropertyLinking.GetGameObject(runner);
        }
        
        protected Camera AssignCamera()
        {
            MarkDirty();
            return PropertyLinking.GetCamera();
        }
        
        public void OnEnableChange ()
        {
            MarkDirty();
            onEnableChange?.Invoke();
        }
        
        private void ShowAdvanceSettings()
        {
            showAdvanceSettings = !showAdvanceSettings;
        }
#endif
        public delegate void NodeDelegate ();
        public event NodeDelegate onEnableChange;
        protected bool showAdvanceSettings;

        public State Update (GraphExecution execution, int updateId)
        {
            if (!enabled)
                return OnDisabled(execution, updateId);

            bool started = execution.variables.GetStarted(guid, false);
            if (!started)
            {
                OnStart(execution, updateId);
                execution.variables.SetStarted(guid, true);
            }

            State state = OnUpdate(execution, updateId);

            if (state != State.Running)
            {
                OnStop(execution, updateId);
                execution.variables.SetStarted(guid, false);
            }

            return state;
        }
        
        public void Init ()
        {
            OnInit();
        }
        
        public void Reset ()
        {
            OnReset();
        }

        public void Pause (GraphExecution execution)
        {
            OnPause(execution);
        }
        
        public void Unpause (GraphExecution execution)
        {
            OnUnpause(execution);
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos () { }
        public virtual void OnUpdateGraphId (int previousId, int newId) { }
        public virtual void OnClone (GraphNode selectedNode) { }
        public virtual void OnDelete () { }
#endif

        protected abstract void OnInit ();
        protected abstract void OnReset ();
        protected abstract void OnPause (GraphExecution execution);
        protected abstract void OnUnpause (GraphExecution execution);
        protected abstract void OnStart (GraphExecution execution, int updateId);
        protected abstract void OnStop (GraphExecution execution, int updateId);
        protected abstract State OnDisabled (GraphExecution execution, int updateId);
        protected abstract State OnUpdate (GraphExecution execution, int updateId);
    }
}