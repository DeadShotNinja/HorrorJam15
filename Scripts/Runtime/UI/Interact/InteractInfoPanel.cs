using HJ.Input;
using UnityEngine;
using TMPro;
using static HJ.Input.InputManager;

namespace HJ.Runtime
{
    public struct InteractInfo
    {
        public string ObjectName;
        public InteractContext[] Contexts;
    }

    public sealed class InteractContext
    {
        public InputReference InputAction;
        public string InteractName;
    }
    
    public class InteractInfoPanel : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CanvasGroup CanvasGroup;
        [SerializeField] private  TMP_Text InteractName;
        [SerializeField] private  InteractButton[] InteractButtons;

        [Header("Fading")]
        [SerializeField] private  float FadeSpeed = 5f;

        private BindingPath[] bindingPaths;
        private bool fadeState;

        private void Update()
        {
            CanvasGroup.alpha = Mathf.MoveTowards(CanvasGroup.alpha, fadeState ? 1 : 0, Time.deltaTime * FadeSpeed);
        }

        public void ShowInfo(InteractInfo interactInfo)
        {
            // initialize binding paths
            if (bindingPaths == null || bindingPaths.Length <= 0)
                bindingPaths = new BindingPath[interactInfo.Contexts.Length];

            // interact name
            if (!string.IsNullOrEmpty(interactInfo.ObjectName))
                InteractName.text = interactInfo.ObjectName;

            // interact buttons
            for (int i = 0; i < interactInfo.Contexts.Length; i++)
            {
                var context = interactInfo.Contexts[i];
                var button = InteractButtons[i];

                if(context != null)
                {
                    if (bindingPaths[i] == null)
                        bindingPaths[i] = GetBindingPath(context.InputAction.ActionName, context.InputAction.BindingIndex);

                    string name = context.InteractName;
                    var glyph = bindingPaths[i].InputGlyph;
                    button.SetButton(name, glyph.GlyphSprite, glyph.GlyphScale);
                }
                else
                {
                    button.HideButton();
                }
            }

            // show info
            fadeState = true;
        }

        public void HideInfo()
        {
            bindingPaths = new BindingPath[0];
            fadeState = false;
        }
    }
}
