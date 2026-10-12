using HuntrX.Data;
using UnityEngine;
namespace HuntrX.Presentation.Art
{
    /// <summary>Optional development-only controls to inspect six effect shapes.</summary>
    public sealed class GameplayVfxPreview2D : MonoBehaviour
    {
        [SerializeField] private GameplayVfxPool2D pool;
        private void OnGUI(){if(pool==null)return;for(int i=0;i<6;i++)if(GUI.Button(new Rect(20+i*135,80,130,30),((GameplayVfxKind)i).ToString()))pool.TryEmit((GameplayVfxKind)i,transform.position,transform.position+Vector3.right*2);}
    }
}
