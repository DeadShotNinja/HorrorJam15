using UnityEngine;

namespace HJ.Runtime
{
    public class WaitForAnimatorClip : CustomYieldInstruction
    {
        private const string BASE_LAYER = "Base Layer";

        private readonly Animator _animator;
        private readonly float _timeOffset;
        private readonly int _stateHash;

        private bool _isStateEntered;
        private float _stateWaitTime;
        private float _timeWaited;

        public WaitForAnimatorClip(Animator animator, string state, float timeOffset = 0)
        {
            _animator = animator;
            _timeOffset = timeOffset;
            _stateHash = Animator.StringToHash(BASE_LAYER + "." + state);
        }

        public override bool keepWaiting
        {
            get
            {
                AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);

                if (info.fullPathHash == _stateHash && !_isStateEntered)
                {
                    _stateWaitTime = info.length - _timeOffset;
                    _isStateEntered = true;
                }
                else if (_isStateEntered)
                {
                    if (_timeWaited < _stateWaitTime) 
                        _timeWaited += Time.deltaTime;
                    else return false;
                }

                return true;
            }
        }
    }
}