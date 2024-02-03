using System;
using System.Collections.Generic;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public class LeversPuzzleChain : LeversPuzzleType
    {
        [Serializable]
        public sealed class LeversChain
        {
            public List<int> ChainIndex = new();
        }

        [SerializeField] private List<LeversChain> _leversChains;
        [SerializeField] private int _maxLeverReactions;
        [SerializeField] private int _maxReactiveLevers;

        public List<LeversChain> LeversChains => _leversChains;

        public override void OnLeverInteract(LeversPuzzleLever lever)
        {
            int leverIndex = Levers.IndexOf(lever);
            LeversChain leverChain = _leversChains[leverIndex];

            if(leverChain.ChainIndex.Count > 0)
            {
                foreach (var chain in leverChain.ChainIndex)
                {
                    LeversPuzzleLever chainLever = Levers[chain];
                    chainLever.ChangeLeverState();
                }
            }

            TryToValidate();
        }

        public override void TryToValidate()
        {
            ValidateLevers();
        }

        public override bool OnValidate()
        {
            int correctLeverStates = 0;
            for (int i = 0; i < Levers.Count; i++)
            {
                bool leverState = Levers[i].LeverState;
                if (leverState == true)
                    correctLeverStates++;
            }

            if (correctLeverStates == Levers.Count)
            {
                DisableLevers();
                return true;
            }

            return false;
        }
    }
}