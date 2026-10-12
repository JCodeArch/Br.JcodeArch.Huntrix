using HuntrX.Data;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Fans;
using HuntrX.Gameplay.Levels;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    /// <summary>Explicit standalone/slice presentation composition. No global lookup.</summary>
    [DisallowMultipleComponent]
    public sealed class StageArtBridge2D : MonoBehaviour
    {
        [SerializeField] private BlockoutLevelController2D standalone;
        [SerializeField] private StageBlockoutBuilder2D builder;
        [SerializeField] private CharacterManager2D player;
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private Camera view;
        [SerializeField] private CharacterArtBinder2D art;
        [SerializeField] private CharacterAnimationBinder2D animationBinder;
        [SerializeField] private StageEnvironmentRenderer2D environment;
        [SerializeField] private GameplayVfxPool2D effects;
        [SerializeField] private GameplayVfxAdapter2D effectAdapter;
        [SerializeField] private GameObject[] characterPrefabs;
        [SerializeField] private CharacterArtDefinition[] characters;
        [SerializeField] private CharacterRigDefinition[] rigs;
        [SerializeField] private FanSoulDrainController2D drainer;
        private CharacterManager2D configuredPlayer;
        private StageBlockoutDefinition configuredStage;
        private DamageReceiver2D configuredActor;
        private EnemyAgent2D configuredBoss;
        private bool effectsBound;
        private void OnEnable(){if(art!=null)art.enabled=true;if(animationBinder!=null)animationBinder.enabled=true;if(standalone!=null)standalone.StageLoaded+=StageLoaded;if(session!=null)session.StageLoaded+=SliceStageLoaded;}
        private void OnDisable()
        {
            if(standalone!=null)standalone.StageLoaded-=StageLoaded;
            if(session!=null){session.StageLoaded-=SliceStageLoaded;session.SetExternalCharacterArt(false);}
            if(effectAdapter!=null)effectAdapter.Unbind();if(environment!=null)environment.Clear();
            if(art!=null){CharacterArtRenderer2D visual=art.CurrentRenderer;if(visual!=null){CharacterGameplayAnimator2D animator=visual.GetComponent<CharacterGameplayAnimator2D>();if(animator!=null)animator.enabled=false;visual.enabled=false;BlockoutPlayerSilhouette2D fallback=visual.GetComponent<BlockoutPlayerSilhouette2D>();if(fallback!=null)fallback.enabled=true;}art.enabled=false;}
            if(animationBinder!=null)animationBinder.enabled=false;
            configuredPlayer=null;configuredStage=null;configuredActor=null;configuredBoss=null;effectsBound=false;
        }
        private void StageLoaded(StageBlockoutDefinition stage){Build(stage);}
        private void SliceStageLoaded(StageBlockoutBuilder2D stage){if(stage!=null)Build(stage.Definition);}
        private void Build(StageBlockoutDefinition stage){if(!isActiveAndEnabled||environment==null||view==null||stage==null)return;if(environment.TryBuild(stage,view))configuredStage=stage;}
        private void LateUpdate()
        {
            CharacterManager2D owner=session!=null?session.Manager:player;StageBlockoutBuilder2D activeStage=session!=null?session.Stage:builder;
            if(owner==null||art==null||animationBinder==null)return;
            if(configuredPlayer!=owner&&art.TryConfigure(owner,characterPrefabs,characters)&&animationBinder.TryConfigure(art,rigs)){configuredPlayer=owner;effectsBound=false;}
            if(activeStage!=null&&activeStage.Definition!=configuredStage)Build(activeStage.Definition);
            if(session!=null)session.SetExternalCharacterArt(art.CurrentRenderer!=null&&art.CurrentRenderer.VisualRoot!=null);
            DamageReceiver2D actor=owner.ActiveCharacter;EnemyAgent2D boss=session!=null&&session.BossArena!=null?session.BossArena.Boss:null;
            if(effectAdapter!=null&&effects!=null&&(!effectsBound||actor!=configuredActor||boss!=configuredBoss))
            {
                effectsBound=effectAdapter.TryConfigure(effects,actor,boss,session!=null?session.Honmoon:null,session!=null?session.Special:null,drainer);configuredActor=actor;configuredBoss=boss;
            }
        }
    }
}
