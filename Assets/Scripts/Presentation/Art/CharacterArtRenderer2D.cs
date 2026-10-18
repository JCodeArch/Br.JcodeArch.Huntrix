using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    /// <summary>Original geometric prototype rig. Does not change physics or combat.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterArtRenderer2D : MonoBehaviour
    {
        [SerializeField] private CharacterArtDefinition definition;
        private Texture2D texture;
        private Sprite sprite;
        private bool ownsSprite;
        private Sprite characterArtSprite;
        private bool ownsCharacterArtSprite;
        private SpriteRenderer hdSpriteRenderer;
        private readonly System.Collections.Generic.List<Sprite> ownedPoseSprites = new System.Collections.Generic.List<Sprite>(64);
        private Sprite[][] poseFrames;
        public CharacterArtDefinition Definition => definition;
        public Transform VisualRoot { get; private set; }
        public Transform LeftLeg { get; private set; }
        public Transform RightLeg { get; private set; }
        public Transform LeftArm { get; private set; }
        public Transform RightArm { get; private set; }
        public Transform Weapon { get; private set; }
        public bool TryConfigure(CharacterArtDefinition profile)
        {
            if (!isActiveAndEnabled || profile==null || !profile.IsValid) return false;
            Clear(); definition=profile;
            sprite=Resources.Load<Sprite>("Art/SharedQuad");ownsSprite=false;
            if(sprite==null)
            {
                texture=new Texture2D(1,1);texture.SetPixel(0,0,Color.white);texture.Apply();
                sprite=Sprite.Create(texture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);ownsSprite=true;
            }
            VisualRoot=new GameObject("Original_"+profile.CharacterName+"_Rig").transform;
            VisualRoot.SetParent(transform,false);
            float h=profile.Height,w=profile.Width;
            Part("Torso",new Vector2(0,.04f*h),new Vector2(w,.42f*h),profile.Primary,11);
            Part("HairBack",new Vector2(-.03f,.34f*h-profile.HairLength*.5f),new Vector2(w*.8f,.2f*h+profile.HairLength),profile.Accent,10);
            Part("Head",new Vector2(0,.34f*h),new Vector2(w*.64f,.24f*h),profile.Skin,12);
            Part("HairTop",new Vector2(-.03f,.46f*h),new Vector2(w*.8f,.07f*h),profile.Accent,13);
            Part("Eyes",new Vector2(w*.15f,.36f*h),new Vector2(w*.2f,.025f*h),Color.white,14);
            Part("Belt",new Vector2(0,-.16f*h),new Vector2(w*1.06f,.04f*h),profile.Accent,13);
            LeftLeg=Part("Leg_L",new Vector2(-w*.25f,-.32f*h),new Vector2(w*.34f,.34f*h),profile.Primary*.7f,11);
            RightLeg=Part("Leg_R",new Vector2(w*.25f,-.32f*h),new Vector2(w*.34f,.34f*h),profile.Primary*.85f,12);
            LeftArm=Part("Arm_L",new Vector2(-w*.66f,.03f*h),new Vector2(w*.25f,.35f*h),profile.Skin,11);
            RightArm=Part("Arm_R",new Vector2(w*.66f,.03f*h),new Vector2(w*.25f,.35f*h),profile.Skin,13);
            Vector2 weaponSize=new Vector2(.08f,.65f*h);
            if(profile.Weapon==CharacterWeaponShape.Polearm) weaponSize=new Vector2(.06f,1.25f*h);
            if(profile.Weapon==CharacterWeaponShape.TwinStars) weaponSize=new Vector2(.25f,.25f);
            if(profile.Weapon==CharacterWeaponShape.Microphone) weaponSize=new Vector2(.13f,.28f);
            if(profile.Weapon==CharacterWeaponShape.Staff) weaponSize=new Vector2(.09f,.9f*h);
            if(profile.Weapon==CharacterWeaponShape.Fans) weaponSize=new Vector2(.38f,.18f);
            Weapon=Part("Weapon",new Vector2(w*.95f,.04f*h),weaponSize,profile.Accent,14);
            if(profile.Weapon==CharacterWeaponShape.TwinStars) { Weapon.localRotation=Quaternion.Euler(0,0,45); Part("TwinStar",new Vector2(-w*.9f,.05f*h),weaponSize,profile.Accent,14).localRotation=Quaternion.Euler(0,0,45); }
            if(profile.Weapon==CharacterWeaponShape.Horns) { Weapon.gameObject.SetActive(false); Part("Horn_L",new Vector2(-w*.32f,.56f*h),new Vector2(.12f,.28f),profile.Accent,14).localRotation=Quaternion.Euler(0,0,25); Part("Horn_R",new Vector2(w*.32f,.56f*h),new Vector2(.12f,.28f),profile.Accent,14).localRotation=Quaternion.Euler(0,0,-25); }
            TryAttachHighResCharacterSprite(profile.CharacterName, h);
            return true;
        }
        public void UpdatePoseFrame(CharacterVisualPose pose, float clock, float cycleSeconds)
        {
            if (hdSpriteRenderer == null || definition == null) return;
            if (poseFrames == null) poseFrames = new Sprite[10][];
            int pIndex = (int)pose;
            if (pIndex < 0 || pIndex >= poseFrames.Length) return;
            Sprite[] frames = poseFrames[pIndex];
            if (frames == null)
            {
                frames = LoadPoseFrames(definition.CharacterName, pose.ToString());
                poseFrames[pIndex] = frames;
            }
            if (frames.Length == 0)
            {
                if (characterArtSprite != null) hdSpriteRenderer.sprite = characterArtSprite;
                return;
            }
            float period = Mathf.Max(0.15f, cycleSeconds);
            int idx = Mathf.Abs(Mathf.FloorToInt((clock / period) * frames.Length)) % frames.Length;
            hdSpriteRenderer.sprite = frames[idx];
        }
        private Sprite[] LoadPoseFrames(string characterName, string poseName)
        {
            string key = characterName == "Gwi-Ma" ? "GwiMa" : characterName;
            var list = new System.Collections.Generic.List<Sprite>(8);
            for (int i = 0; i < 12; i++)
            {
                string path = "Art/Animations/" + key + "/" + poseName + "/" + i;
                Sprite s = Resources.Load<Sprite>(path);
                if (s == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>(path);
                    if (tex != null)
                    {
                        s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                        ownedPoseSprites.Add(s);
                    }
                }
                if (s == null) break;
                list.Add(s);
            }
            return list.ToArray();
        }
        private void TryAttachHighResCharacterSprite(string characterName, float targetHeight)
        {
            string key = characterName == "Gwi-Ma" ? "GwiMa" : characterName;
            characterArtSprite = Resources.Load<Sprite>("Art/Characters/" + key);
            ownsCharacterArtSprite = false;
            if (characterArtSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("Art/Characters/" + key);
                if (tex != null)
                {
                    characterArtSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                    ownsCharacterArtSprite = true;
                }
            }
            if (characterArtSprite == null) return;
            GameObject artObj = new GameObject("HD_CharacterSprite");
            artObj.transform.SetParent(VisualRoot, false);
            artObj.transform.localPosition = Vector3.zero;
            float spriteWorldH = characterArtSprite.bounds.size.y;
            float scale = spriteWorldH > 0.001f ? (targetHeight * 1.15f) / spriteWorldH : 1f;
            artObj.transform.localScale = new Vector3(scale, scale, 1f);
            hdSpriteRenderer = artObj.AddComponent<SpriteRenderer>();
            hdSpriteRenderer.sprite = characterArtSprite;
            hdSpriteRenderer.sortingOrder = 15;
        }
        private Transform Part(string name,Vector2 position,Vector2 size,Color color,int order)
        {
            GameObject part=new GameObject(name); part.transform.SetParent(VisualRoot,false); part.transform.localPosition=position; part.transform.localScale=new Vector3(size.x,size.y,1);
            SpriteRenderer renderer=part.AddComponent<SpriteRenderer>(); renderer.sprite=sprite; color.a=1; renderer.color=color; renderer.sortingOrder=order; return part.transform;
        }
        private void Awake() { if(definition!=null) TryConfigure(definition); }
        private void OnEnable() { if(VisualRoot!=null) VisualRoot.gameObject.SetActive(true); }
        private void OnDisable() { if(VisualRoot!=null) VisualRoot.gameObject.SetActive(false); }
        private void Clear() { if(VisualRoot!=null) { VisualRoot.gameObject.SetActive(false); Destroy(VisualRoot.gameObject); } VisualRoot=null; LeftLeg=null;RightLeg=null;LeftArm=null;RightArm=null;Weapon=null; hdSpriteRenderer=null; poseFrames=null; for(int i=0;i<ownedPoseSprites.Count;i++)if(ownedPoseSprites[i]!=null)Destroy(ownedPoseSprites[i]);ownedPoseSprites.Clear(); if(ownsCharacterArtSprite&&characterArtSprite!=null)Destroy(characterArtSprite);characterArtSprite=null;ownsCharacterArtSprite=false; if(ownsSprite&&sprite!=null)Destroy(sprite);if(texture!=null)Destroy(texture);sprite=null;texture=null;ownsSprite=false; }
        private void OnDestroy() { Clear(); }
    }
}
