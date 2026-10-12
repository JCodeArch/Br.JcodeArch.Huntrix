using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Resources;
using HuntrX.Gameplay.Special;
using HuntrX.Gameplay.AgeAccessibility;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace HuntrX.Tests.PlayMode
{
    public sealed class CriticalFanResourceAndSpecialTests
    {
        private readonly List<GameObject> instances=new List<GameObject>();
        private GameObject Spawn(string path)
        {
            var prefab=UnityEngine.Resources.Load<GameObject>(path);Assert.That(prefab,Is.Not.Null,path);
            var instance=UnityEngine.Object.Instantiate(prefab);instances.Add(instance);return instance;
        }
        private static T Field<T>(object instance,string name)=>(T)instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);
        [TearDown] public void TearDown(){foreach(var instance in instances)if(instance!=null)UnityEngine.Object.DestroyImmediate(instance);instances.Clear();}
        [Test] public void CancelledRescueGivesNoRewardAndReleasesProtection()
        {
            var source=Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var go=Spawn("Fans/RescuableFan_Prototype");var fan=go.GetComponent<FanActor2D>();var rescue=go.GetComponent<FanRescueController2D>();
            int rewards=0;rescue.RescueCompleted+=(actor,reward)=>rewards++;
            Assert.That(fan.TryDrain(fan.Definition.MaximumSoul*.5f),Is.True);
            Assert.That(rescue.TryBeginRescue(source),Is.True);Assert.That(fan.IsProtected,Is.True);
            rescue.Cancel();Assert.That(rewards,Is.Zero);Assert.That(rescue.CompletedRescueCount,Is.Zero);Assert.That(fan.IsProtected,Is.False);
        }
        [UnityTest] public IEnumerator DisabledRescuerCancelsWithoutCompletion()
        {
            var source=Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var go=Spawn("Fans/RescuableFan_Prototype");var fan=go.GetComponent<FanActor2D>();var rescue=go.GetComponent<FanRescueController2D>();
            Assert.That(fan.TryDrain(fan.Definition.MaximumSoul*.5f),Is.True);Assert.That(rescue.TryBeginRescue(source),Is.True);
            source.gameObject.SetActive(false);yield return null;
            Assert.That(rescue.IsRecovering,Is.False);Assert.That(rescue.CompletedRescueCount,Is.Zero);Assert.That(fan.IsProtected,Is.False);
        }
        [Test] public void FoodReservationBlocksNestedMutationAndRollbackRestoresItem()
        {
            var go=new GameObject("food inventory test");instances.Add(go);var inventory=go.AddComponent<FoodInventory2D>();
            var pickup=Spawn("Food/Ramen_Pickup").GetComponent<FoodPickup2D>();var food=Field<FoodDefinition>(pickup,"definition");
            Assert.That(inventory.TryAdd(food),Is.True);
            var consume=typeof(FoodInventory2D).GetMethod("TryConsume",BindingFlags.Instance|BindingFlags.NonPublic);
            Func<bool> benefit=()=>{Assert.That(inventory.Count(food),Is.Zero);Assert.That(inventory.TryAdd(food),Is.False);return false;};
            Assert.That((bool)consume.Invoke(inventory,new object[]{food,benefit}),Is.False);
            Assert.That(inventory.Count(food),Is.EqualTo(1));Assert.That(inventory.TotalCount,Is.EqualTo(1));Assert.That(inventory.IsBusy,Is.False);
        }
        [Test] public void SpecialDoesNotSelectFanActorFromExplicitEnemyRoster()
        {
            var owner=Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var enemy=Spawn("Enemies/GroundDemon_Prototype");enemy.AddComponent<FanActor2D>();
            var special=Spawn("Special/HuntrXSpecial_Prototype").GetComponent<HuntrXSpecialController2D>();
            var definition=Field<HuntrXSpecialDefinition>(special,"definition");
            Assert.That(special.TryConfigure(definition,owner,new[]{enemy.GetComponent<DamageReceiver2D>()},out _),Is.True);
            Assert.That(special.TryAddCharge(definition.MaximumCharge),Is.True);
            Assert.That(special.TryActivate(),Is.False);Assert.That(special.SelectedCount,Is.Zero);
        }
        [Test] public void DestroyedDamageSourceFailsAgeCalculationClosed()
        {
            var owner=Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var enemy=Spawn("Enemies/GroundDemon_Prototype");var target=enemy.GetComponent<DamageReceiver2D>();
            var attack=enemy.GetComponent<HuntrX.Gameplay.Enemies.EnemyAgent2D>().Definition.Attack;
            UnityEngine.Object.DestroyImmediate(owner.gameObject);
            Assert.That(float.IsNaN(AgeDifficultyPolicy2D.CalculateDamage(attack,owner,target)),Is.True);
        }
    }
}
