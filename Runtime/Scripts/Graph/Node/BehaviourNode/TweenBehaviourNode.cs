using System;
using System.Collections.Generic;
using Reshape.ReFramework;
using Reshape.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.IO;
using Reshape.Unity.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace Reshape.ReGraph
{
    [System.Serializable]
    public class TweenBehaviourNode : BehaviourNode
    {
        public const string VAR_PROCEED = "_proceed";

        public enum ExecutionType
        {
            None,
            PlayTween = 11,
            StopTween = 21,
            StopAnyTween = 22,
        }

        public enum TweenType
        {
            None,
            Transform = 10,
            RectTransform = 11,
            Rigidbody = 20,
            Rigidbody2D = 21,
            Light = 30,
            Camera = 40,
            AudioMixer = 50,
            AudioSource = 60,
            ParticleSystem = 70,
            Material = 80,
            SpriteRenderer = 81,
            LineRenderer = 82,
            TrailRenderer = 83,
            Graphic = 91,
            Image = 101,
            Text = 102,
            Slider = 103,
            CanvasGroup = 111,
            LayoutElement = 112,
            ScrollRect = 113,
            TextMeshProGUI = 121,
            TextMeshPro = 122
        }

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [LabelText("Execution")]
        [ValueDropdown("TypeChoice")]
        private ExecutionType executionType;

        [SerializeField]
        [OnValueChanged("OnChangeTweenType")]
        [LabelText("Tween")]
        [ValueDropdown("TweenChoice")]
        [ShowIf("@executionType == ExecutionType.PlayTween || executionType == ExecutionType.StopAnyTween")]
        [InlineButton("ClearTweenData", "↺", ShowIf = "@tweenData != null")]
        private TweenType tweenType;

        [SerializeField]
        [ShowIf("ShowTweenObject")]
        [HideLabel, InlineProperty, OnInspectorGUI("@MarkPropertyDirty(tweenObject)")]
        [InlineButton("SetDefaultTweenObject", "♺", ShowIf = "ShowSetDefaultTweenObjectButton")]
        private SceneObjectProperty tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Transform);

        [SerializeField]
        [OnInspectorGUI("@MarkPropertyDirty(tweenName)")]
        [InlineProperty]
        [ShowIf("ShowTweenName")]
        [LabelText("Name")]
        private StringProperty tweenName;

        [SerializeField]
        [OnValueChanged("MarkDirty")]
        [InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Hidden, Expanded = true)]
        [ShowIf("ShowTweenData")]
        [InlineButton("CreateTweenData", "✚", ShowIf = "@tweenData == null")]
        [DisableIf("@runner.graph.haveValidGraphId == false")]
        [InfoBox("Please save your scene so these property will be editable", InfoMessageType.Warning, "@runner.graph.haveValidGraphId == false", GUIAlwaysEnabled = true)]
        private TweenData tweenData;

        private string proceedKey;

        private void InitVariables ()
        {
            if (string.IsNullOrEmpty(proceedKey))
                proceedKey = guid + VAR_PROCEED;
        }

        protected override void OnStart (GraphExecution execution, int updateId)
        {
            if (executionType == ExecutionType.None)
            {
                ReDebug.LogWarning("Graph Warning", "Found an empty Tween Behaviour node in " + context.gameObject.name);
            }
            else if (executionType == ExecutionType.PlayTween)
            {
                if (tweenObject.IsNull || tweenData == null)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Tween Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    Tween tween = null;
                    if (tweenType == TweenType.Transform)
                    {
                        tween = tweenData.GetTween((Transform) tweenObject);
                        tween.SetTarget((Transform) tweenObject);
                    }
                    else if (tweenType == TweenType.RectTransform)
                    {
                        tween = tweenData.GetTween((RectTransform) tweenObject);
                        tween.SetTarget((RectTransform) tweenObject);
                    }
                    else if (tweenType == TweenType.Rigidbody)
                    {
                        tween = tweenData.GetTween((Rigidbody) tweenObject);
                        tween.SetTarget((Rigidbody) tweenObject);
                    }
                    else if (tweenType == TweenType.Rigidbody2D)
                    {
                        tween = tweenData.GetTween((Rigidbody2D) tweenObject);
                        tween.SetTarget((Rigidbody2D) tweenObject);
                    }
                    else if (tweenType == TweenType.Light)
                    {
                        tween = tweenData.GetTween((Light) tweenObject);
                        tween.SetTarget((Light) tweenObject);
                    }
                    else if (tweenType == TweenType.Camera)
                    {
                        tween = tweenData.GetTween((Camera) tweenObject);
                        tween.SetTarget((Camera) tweenObject);
                    }
                    else if (tweenType == TweenType.AudioSource)
                    {
                        tween = tweenData.GetTween((AudioSource) tweenObject);
                        tween.SetTarget((AudioSource) tweenObject);
                    }
                    else if (tweenType == TweenType.AudioMixer)
                    {
                        tween = tweenData.GetTween((AudioMixer) tweenObject);
                        tween.SetTarget((AudioMixer) tweenObject);
                    }
                    else if (tweenType == TweenType.ParticleSystem)
                    {
                        tween = tweenData.GetTween((ParticleSystem) tweenObject);
                        tween.SetTarget((ParticleSystem) tweenObject);
                    }
                    else if (tweenType == TweenType.Material)
                    {
                        tween = tweenData.GetTween((MeshRenderer) tweenObject);
                        tween.SetTarget((MeshRenderer) tweenObject);
                    }
                    else if (tweenType == TweenType.SpriteRenderer)
                    {
                        tween = tweenData.GetTween((SpriteRenderer) tweenObject);
                        tween.SetTarget((SpriteRenderer) tweenObject);
                    }
                    else if (tweenType == TweenType.LineRenderer)
                    {
                        tween = tweenData.GetTween((LineRenderer) tweenObject);
                        tween.SetTarget((LineRenderer) tweenObject);
                    }
                    else if (tweenType == TweenType.TrailRenderer)
                    {
                        tween = tweenData.GetTween((TrailRenderer) tweenObject);
                        tween.SetTarget((TrailRenderer) tweenObject);
                    }
                    else if (tweenType == TweenType.Graphic)
                    {
                        tween = tweenData.GetTween((Graphic) tweenObject);
                        tween.SetTarget((Graphic) tweenObject);
                    }
                    else if (tweenType == TweenType.Image)
                    {
                        tween = tweenData.GetTween((Image) tweenObject);
                        tween.SetTarget((Image) tweenObject);
                    }
                    else if (tweenType == TweenType.Text)
                    {
                        tween = tweenData.GetTween((Text) tweenObject);
                        tween.SetTarget((Text) tweenObject);
                    }
                    else if (tweenType == TweenType.Slider)
                    {
                        tween = tweenData.GetTween((Slider) tweenObject);
                        tween.SetTarget((Slider) tweenObject);
                    }
                    else if (tweenType == TweenType.CanvasGroup)
                    {
                        tween = tweenData.GetTween((CanvasGroup) tweenObject);
                        tween.SetTarget((CanvasGroup) tweenObject);
                    }
                    else if (tweenType == TweenType.LayoutElement)
                    {
                        tween = tweenData.GetTween((LayoutElement) tweenObject);
                        tween.SetTarget((LayoutElement) tweenObject);
                    }
                    else if (tweenType == TweenType.ScrollRect)
                    {
                        tween = tweenData.GetTween((ScrollRect) tweenObject);
                        tween.SetTarget((ScrollRect) tweenObject);
                    }
                    else if (tweenType == TweenType.TextMeshProGUI)
                    {
                        tween = tweenData.GetTween((TextMeshProUGUI) tweenObject);
                        tween.SetTarget((TextMeshProUGUI) tweenObject);
                    }
                    else if (tweenType == TweenType.TextMeshPro)
                    {
                        tween = tweenData.GetTween((TextMeshPro) tweenObject);
                        tween.SetTarget((TextMeshPro) tweenObject);
                    }

                    if (tween != null)
                    {
                        tween.SetUpdate(UpdateType.Normal, false);
                        tween.SetLoops(0, LoopType.Restart);
                        tween.SetId(GetTweeName(execution, updateId));
                        tween.timeScale = 1;
                        tween.onComplete += OnTweenComplete;
                        tween.Play();
                        InitVariables();
                        execution.variables.SetInt(proceedKey, updateId);
                    }
                }
            }
            else if (executionType == ExecutionType.StopTween)
            {
                if (tweenObject.IsNull || string.IsNullOrEmpty(tweenName))
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Tween Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    /*if (tweenType == TweenType.AudioMixer)
                    {
                        DOTween.Kill((AudioMixer) tweenObject, tweenName.ToString());
                    }
                    else if (tweenType == TweenType.Material)
                    {
                        var renderer = (MeshRenderer) tweenObject;
                        DOTween.Kill(renderer.sharedMaterial, tweenName.ToString());
                        DOTween.Kill(renderer.material, tweenName.ToString());
                    }
                    else if (tweenType == TweenType.ParticleSystem)
                    {
                        var comp = (ParticleSystem) tweenObject;
                        DOTween.Kill(tweenName.ToString());
                    }
                    else
                    {
                        Component comp = null;
                        if (tweenType == TweenType.Transform)
                            comp = (Transform) tweenObject;
                        else if (tweenType == TweenType.RectTransform)
                            comp = (RectTransform) tweenObject;
                        else if (tweenType == TweenType.Rigidbody)
                            comp = (Rigidbody) tweenObject;
                        else if (tweenType == TweenType.Rigidbody2D)
                            comp = (Rigidbody2D) tweenObject;
                        else if (tweenType == TweenType.Light)
                            comp = (Light) tweenObject;
                        else if (tweenType == TweenType.Camera)
                            comp = (Camera) tweenObject;
                        else if (tweenType == TweenType.AudioSource)
                            comp = (AudioSource) tweenObject;
                        else if (tweenType == TweenType.SpriteRenderer)
                            comp = (SpriteRenderer) tweenObject;
                        else if (tweenType == TweenType.LineRenderer)
                            comp = (LineRenderer) tweenObject;
                        else if (tweenType == TweenType.TrailRenderer)
                            comp = (TrailRenderer) tweenObject;
                        else if (tweenType == TweenType.Graphic)
                            comp = (Graphic) tweenObject;
                        else if (tweenType == TweenType.Image)
                            comp = (Image) tweenObject;
                        else if (tweenType == TweenType.Text)
                            comp = (Text) tweenObject;
                        else if (tweenType == TweenType.Slider)
                            comp = (Slider) tweenObject;
                        else if (tweenType == TweenType.CanvasGroup)
                            comp = (CanvasGroup) tweenObject;
                        else if (tweenType == TweenType.LayoutElement)
                            comp = (LayoutElement) tweenObject;
                        else if (tweenType == TweenType.ScrollRect)
                            comp = (ScrollRect) tweenObject;
                        else if (tweenType == TweenType.TextMeshProGUI)
                            comp = (TextMeshProUGUI) tweenObject;
                        else if (tweenType == TweenType.TextMeshPro)
                            comp = (TextMeshPro) tweenObject;
                        DOTween.Kill(comp, tweenName.ToString());
                    }*/
                    DOTween.Kill(tweenName.ToString());
                }
            }
            else if (executionType == ExecutionType.StopAnyTween)
            {
                if (tweenObject.IsNull)
                {
                    ReDebug.LogWarning("Graph Warning", "Found an empty Tween Behaviour node in " + context.gameObject.name);
                }
                else
                {
                    if (tweenType == TweenType.AudioMixer)
                    {
                        DOTween.Kill((AudioMixer) tweenObject);
                    }
                    else if (tweenType == TweenType.Material)
                    {
                        var renderer = (MeshRenderer) tweenObject;
                        DOTween.Kill(renderer);
                    }
                    else
                    {
                        Component comp = null;
                        if (tweenType == TweenType.Transform)
                            comp = (Transform) tweenObject;
                        else if (tweenType == TweenType.RectTransform)
                            comp = (RectTransform) tweenObject;
                        else if (tweenType == TweenType.Rigidbody)
                            comp = (Rigidbody) tweenObject;
                        else if (tweenType == TweenType.Rigidbody2D)
                            comp = (Rigidbody2D) tweenObject;
                        else if (tweenType == TweenType.Light)
                            comp = (Light) tweenObject;
                        else if (tweenType == TweenType.Camera)
                            comp = (Camera) tweenObject;
                        else if (tweenType == TweenType.AudioSource)
                            comp = (AudioSource) tweenObject;
                        else if (tweenType == TweenType.ParticleSystem)
                            comp = (ParticleSystem) tweenObject;
                        else if (tweenType == TweenType.SpriteRenderer)
                            comp = (SpriteRenderer) tweenObject;
                        else if (tweenType == TweenType.LineRenderer)
                            comp = (LineRenderer) tweenObject;
                        else if (tweenType == TweenType.TrailRenderer)
                            comp = (TrailRenderer) tweenObject;
                        else if (tweenType == TweenType.Graphic)
                            comp = (Graphic) tweenObject;
                        else if (tweenType == TweenType.Image)
                            comp = (Image) tweenObject;
                        else if (tweenType == TweenType.Text)
                            comp = (Text) tweenObject;
                        else if (tweenType == TweenType.Slider)
                            comp = (Slider) tweenObject;
                        else if (tweenType == TweenType.CanvasGroup)
                            comp = (CanvasGroup) tweenObject;
                        else if (tweenType == TweenType.LayoutElement)
                            comp = (LayoutElement) tweenObject;
                        else if (tweenType == TweenType.ScrollRect)
                            comp = (ScrollRect) tweenObject;
                        else if (tweenType == TweenType.TextMeshProGUI)
                            comp = (TextMeshProUGUI) tweenObject;
                        else if (tweenType == TweenType.TextMeshPro)
                            comp = (TextMeshPro) tweenObject;
                        DOTween.Kill(comp);
                    }
                }
            }

            base.OnStart(execution, updateId);

            void OnTweenComplete ()
            {
                execution.variables.SetInt(proceedKey, int.MaxValue);
                context.runner.ResumeTrigger(execution.id, updateId);
            }
        }

        protected override State OnUpdate (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.PlayTween)
            {
                InitVariables();
                int key = execution.variables.GetInt(proceedKey);
                if (key == 0 || key == int.MaxValue)
                    return base.OnUpdate(execution, updateId);
                return State.Running;
            }

            return base.OnUpdate(execution, updateId);
        }

        protected override void OnStop (GraphExecution execution, int updateId)
        {
            if (executionType is ExecutionType.PlayTween)
            {
                bool started = execution.variables.GetStarted(guid, false);
                if (started)
                {
                    int key = execution.variables.GetInt(proceedKey);
                    if (key > 0 && key < int.MaxValue)
                    {
                        DOTween.Kill(GetTweeName(execution, key));
                    }
                }
            }

            base.OnStop(execution, updateId);
        }

        protected override void OnPause (GraphExecution execution)
        {
            if (executionType is ExecutionType.PlayTween)
            {
                bool started = execution.variables.GetStarted(guid, false);
                if (started)
                {
                    int key = execution.variables.GetInt(proceedKey);
                    if (key > 0 && key < int.MaxValue)
                        DOTween.Pause(GetTweeName(execution, key));
                }
            }

            base.OnPause(execution);
        }

        protected override void OnUnpause (GraphExecution execution)
        {
            if (executionType is ExecutionType.PlayTween)
            {
                bool started = execution.variables.GetStarted(guid, false);
                if (started)
                {
                    int key = execution.variables.GetInt(proceedKey);
                    if (key > 0 && key < int.MaxValue)
                        DOTween.Play(GetTweeName(execution, key));
                }
            }

            base.OnUnpause(execution);
        }

        public override bool IsRequireUpdate ()
        {
            return enabled;
        }

        private string GetTweeName (GraphExecution execution, int key)
        {
            if (string.IsNullOrEmpty(tweenName))
                return execution.id + ID_SEPERATOR + guid + ID_SEPERATOR + key;
            return tweenName;
        }

