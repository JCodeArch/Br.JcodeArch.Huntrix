using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Pooling;
using NUnit.Framework;
using UnityEngine;
namespace HuntrX.Tests.PlayMode
{
    public sealed class GroundEnemyPoolLifecycleTests
    {
        [Test] public void DeathNotificationCannotReuseLeaseUntilCommitEndsAndNextRentResetsHealth()
        {
            var prefab=UnityEngine.Resources.Load<GameObject>("Enemies/GroundDemon_Prototype");Assert.That(prefab,Is.Not.Null);
            var playerPrefab=UnityEngine.Resources.Load<GameObject>("Characters/Rumi_Prototype");Assert.That(playerPrefab,Is.Not.Null);
            GameObject player=UnityEngine.Object.Instantiate(playerPrefab);
            try
            {
                using(var pool=new BoundedPrefabPool(prefab,1,1))
                {
                    Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out var instance),Is.True);
                    var agent=instance.GetComponent<EnemyAgent2D>();var receiver=agent.Self;int deaths=0;
                    receiver.Died+=actor=>
                    {
                        deaths++;Assert.That(instance.GetComponent<PooledInstance>().TryReturn(),Is.True);
                        Assert.That(pool.TryRent(Vector3.zero,Quaternion.identity,out _),Is.False);
                    };
                    var source=player.GetComponent<DamageReceiver2D>();
                    for(int i=0;i<100&&receiver.IsAlive;i++)Assert.That(receiver.TryReceiveHit(agent.Definition.Attack,source,1f),Is.True);
                    Assert.That(receiver.IsAlive,Is.False);Assert.That(deaths,Is.EqualTo(1));
                    Assert.That(pool.TryRent(Vector3.one,Quaternion.identity,out var reused),Is.True);
                    Assert.That(reused,Is.SameAs(instance));Assert.That(receiver.CurrentHealth,Is.EqualTo(receiver.MaximumHealth));
                    Assert.That(agent.State,Is.EqualTo(EnemyState2D.Idle));Assert.That(agent.Target,Is.Null);
                    Assert.That(reused.GetComponent<Rigidbody2D>().linearVelocity,Is.EqualTo(Vector2.zero));
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(player);}
        }
    }
}
