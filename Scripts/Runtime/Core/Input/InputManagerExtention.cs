using System;
using System.Reactive.Disposables;

namespace HJ.Input
{
    public static class InputManagerExtention
    {
        public static void ObserveEffectivePath(this BindingPath bindingPath, Action<string> effectivePath)
        {
            CompositeDisposable disposables = InputManager.Instance.Disposables;
            disposables.Add(bindingPath.EffectivePathObservable.Subscribe(effectivePath));
        }
        
        public static void ObserveGlyphPath(this BindingPath bindingPath, Action<string> glyphPath)
        {
            CompositeDisposable disposables = InputManager.Instance.Disposables;
            disposables.Add(bindingPath.GlyphPathObservable.Subscribe(glyphPath));
        }
        
        public static void ObserveGlyphPath(string actionName, int bindingIndex, Action<string> glyphPath)
        {
            CompositeDisposable disposables = InputManager.Instance.Disposables;
            var bindingPath = InputManager.GetBindingPath(actionName, bindingIndex);
            if (bindingPath != null) disposables.Add(bindingPath.GlyphPathObservable.Subscribe(glyphPath));
        }
        
        public static void ObserveInputGlyph(string actionName, int bindingIndex, Action<InputGlyph> inputGlyph)
        {
            CompositeDisposable disposables = InputManager.Instance.Disposables;
            var bindingPath = InputManager.GetBindingPath(actionName, bindingIndex);
            if (bindingPath != null) disposables.Add(bindingPath.InputGlyphObservable.Subscribe(inputGlyph));
        }
        
        public static void ObserveBindingPath(string actionName, int bindingIndex, Action<bool, string> bindingPath)
        {
            CompositeDisposable disposables = InputManager.Instance.Disposables;
            disposables.Add(InputManager.ObserveBindingPath(actionName, bindingIndex).Subscribe(evt => bindingPath?.Invoke(evt.apply, evt.path)));
        }
    }
}
