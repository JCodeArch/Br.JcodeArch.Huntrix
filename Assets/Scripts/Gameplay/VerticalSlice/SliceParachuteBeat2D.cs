using UnityEngine;
using HuntrX.Data;
namespace HuntrX.Gameplay.VerticalSlice
{
    public sealed class SliceParachuteBeat2D : SliceBeat2D
    {
        [SerializeField] private StageBlockoutDefinition landingStage;
        [SerializeField] private float fallHeight = 35f;
        [SerializeField] private float terminalSpeed = 4f;
        [SerializeField] private float landingWidth = 8f;
        private VerticalSliceSession2D session;
        private Rigidbody2D body;
        private float originalGravity, originalDamping;
        private Vector2 landing;
        private GameObject parachute;
        private Sprite sprite;
        public override string Title => "Paraquedas: controle a queda e pouse na zona do show";
        public override bool TryBegin(VerticalSliceSession2D value)
        {
            if (value == null || value.Manager.ActiveCharacter == null || !Positive(fallHeight) || !Positive(terminalSpeed) || !Positive(landingWidth) || !value.TryLoadStage(landingStage)) return false;
            session = value; IsComplete = false; session.Falling = true; session.Input.SetCharacterSwitchAllowed(false);
            landing = value.Stage.EndPoint.position;
            body = value.Manager.ActiveCharacter.GetComponent<Rigidbody2D>(); originalGravity = body.gravityScale; originalDamping = body.linearDamping;
            body.position = landing + Vector2.up * fallHeight; body.linearVelocity = Vector2.zero; body.gravityScale = .3f;
            parachute = new GameObject("Original prototype parachute"); parachute.transform.SetParent(body.transform, false); parachute.transform.localPosition = Vector3.up * 1.8f; parachute.transform.localScale = new Vector3(2.5f,.5f,1f);
            var renderer = parachute.AddComponent<SpriteRenderer>(); sprite = Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f,1f); renderer.sprite=sprite;renderer.color=Color.cyan;
            return true;
        }
        public override void Tick(float deltaTime)
        {
            if (session == null) return;
            if (session.Manager.ActiveCharacter == null) return;
            if (body == null || body.gameObject != session.Manager.ActiveCharacter.gameObject)
            {
                if (body != null) { body.gravityScale = originalGravity; body.linearDamping = originalDamping; }
                body = session.Manager.ActiveCharacter.GetComponent<Rigidbody2D>();
                if (body == null) return;
                originalGravity = body.gravityScale; originalDamping = body.linearDamping;
                body.position = landing + Vector2.up * fallHeight; body.linearVelocity = Vector2.zero; body.gravityScale = .3f;
                if (parachute != null) { parachute.transform.SetParent(body.transform, false); parachute.transform.localPosition = Vector3.up * 1.8f; }
            }
            if (body.position.y < landing.y - 5f || !Finite(body.position.x) || !Finite(body.position.y))
            { body.position = landing + Vector2.up * fallHeight; body.linearVelocity = Vector2.zero; return; }
            body.linearVelocity = new Vector2(body.linearVelocity.x, Mathf.Max(body.linearVelocity.y, -terminalSpeed));
            if (body.position.y <= landing.y + .4f)
            {
                if (Mathf.Abs(body.position.x-landing.x) <= landingWidth*.5f) IsComplete = session.Input.IsGrounded;
                else { body.position = landing + Vector2.up * fallHeight; body.linearVelocity = Vector2.zero; }
            }
        }
        public override void Exit()
        {
            if (body != null) { body.gravityScale = originalGravity; body.linearDamping = originalDamping; }
            if (session != null) { session.Falling = false; session.Input.SetCharacterSwitchAllowed(true); } if (parachute != null) { parachute.SetActive(false); Destroy(parachute); } if (sprite != null) Destroy(sprite);
            parachute=null;sprite=null;session=null;body=null;base.Exit();
        }
        private static bool Finite(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value);
        private static bool Positive(float value)=>!float.IsNaN(value)&&!float.IsInfinity(value)&&value>0f;
    }
}
