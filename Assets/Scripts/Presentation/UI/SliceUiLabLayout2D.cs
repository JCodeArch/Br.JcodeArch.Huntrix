using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using HuntrX.Gameplay.AgeAccessibility;
using HuntrX.Presentation.UI.Views;

namespace HuntrX.Presentation.UI
{
    /// <summary>Self-contained demonstrator. Never loads or modifies the gameplay scene.</summary>
    [DefaultExecutionOrder(-200)]
    public sealed class SliceUiLabLayout2D : MonoBehaviour
    {
        private Font font;
        private void Awake() { if (GetComponentInChildren<Canvas>(true) == null) Build(); }

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = new GameObject("UiCanvasRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SliceUiTokens.ReferenceResolution;
            scaler.matchWidthOrHeight = .5f;
            var safe = Rect("SafeAreaContainer", root.transform, 0, 0, 1, 1);
            safe.gameObject.AddComponent<SliceSafeAreaFitter>();
            var menu = Panel("MainMenuPanel", safe, "HUNTR/X — HONMOON", "Protótipo não oficial • laboratório visual isolado");
            Button(menu,"startButton","Começar",.35f,.52f,.65f,.64f);
            Button(menu,"continueButton","Continuar — indisponível",.35f,.36f,.65f,.48f).interactable=false;
            Button(menu,"optionsButton","Opções",.35f,.20f,.65f,.32f);
            var profile=Panel("TeamProfilePanel",safe,"Sua equipe","Escolha uma personagem e um perfil de conteúdo; não há coleta de data de nascimento.");
            string[] heroes={"Rumi","Mira","Zoey"}; string[] names={"rumiButton","miraButton","zoeyButton"};
            for(int i=0;i<3;i++) Button(profile,names[i],heroes[i],.12f+i*.26f,.57f,.34f+i*.26f,.70f);
            string[] ages={"4-5","6-8","9-12","13-15","16+"}; string[] fields={"profile4To5Button","profile6To8Button","profile9To12Button","profile13To15Button","profile16PlusButton"};
            for(int i=0;i<5;i++) Button(profile,fields[i],ages[i],.08f+i*.17f,.36f,.23f+i*.17f,.48f);
            Label(profile,"selectionText","Selecionado: Rumi • 9-12",.12f,.24f,.88f,.33f);
            Button(profile,"backButton","Voltar",.12f,.08f,.35f,.20f);
            Button(profile,"confirmButton","Abrir demonstração",.59f,.08f,.88f,.20f);
            var hud=Panel("HudPanel",safe,"Demonstração do HUD","Valores ilustrativos • controles sem ação no jogo");
            Slider(hud,"healthBar",0,100,100,.03f,.66f,.27f,.72f,false); Label(hud,"healthText","100 / 100",.03f,.72f,.27f,.79f);
            Slider(hud,"honmoonBar",0,100,100,.03f,.53f,.27f,.59f,false); Label(hud,"honmoonText","Honmoon: 100",.03f,.59f,.27f,.66f);
            Label(hud,"activeCharacterText","Rumi",.38f,.70f,.60f,.78f); Button(hud,"switchButton","Trocar",.38f,.58f,.60f,.70f);
            Button(hud,"pauseButton","Pausa / opções",.74f,.67f,.97f,.79f);
            Label(hud,"beatTitleText","Palco demonstrativo",.32f,.46f,.70f,.55f);
            Label(hud,"fansRescuedText","Fãs: 0 / 5",.03f,.43f,.27f,.50f); Label(hud,"foodCountText","Comida: 3",.03f,.36f,.27f,.43f);
            Label(hud,"movementZone","MOVIMENTO\n←  ↑  ↓  →\nPrévia visual",.03f,.08f,.28f,.31f);
            string[] acts={"jumpButton","attackButton","dashButton","parryButton","specialButton","rescueButton","foodSelfButton","foodFanButton"};
            string[] captions={"Pular","Atacar","Dash","Defender","Especial","Resgatar","Comer","Alimentar fã"};
            for(int i=0;i<8;i++) Button(hud,acts[i],captions[i],.33f+(i%4)*.16f,.08f+(i/4)*.15f,.48f+(i%4)*.16f,.21f+(i/4)*.15f);
            Slider(hud,"specialChargeBar",0,1,0,.74f,.53f,.97f,.58f,false);
            var glow=Rect("specialReadyGlow",hud,.74f,.59f,.97f,.62f).gameObject.AddComponent<Image>(); glow.color=SliceUiTokens.Special;
            Button(hud,"finishPreviewButton","Ver resultado demonstrativo",.67f,.39f,.97f,.51f);
            var options=Panel("OptionsPanel",safe,"Opções e acessibilidade","Preferências apenas nesta prévia; sem persistência e sem alterar gameplay.");
            Toggle(options,"reduceFlashesToggle","Reduzir flashes",.10f,.64f,.46f,.75f);
            Toggle(options,"subtitlesToggle","Legendas",.54f,.64f,.90f,.75f);
            Toggle(options,"aimAssistToggle","Assistência de mira",.10f,.49f,.46f,.60f);
            Toggle(options,"toggleJumpToggle","Pulo: alternar",.54f,.49f,.90f,.60f);
            Toggle(options,"toggleRescueToggle","Resgate: alternar",.54f,.34f,.90f,.45f);
            Button(options,"ptLanguageButton","Português",.10f,.32f,.28f,.44f); Button(options,"enLanguageButton","English",.30f,.32f,.48f,.44f);
            Label(options,"languageStatusText","Idioma selecionado: Português",.10f,.27f,.90f,.32f,18);
            Button(options,"remapKeysButton","Remapear teclas",.10f,.15f,.48f,.27f);
            Button(options,"closeButton","Voltar",.64f,.15f,.90f,.27f);
            var results=Panel("ResultsPanel",safe,"Resultado demonstrativo","Prévia ilustrativa; não representa uma partida realizada.");
            Label(results,"outcomeTitleText","Demonstração concluída",.06f,.70f,.46f,.79f); Label(results,"outcomeDetailsText","Explore as telas e volte ao HUD.",.06f,.60f,.46f,.70f);
            Label(results,"honmoonScoreText","Honmoon: 100 (exemplo)",.06f,.49f,.46f,.59f); Label(results,"fansRescuedScoreText","Fãs: 0 / 5 (exemplo)",.06f,.39f,.46f,.49f);
            Label(results,"funLabel","Diversão",.54f,.71f,.73f,.79f); Label(results,"funRatingText","3 / 5",.79f,.71f,.93f,.79f);
            // Keep survey targets above the 64-unit minimum at the 1280x720 reference.
            Slider(results,"funRatingSlider",1,5,3,.54f,.62f,.93f,.71f,true);
            Label(results,"difficultyLabel","Dificuldade",.54f,.54f,.77f,.62f); Label(results,"difficultyRatingText","3 / 5",.79f,.54f,.93f,.62f);
            Slider(results,"difficultyRatingSlider",1,5,3,.54f,.44f,.93f,.53f,true);
            var inputRect=Rect("commentsInputField",results,.54f,.32f,.93f,.44f); inputRect.gameObject.AddComponent<Image>().color=Color.white;
            var input=inputRect.gameObject.AddComponent<InputField>(); input.targetGraphic=inputRect.GetComponent<Image>(); input.textComponent=Label(inputRect,"InputText","",.03f,.05f,.97f,.95f); input.characterLimit=500;
            var placeholder=Label(inputRect,"Placeholder","Comentários opcionais",.03f,.05f,.97f,.95f); placeholder.color=SliceUiTokens.TextSecondary; input.placeholder=placeholder;
            Button(results,"submitSurveyButton","Enviar prévia",.54f,.17f,.93f,.29f);
            Label(results,"surveyStatusText","Nenhum arquivo será salvo nesta prévia.",.50f,.04f,.97f,.15f);
            Button(results,"replayButton","Voltar ao HUD",.06f,.20f,.45f,.32f); Button(results,"mainMenuButton","Menu principal",.06f,.05f,.45f,.17f);
            var remap=Panel("RemapPanel",safe,"Remapear teclas","Ajustes valem apenas nesta prévia.");
            Find<Text>(remap,"Title").name="titleText";
            var actionRect=Rect("actionDropdown",remap,.12f,.59f,.88f,.69f); var actionImage=actionRect.gameObject.AddComponent<Image>(); actionImage.color=SliceUiTokens.Surface;
            var actionDropdown=actionRect.gameObject.AddComponent<Dropdown>(); actionDropdown.targetGraphic=actionImage; actionDropdown.captionText=Label(actionRect,"Caption", "Ação: Attack",.04f,0,.96f,1,18);
            var template=Rect("Template",actionRect,0,-3,1,-1); var templateImage=template.gameObject.AddComponent<Image>(); templateImage.color=SliceUiTokens.Surface;
            var scroll=template.gameObject.AddComponent<ScrollRect>(); var viewport=Rect("Viewport",template,0,0,1,1); var viewportImage=viewport.gameObject.AddComponent<Image>(); var mask=viewport.gameObject.AddComponent<Mask>(); mask.showMaskGraphic=false;
            var content=Rect("Content",viewport,0,0,1,1); var item=Rect("Item",content,0,0,1,1); var itemImage=item.gameObject.AddComponent<Image>(); var itemToggle=item.gameObject.AddComponent<Toggle>(); itemToggle.targetGraphic=itemImage;
            var itemLabel=Label(item,"Item Label","Attack",.04f,0,.96f,1,18); scroll.viewport=viewport; scroll.content=content; actionDropdown.template=template; actionDropdown.itemText=itemLabel; template.gameObject.SetActive(false);
            Label(remap,"currentBindingText","J",.12f,.50f,.88f,.57f,18);
            var keyRect=Rect("keyInputField",remap,.12f,.37f,.88f,.47f); var keyImage=keyRect.gameObject.AddComponent<Image>(); keyImage.color=Color.white;
            var keyInput=keyRect.gameObject.AddComponent<InputField>(); keyInput.targetGraphic=keyImage; keyInput.textComponent=Label(keyRect,"InputText","",.04f,.05f,.96f,.95f,18);
            var keyPlaceholder=Label(keyRect,"Placeholder","Digite uma tecla",.04f,.05f,.96f,.95f,18); keyPlaceholder.color=SliceUiTokens.TextSecondary; keyInput.placeholder=keyPlaceholder;
            Button(remap,"applyButton","Aplicar",.12f,.24f,.37f,.33f); Button(remap,"restoreButton","Restaurar padrão",.42f,.24f,.88f,.33f);
            Label(remap,"helpText","",.12f,.16f,.88f,.22f,16); Label(remap,"statusText","",.12f,.09f,.88f,.15f,16);
            Button(remap,"remapBackButton","Voltar às opções",.12f,.02f,.88f,.08f);
            var menuView=menu.gameObject.AddComponent<MainMenuUiView>(); var profileView=profile.gameObject.AddComponent<TeamProfileUiView>();
            var hudView=hud.gameObject.AddComponent<MobileHudUiView>(); var optionsView=options.gameObject.AddComponent<OptionsUiView>(); var resultsView=results.gameObject.AddComponent<ResultsReplayUiView>(); var remapView=remap.gameObject.AddComponent<RemapPanelUiView>();
            var preferences=root.gameObject.AddComponent<AccessibilityPreferences2D>();
            // Activate once so every view resolves controls and binds before the controller hides screens.
            foreach(var panel in new[]{menu,profile,hud,options,results,remap}) panel.gameObject.SetActive(true);
            remapView.BindPreferences(preferences);
            optionsView.SetPreferences(preferences.ReduceFlashes, preferences.CaptionsEnabled, preferences.AssistedAim,
                preferences.ToggleModeFor(HuntrX.Gameplay.VerticalSlice.GameplayControl.Jump),
                preferences.ToggleModeFor(HuntrX.Gameplay.VerticalSlice.GameplayControl.Interact), "PT");
            optionsView.ReduceFlashesChanged += value => preferences.SetPresentationPreferences(value, preferences.CaptionsEnabled, preferences.AssistedAim);
            optionsView.SubtitlesChanged += value => preferences.SetPresentationPreferences(preferences.ReduceFlashes, value, preferences.AssistedAim);
            optionsView.AimAssistChanged += value => preferences.SetPresentationPreferences(preferences.ReduceFlashes, preferences.CaptionsEnabled, value);
            optionsView.ToggleJumpChanged += value => preferences.TrySetToggle(HuntrX.Gameplay.VerticalSlice.GameplayControl.Jump, value);
            optionsView.ToggleRescueChanged += value => preferences.TrySetToggle(HuntrX.Gameplay.VerticalSlice.GameplayControl.Interact, value);
            var flow=root.AddComponent<SliceUiFlow2D>(); var controller=root.AddComponent<SliceUiController2D>();
            controller.Initialize(flow,menuView,profileView,hudView,optionsView,resultsView,remapView);
            Find<Button>(hud,"finishPreviewButton").onClick.AddListener(()=>flow.TransitionTo(UiScreen.ResultsReplay));
            UpdateSelection(profile,profileView,hudView);
            profileView.CharacterSelected += _ => UpdateSelection(profile,profileView,hudView);
            profileView.AgeProfileSelected += _ => UpdateSelection(profile,profileView,hudView);
            resultsView.SurveySubmitted += (fun,difficulty,comment)=>resultsView.SetSurveyStatus("Prévia recebida nesta sessão; não salva.");
            if(EventSystem.current==null) { var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); es.transform.SetParent(transform,false); }
        }
        private void UpdateSelection(Transform panel,TeamProfileUiView view,MobileHudUiView hud)
        {
            Find<Text>(panel,"selectionText").text=$"Selecionado: {view.SelectedCharacter} • {view.SelectedAgeProfile}";
            var state=SliceUiState2D.CreateDefault(); state.ActiveCharacter=view.SelectedCharacter; state.SelectedAgeProfile=view.SelectedAgeProfile; state.FoodCount=3; state.FansTotal=5; state.BeatTitle="Palco demonstrativo"; hud.RenderState(state);
        }
        private static T Find<T>(Transform parent,string name) where T:Component { foreach(var item in parent.GetComponentsInChildren<T>(true)) if(item.name==name)return item; return null; }
        private RectTransform Panel(string name,Transform parent,string title,string subtitle)
        {
            var panel=Rect(name,parent,0,0,1,1); panel.gameObject.SetActive(false); panel.gameObject.AddComponent<Image>().color=SliceUiTokens.Background;
            Label(panel,"Title",title,.04f,.86f,.96f,.97f,36); Label(panel,"Subtitle",subtitle,.04f,.79f,.96f,.86f,18); return panel;
        }
        private static RectTransform Rect(string name,Transform parent,float x,float y,float xx,float yy)
        {
            var go=new GameObject(name,typeof(RectTransform)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false); rect.anchorMin=new Vector2(x,y); rect.anchorMax=new Vector2(xx,yy); rect.offsetMin=rect.offsetMax=Vector2.zero; return rect;
        }
        private Text Label(Transform parent,string name,string text,float x,float y,float xx,float yy,int size=22)
        {var r=Rect(name,parent,x,y,xx,yy);var label=r.gameObject.AddComponent<Text>();label.font=font;label.text=text;label.fontSize=size;label.color=SliceUiTokens.TextPrimary;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;label.resizeTextForBestFit=true;label.resizeTextMinSize=14;label.resizeTextMaxSize=size;return label;}
        private Button Button(Transform parent,string name,string caption,float x,float y,float xx,float yy)
        {var r=Rect(name,parent,x,y,xx,yy);var image=r.gameObject.AddComponent<Image>();image.color=SliceUiTokens.PrimaryAction;var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;var label=Label(r,"Label",caption,.03f,0,.97f,1);label.color=Color.white;return button;}
        private Slider Slider(Transform parent,string name,float min,float max,float value,float x,float y,float xx,float yy,bool interactive)
        {var r=Rect(name,parent,x,y,xx,yy);r.gameObject.AddComponent<Image>().color=SliceUiTokens.Protection;var fill=Rect("Fill",r,0,0,1,1);fill.gameObject.AddComponent<Image>().color=SliceUiTokens.PrimaryAction;var slider=r.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.minValue=min;slider.maxValue=max;slider.wholeNumbers=interactive;slider.value=value;slider.interactable=interactive;if(interactive){var handle=Rect("Handle",r,.45f,0,.55f,1);var image=handle.gameObject.AddComponent<Image>();image.color=SliceUiTokens.Special;slider.handleRect=handle;slider.targetGraphic=image;}return slider;}
        private void Toggle(Transform parent,string name,string caption,float x,float y,float xx,float yy)
        {var r=Rect(name,parent,x,y,xx,yy);var bg=r.gameObject.AddComponent<Image>();bg.color=SliceUiTokens.Surface;var check=Rect("Selected",r,.01f,.18f,.14f,.82f).gameObject.AddComponent<Image>();check.color=SliceUiTokens.PrimaryAction;var toggle=r.gameObject.AddComponent<Toggle>();toggle.targetGraphic=bg;toggle.graphic=check;Label(r,"Label",caption,.16f,0,1,1);}
    }
}
