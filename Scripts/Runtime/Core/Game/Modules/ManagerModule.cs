using System;

namespace HJ.Runtime
{
    [Serializable]
    public abstract class ManagerModule
    {
        protected Inventory _inventory => GameManager.Inventory;
        protected PlayerPresenceManager _playerPresence => GameManager.PlayerPresence;
        
        public GameManager GameManager { get; internal set; }
        public abstract string Name { get; }

        /// <summary>
        /// Override this method to define behavior at Awake.
        /// </summary>
        public virtual void OnAwake() { }

        /// <summary>
        /// Override this method to define behavior at Start.
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Override this method to define behavior at Update.
        /// </summary>
        public virtual void OnUpdate() { }
    }
}