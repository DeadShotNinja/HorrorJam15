using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Scriptable
{
    public abstract class PlayerStatesGroup : ScriptableObject
    {
        public List<PlayerStateData> PlayerStates = new();
        
        public List<State> GetStates(PlayerStateMachine machine)
        {
            return PlayerStates.Select(x => new State()
            {
                StateData = x,
                FSMState = x.StateAsset.InitState(machine, this)
            }).ToList();
        }
    }
}
