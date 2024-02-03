using UnityEngine;
using HJ.Runtime;

namespace HJ.Scriptable
{
    [CreateAssetMenu(fileName = "MonsterStateGroup", menuName = "HJ/AI/Monster State Group")]
    public class MonsterStateGroup : AIStatesGroup
    {
        public string IdleParameter = "Idle";
        public string WalkParameter = "Walk";
        public string RunParameter = "Run";
        public string PatrolParameter = "Patrol";
        public string AttackTrigger = "Attack";

        [Header("States")]
        public string AttackState = "Attack 01";

        [Header("Player Damage")]
        public MinMaxInt DamageRange;

        public void ResetAnimatorPrameters(Animator animator)
        {
            animator.SetBool(IdleParameter, false);
            animator.SetBool(WalkParameter, false);
            animator.SetBool(RunParameter, false);
            animator.SetBool(PatrolParameter, false);
        }
    }
}
