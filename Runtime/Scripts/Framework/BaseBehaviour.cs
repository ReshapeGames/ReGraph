using System.Runtime.CompilerServices;
using UnityEngine;
using Reshape.Unity;

namespace Reshape.ReFramework
{
    [AddComponentMenu("")]
	public class BaseBehaviour : ReMonoBehaviour
	{
	    protected virtual void Start()
	    {
		    
	    }

	    public bool IsGameObjectMatch (GameObject go, string[] excludeTags, string[] excludeLayers, string[] onlyTags, string[] onlyLayers, string[] specificNames)
        {
            return IsGameObjectMatchFilter(go, excludeTags, excludeLayers, onlyTags, onlyLayers, specificNames);
        }
	    
	    [SpecialName] public override bool ReceivedRayCast(ReMonoBehaviour mono, string rayName, RaycastHit? hit) { return base.ReceivedRayCast(mono, rayName, hit); }
		
	    [SpecialName] public override void InitSystemFlow() { base.InitSystemFlow(); }
	    [SpecialName] public override void ClearSystemFlow() { base.ClearSystemFlow(); }
	    [SpecialName] public override void UpdateSystemFlow() { base.UpdateSystemFlow(); }
	    [SpecialName] public override void StartSystemInitFlow() { base.StartSystemInitFlow(); }
	    [SpecialName] public override void StartSystemTickFlow() { base.StartSystemTickFlow(); }
	    [SpecialName] public override void StartSystemBeginFlow() { base.StartSystemBeginFlow(); }
	    [SpecialName] public override void StartSystemUninitFlow() { base.StartSystemUninitFlow(); }
	    [SpecialName] public override void GenerateReId() { base.GenerateReId(); }
	    [SpecialName] public override void GenerateReIdIfNotExist() { base.GenerateReIdIfNotExist(); }
	    [SpecialName] public override void PlanPreInit() { base.PlanPreInit(); }
	    [SpecialName] public override void PlanInit() { base.PlanInit(); }
	    [SpecialName] public override void PlanPostInit() { base.PlanPostInit(); }
	    [SpecialName] public override void PlanPreBegin() { base.PlanPreBegin(); }
	    [SpecialName] public override void PlanBegin() { base.PlanBegin(); }
	    [SpecialName] public override void PlanPostBegin() { base.PlanPostBegin(); }
	    [SpecialName] public override void PlanPreTick(string @group = null) { base.PlanPreTick(@group); }
	    [SpecialName] public override void PlanTick(string @group = null) { base.PlanTick(@group); }
	    [SpecialName] public override void PlanPostTick(string @group = null) { base.PlanPostTick(@group); }
	    [SpecialName] public override void PlanPreUninit() { base.PlanPreUninit(); }
	    [SpecialName] public override void PlanUninit() { base.PlanUninit(); }
	    [SpecialName] public override void PlanPostUninit() { base.PlanPostUninit(); }
	    [SpecialName] public override void OmitPreTick() { base.OmitPreTick(); }
	    [SpecialName] public override void OmitTick() { base.OmitTick(); }
	    [SpecialName] public override void OmitPostTick() { base.OmitPostTick(); }
	    [SpecialName] public override void OmitPreUninit() { base.OmitPreUninit(); }
	    [SpecialName] public override void OmitUninit() { base.OmitUninit(); }
	    [SpecialName] public override void OmitPostUninit() { base.OmitPostUninit(); }
	    [SpecialName] public override void PausePreTick(string @group) { base.PausePreTick(@group); }
	    [SpecialName] public override void PauseTick(string @group) { base.PauseTick(@group); }
	    [SpecialName] public override void PausePostTick(string @group) { base.PausePostTick(@group); }
	    [SpecialName] public override void UnpausePreTick(string @group) { base.UnpausePreTick(@group); }
	    [SpecialName] public override void UnpauseTick(string @group) { base.UnpauseTick(@group); }
	    [SpecialName] public override void UnpausePostTick(string @group) { base.UnpausePostTick(@group); }
	    [SpecialName] public override void PreInit() { base.PreInit(); }
	    [SpecialName] public override void Init() { base.Init(); }
	    [SpecialName] public override void PostInit() { base.PostInit(); }
	    [SpecialName] public override void PreBegin() { base.PreBegin(); }
	    [SpecialName] public override void Begin() { base.Begin(); }
	    [SpecialName] public override void PostBegin() { base.PostBegin(); }
	    [SpecialName] public override void PreTick() { base.PreTick(); }
	    [SpecialName] public override void Tick() { base.Tick(); }
	    [SpecialName] public override void PostTick() { base.PostTick(); }
	    [SpecialName] public override void PreUninit() { base.PreUninit(); }
	    [SpecialName] public override void Uninit() { base.Uninit(); }
	    [SpecialName] public override void PostUninit() { base.PostUninit(); }
	    [SpecialName] public override void CancelWait(string id) { base.CancelWait(id); }
	    [SpecialName] public override void StopWait(string id) { base.StopWait(id); }
	    [SpecialName] public override void ResumeWait(string id) { base.ResumeWait(id); }
	    [SpecialName] public override void ClearPool(string type) { base.ClearPool(type); }
	    [SpecialName] public override void Forget(string key) { base.Forget(key); }
	    [SpecialName] public override void Forget(int key) { base.Forget(key); }
	}
}