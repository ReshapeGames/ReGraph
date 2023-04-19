using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using Reshape.ReFramework;

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class AudioBehaviourNode : BehaviourNode
    {
        public enum ExecutionType
        {
            None,
            PlayClip = 100,
            Stop = 200,
            Pause = 300
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [HideIf("@executionType == ExecutionType.None")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(audioSource)")]
        [InlineButton("@audioSource.SetObjectValue(AssignComponent<AudioSource>())", "♺", ShowIf = "@audioSource.IsObjectValueType()")]
        private SceneObjectProperty audioSource = new SceneObjectProperty(SceneObject.ObjectType.AudioSource);
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@executionType != ExecutionType.PlayClip")]
        private AudioClip clip;
        
        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [HideIf("@executionType != ExecutionType.PlayClip")]
        private bool loop;
        
        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (audioSource.IsNull || executionType is ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Audio Behaviour node in " + context.gameObject.name);
            }
            else
            {
                var audio = (AudioSource) audioSource;
                if (executionType == ExecutionType.Stop)
                {
                    audio.Stop();
                }
                else if (executionType == ExecutionType.Pause)
                {
                    audio.Pause();
                }
                else if (executionType == ExecutionType.PlayClip)
                {
                    if (clip == null)
                    {
                        ReDebug.LogWarning("Graph Warning", "Found an empty Audio Behaviour node in " + context.gameObject.name);
                    }
                    else
                    {
                        if (loop)
                        {
                            audio.loop = true;
                            if (audio.clip != clip)
                                audio.clip = clip;
                            audio.Play();
                        }
                        else
                        {
                            audio.PlayOneShot(clip);
                        }

                        audio.PlayOneShot(clip);
                    }
                }
            }

            base.OnStart(execution, updateId);
        }

#if UNITY_EDITOR
        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Play \\ Resume", ExecutionType.PlayClip},
            {"Stop", ExecutionType.Stop},
            {"Pause", ExecutionType.Pause}
        };

        public static string displayName = "Audio Behaviour Node";
        public static string nodeName = "Audio";

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
            if (!audioSource.IsNull && executionType is ExecutionType.None == false)
            {
                if (executionType is ExecutionType.Stop)
                    return "Stop " + audioSource.name;
                if (executionType is ExecutionType.Pause)
                    return "Pause " + audioSource.name;
                if (executionType is ExecutionType.PlayClip && clip != null)
                    return "Play " + clip.name + " on " + audioSource.name;
            }

            return string.Empty;
        }
#endif
    }
}