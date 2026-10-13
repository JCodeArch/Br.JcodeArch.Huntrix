using HuntrX.Pooling;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace HuntrX.Tests.PlayMode
{
    public sealed class BoundedPrefabPoolTests
    {
        [Test] public void CapacityRejectsOverflowAndReturnedInstanceCanBeReused()
        {
            var prefab=Resources.Load<GameObject>("Effects/PooledCombatImpact_Prototype");Assert.That(prefab,Is.Not.Null);
            using(var pool=new BoundedPrefabPool(prefab,1,1))
            {
                Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out var first),Is.True);
                Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out _),Is.False);
                Assert.That(first.GetComponent<PooledInstance>().TryReturn(),Is.True);
                Assert.That(first.activeSelf,Is.False);
                Assert.That(pool.TryRent(Vector3.one,Quaternion.identity,out var second),Is.True);
                Assert.That(second,Is.SameAs(first));Assert.That(second.transform.position,Is.EqualTo(Vector3.one));
            }
        }
        [Test] public void DoubleReturnIsRejectedAndDisposedPoolCannotRent()
        {
            var prefab=Resources.Load<GameObject>("Effects/PooledCombatImpact_Prototype");Assert.That(prefab,Is.Not.Null);
            var pool=new BoundedPrefabPool(prefab,1);
            Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out var instance),Is.True);
            var lease=instance.GetComponent<PooledInstance>();Assert.That(lease.TryReturn(),Is.True);Assert.That(lease.TryReturn(),Is.False);
            pool.Dispose();Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out _),Is.False);
        }
        [Test] public void ProjectileImpactStopsOldSubscribersAfterCallbackRentsANewLease()
        {
            var prefab=Resources.Load<GameObject>("Enemies/DemonProjectile_Prototype");Assert.That(prefab,Is.Not.Null);
            using(var pool=new BoundedPrefabPool(prefab,1,1))
            {
                Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out var instance),Is.True);
                var projectile=instance.GetComponent<EnemyProjectile2D>();Assert.That(projectile,Is.Not.Null);
                int retiredCalls=0,newLeaseCalls=0;bool returned=false,rented=false;GameObject reused=null;
                projectile.ImpactOccurred+=_=>
                {
                    returned=instance.GetComponent<PooledInstance>().TryReturn();
                    rented=pool.TryRent(Vector3.one,Quaternion.identity,out reused);
                    if(rented)projectile.ImpactOccurred+=__=>newLeaseCalls++;
                };
                projectile.ImpactOccurred+=_=>retiredCalls++;
                // Isolate dispatch from raycasting: exercise real callbacks and real pool return/rent.
                var publish=typeof(EnemyProjectile2D).GetMethod("PublishImpact",BindingFlags.Instance|BindingFlags.NonPublic);
                Assert.That(publish,Is.Not.Null);
                publish.Invoke(projectile,new object[]{default(CombatImpactEvent)});
                Assert.That(returned,Is.True);Assert.That(rented,Is.True);Assert.That(reused,Is.SameAs(instance));
                Assert.That(retiredCalls,Is.Zero,"A previous lease subscriber must not run after reuse.");
                Assert.That(newLeaseCalls,Is.Zero,"The new lease must not receive the preceding impact.");
                publish.Invoke(projectile,new object[]{default(CombatImpactEvent)});
                Assert.That(newLeaseCalls,Is.EqualTo(1));
            }
        }
    }
}