#if UNITY_EDITOR
        private bool ShowSetDefaultTweenObjectButton ()
        {
            if (tweenType == TweenType.AudioMixer)
                return false;
            return tweenObject.IsObjectValueType();
        }

        private void ClearTweenData ()
        {
            tweenData = null;
            MarkDirty();
        }

        private void CreateTweenData ()
        {
            if (tweenType == TweenType.Transform)
                tweenData = TweenData.CreateTweenData<TransformTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Rigidbody)
                tweenData = TweenData.CreateTweenData<RigidbodyTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Rigidbody2D)
                tweenData = TweenData.CreateTweenData<Rigidbody2DTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.RectTransform)
                tweenData = TweenData.CreateTweenData<RectTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Light)
                tweenData = TweenData.CreateTweenData<LightTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Camera)
                tweenData = TweenData.CreateTweenData<CameraTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.AudioMixer)
                tweenData = TweenData.CreateTweenData<AudioMixerTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.AudioSource)
                tweenData = TweenData.CreateTweenData<AudioSourceTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.ParticleSystem)
                tweenData = TweenData.CreateTweenData<ParticleSystemTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Material)
                tweenData = TweenData.CreateTweenData<MaterialTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.SpriteRenderer)
                tweenData = TweenData.CreateTweenData<SpriteRendererTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.LineRenderer)
                tweenData = TweenData.CreateTweenData<LineRendererTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.TrailRenderer)
                tweenData = TweenData.CreateTweenData<TrailRendererTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Graphic)
                tweenData = TweenData.CreateTweenData<GraphicTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Image)
                tweenData = TweenData.CreateTweenData<ImageTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Text)
                tweenData = TweenData.CreateTweenData<TextTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.Slider)
                tweenData = TweenData.CreateTweenData<SliderTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.CanvasGroup)
                tweenData = TweenData.CreateTweenData<CanvasGroupTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.LayoutElement)
                tweenData = TweenData.CreateTweenData<LayoutElementTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.ScrollRect)
                tweenData = TweenData.CreateTweenData<ScrollRectTween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.TextMeshProGUI)
                tweenData = TweenData.CreateTweenData<TextMeshProUGUITween>(id, runner.gameObject.scene.path);
            else if (tweenType == TweenType.TextMeshPro)
                tweenData = TweenData.CreateTweenData<TextMeshProTween>(id, runner.gameObject.scene.path);
            MarkDirty();
        }

        private bool ShowTweenData ()
        {
            if (executionType == ExecutionType.PlayTween && tweenType != TweenType.None)
                return true;
            return false;
        }

        private bool ShowTweenName ()
        {
            if (executionType == ExecutionType.PlayTween && tweenType != TweenType.None)
                return true;
            if (executionType == ExecutionType.StopTween && tweenType != TweenType.None)
                return true;
            return false;
        }

        private bool ShowTweenObject ()
        {
            if (executionType == ExecutionType.PlayTween && tweenType != TweenType.None)
                return true;
            if (executionType == ExecutionType.StopAnyTween)
                return true;
            return false;
        }

        private void SetDefaultTweenObject ()
        {
            if (tweenType == TweenType.Transform)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.Transform>());
            else if (tweenType == TweenType.Rigidbody)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.Rigidbody>());
            else if (tweenType == TweenType.Rigidbody2D)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.Rigidbody2D>());
            else if (tweenType == TweenType.RectTransform)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.RectTransform>());
            else if (tweenType == TweenType.Light)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.Light>());
            else if (tweenType == TweenType.Camera)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.Camera>());
            else if (tweenType == TweenType.AudioSource)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.AudioSource>());
            else if (tweenType == TweenType.ParticleSystem)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.ParticleSystem>());
            else if (tweenType == TweenType.SpriteRenderer)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.SpriteRenderer>());
            else if (tweenType == TweenType.LineRenderer)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.LineRenderer>());
            else if (tweenType == TweenType.TrailRenderer)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.TrailRenderer>());
            else if (tweenType == TweenType.Material)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.MeshRenderer>());
            else if (tweenType == TweenType.Graphic)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.Graphic>());
            else if (tweenType == TweenType.Image)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.Image>());
            else if (tweenType == TweenType.Text)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.Text>());
            else if (tweenType == TweenType.Slider)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.Slider>());
            else if (tweenType == TweenType.LayoutElement)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.LayoutElement>());
            else if (tweenType == TweenType.ScrollRect)
                tweenObject.SetObjectValue(AssignComponent<UnityEngine.UI.ScrollRect>());
            else if (tweenType == TweenType.TextMeshProGUI)
                tweenObject.SetObjectValue(AssignComponent<TMPro.TextMeshProUGUI>());
            else if (tweenType == TweenType.TextMeshPro)
                tweenObject.SetObjectValue(AssignComponent<TMPro.TextMeshPro>());
        }

        private void OnChangeTweenType ()
        {
            if (tweenType == TweenType.Transform)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Transform);
                tweenData = TweenData.GetTweenData<TransformTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Rigidbody)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Rigidbody);
                tweenData = TweenData.GetTweenData<RigidbodyTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Rigidbody2D)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Rigidbody2D);
                tweenData = TweenData.GetTweenData<Rigidbody2DTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.RectTransform)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.RectTransform);
                tweenData = TweenData.GetTweenData<RectTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Light)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Light);
                tweenData = TweenData.GetTweenData<LightTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Camera)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Camera);
                tweenData = TweenData.GetTweenData<CameraTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.AudioMixer)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.AudioMixer);
                tweenData = TweenData.GetTweenData<AudioMixerTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.AudioSource)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.AudioSource);
                tweenData = TweenData.GetTweenData<AudioSourceTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.ParticleSystem)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.ParticleSystem);
                tweenData = TweenData.GetTweenData<ParticleSystemTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Material)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.MeshRenderer);
                tweenData = TweenData.GetTweenData<MaterialTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.SpriteRenderer)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.SpriteRenderer);
                tweenData = TweenData.GetTweenData<SpriteRendererTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.LineRenderer)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.LineRenderer);
                tweenData = TweenData.GetTweenData<LineRendererTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.TrailRenderer)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.TrailRenderer);
                tweenData = TweenData.GetTweenData<TrailRendererTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Graphic)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Graphic);
                tweenData = TweenData.GetTweenData<GraphicTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Image)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Image);
                tweenData = TweenData.GetTweenData<ImageTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Text)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Text);
                tweenData = TweenData.GetTweenData<TextTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.Slider)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.Slider);
                tweenData = TweenData.GetTweenData<SliderTween>(id, runner.gameObject.scene.path);
            }

            else if (tweenType == TweenType.CanvasGroup)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.CanvasGroup);
                tweenData = TweenData.GetTweenData<CanvasGroupTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.LayoutElement)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.LayoutElement);
                tweenData = TweenData.GetTweenData<LayoutElementTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.ScrollRect)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.ScrollRect);
                tweenData = TweenData.GetTweenData<ScrollRectTween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.TextMeshProGUI)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.TextMeshProGUI);
                tweenData = TweenData.GetTweenData<TextMeshProUGUITween>(id, runner.gameObject.scene.path);
            }
            else if (tweenType == TweenType.TextMeshPro)
            {
                tweenObject = new SceneObjectProperty(SceneObject.ObjectType.TextMeshPro);
                tweenData = TweenData.GetTweenData<TextMeshProTween>(id, runner.gameObject.scene.path);
            }

            MarkDirty();
        }

        public override void OnUpdateGraphId (int previousGraphId, int newId)
        {
            if (tweenData == null)
                return;
            string previousId = GenerateId(previousGraphId);
            if (tweenData.id.Equals(previousId, StringComparison.Ordinal)) 
            {
                tweenData = TweenData.CloneTweenData(GenerateId(newId), runner.gameObject.scene.path, tweenData);
            }
        }

        public override void OnDelete ()
        {
            var settings = FrameworkSettings.GetSettings();
            if (settings == null)
                return;
            if (!settings.removeAtDeleteNode)
                return;
            if (tweenData == null)
                return;
            string sceneFolder = Path.GetDirectoryName(runner.gameObject.scene.path);
            string folderName = Path.GetFileNameWithoutExtension(runner.gameObject.scene.path);
#if UNITY_EDITOR_WIN
            string dataFolder = sceneFolder + $"\\{folderName}\\";
#elif UNITY_EDITOR_OSX
            string dataFolder = sceneFolder + $"/{folderName}/";
#endif
            string fileName = id;
            string filePath = dataFolder + fileName + ".asset";
            if (!Directory.Exists(dataFolder))
                return;
            if (!File.Exists(filePath))
                return;
            AssetDatabase.DeleteAsset(filePath);
            AssetDatabase.Refresh();
        }

        public override void OnClone (GraphNode selectedNode)
        {
            var node = (TweenBehaviourNode) selectedNode;
            if (node.tweenData != null)
            {
                if (node.tweenData.id == node.id)
                    tweenData = TweenData.CloneTweenData(id, runner.gameObject.scene.path, node.tweenData);
                else
                    tweenData = node.tweenData;
            }
            else
            {
                tweenData = null;
            }

            MarkDirty();
        }

        private static IEnumerable TweenChoice = new ValueDropdownList<TweenType>()
        {
            {"Transform", TweenType.Transform},
            {"Rect Transform", TweenType.RectTransform},
            {"Rigidbody", TweenType.Rigidbody},
            {"Rigidbody 2D", TweenType.Rigidbody2D},
            {"Light", TweenType.Light},
            {"Camera", TweenType.Camera},
            {"Audio Mixer", TweenType.AudioMixer},
            {"Audio Source", TweenType.AudioSource},
            {"Particle System", TweenType.ParticleSystem},
            {"Material", TweenType.Material},
            {"Sprite Renderer", TweenType.SpriteRenderer},
            {"Line Renderer", TweenType.LineRenderer},
            {"Trail Renderer", TweenType.TrailRenderer},
            {"Graphic", TweenType.Graphic},
            {"Image", TweenType.Image},
            {"Text", TweenType.Text},
            {"Slider", TweenType.Slider},
            {"Canvas Group", TweenType.CanvasGroup},
            {"Layout Element", TweenType.LayoutElement},
            {"Scroll Rect", TweenType.ScrollRect},
            {"Text Mesh Pro GUI", TweenType.TextMeshProGUI},
            {"Text Mesh Pro", TweenType.TextMeshPro},
        };

        private static IEnumerable TypeChoice = new ValueDropdownList<ExecutionType>()
        {
            {"Play", ExecutionType.PlayTween},
            {"Stop", ExecutionType.StopTween},
            {"Stop Any In", ExecutionType.StopAnyTween}
        };

        public static string displayName = "Tween Behaviour Node";
        public static string nodeName = "Tween";

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
            if (executionType == ExecutionType.PlayTween && tweenType != TweenType.None && tweenData != null && !tweenObject.IsNull)
            {
                var tweenDataName = tweenData.GetType().ToString();
                tweenDataName = tweenDataName.Substring(20);
                return $"Play {tweenDataName.SplitCamelCase()}\n<color=#FFF600>Continue at tween end</color>";
            }
            else if (executionType == ExecutionType.StopTween && !string.IsNullOrEmpty(tweenName))
            {
                return $"Stop tween : {tweenName}";
            }
            else if (executionType == ExecutionType.StopAnyTween && !tweenObject.IsNull)
            {
                return $"Stop any tween in {tweenObject}";
            }

            return string.Empty;
        }
#endif
    }
}