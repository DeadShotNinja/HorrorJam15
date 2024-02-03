using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace HJ.Input
{
    public sealed class BindingPath
    {
        public string BindedPath;
        public string OverridePath;
        public InputGlyph InputGlyph;

        private readonly BehaviorSubject<Unit> _observer;

        public BindingPath(string bindingPath, string overridePath)
        {
            BindedPath = bindingPath;
            OverridePath = overridePath;

            GetGlyphPath();
            _observer = new BehaviorSubject<Unit>(Unit.Default);
        }

        /// <summary>
        /// Currently used binding path.
        /// </summary>
        public string EffectivePath
        {
            get
            {
                return !string.IsNullOrEmpty(OverridePath) ? OverridePath : BindedPath;
            }
            set
            {
                OverridePath = BindedPath == value ? string.Empty : value;
                
                GetGlyphPath();
                _observer.OnNext(Unit.Default);
            }
        }

        /// <summary>
        /// Currently used binding path, but observable.
        /// </summary>
        public IObservable<string> EffectivePathObservable
        {
            get => _observer.Select(_ => EffectivePath);
        }
        
        /// <summary>
        /// Current input glyph, but observable.
        /// </summary>
        public IObservable<InputGlyph> InputGlyphObservable
        {
            get => _observer.Select(_ => InputGlyph);
        }
        
        /// <summary>
        /// Current glyph path, but observable.
        /// </summary>
        public IObservable<string> GlyphPathObservable
        {
            get => _observer.Select(_ => InputGlyph.GlyphPath);
        }
        
        /// <summary>
        /// Current glyph sprite, but observable.
        /// </summary>
        public IObservable<Sprite> GlyphSpriteObservable
        {
            get => _observer.Select(_ => InputGlyph.GlyphSprite);
        }
        
        /// <summary>
        /// Update the glyph path.
        /// </summary>
        public void GetGlyphPath()
        {
            InputGlyph = InputManager.SpritesAsset.GetInputGlyph(EffectivePath);
        }
        
        /// <summary>
        /// Format the glyph path.
        /// </summary>
        public string Format(string format)
        {
            return string.Format(format, InputGlyph.GlyphPath);
        }
    }
}
