using UnityEngine;

namespace HJ.Runtime
{
    public class QuickWwiseEventPlayer : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event _wwiseEvent;
        
        public void PlayWwiseEvent()
        {
            //AudioManager.PostAudioEvent(_wwiseEvent, gameObject);
            _wwiseEvent.Post(gameObject);
        }
    }
}
