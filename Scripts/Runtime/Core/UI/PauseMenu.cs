using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

enum PauseMenuScreen
{
    MainMenu,
    Settings
}

/// <summary>
/// Usage & Integration.
///
/// When player hits Escape during gameplay, `PauseMenu.Show()` gets called.
/// Existing input listeners should go off at this point.
///
/// `OnExit` delegate gets called on player exiting the pause menu.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _mainPauseScreen;
    [SerializeField] private GameObject _settingsScreen;
    
    private PauseMenuScreen _screen = PauseMenuScreen.MainMenu;
    
    public Action OnExit = delegate { };

    // Outside bindings
    public void Show()
    {
        // _mainPauseScreen.enabled = true;
    }

    public void Hide()
    {
        if (_screen == PauseMenuScreen.Settings)
        {
            OnHideSettings();
        } else {
            // _mainPauseScreen.SetActive()false;
            OnExit?.Invoke();
        }
    }

    // UI Bindings
    public void OnButtonSettingsClicked()
    {
        _settingsScreen.SetActive(true);
        _screen = PauseMenuScreen.Settings;
    }

    public void OnHideSettings()
    {
        _settingsScreen.SetActive(false);
        _screen = PauseMenuScreen.MainMenu;
    }
}