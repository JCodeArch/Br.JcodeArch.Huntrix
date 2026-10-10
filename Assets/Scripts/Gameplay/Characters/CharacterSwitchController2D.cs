using UnityEngine;

namespace HuntrX.Gameplay.Characters
{
    /// <summary>Routes logical character selection to one manager-owned local player slot.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterSwitchController2D : MonoBehaviour
    {
        [SerializeField] private CharacterManager2D manager;
        [SerializeField] private GameObject[] roster = new GameObject[0];
        private bool switching;

        public int RosterCount => roster == null ? 0 : roster.Length;
        public int ActiveIndex
        {
            get
            {
                if (manager == null || roster == null) return -1;
                for (int i = 0; i < roster.Length; i++)
                    if (roster[i] == manager.ActivePrefab) return i;
                return -1;
            }
        }

        public bool Configure(CharacterManager2D owner, GameObject[] prefabs, out string error)
        {
            if (switching)
            {
                error = "Cannot reconfigure during a character switch.";
                return false;
            }
            if (!ValidateRoster(owner, prefabs, out error)) return false;
            // Own the array so external callers cannot mutate the selection contract after validation.
            GameObject[] copy = (GameObject[])prefabs.Clone();
            manager = owner;
            roster = copy;
            return true;
        }

        public bool TrySwitchNext(out string error)
        {
            if (!CanRequest(out error)) return false;
            int current = ActiveIndex;
            if (current < 0)
            {
                error = "The active prefab is not in this slot's roster.";
                return false;
            }
            return TrySwitchTo((current + 1) % roster.Length, out error);
        }

        public bool TrySwitchTo(int index, out string error)
        {
            if (!CanRequest(out error)) return false;
            if (index < 0 || index >= roster.Length)
            {
                error = "Character index is outside the roster.";
                return false;
            }
            if (ActiveIndex < 0 || index == ActiveIndex)
            {
                error = "Choose a different character from the active slot's roster.";
                return false;
            }
            switching = true;
            try
            {
                if (!manager.TrySwitch(roster[index]))
                {
                    error = "The manager rejected the switch; inspect actor state and target configuration.";
                    return false;
                }
                error = string.Empty;
                return true;
            }
            finally { switching = false; }
        }

        private bool CanRequest(out string error)
        {
            if (!isActiveAndEnabled || switching)
            {
                error = "The switch controller is disabled or already switching.";
                return false;
            }
            if (!ValidateRoster(manager, roster, out error)) return false;
            if (!manager.isActiveAndEnabled || manager.State != CharacterManagerState2D.Alive ||
                manager.ActiveCharacter == null || !manager.ActiveCharacter.IsAlive)
            {
                error = "The manager must own a live active character before switching.";
                return false;
            }
            return true;
        }

        private static bool ValidateRoster(CharacterManager2D owner, GameObject[] prefabs, out string error)
        {
            if (owner == null || prefabs == null || prefabs.Length < 2)
            {
                error = "A manager and at least two distinct character prefabs are required.";
                return false;
            }
            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] == null)
                {
                    error = "Roster entries cannot be null.";
                    return false;
                }
                for (int j = 0; j < i; j++)
                    if (prefabs[j] == prefabs[i])
                    {
                        error = "Roster entries must be distinct.";
                        return false;
                    }
            }
            error = string.Empty;
            return true;
        }
    }
}
