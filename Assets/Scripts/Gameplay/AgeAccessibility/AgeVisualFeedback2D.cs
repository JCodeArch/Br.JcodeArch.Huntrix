using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using UnityEngine;
namespace HuntrX.Gameplay.AgeAccessibility
{
    [DisallowMultipleComponent]
    public sealed class AgeVisualFeedback2D : MonoBehaviour
    {
        [SerializeField] private AgeProfileSelection2D profiles;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private ParticleSystem bloodParticles;
        private AttackController2D melee;
        private ZoeyRangedAttack2D ranged;
        private EnemyAgent2D enemy;
        private Color original;
        private Sprite ownedSprite;
        private float remaining;
        private bool reducedFlashes;
        public void SetReducedFlashes(bool value) { reducedFlashes=value; if(value) { remaining=0f; if(visual!=null) visual.color=original; StopParticles(); } }
        private void Awake() { melee=GetComponent<AttackController2D>(); ranged=GetComponent<ZoeyRangedAttack2D>(); enemy=GetComponent<EnemyAgent2D>(); if(visual==null)
            {
                var child=new GameObject("Age prototype silhouette"); child.transform.SetParent(transform,false); child.transform.localScale=new Vector3(.5f,1f,1f);
                visual=child.AddComponent<SpriteRenderer>(); ownedSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0f,0f,1f,1f),Vector2.one*.5f,1f); visual.sprite=ownedSprite; visual.color=new Color(.3f,.6f,.9f);
            }
            original=visual.color; }
        private void OnDestroy() { if(ownedSprite!=null) Destroy(ownedSprite); }
        private void OnEnable() { if(profiles!=null) profiles.ProfileChanged+=OnProfileChanged; if(melee!=null) melee.ImpactOccurred+=OnImpact; if(ranged!=null) ranged.HitConfirmed+=OnImpact; if(enemy!=null) enemy.ImpactOccurred+=OnImpact; }
        private void OnDisable() { if(profiles!=null) profiles.ProfileChanged-=OnProfileChanged; if(melee!=null) melee.ImpactOccurred-=OnImpact; if(ranged!=null) ranged.HitConfirmed-=OnImpact; if(enemy!=null) enemy.ImpactOccurred-=OnImpact; remaining=0f; if(visual!=null) visual.color=original; StopParticles(); }
        private void OnProfileChanged(HuntrX.Data.AgeProfileData profile) { remaining=0f; if(visual!=null) visual.color=original; StopParticles(); }
        private void Update()
        {
            if(!Valid()) { remaining=0f; if(visual!=null) visual.color=original; StopParticles(); return; }
            if(!profiles.Current.AllowBlood) { if(bloodParticles!=null) bloodParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear); }
            if(remaining>0f) { remaining=Mathf.Max(0f,remaining-Time.deltaTime); if(remaining<=0f && visual!=null) visual.color=original; }
        }
        private bool Valid()=>profiles!=null && profiles.isActiveAndEnabled && profiles.Current!=null && profiles.Current.IsValid(out _);
        private void OnImpact(CombatImpactEvent impact)
        {
            if(!isActiveAndEnabled || !Valid()) return;
            float intensity=profiles.Current.VisualIntensity;
            if(reducedFlashes) intensity=Mathf.Min(intensity,.15f);
            if(visual!=null) { visual.color=Color.Lerp(original,Color.white,intensity); remaining=.08f*intensity; }
            if(impactParticles!=null && !reducedFlashes) impactParticles.Emit(profiles.Current.ParticleBudget);
            if(bloodParticles!=null && !reducedFlashes && profiles.Current.AllowBlood) bloodParticles.Emit(profiles.Current.ParticleBudget);
        }
        private void StopParticles() { if(impactParticles!=null) impactParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear); if(bloodParticles!=null) bloodParticles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear); }
    }
}
