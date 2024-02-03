using UnityEngine;

namespace HJ.Runtime
{
    public class QuickLadder : MonoBehaviour, IInteractStart
    {
        [SerializeField] private Transform _topExit;
        [SerializeField] private Transform _bottomExit;
        
        public void InteractStart()
        {
            Vector3 playerPos = PlayerPresenceManager.Instance.Player.transform.position;
            float distanceToTop = Vector3.Distance(playerPos, _topExit.position);
            float distanceToBottom = Vector3.Distance(playerPos, _bottomExit.position);

            PlayerPresenceManager.Instance.Player.transform.position =
                distanceToTop > distanceToBottom ? _topExit.position : _bottomExit.position;
        }
    }
}
