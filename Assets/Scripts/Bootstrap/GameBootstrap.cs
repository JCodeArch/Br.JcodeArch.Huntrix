using System;
using UnityEngine;

namespace HuntrX.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] systems = Array.Empty<MonoBehaviour>();

        private void Start()
        {
            if (!ValidateSystems())
            {
                return;
            }

            for (int i = 0; i < systems.Length; i++)
            {
                MonoBehaviour system = systems[i];
                try
                {
                    ((IGameInitializable)system).Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogException(
                        new InvalidOperationException(
                            $"Game initialization stopped at '{system.GetType().Name}' (index {i}). " +
                            "Later systems were not initialized; earlier systems may already be initialized.",
                            exception),
                        system);
                    return;
                }
            }
        }

        private bool ValidateSystems()
        {
            if (systems == null)
            {
                Debug.LogError("GameBootstrap systems list is null. No systems were initialized.", this);
                return false;
            }

            for (int i = 0; i < systems.Length; i++)
            {
                MonoBehaviour system = systems[i];
                if (system == null)
                {
                    Debug.LogError($"GameBootstrap has a null system at index {i}. No systems were initialized.", this);
                    return false;
                }

                if (!(system is IGameInitializable))
                {
                    Debug.LogError(
                        $"GameBootstrap component '{system.GetType().Name}' does not implement IGameInitializable. " +
                        "No systems were initialized.",
                        system);
                    return false;
                }

                if (!system.isActiveAndEnabled)
                {
                    Debug.LogError(
                        $"GameBootstrap component '{system.GetType().Name}' is not active and enabled. " +
                        "No systems were initialized.",
                        system);
                    return false;
                }

                for (int previousIndex = 0; previousIndex < i; previousIndex++)
                {
                    if (systems[previousIndex] == system)
                    {
                        Debug.LogError(
                            $"GameBootstrap references the same component more than once at index {i}. " +
                            "No systems were initialized.",
                            system);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
