using HJ.Runtime;
using UnityEngine;

namespace HJ.Scriptable
{
    public abstract class PlayerStateAsset : StateAsset
    {
        /// <summary>
        /// Initialize and get FSM Player State.
        /// </summary>
        public abstract FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group);

        /// <summary>
        /// Get a state key to help recognize which state is currently active.
        /// Can be overriden to define other states...
        /// </summary>
        public virtual string GetStateKey() => ToString();

        /// <summary>
        /// Get FSM State display name.
        /// </summary>
        public override string ToString() => GetType().Name;
    }
}
