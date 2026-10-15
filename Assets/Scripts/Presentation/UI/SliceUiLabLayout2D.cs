using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
            Toggle(options,"toggleJumpRescueToggle","Alternar pulo / resgate",.54f,.49f,.90f,.60f);
            Button(options,"ptLanguageButton","Português",.10f,.32f,.35f,.44f); Button(options,"enLanguageButton","English",.39f,.32f,.64f,.44f);
            Button(options,"remapKeysButton","Remapear — indisponível",.10f,.15f,.48f,.27f).interactable=false;
            Button(options,"closeButton","Voltar",.64f,.15f,.90f,.27f);
            var results=Panel("ResultsPanel",safe,"Resultado demonstrativo","Prévia ilustrativa; não representa uma partida realizada.");
            Label(results,"outcomeTitleText","Demonstração concluída",.06f,.70f,.46f,.79f); Label(results,"outcomeDetailsText","Explore as telas e volte ao HUD.",.06f,.60f,.46f,.70f);
            Label(results,"honmoonScoreText","Honmoon: 100 (exemplo)",.06f,.49f,.46f,.59f); Label(results,"fansRescuedScoreText","Fãs: 0 / 5 (exemplo)",.06f,.39f,.46f,.49f);
            Label(results,"funLabel","Diversão",.54f,.71f,.73f,.79f); Label(results,"funRatingText","3 / 5",.79f,.71f,.93f,.79f);
            Slider(results,"funRatingSlider",1,5,3,.54f,.64f,.93f,.70f,true);
            Label(results,"difficultyLabel","Dificuldade",.54f,.54f,.77f,.62f); Label(results,"difficultyRatingText","3 / 5",.79f,.54f,.93f,.62f);
            Slider(results,"difficultyRatingSlider",1,5,3,.54f,.47f,.93f,.53f,true);
            var inputRect=Rect("commentsInputField",results,.54f,.32f,.93f,.44f); inputRect.gameObject.AddComponent<Image>().color=Color.white;
            var input=inputRect.gameObject.AddComponent<InputField>(); input.targetGraphic=inputRect.GetComponent<Image>(); input.textComponent=Label(inputRect,"InputText","",.03f,.05f,.97f,.95f); input.characterLimit=500;
            var placeholder=Label(inputRect,"Placeholder","Comentários opcionais",.03f,.05f,.97f,.95f); placeholder.color=SliceUiTokens.TextSecondary; input.placeholder=placeholder;
            Button(results,"submitSurveyButton","Enviar prévia",.54f,.17f,.93f,.29f);
            Label(results,"surveyStatusText","Nenhum arquivo será salvo nesta prévia.",.50f,.04f,.97f,.15f);
            Button(results,"replayButton","Voltar ao HUD",.06f,.20f,.45f,.32f); Button(results,"mainMenuButton","Menu principal",.06f,.05f,.45f,.17f);
            var menuView=menu.gameObject.AddComponent<MainMenuUiView>(); var profileView=profile.gameObject.AddComponent<TeamProfileUiView>();
            var hudView=hud.gameObject.AddComponent<MobileHudUiView>(); var optionsView=options.gameObject.AddComponent<OptionsUiView>(); var resultsView=results.gameObject.AddComponent<ResultsReplayUiView>();
            // Activate once so every view resolves controls and binds before the controller hides screens.
            foreach(var panel in new[]{menu,profile,hud,options,results}) panel.gameObject.SetActive(true);
            var flow=root.AddComponent<SliceUiFlow2D>(); var controller=root.AddComponent<SliceUiController2D>();
            controller.Initialize(flow,menuView,profileView,hudView,optionsView,resultsView);
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
        {var r=Rect(name,parent,x,y,xx,yy);r.gameObject.AddComponent<Image>().color=SliceUiTokens.Protection;var fill=Rect("Fill",r,0,0,1,1);fill.gameObject.AddComponent<Image>().color=SliceUiTokens.PrimaryAction;var slider=r.gameObject.AddComponent<Slider>();slider.fillRect=fill;slider.minValue=min;slider.maxValue=max;slider.wholeNumbers=interactive;slider.value=value;slider.interactable=interactive;if(interactive){var handle=Rect("Handle",r,.45f,-.3f,.55f,1.3f);var image=handle.gameObject.AddComponent<Image>();image.color=SliceUiTokens.Special;slider.handleRect=handle;slider.targetGraphic=image;}return slider;}
        private void Toggle(Transform parent,string name,string caption,float x,float y,float xx,float yy)
        {var r=Rect(name,parent,x,y,xx,yy);var bg=r.gameObject.AddComponent<Image>();bg.color=SliceUiTokens.Surface;var check=Rect("Selected",r,.01f,.18f,.14f,.82f).gameObject.AddComponent<Image>();check.color=SliceUiTokens.PrimaryAction;var toggle=r.gameObject.AddComponent<Toggle>();toggle.targetGraphic=bg;toggle.graphic=check;Label(r,"Label",caption,.16f,0,1,1);}
    }
}
