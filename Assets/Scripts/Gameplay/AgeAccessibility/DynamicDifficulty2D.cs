using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using System.Collections.Generic;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class DynamicDifficulty2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private CheckpointAttemptFlow2D flow;
        [SerializeField] private AgeDifficultyPolicy2D[] policies=new AgeDifficultyPolicy2D[0];
        [SerializeField] private FanActor2D[] fans=new FanActor2D[0];
        private readonly HashSet<FanActor2D> lost=new HashSet<FanActor2D>();
        private int deaths,fanLosses;
        private float observedHealth;
        private DamageReceiver2D observed;
        private float pressure;
        private void OnEnable() { if(flow!=null) flow.RespawnRequested+=OnRespawn; if(fans!=null && fans.Length<=64) foreach(var fan in fans) if(fan!=null) fan.StateChanged+=OnFanState; }
        private void OnDisable() { if(flow!=null) flow.RespawnRequested-=OnRespawn; if(fans!=null && fans.Length<=64) foreach(var fan in fans) if(fan!=null) fan.StateChanged-=OnFanState; if(observed!=null) { var selected=observed.GetComponent<AgeDifficultyPolicy2D>(); if(selected!=null) selected.TrySetAssistance(1f); } if(policies!=null && policies.Length<=64) foreach(var policy in policies) if(policy!=null) policy.TrySetAssistance(1f); }
        private void OnRespawn(RespawnRequest request) { deaths=Mathf.Min(deaths+1,12); }
        private void OnFanState(FanActor2D fan,FanState previous,FanState next) { if(next==FanState.Corrupted && lost.Add(fan)) ReportFanLost(); }
        public void ReportFanLost() => fanLosses=Mathf.Min(fanLosses+1,12);
        public void ReportEncounterCompleted() { deaths=Mathf.Max(0,deaths-1); fanLosses=Mathf.Max(0,fanLosses-1); pressure=Mathf.Max(0f,pressure-.25f); }
        private void Update()
        {
            var actor=manager!=null?manager.ActiveCharacter:null;
            if(actor!=observed) { if(observed!=null) { var oldPolicy=observed.GetComponent<AgeDifficultyPolicy2D>(); if(oldPolicy!=null) oldPolicy.TrySetAssistance(1f); } observed=actor; observedHealth=actor!=null?actor.CurrentHealth:0f; }
            if(actor!=null && actor.MaximumHealth>0f)
            {
                pressure=Mathf.Clamp01(pressure+Mathf.Max(0f,observedHealth-actor.CurrentHealth)/actor.MaximumHealth);
                observedHealth=actor.CurrentHealth;
            }
            int attempts=flow!=null?Mathf.Min(Mathf.Max(0,flow.AttemptNumber-1),12):0;
            float target=Mathf.Clamp(1f-deaths*.025f-fanLosses*.01f-attempts*.01f-pressure*.08f,.6f,1f);
            if(policies==null || policies.Length>64) return;
            var activePolicy=actor!=null?actor.GetComponent<AgeDifficultyPolicy2D>():null;
            if(activePolicy!=null && activePolicy.isActiveAndEnabled) activePolicy.TrySetAssistance(Mathf.MoveTowards(activePolicy.Assistance,target,Time.deltaTime*.03f));
            foreach(var policy in policies) if(policy!=null && policy!=activePolicy && policy.isActiveAndEnabled) policy.TrySetAssistance(Mathf.MoveTowards(policy.Assistance,target,Time.deltaTime*.03f));
        }
    }
}
