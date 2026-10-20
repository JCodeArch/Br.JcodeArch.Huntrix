using HuntrX.Gameplay.BossFinal;
using HuntrX.Gameplay.Combat;
using HuntrX.Gameplay.Rivals;
using UnityEngine;

namespace HuntrX.Gameplay.Enemies
{
    /// <summary>Capcom-style HD Demon Caste & Saja Boys / Gwi-Ma sprite presenter with telegraph flash and floating HP bar.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyAgent2D))]
    public sealed class EnemyPrototypeVisual2D : MonoBehaviour
    {
        private EnemyAgent2D agent;
        private GameObject visual;
        private SpriteRenderer renderer2D;
        private Sprite fallbackSprite;
        private Sprite loadedSprite;
        private readonly System.Collections.Generic.List<Sprite> ownedSprites = new System.Collections.Generic.List<Sprite>(8);
        private Sprite[] morphFrames;
        private Sprite[] actionFrames;
        private Rigidbody2D body2D;
        private Vector3 lastPosition;
        private float spawnClock;
        private float baseScale = 1f;
        private float targetSpriteHeight = 1.85f;
        private float animPhase;

        private GameObject hpBarBg;
        private GameObject hpBarFill;
        private SpriteRenderer hpFillRenderer;

        public bool IsDisguisedAsHuman => morphFrames != null && morphFrames.Length >= 4 && spawnClock < 1.35f;
        public bool IsTransformingFromDisguise => morphFrames != null && morphFrames.Length >= 4 && spawnClock >= 1.35f && spawnClock < 2.25f;

        private void Awake()
        {
            agent = GetComponent<EnemyAgent2D>();
            body2D = GetComponent<Rigidbody2D>();
            lastPosition = transform.position;
            animPhase = Mathf.Abs(GetHashCode() % 100) * 0.07f;

            visual = new GameObject("Prototype silhouette");
            visual.transform.SetParent(transform, false);
            renderer2D = visual.AddComponent<SpriteRenderer>();
            renderer2D.sortingOrder = 12;

            fallbackSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.one * 0.5f, 1f);
            AssignDemonCasteSprite();
            CreateFloatingHealthBar();
        }

        public void RefreshCasteAssignment()
        {
            spawnClock = 0f;
            AssignDemonCasteSprite();
        }

