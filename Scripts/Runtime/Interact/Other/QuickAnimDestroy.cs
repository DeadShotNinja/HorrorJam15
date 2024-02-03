using UnityEngine;

namespace HJ.Runtime
{
    public class QuickAnimDestroy : MonoBehaviour
    {
        public void DestroyMe()
        {
            gameObject.SetActive(false);
        }
    }
}
