using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Checkpoints;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Performance;
using NUnit.Framework;
using UnityEngine;
namespace HuntrX.Tests.PlayMode
{
    public sealed class CombatPerformanceEpochTests
    {
        private readonly List<GameObject> instances=new List<GameObject>();
        private GameObject Spawn(string path)
        {
            var prefab=UnityEngine.Resources.Load<GameObject>(path);Assert.That(prefab,Is.Not.Null,path);
            var instance=UnityEngine.Object.Instantiate(prefab);instances.Add(instance);return instance;
        }
        [TearDown] public void TearDown(){foreach(var instance in instances)if(instance!=null)UnityEngine.Object.DestroyImmediate(instance);instances.Clear();}
        private void Setup(out CharacterManager2D manager,out CombatPerformanceController2D performance,out EnemyAgent2D enemy)
        {
            var slot=Spawn("Characters/HuntrXPlayerSlot_Prototype");manager=slot.GetComponent<CharacterManager2D>();
            var rumi=UnityEngine.Resources.Load<GameObject>("Characters/Rumi_Prototype");
            Assert.That(manager.Configure(rumi,slot.GetComponent<CheckpointAttemptFlow2D>(),slot.transform.Find("StageStart"),"test-start"),Is.True);
            Assert.That(manager.TrySpawn(),Is.True);
            performance=Spawn("Performance/PerformanceRun_Prototype").GetComponent<CombatPerformanceController2D>();
            var definition=(CombatPerformanceDefinition)typeof(CombatPerformanceController2D).GetField("definition",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(performance);
            Assert.That(performance.Configure(definition,manager,new FanRescueController2D[0]),Is.True);
            Assert.That(performance.TryBeginRun(),Is.True);enemy=Spawn("Enemies/GroundDemon_Prototype").GetComponent<EnemyAgent2D>();
        }
        [Test] public void AcceptedHitSurvivesCharacterSwitchWithinSameRun()
        {
            Setup(out var manager,out var performance,out var enemy);var previous=manager.ActiveCharacter;
            enemy.Self.DamageDealt+=impact=>Assert.That(manager.TrySwitch(UnityEngine.Resources.Load<GameObject>("Characters/Mira_Prototype")),Is.True);
            Assert.That(previous.TryReceiveHit(enemy.Definition.Attack,enemy.Self,1f),Is.True);
            Assert.That(manager.ActiveCharacter,Is.Not.SameAs(previous));Assert.That(performance.Snapshot.NoDamage,Is.False);
        }
        [Test] public void CapturedOldHitCannotDamageNewlyStartedRunMetrics()
        {
            Setup(out var manager,out var performance,out var enemy);var previous=manager.ActiveCharacter;
            enemy.Self.DamageDealt+=impact=>{Assert.That(performance.ResetRun(),Is.True);Assert.That(performance.TryBeginRun(),Is.True);};
            Assert.That(previous.TryReceiveHit(enemy.Definition.Attack,enemy.Self,1f),Is.True);
            Assert.That(performance.IsRunning,Is.True);Assert.That(performance.Snapshot.NoDamage,Is.True);
        }
    }
}
