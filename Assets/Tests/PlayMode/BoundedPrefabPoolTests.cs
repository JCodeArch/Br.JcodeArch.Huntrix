using HuntrX.Pooling;
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
    }
}
