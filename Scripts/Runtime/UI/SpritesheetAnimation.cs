using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class SpritesheetAnimation : MonoBehaviour
    {
        [Header("Spritesheet Configuration")]
        [Tooltip("The spritesheet containing all the animation frames.")]
        [SerializeField] private Sprite _spritesheet;
        [Tooltip("The Image component where the animation will be displayed.")]
        [SerializeField] private Image _image;
        [Tooltip("The number of frames to be played per second.")]
        [SerializeField, Range(1f, 60f)] private float _frameRate = 30f;
        [Tooltip("Should the animation play as soon as the game starts?")]
        [SerializeField] private bool _playOnStart = true;

        [Header("Sprites")]
        [Tooltip("Individual frames of the spritesheet.")]
        [SerializeField] private Sprite[] _sprites;
        
        private int _currentSpriteIndex;
        
        public bool PlayOnStart => _playOnStart;

        private void Start()
        {
            if(_playOnStart) StartCoroutine(AnimateSpriteSheet());
        }

        private IEnumerator AnimateSpriteSheet()
        {
            while (true)
            {
                _image.sprite = _sprites[_currentSpriteIndex];
                yield return new WaitForSeconds(1f / _frameRate);
                _currentSpriteIndex = (_currentSpriteIndex + 1) % _sprites.Length;
            }
        }

        public void SetAnimationStatus(bool state)
        {
            if (state) StartCoroutine(AnimateSpriteSheet());
            else StopAllCoroutines();
        }
    }
}