        private void AssignDemonCasteSprite()
        {
            string path;
            targetSpriteHeight = 1.85f;
            string objName = gameObject.name ?? string.Empty;
            morphFrames = null;
            actionFrames = null;

            if (GetComponent<GwiMaBossController2D>() != null || objName.IndexOf("Gwi", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                path = "Art/Characters/GwiMa";
                targetSpriteHeight = 2.65f;
            }
            else if (GetComponent<JinuEncounterController2D>() != null || GetComponent<SajaRivalController2D>() != null || GetComponent<MinibossPhaseController2D>() != null)
            {
                string[] rivals = { "Art/Characters/Jinu", "Art/Characters/Abby", "Art/Characters/Mystery", "Art/Characters/Romance", "Art/Characters/Baby" };
                int idx = Mathf.Abs(GetHashCode()) % rivals.Length;
                if (objName.IndexOf("CaptainDokkaebi", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    path = "Art/Enemies/CaptainDokkaebi";
                    targetSpriteHeight = 2.38f;
                    morphFrames = LoadMorphSequence("Art/Enemies/Morphs/CaptainDokkaebi");
                    actionFrames = LoadMorphSequence("Art/Enemies/Actions/CaptainDokkaebi");
                }
                else
                {
                    path = objName.IndexOf("Jinu", System.StringComparison.OrdinalIgnoreCase) >= 0 ? "Art/Characters/Jinu" : rivals[idx];
                    targetSpriteHeight = 2.10f;
                }
            }
            else if (objName.IndexOf("CaptainDokkaebi", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                path = "Art/Enemies/CaptainDokkaebi";
                targetSpriteHeight = 2.22f;
                morphFrames = LoadMorphSequence("Art/Enemies/Morphs/CaptainDokkaebi");
                actionFrames = LoadMorphSequence("Art/Enemies/Actions/CaptainDokkaebi");
            }
            else if (objName.IndexOf("MagentaStewardess", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                path = "Art/Enemies/MagentaStewardess";
                targetSpriteHeight = 2.05f;
                morphFrames = LoadMorphSequence("Art/Enemies/Morphs/MagentaStewardess");
                actionFrames = LoadMorphSequence("Art/Enemies/Actions/MagentaStewardess");
            }
            else if (objName.IndexOf("BlueCyclops", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                path = "Art/Enemies/BlueCyclopsCrew";
                targetSpriteHeight = 2.02f;
                morphFrames = LoadMorphSequence("Art/Enemies/Morphs/BlueCyclopsCrew");
            }
            else if (GetComponent<FlyingEnemyController2D>() != null)
            {
                bool useTalchum = (Mathf.Abs(GetHashCode()) % 2) == 0;
                path = useTalchum ? "Art/Enemies/TalchumPhantom" : "Art/Enemies/WingedJeoseung";
                targetSpriteHeight = 1.95f;
            }
            else if (GetComponent<SupportEnemyController2D>() != null)
            {
                bool useGumiho = (Mathf.Abs(GetHashCode()) % 2) == 0;
                path = useGumiho ? "Art/Enemies/GumihoFox" : "Art/Enemies/DokkaebiShield";
                targetSpriteHeight = 1.95f;
            }
            else
            {
                string[] groundCastes =
                {
                    "Art/Enemies/CaptainDokkaebi",
                    "Art/Enemies/MagentaStewardess",
                    "Art/Enemies/BlueCyclopsCrew",
                    "Art/Enemies/SmokeImp",
                    "Art/Enemies/CabinBrute",
                    "Art/Enemies/GumihoFox",
                    "Art/Enemies/CorruptedHaetae",
                    "Art/Enemies/BulgasariBrute",
                    "Art/Enemies/TalchumPhantom"
                };
                int casteIdx = Mathf.Abs(GetHashCode()) % groundCastes.Length;
                path = groundCastes[casteIdx];
                targetSpriteHeight = casteIdx == 0 ? 2.22f : (casteIdx == 3 ? 1.65f : (casteIdx == 7 ? 2.15f : 1.98f));

                if (casteIdx == 0)
                {
                    morphFrames = LoadMorphSequence("Art/Enemies/Morphs/CaptainDokkaebi");
                    actionFrames = LoadMorphSequence("Art/Enemies/Actions/CaptainDokkaebi");
                }
                else if (casteIdx == 1)
                {
                    morphFrames = LoadMorphSequence("Art/Enemies/Morphs/MagentaStewardess");
                    actionFrames = LoadMorphSequence("Art/Enemies/Actions/MagentaStewardess");
                }
                else if (casteIdx == 2)
                {
                    morphFrames = LoadMorphSequence("Art/Enemies/Morphs/BlueCyclopsCrew");
                }
            }

            loadedSprite = LoadOrCreateSprite(path);
            if (morphFrames != null && morphFrames.Length >= 4 && morphFrames[0] != null)
            {
                ApplyActiveSprite(morphFrames[0]);
            }
            else if (loadedSprite != null && loadedSprite.bounds.size.y > 0.01f)
            {
                ApplyActiveSprite(loadedSprite);
            }
            else
            {
                renderer2D.sprite = fallbackSprite;
                baseScale = 1f;
                visual.transform.localScale = new Vector3(0.65f, 1.1f, 1f);
            }
        }

        private Sprite[] LoadMorphSequence(string folderPath)
        {
            Sprite[] seq = new Sprite[4];
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                seq[i] = LoadOrCreateSprite(folderPath + "/" + i);
                if (seq[i] != null) count++;
            }
            return count == 4 ? seq : null;
        }

        private Sprite LoadOrCreateSprite(string resourcePath)
        {
            Sprite s = UnityEngine.Resources.Load<Sprite>(resourcePath);
            if (s != null) return s;
            Texture2D tex = UnityEngine.Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100f);
                ownedSprites.Add(s);
                return s;
            }
            return null;
        }

        private void ApplyActiveSprite(Sprite s)
        {
            if (s == null || renderer2D == null) return;
            renderer2D.sprite = s;
            if (s.bounds.size.y > 0.01f)
            {
                baseScale = targetSpriteHeight / s.bounds.size.y;
            }
        }

        private void CreateFloatingHealthBar()
        {
            hpBarBg = new GameObject("Demon_HP_Bg");
            hpBarBg.transform.SetParent(transform, false);
            hpBarBg.transform.localPosition = new Vector3(0f, 1.32f, 0f);
            hpBarBg.transform.localScale = new Vector3(0.95f, 0.11f, 1f);
            var bgSr = hpBarBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = fallbackSprite;
            bgSr.color = new Color(0.08f, 0.05f, 0.14f, 0.85f);
            bgSr.sortingOrder = 18;

            hpBarFill = new GameObject("Demon_HP_Fill");
            hpBarFill.transform.SetParent(transform, false);
            hpBarFill.transform.localPosition = new Vector3(0f, 1.32f, 0f);
            hpBarFill.transform.localScale = new Vector3(0.90f, 0.07f, 1f);
            hpFillRenderer = hpBarFill.AddComponent<SpriteRenderer>();
            hpFillRenderer.sprite = fallbackSprite;
            hpFillRenderer.color = new Color(1f, 0.18f, 0.45f, 0.95f);
            hpFillRenderer.sortingOrder = 19;
        }

        private void OnEnable()
        {
            spawnClock = 0f;
            lastPosition = transform.position;
            if (agent != null)
            {
                agent.StateChanged += UpdateColor;
                UpdateColor(agent.State);
            }
        }

        private void OnDisable()
        {
            if (agent != null) agent.StateChanged -= UpdateColor;
        }

        private void LateUpdate()
        {
            if (visual == null || renderer2D == null || agent == null) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
            spawnClock += dt;

            // If enemy takes damage or starts attacking early, accelerate transformation out of human disguise
            if (morphFrames != null && morphFrames.Length >= 4 && spawnClock < 1.35f)
            {
                bool damaged = agent.Self != null && agent.Self.CurrentHealth < agent.Self.MaximumHealth - 0.01f;
                bool aggro = agent.State == EnemyState2D.Telegraph || agent.State == EnemyState2D.Attacking;
                if (damaged || aggro)
                {
                    spawnClock = 1.35f;
                }
            }

            float speedX = body2D != null ? Mathf.Abs(body2D.linearVelocity.x) : (dt > 0.0001f ? Mathf.Abs(transform.position.x - lastPosition.x) / dt : 0f);
            lastPosition = transform.position;
            bool isWalking = speedX > 0.12f || agent.State == EnemyState2D.Chasing;

            if (morphFrames != null && morphFrames.Length >= 4 && spawnClock < 2.25f)
            {
                int mIdx = spawnClock < 1.35f ? 0 : (spawnClock < 1.80f ? 1 : 2);
                ApplyActiveSprite(morphFrames[mIdx]);
            }
            else if (actionFrames != null && actionFrames.Length >= 4)
            {
                int aIdx;
                if (agent.State == EnemyState2D.Telegraph)
                {
                    aIdx = 2; // Fierce Roar / Claw Windup Frame
                }
                else if (agent.State == EnemyState2D.Attacking || agent.State == EnemyState2D.Recovering)
                {
                    aIdx = 3; // Explosive Claw Slash Lunge Frame!
                }
                else if (isWalking)
                {
                    aIdx = (Mathf.FloorToInt((Time.time + animPhase) * 6.5f) % 2 == 0) ? 0 : 1;
                }
                else
                {
                    aIdx = (Mathf.FloorToInt((Time.time + animPhase) * 2.2f) % 2 == 0) ? 0 : 1;
                }
                ApplyActiveSprite(actionFrames[aIdx]);
            }
            else if (morphFrames != null && morphFrames.Length >= 4)
            {
                ApplyActiveSprite(morphFrames[3]);
            }

            int dir = 1;
            if (agent.Target != null)
            {
                float dx = agent.Target.transform.position.x - transform.position.x;
                if (Mathf.Abs(dx) > 0.05f) dir = dx < 0f ? 1 : -1; // Demon sprites face left by default
            }

            float t = Time.time + animPhase;
            float bob = isWalking ? Mathf.Abs(Mathf.Sin(t * 11f)) * 0.09f : Mathf.Sin(t * 4.5f) * 0.04f;
            float morphShake = IsTransformingFromDisguise ? Mathf.Sin(spawnClock * 55f) * 0.06f : 0f;
            float lungeX = agent.State == EnemyState2D.Attacking ? (-dir * 0.28f) : (agent.State == EnemyState2D.Telegraph ? (dir * 0.10f) : 0f);
            float walkTilt = isWalking ? Mathf.Sin(t * 11f) * 4.5f : (agent.State == EnemyState2D.Attacking ? (dir * 8f) : 0f);

            visual.transform.localPosition = new Vector3(morphShake + lungeX, bob, 0f);
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, walkTilt);

            if (renderer2D.sprite != null && renderer2D.sprite != fallbackSprite)
            {
                float breathScale = isWalking ? (1f + Mathf.Sin(t * 11f) * 0.04f) : (1f + Mathf.Sin(t * 3.8f) * 0.025f);
                visual.transform.localScale = new Vector3(baseScale * dir * (2f - breathScale), baseScale * breathScale, 1f);
            }

            if (hpBarBg != null && hpBarFill != null && agent.Self != null)
            {
                bool alive = agent.Self.IsAlive && agent.State != EnemyState2D.Dead;
                hpBarBg.SetActive(alive);
                hpBarFill.SetActive(alive);
                if (alive && agent.Self.MaximumHealth > 0.01f)
                {
                    float ratio = Mathf.Clamp01(agent.Self.CurrentHealth / agent.Self.MaximumHealth);
                    hpBarFill.transform.localScale = new Vector3(0.90f * ratio, 0.07f, 1f);
                    hpBarFill.transform.localPosition = new Vector3(-0.45f * (1f - ratio), 1.32f, 0f);
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < ownedSprites.Count; i++)
            {
                if (ownedSprites[i] != null) Destroy(ownedSprites[i]);
            }
            ownedSprites.Clear();
            if (fallbackSprite != null) Destroy(fallbackSprite);
        }

        private void UpdateColor(EnemyState2D state)
        {
            if (renderer2D == null) return;
            if (loadedSprite != null || morphFrames != null || actionFrames != null)
            {
                renderer2D.color = state == EnemyState2D.Telegraph ? new Color(1f, 0.85f, 0.3f, 1f) :
                    state == EnemyState2D.Attacking ? new Color(1f, 0.55f, 0.75f, 1f) :
                    state == EnemyState2D.Dead ? new Color(0.4f, 0.4f, 0.4f, 0.45f) :
                    Color.white;
            }
            else
            {
                renderer2D.color = state == EnemyState2D.Telegraph ? Color.yellow :
                    state == EnemyState2D.Attacking ? Color.white : state == EnemyState2D.Dead ? Color.gray :
                    new Color(0.75f, 0.2f, 0.45f);
            }
        }
    }
}
