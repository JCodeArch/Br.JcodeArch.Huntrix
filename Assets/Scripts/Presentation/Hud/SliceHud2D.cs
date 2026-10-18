using HuntrX.Data;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Gameplay.Characters;
using HuntrX.Gameplay.Enemies;
using HuntrX.Gameplay.Touch;
using HuntrX.Gameplay.Narrative;
using HuntrX.Gameplay.Combat;
using HuntrX.Presentation.Art;
using HuntrX.Gameplay.VerticalSlice;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
namespace HuntrX.Presentation.Hud
{
    [DefaultExecutionOrder(-100), DisallowMultipleComponent]
    public sealed class SliceHud2D : MonoBehaviour
    {
        [SerializeField] private VerticalSliceSession2D session;
        [SerializeField] private SlicePlaytestRecorder2D recorder;
        [SerializeField] private AccessibilityPreferences2D preferences;
        [SerializeField] private AgeProfileSelection2D ageSelection;
        [SerializeField] private AgeProfileScope2D ageScope;
        [SerializeField] private AgeProfileData[] ageOptions;
        [SerializeField] private SliceTouchControls2D touch;
        [SerializeField] private SliceHordeSpecialBeat2D hordeBeat;
        [SerializeField] private SliceNarrativeComposition2D narrative;
        [SerializeField] private GameplayVfxAdapter2D effects;
        private CharacterManager2D visualManager;
        private DamageReceiver2D visualActor;
        private AgeProfileSelection2D visualProfile;
        private AgeVisualFeedback2D ageVisual;
        private bool english, narrativeOwned, priorNarrativeBlocked;
        private DialoguePresenter2D blockedPresenter;
        private CharacterManager2D boundManager, scopedManager;
        private HordeController2D boundHorde;
        private bool options, pauseOwned, inputOwned, priorInputEnabled;
        private HitStopService pauseService;
        private bool priorServicePaused;
        private SlicePlayerInput2D blockedInput;
        private Vector2 optionsScroll, feedbackScroll;
        private string key="space", comments=string.Empty, feedback=string.Empty;
        private GameplayControl selected=GameplayControl.Jump;
        private int fun=3, difficulty=3;
        // Menu input must remain enabled while the gameplay component is disabled by options.
        private InputAction pauseAction;
        private string pausePath;
        private Behaviour telemetryPresenter;
        public bool IsOptionsOpen => options;
        public VerticalSliceSession2D Session => session;
        public bool TelemetryPresentedExternally => telemetryPresenter != null && telemetryPresenter.isActiveAndEnabled;
        public bool TryClaimTelemetry(Behaviour presenter)
        {
            if (!isActiveAndEnabled || presenter == null || !presenter.isActiveAndEnabled || (telemetryPresenter != null && telemetryPresenter != presenter)) return false;
            telemetryPresenter = presenter;
            return true;
        }
        public void ReleaseTelemetry(Behaviour presenter)
        {
            if (telemetryPresenter == presenter) telemetryPresenter = null;
        }
        private void OnEnable()
        {
            if(pauseAction==null)
            {
                pauseAction=new InputAction("PauseOptions",InputActionType.Button);
                pauseAction.AddBinding("<Keyboard>/escape");
                pauseAction.AddBinding("<Gamepad>/start");
            }
            pausePath=null;
            pauseAction.Enable();
        }
        private bool PausePressed()
        {
            string next=preferences!=null && preferences.isActiveAndEnabled?preferences.KeyboardBinding(GameplayControl.Pause):null;
            if(string.IsNullOrWhiteSpace(next)) next="<Keyboard>/escape";
            if(next!=pausePath) { pauseAction.ApplyBindingOverride(0,next); pausePath=next; }
            return pauseAction.WasPressedThisFrame();
        }
        private void OnDestroy() { pauseAction?.Dispose(); }
        private void Update()
        {
            if(PausePressed()) options=!options;
            SetPause(options);
            SetInputBlocked(options || (session != null && session.IsComplete));
            SetNarrativeBlocked(options);
            RefreshVisualSource();
            if(narrative != null && narrative.Presenter != null) { narrative.Presenter.SetEnglish(english); narrative.Presenter.SetCaptionsEnabled(preferences == null || preferences.CaptionsEnabled); }
            if(session==null || session.Manager==null || !session.Manager.isActiveAndEnabled) return;
            if(boundManager!=session.Manager)
            {
                if(preferences==null || !preferences.TryBindManager(session.Manager) || !session.Input.BindPreferences(preferences)) return;
                if(touch!=null && !touch.TryBind(session,preferences)) return;
                boundManager=session.Manager;
            }
            HordeController2D horde=hordeBeat!=null?hordeBeat.CurrentHorde:null;
            if(ageScope!=null && (boundHorde!=horde || scopedManager!=boundManager))
            { if(ageScope.TryBind(boundManager,horde)) { boundHorde=horde; scopedManager=boundManager; } }
        }
        private void OnGUI()
        {
            if(session==null) return;
            Rect safe=Screen.safeArea; float width=safe.width, height=safe.height;
            Matrix4x4 previous=GUI.matrix;
            int labelFont=GUI.skin.label.fontSize, buttonFont=GUI.skin.button.fontSize, textFont=GUI.skin.textField.fontSize, toggleFont=GUI.skin.toggle.fontSize;
            GUI.skin.label.fontSize=16; GUI.skin.button.fontSize=16; GUI.skin.textField.fontSize=16; GUI.skin.toggle.fontSize=16;
            GUI.matrix=Matrix4x4.TRS(new Vector3(safe.xMin,Screen.height-safe.yMax,0),Quaternion.identity,Vector3.one);
            try
            {
                if(options) { DrawOptions(width,height); return; }
                GUILayout.BeginArea(new Rect(8,8,Mathf.Min(620,width-16),180),GUI.skin.box);
                if (!TelemetryPresentedExternally)
                {
                GUILayout.Label(session.IsComplete?"Chegamos ao show!":session.BeatTitle);
                var actor=session.Manager!=null?session.Manager.ActiveCharacter:null;
                if(actor!=null) GUILayout.Label("Vida "+Mathf.CeilToInt(actor.CurrentHealth)+" / "+Mathf.CeilToInt(actor.MaximumHealth));
                if(session.Honmoon!=null) GUILayout.Label("Honmoon "+Mathf.RoundToInt(session.Honmoon.Value));
                if(session.Special!=null) GUILayout.Label("Special "+(session.Special.IsReady?"pronto":"carregando "+Mathf.RoundToInt(session.Special.Charge)));
                if(session.Inventory!=null) GUILayout.Label("Comida "+session.Inventory.TotalCount+" • R: você • F: fã");
                }
                if(GUILayout.Button("Opções e acessibilidade")) { options=true; SetPause(true); SetInputBlocked(true); SetNarrativeBlocked(true); }
                if(session.IsComplete && recorder==null && GUILayout.Button("Jogar novamente")) RestartCompletedSession();
                GUILayout.EndArea();
                if(session.IsComplete && recorder!=null) DrawFeedback(width,height);
            }
            finally { GUI.matrix=previous; GUI.skin.label.fontSize=labelFont; GUI.skin.button.fontSize=buttonFont; GUI.skin.textField.fontSize=textFont; GUI.skin.toggle.fontSize=toggleFont; }
        }
        private void RefreshVisualSource()
        {
            CharacterManager2D current=session!=null?session.Manager:null;
            if(current!=null && !current.isActiveAndEnabled) current=null;
            if(visualManager!=current)
            {
                effects?.ClearActiveEffects();
                UnbindVisualSource(); visualManager=current;
                if(current!=null) { current.ActiveCharacterChanged+=VisualActorChanged; BindVisualActor(current.ActiveCharacter); }
            }
            if(visualManager!=null && visualActor!=visualManager.ActiveCharacter) BindVisualActor(visualManager.ActiveCharacter);
            ApplyVisualPolicy();
        }
        private void VisualActorChanged(DamageReceiver2D previous,DamageReceiver2D current) { BindVisualActor(current); ApplyVisualPolicy(); }
        private void BindVisualActor(DamageReceiver2D actor)
        {
            if(visualProfile!=null) visualProfile.ProfileChanged-=VisualProfileChanged;
            visualActor=actor; visualProfile=actor!=null?actor.GetComponent<AgeProfileSelection2D>():null;
            ageVisual=actor!=null?actor.GetComponent<AgeVisualFeedback2D>():null;
            if(visualProfile!=null) visualProfile.ProfileChanged+=VisualProfileChanged;
            effects?.ClearActiveEffects();
        }
        private void VisualProfileChanged(AgeProfileData value) { effects?.ClearActiveEffects(); ApplyVisualPolicy(); }
        private void ApplyVisualPolicy()
        {
            if(effects==null) return;
            AgeProfileData profile=visualProfile!=null?visualProfile.Current:null;
            bool valid=isActiveAndEnabled && visualManager!=null && visualManager.isActiveAndEnabled &&
                visualActor!=null && visualActor.isActiveAndEnabled && visualActor.IsAlive && visualManager.ActiveCharacter==visualActor &&
                visualProfile!=null && visualProfile.isActiveAndEnabled && profile!=null && profile.IsValid(out _) &&
                ageSelection!=null && ageSelection.isActiveAndEnabled && ageSelection.Current==profile;
            bool reduced=preferences==null || !preferences.isActiveAndEnabled || preferences.ReduceFlashes ||
                (ageVisual!=null && ageVisual.isActiveAndEnabled && ageVisual.IsReducedFlashes);
            effects.SetPresentationPolicy(valid?profile.VisualIntensity:0f,reduced,valid);
        }
        private void UnbindVisualSource()
        {
            if(visualManager!=null) visualManager.ActiveCharacterChanged-=VisualActorChanged;
            if(visualProfile!=null) visualProfile.ProfileChanged-=VisualProfileChanged;
            visualManager=null; visualActor=null; visualProfile=null; ageVisual=null;
        }
        private void SetNarrativeBlocked(bool value)
        {
            DialoguePresenter2D current=narrative!=null?narrative.Presenter:null;
            if(narrativeOwned && (!value || blockedPresenter!=current))
            { if(blockedPresenter!=null) blockedPresenter.SetUiBlocked(priorNarrativeBlocked); narrativeOwned=false; blockedPresenter=null; }
            if(value && !narrativeOwned && current!=null)
            { blockedPresenter=current; priorNarrativeBlocked=current.IsUiBlocked; narrativeOwned=true; current.SetUiBlocked(true); }
        }
        private void SetPause(bool value)
        {
            if(value && !pauseOwned)
            {
                pauseService=HitStopService.EnsureInstance();
                priorServicePaused=pauseService.IsPaused;
                pauseService.SetPaused(true);
                pauseOwned=true;
            }
            else if(!value && pauseOwned)
            {
                if(pauseService!=null && !priorServicePaused) pauseService.SetPaused(false);
                pauseService=null;
                pauseOwned=false;
            }
        }
        private void SetInputBlocked(bool value)
        {
            SlicePlayerInput2D current=session!=null?session.Input:null;
            if(inputOwned && (!value || blockedInput!=current)) { if(blockedInput!=null) blockedInput.enabled=priorInputEnabled; inputOwned=false; blockedInput=null; }
            if(value && !inputOwned && current!=null) { blockedInput=current; priorInputEnabled=current.enabled; inputOwned=true; current.enabled=false; }
        }
        private void OnDisable() { pauseAction?.Disable(); options=false; SetPause(false); SetInputBlocked(false); SetNarrativeBlocked(false); UnbindVisualSource(); effects?.SetPresentationPolicy(0f,true,false); }
        private void DrawOptions(float width,float height)
        {
            GUILayout.BeginArea(new Rect(8,8,Mathf.Min(430,width-16),height-16),GUI.skin.box);
            optionsScroll=GUILayout.BeginScrollView(optionsScroll);
            if(GUILayout.Button("Voltar ao jogo")) { options=false; SetPause(false); SetInputBlocked(session.IsComplete); SetNarrativeBlocked(false); }
            if(ageSelection!=null && ageOptions!=null && ageOptions.Length>0 && ageOptions.Length<=5)
            {
                GUILayout.Label("Faixa etária: "+(ageSelection.Current!=null?ageSelection.Current.Band.ToString():"Escolher"));
                if(GUILayout.Button("Trocar faixa etária")) { int index=System.Array.IndexOf(ageOptions,ageSelection.Current); ageSelection.TrySelect(ageOptions[(index+1)%ageOptions.Length]); }
            }
            if(preferences!=null)
            {
                bool flashes=GUILayout.Toggle(preferences.ReduceFlashes,"Reduzir flashes"), captions=GUILayout.Toggle(preferences.CaptionsEnabled,"Legendas"), aim=GUILayout.Toggle(preferences.AssistedAim,"Ajuda na mira");
                preferences.SetPresentationPreferences(flashes,captions,aim);
                ApplyVisualPolicy();
                preferences.TrySetToggle(GameplayControl.Jump,GUILayout.Toggle(preferences.ToggleModeFor(GameplayControl.Jump),"Pulo: alternar"));
                preferences.TrySetToggle(GameplayControl.Interact,GUILayout.Toggle(preferences.ToggleModeFor(GameplayControl.Interact),"Resgate: alternar"));
                if(GUILayout.Button("Ação: "+selected)) selected=selected==GameplayControl.Pause?GameplayControl.MoveLeft:(GameplayControl)((int)selected+1);
                key=GUILayout.TextField(key,24);
                if(GUILayout.Button("Aplicar tecla")) feedback=preferences.TryRemap(selected,"<Keyboard>/"+key)?"Tecla atualizada":"Tecla inválida";
                if(GUILayout.Button("Restaurar tecla")) {preferences.ClearRemap(selected);feedback="Tecla restaurada";}
            }
            english=GUILayout.Toggle(english,"Narrativa em inglês");
            GUILayout.Label("A/D: mover • Espaço: pulo • Shift: dash\nJ: ataque • K: parry • Q: troca\nE: resgate • L: Special\nR: comida (você) • F: comida (fã)\nEsc / Start: pausar e opções");
            GUILayout.Space(6);
            GUILayout.Label("Navegação de Fases (Seul):");
            string[] stageScenes = { "Aviao_Blockout", "Show_Blockout", "Cheongdam_Blockout", "Myeongdong_Blockout", "COEX_Blockout", "Hanok_Blockout", "Naksan_Blockout", "Namsan_Blockout", "Awards_Blockout", "UiLayoutLab" };
            string[] stageLabels = { "1. Avião (Abertura)", "2. Show (Estádio)", "3. Cheongdam", "4. Myeongdong", "5. COEX Square", "6. Bukchon Hanok", "7. Naksan Park", "8. Namsan Tower", "9. Idol Awards", "Menu / Telas HD" };
            for (int i = 0; i < stageScenes.Length; i++)
            {
                if (GUILayout.Button(stageLabels[i]))
                {
                    options = false; SetPause(false); SetInputBlocked(false); SetNarrativeBlocked(false);
                    SceneManager.LoadScene(stageScenes[i]);
                }
            }
            if(!string.IsNullOrEmpty(feedback)) GUILayout.Label(feedback);
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        private void DrawFeedback(float width,float height)
        {
            GUILayout.BeginArea(new Rect(8,190,Mathf.Min(480,width-16),Mathf.Max(40,height-198)),GUI.skin.box);
            feedbackScroll=GUILayout.BeginScrollView(feedbackScroll);
            GUILayout.Label("Como foi jogar?");
            GUILayout.Label("Diversão: "+fun); fun=Mathf.RoundToInt(GUILayout.HorizontalSlider(fun,1,5));
            GUILayout.Label("Dificuldade: "+difficulty); difficulty=Mathf.RoundToInt(GUILayout.HorizontalSlider(difficulty,1,5));
            comments=GUILayout.TextArea(comments,1000,GUILayout.Height(90));
            if(GUILayout.Button("Salvar avaliação desta sessão"))
            {
                feedback=recorder.TrySubmitFeedback(fun,difficulty,comments) && recorder.TrySaveReport(out _)?"Avaliação salva":"Não foi possível salvar";
            }
            if(GUILayout.Button("Salvar avaliação e jogar novamente")) RestartCompletedSession();
            if(recorder.Report!=null) GUILayout.Label(recorder.Report.CompleteWindow?"Registro de 60 segundos completo":"Registro ainda incompleto");
            if(!string.IsNullOrEmpty(feedback)) GUILayout.Label(feedback);
            GUILayout.EndScrollView(); GUILayout.EndArea();
        }
        private void RestartCompletedSession()
        {
            if(session==null || !session.IsComplete) return;
            Scene scene=gameObject.scene;
            if(!scene.IsValid() || scene.buildIndex<0) { feedback="Cena indisponível para reiniciar"; return; }
            if(recorder!=null && (!recorder.TrySubmitFeedback(fun,difficulty,comments) || !recorder.TrySaveReport(out _)))
            { feedback="Não foi possível salvar. Sua avaliação foi mantida; tente novamente"; return; }
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
