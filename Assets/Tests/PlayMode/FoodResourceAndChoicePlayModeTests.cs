using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HuntrX.Data;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Resources;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HuntrX.Tests.PlayMode
{
    public sealed class FoodResourceAndChoicePlayModeTests
    {
        private static readonly BindingFlags NonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private readonly List<GameObject> instances = new List<GameObject>();

        private GameObject Spawn(string path)
        {
            var prefab = UnityEngine.Resources.Load<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var instance = UnityEngine.Object.Instantiate(prefab);
            instances.Add(instance);
            return instance;
        }

        private static T Field<T>(object instance, string name) =>
            (T)instance.GetType().GetField(name, NonPublicInstance).GetValue(instance);

        private static void SetField(object instance, string name, object value) =>
            instance.GetType().GetField(name, NonPublicInstance).SetValue(instance, value);

        [TearDown]
        public void TearDown()
        {
            foreach (var instance in instances)
            {
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
            instances.Clear();
        }

        [Test]
        public void AllFourFoodPickupsLoadAndHaveValidDefinitions()
        {
            string[] paths =
            {
                "Food/Ramen_Pickup",
                "Food/Onigiri_Pickup",
                "Food/Gyoza_Pickup",
                "Food/Dango_Pickup"
            };

            FoodKind[] expectedKinds =
            {
                FoodKind.Ramen,
                FoodKind.Onigiri,
                FoodKind.Gyoza,
                FoodKind.Dango
            };

            for (int i = 0; i < paths.Length; i++)
            {
                var go = Spawn(paths[i]);
                var pickup = go.GetComponent<FoodPickup2D>();
                Assert.That(pickup, Is.Not.Null, paths[i]);

                var def = Field<FoodDefinition>(pickup, "definition");
                Assert.That(def, Is.Not.Null, paths[i] + " definition");
                Assert.That(def.IsValid(out string error), Is.True, error);
                Assert.That(def.Kind, Is.EqualTo(expectedKinds[i]));
                Assert.That(def.SelfRecovery, Is.GreaterThan(0f));
                Assert.That(def.FanRecovery, Is.GreaterThan(0f));
            }
        }

        [Test]
        public void FoodPickupCollectIncrementsInventoryAndDeactivates()
        {
            var inventoryGo = new GameObject("food_inventory_test");
            instances.Add(inventoryGo);
            var inventory = inventoryGo.AddComponent<FoodInventory2D>();

            var pickupGo = Spawn("Food/Ramen_Pickup");
            var pickup = pickupGo.GetComponent<FoodPickup2D>();
            var def = Field<FoodDefinition>(pickup, "definition");

            Assert.That(inventory.Count(def), Is.Zero);
            Assert.That(inventory.TotalCount, Is.Zero);
            Assert.That(pickupGo.activeSelf, Is.True);

            bool collected = pickup.TryCollect(inventory);
            Assert.That(collected, Is.True);
            Assert.That(inventory.Count(def), Is.EqualTo(1));
            Assert.That(inventory.TotalCount, Is.EqualTo(1));
            Assert.That(pickupGo.activeSelf, Is.False);

            bool collectAgain = pickup.TryCollect(inventory);
            Assert.That(collectAgain, Is.False);
            Assert.That(inventory.TotalCount, Is.EqualTo(1));
        }

        [Test]
        public void FoodInventoryEnforcesCapacityLimit()
        {
            var inventoryGo = new GameObject("food_inventory_cap_test");
            instances.Add(inventoryGo);
            var inventory = inventoryGo.AddComponent<FoodInventory2D>();
            SetField(inventory, "capacity", 2);

            var pickupGo = Spawn("Food/Ramen_Pickup");
            var pickup = pickupGo.GetComponent<FoodPickup2D>();
            var def = Field<FoodDefinition>(pickup, "definition");

            Assert.That(inventory.TryAdd(def, 1), Is.True);
            Assert.That(inventory.TryAdd(def, 1), Is.True);
            Assert.That(inventory.TotalCount, Is.EqualTo(2));

            // Exceeds capacity 2
            Assert.That(inventory.TryAdd(def, 1), Is.False);
            Assert.That(inventory.TotalCount, Is.EqualTo(2));
        }

        [Test]
        public void FoodInventoryRaisesQuantityChangedEvent()
        {
            var inventoryGo = new GameObject("food_inventory_event_test");
            instances.Add(inventoryGo);
            var inventory = inventoryGo.AddComponent<FoodInventory2D>();

            var pickupGo = Spawn("Food/Onigiri_Pickup");
            var pickup = pickupGo.GetComponent<FoodPickup2D>();
            var def = Field<FoodDefinition>(pickup, "definition");

            int eventCount = 0;
            FoodDefinition reportedFood = null;
            int reportedAmount = 0;

            inventory.QuantityChanged += (food, amount) =>
            {
                eventCount++;
                reportedFood = food;
                reportedAmount = amount;
            };

            Assert.That(inventory.TryAdd(def, 1), Is.True);
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(reportedFood, Is.EqualTo(def));
            Assert.That(reportedAmount, Is.EqualTo(1));
        }

        [Test]
        public void FoodChoiceControllerBindsSelfAndRejectsInvalidFaction()
        {
            var rumi = Spawn("Characters/Rumi_Prototype").GetComponent<DamageReceiver2D>();
            var demon = Spawn("Enemies/GroundDemon_Prototype").GetComponent<DamageReceiver2D>();

            var choiceGo = Spawn("Food/RumiFoodChoice_Prototype");
            var choice = choiceGo.GetComponent<FoodChoiceController2D>();

            Assert.That(choice.TryBindSelf(demon), Is.False, "Should reject Demon faction for self binding.");
            Assert.That(choice.TryBindSelf(rumi), Is.True, "Should accept HuntrX faction for self binding.");
        }

        [Test]
        public void FoodChoiceSelfRecoveryFailsAtFullHealthAndSucceedsWhenDamaged()
        {
            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var rumi = rumiGo.GetComponent<DamageReceiver2D>();

            var choiceGo = Spawn("Food/RumiFoodChoice_Prototype");
            var choice = choiceGo.GetComponent<FoodChoiceController2D>();
            var inventory = choiceGo.GetComponent<FoodInventory2D>();
            Assert.That(choice.TryBindSelf(rumi), Is.True);

            var pickupGo = Spawn("Food/Ramen_Pickup");
            var pickup = pickupGo.GetComponent<FoodPickup2D>();
            var food = Field<FoodDefinition>(pickup, "definition");
            Assert.That(inventory.TryAdd(food, 2), Is.True);

            // Full health: TryUseOnSelf must fail and preserve food count
            Assert.That(rumi.CurrentHealth, Is.EqualTo(rumi.MaximumHealth));
            Assert.That(choice.TryUseOnSelf(food), Is.False);
            Assert.That(inventory.Count(food), Is.EqualTo(2));

            // Damage player
            var demonGo = Spawn("Enemies/GroundDemon_Prototype");
            var demon = demonGo.GetComponent<DamageReceiver2D>();
            var attack = demonGo.GetComponent<EnemyAgent2D>().Definition.Attack;
            rumi.TryReceiveHit(attack, demon, 1f);
            Assert.That(rumi.CurrentHealth, Is.LessThan(rumi.MaximumHealth));
            float damagedHealth = rumi.CurrentHealth;

            // Use food on self: succeeds, restores health, decrements inventory
            Assert.That(choice.TryUseOnSelf(food), Is.True);
            Assert.That(rumi.CurrentHealth, Is.GreaterThan(damagedHealth));
            Assert.That(inventory.Count(food), Is.EqualTo(1));
        }

        [Test]
        public void FoodChoiceFanRecoveryFailsAtFullSoulAndSucceedsWhenDrained()
        {
            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var rumi = rumiGo.GetComponent<DamageReceiver2D>();

            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            fanGo.transform.position = rumiGo.transform.position + new Vector3(1f, 0f, 0f);

            var choiceGo = Spawn("Food/RumiFoodChoice_Prototype");
            var choice = choiceGo.GetComponent<FoodChoiceController2D>();
            var inventory = choiceGo.GetComponent<FoodInventory2D>();
            Assert.That(choice.TryBindSelf(rumi), Is.True);

            var pickupGo = Spawn("Food/Gyoza_Pickup");
            var food = Field<FoodDefinition>(pickupGo.GetComponent<FoodPickup2D>(), "definition");
            Assert.That(inventory.TryAdd(food, 2), Is.True);

            // Full soul: TryUseOnFan fails and preserves food
            Assert.That(choice.TryUseOnFan(food, fan), Is.False);
            Assert.That(inventory.Count(food), Is.EqualTo(2));

            // Drain fan soul
            Assert.That(fan.TryDrain(food.FanRecovery * 1.5f), Is.True);
            float drainedSoul = fan.Soul;

            // Within range (distance = 1f <= fanUseRange = 3f): succeeds, recovers fan soul, decrements food
            Assert.That(choice.TryUseOnFan(food, fan), Is.True);
            Assert.That(fan.Soul, Is.GreaterThan(drainedSoul));
            Assert.That(inventory.Count(food), Is.EqualTo(1));
        }

        [Test]
        public void FoodChoiceFanRecoveryRejectedBeyondRange()
        {
            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var rumi = rumiGo.GetComponent<DamageReceiver2D>();

            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            // Place fan 10 units away (well beyond default fanUseRange = 3f)
            fanGo.transform.position = rumiGo.transform.position + new Vector3(10f, 0f, 0f);

            var choiceGo = Spawn("Food/RumiFoodChoice_Prototype");
            var choice = choiceGo.GetComponent<FoodChoiceController2D>();
            var inventory = choiceGo.GetComponent<FoodInventory2D>();
            Assert.That(choice.TryBindSelf(rumi), Is.True);

            var pickupGo = Spawn("Food/Dango_Pickup");
            var food = Field<FoodDefinition>(pickupGo.GetComponent<FoodPickup2D>(), "definition");
            Assert.That(inventory.TryAdd(food, 1), Is.True);

            Assert.That(fan.TryDrain(20f), Is.True);
            float drainedSoul = fan.Soul;

            // Beyond range: TryUseOnFan fails, fan soul untouched, food preserved
            Assert.That(choice.TryUseOnFan(food, fan), Is.False);
            Assert.That(fan.Soul, Is.EqualTo(drainedSoul));
            Assert.That(inventory.Count(food), Is.EqualTo(1));
        }

        [Test]
        public void FoodChoiceControllerRejectsWhenDisabled()
        {
            var rumiGo = Spawn("Characters/Rumi_Prototype");
            var rumi = rumiGo.GetComponent<DamageReceiver2D>();

            var fanGo = Spawn("Fans/RescuableFan_Prototype");
            var fan = fanGo.GetComponent<FanActor2D>();
            fanGo.transform.position = rumiGo.transform.position;

            var choiceGo = Spawn("Food/RumiFoodChoice_Prototype");
            var choice = choiceGo.GetComponent<FoodChoiceController2D>();
            var inventory = choiceGo.GetComponent<FoodInventory2D>();
            Assert.That(choice.TryBindSelf(rumi), Is.True);

            var pickupGo = Spawn("Food/Ramen_Pickup");
            var food = Field<FoodDefinition>(pickupGo.GetComponent<FoodPickup2D>(), "definition");
            Assert.That(inventory.TryAdd(food, 2), Is.True);

            choice.enabled = false;
            Assert.That(choice.TryUseOnSelf(food), Is.False);
            Assert.That(choice.TryUseOnFan(food, fan), Is.False);
            Assert.That(inventory.Count(food), Is.EqualTo(2));
        }
    }
}
