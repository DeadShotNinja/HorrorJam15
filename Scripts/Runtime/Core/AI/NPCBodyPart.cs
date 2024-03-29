using UnityEngine;

namespace HJ.Runtime
{
    public class NPCBodyPart : MonoBehaviour, IDamagable
    {
        [HideInInspector] public NPCHealth HealthScript;
        
        public bool IsHeadDamage;

        public void OnApplyDamage(int damage, Transform sender = null)
        {
            if (HealthScript.AllowHeadhsot && IsHeadDamage)
                damage = Mathf.RoundToInt(damage * HealthScript.HeadshotMultiplier);

            HealthScript.OnApplyDamage(damage, sender);
        }

        public void ApplyDamageMax(Transform sender = null)
        {
            HealthScript.ApplyDamageMax(sender);
        }
    }
}