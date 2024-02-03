using UnityEngine;

namespace HJ.Runtime
{
    public class WaitForAnimatorStateExit : CustomYieldInstruction
    {
        private bool _isStateEntered = false;
        private readonly Animator _animator;
        private readonly string _state;

        public WaitForAnimatorStateExit(Animator animator, string state)
        {
            _animator = animator;
            _state = state;
        }

        public override bool keepWaiting
        {
            get
            {
                AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
                bool isState = info.IsName(_state);

                if (isState && !_isStateEntered) _isStateEntered = true;
                else if (!isState && _isStateEntered) return false;

                return true;
            }
        }
    }
}