using System.Collections.Generic;
using HJ.Input;
using UnityEngine;
using static HJ.Input.InputManager;

namespace HJ.Runtime
{
    public class ControlsInfoPanel : MonoBehaviour
    {
        private Stack<ControlsContext[]> _contextsQueue = new();
        private BindingPath[] _bindingPaths;
        
        public InteractButton[] InteractButtons;

        public void ShowInfo(ControlsContext[] contexts)
        {
            // add contexts to the queue
            _contextsQueue.Push(contexts);

            // initialize binding paths
            if (_bindingPaths == null || _bindingPaths.Length <= 0 || _bindingPaths.Length < contexts.Length)
                _bindingPaths = new BindingPath[contexts.Length];

            // interact buttons
            if (InteractButtons != null)
            {
                for (int i = 0; i < InteractButtons.Length; i++)
                {
                    var button = InteractButtons[i];
                    if (button == null) continue;

                    if (i < contexts.Length)
                    {
                        var context = contexts[i];
                        if (context != null)
                        {
                            if (_bindingPaths[i] == null)
                                _bindingPaths[i] = GetBindingPath(context.InputAction.ActionName, context.InputAction.BindingIndex);

                            string name = context.InteractName;
                            if (name != null && _bindingPaths[i] != null)
                            {
                                var glyph = _bindingPaths[i].InputGlyph;
                                button.SetButton(name, glyph.GlyphSprite, glyph.GlyphScale);
                            }
                        }
                    }
                    else
                    {
                        button.HideButton();
                    }
                }
            }

            gameObject.SetActive(true);
        }

        public void HideInfo()
        {
            if (_contextsQueue.Count > 0)
            {
                _contextsQueue.Pop();

                if(_contextsQueue.Count > 0)
                {
                    var contexts = _contextsQueue.Pop();
                    ShowInfo(contexts);
                }
                else
                {
                    _bindingPaths = new BindingPath[0];
                    gameObject.SetActive(false);
                }
            }
            else
            {
                _bindingPaths = new BindingPath[0];
                gameObject.SetActive(false);
            }
        }
    }
}
