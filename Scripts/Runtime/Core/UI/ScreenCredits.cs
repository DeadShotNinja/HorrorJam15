using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ScreenCredits : MonoBehaviour
{
    [SerializeField] private UnityEvent _onEnd;
    
    [Header("Internal configuration")]
    [SerializeField] private RectTransform _creditsContainer;
    [SerializeField] private DOTweenAnimation _animation;

    private void Start()
    {
        _animation.onComplete.AddListener(() => { _onEnd?.Invoke(); });
    }

    public void StartAnimation()
    {
        Assert.IsNotNull(_creditsContainer);
        
        var pos = _creditsContainer.position;
        _creditsContainer.SetPositionAndRotation(
            new Vector3(pos.x, 0, pos.z),
            _creditsContainer.rotation);

        _animation.DORestart();
        _animation.DOPlay();
    }
}
