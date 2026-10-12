using UnityEngine;
namespace HuntrX.Pooling
{
    [DisallowMultipleComponent]
    public sealed class PooledImpactSprite2D : MonoBehaviour,IPooledLifecycle
    {
        private static Sprite sharedSprite;
        private SpriteRenderer visual;
        private ParticleSystem particles;

        public void PrepareForRent()
        {
            if(visual==null){visual=GetComponent<SpriteRenderer>();if(visual==null)visual=gameObject.AddComponent<SpriteRenderer>();}
            if(sharedSprite==null)sharedSprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),new Vector2(.5f,.5f),Texture2D.whiteTexture.width);
            visual.sprite=sharedSprite;visual.color=new Color(1f,.85f,.3f,0f);visual.sortingOrder=50;
            transform.localScale=new Vector3(.35f,.35f,1f);
            if(particles==null)
            {
                Material material=Resources.Load<Material>("Effects/PrototypeParticle");
                if(material==null)throw new System.InvalidOperationException("Authored prototype particle material is required.");
                GameObject emitter=new GameObject("Bounded impact particles");emitter.transform.SetParent(transform,false);
                particles=emitter.AddComponent<ParticleSystem>();
                particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=particles.main;main.playOnAwake=false;main.loop=false;main.duration=.2f;main.startLifetime=.15f;
                main.startSpeed=1.5f;main.startSize=.12f;main.startColor=new Color(1f,.8f,.2f,.85f);main.maxParticles=16;
                main.simulationSpace=ParticleSystemSimulationSpace.World;
                var emission=particles.emission;emission.enabled=false;
                var shape=particles.shape;shape.enabled=true;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=.05f;
                var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=51;
            }
            particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        public void PrepareForReturn(){if(visual!=null)visual.sprite=null;StopParticles();}
        public bool TryPresent(int particleBudget,float intensity)
        {
            if(!isActiveAndEnabled||visual==null||particles==null||float.IsNaN(intensity)||float.IsInfinity(intensity)||intensity<0f||intensity>1f||particleBudget<0)return false;
            visual.color=new Color(1f,.85f,.3f,.8f*intensity);
            var main=particles.main;main.startColor=new Color(1f,.8f,.2f,.85f*intensity);
            if(particleBudget>0&&intensity>0f){particles.Play(false);particles.Emit(Mathf.Min(8,particleBudget));}return true;
        }
        private void StopParticles(){if(particles!=null)particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
        private void OnDisable(){StopParticles();}
    }
}
