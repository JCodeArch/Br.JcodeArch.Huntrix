using UnityEngine;
namespace HuntrX.Gameplay.Honmoon
{
    /// <summary>Development-lab controls, not final platform input or product UI.</summary>
    public sealed class HonmoonCrisisLabControls2D : MonoBehaviour
    {
        [SerializeField] private HonmoonController2D honmoon;
        [SerializeField] private HonmoonCrisisController2D crisis;
        private void OnGUI()
        {
            if (honmoon == null || crisis == null) return;
            GUILayout.BeginArea(new Rect(12, 12, 310, 140), GUI.skin.box);
            GUILayout.Label("Honmoon lab: " + honmoon.Value.ToString("0") + " /100 — " + honmoon.Band);
            GUILayout.Label(crisis.IsIntensifying ? "Crisis: horde intensified" : "Horde: base budget");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Drain 20")) honmoon.TryChange(-20f);
            if (GUILayout.Button("Recover 20")) honmoon.TryChange(20f);
            GUILayout.EndHorizontal();
            GUI.enabled = crisis.CanUnite;
            if (GUILayout.Button("Unite Rumi + Mira + Zoey")) crisis.TryUnite();
            GUI.enabled = true;
            GUILayout.EndArea();
        }
    }
}
