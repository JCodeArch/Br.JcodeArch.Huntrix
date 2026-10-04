using System;
using UnityEngine;

namespace HuntrX.Data
{
    /// <summary>
    /// Base identity for authored, static game-data assets.
    /// </summary>
    public abstract class GameDataDefinition : ScriptableObject
    {
        [SerializeField]
        private string id = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Stable asset identity used by systems that need to persist references.
        /// </summary>
        public string Id => id;
    }
}