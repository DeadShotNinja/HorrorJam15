/*
 * @ Jacob Wolfe '23
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioStateManager : MonoBehaviour
{
    [Header("General Game State Variables")]
    [SerializeField] private AK.Wwise.State Game_Gameplay;
    [SerializeField] private AK.Wwise.State Game_MainMenu;
    [SerializeField] private AK.Wwise.State Game_GameLose;
    [SerializeField] private AK.Wwise.State Game_GameWin;
    [SerializeField] private AK.Wwise.State Game_GamePaused;
    [SerializeField] private AK.Wwise.State Game_None;

    [Header("General Music State Variables")]
    [SerializeField] private AK.Wwise.State Music_MainMenu;
    [SerializeField] private AK.Wwise.State Music_LevelStart;
    [SerializeField] private AK.Wwise.State Music_LevelWin;
    [SerializeField] private AK.Wwise.State Music_LevelLose;
    [SerializeField] private AK.Wwise.State Music_PauseMenu;
    [SerializeField] private AK.Wwise.State Music_None;

    [Header("Player Lighthouse Level Location State Variables")]
    [SerializeField] private AK.Wwise.State PlayerLocationLevel_None;

    [Header("Player Danger State Variables")]
    [SerializeField] private AK.Wwise.State Danger_None;

    // Awake
    private void Awake()
    {
        ChangeWwiseState(Game_None);
        ChangeWwiseState(Music_None);
        ChangeWwiseState(PlayerLocationLevel_None);
        ChangeWwiseState(Danger_None);
    }
    // On Enable
    private void OnEnable()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;
        //EventManager.StartListening<WinGameEvent>(OnGameWon);
        //EventManager.StartListening<TimeChangedEvent>(OnDimensionChanged);
        //EventManager.StartListening<PlayerDiedEvent>(OnPlayerDied);
        //EventManager.StartListening<PlayerRespawnedEvent>(OnPlayerRespawn);
        //EventManager.StartListening<ChangeRoomEvent>(OnRoomChange);
        //EventManager.StartListening<GameStateChangedEvent>(OnGameStateChanged);
    }
    // On Disable
    private void OnDisable()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;
        //EventManager.StopListening<WinGameEvent>(OnGameWon);
        //EventManager.StopListening<TimeChangedEvent>(OnDimensionChanged);
        //EventManager.StopListening<PlayerDiedEvent>(OnPlayerDied);
        //EventManager.StopListening<PlayerRespawnedEvent>(OnPlayerRespawn);
        //EventManager.StopListening<ChangeRoomEvent>(OnRoomChange);
        //EventManager.StopListening<GameStateChangedEvent>(OnGameStateChanged);
    }
    // On Game State Changed
    //private void OnGameStateChanged(GameStateChangedEvent eventData)
    //{
    //    if (eventData.State == GameState.Playing)
    //    {
    //        ChangeWwiseState(Music_LevelStart);
    //        ChangeWwiseState(Game_Gameplay);
    //    }
    //    else
    //    {
    //        ChangeWwiseState(Game_GamePaused);
    //        ChangeWwiseState(Music_PauseMenu);
    //    }
    //}
    // On Dimension Changed
    //private void OnDimensionChanged(TimeChangedEvent eventData)
    //{
    //    if (eventData.IsPresent)
    //    {
    //        ChangeWwiseState(Dimension_Present);
    //    }
    //    else
    //    {
    //        ChangeWwiseState(Dimension_Past);
    //    }
    //}
    // On Scene Loaded
    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    // Main Menu
    //    if (SceneManager.GetActiveScene().buildIndex == 2)
    //    {
    //        ChangeWwiseState(Game_MainMenu);
    //        ChangeWwiseState(Music_MainMenu);
    //    }
    //    // Gameplay
    //    else if (SceneManager.GetActiveScene().buildIndex == 3)
    //    {
    //        ChangeWwiseState(Game_Gameplay);
    //        ChangeWwiseState(Music_LevelStart);
    //    }
    //    // Credits
    //    else if (SceneManager.GetActiveScene().buildIndex == 4)
    //    {
    //        // Needs to be changed to Credit state and set up in Wwise
    //        ChangeWwiseState(Game_MainMenu);
    //        ChangeWwiseState(Music_MainMenu);
    //    }
    //}
    // On Game Won
    //private void OnGameWon(WinGameEvent eventData)
    //{
    //    ChangeWwiseState(Game_GameWin);
    //    ChangeWwiseState(Music_LevelWin);
    //}
    //// On Player Died
    //private void OnPlayerDied(PlayerDiedEvent eventData)
    //{
    //    ChangeWwiseState(Game_GameLose);
    //    ChangeWwiseState(Music_LevelLose);
    //}
    //// On Player Respawn
    //private void OnPlayerRespawn(PlayerRespawnedEvent eventData)
    //{
    //    ChangeWwiseState(Game_Gameplay);
    //    ChangeWwiseState(Music_LevelStart);
    //}
    //// On Room Change
    //private void OnRoomChange(ChangeRoomEvent eventData)
    //{
    //    if (eventData.TargetRoomID == 1 || eventData.TargetRoomID == 2 || eventData.TargetRoomID == 3) 
    //    {
    //        ChangeWwiseState(PlayerLocationLevel_Level1);
    //    }
    //    else if (eventData.TargetRoomID == 4 || eventData.TargetRoomID == 5 || eventData.TargetRoomID == 6)
    //    {
    //        ChangeWwiseState(PlayerLocationLevel_Level2);
    //    }
    //    else if (eventData.TargetRoomID == 7 || eventData.TargetRoomID == 8 || eventData.TargetRoomID == 9)
    //    {
    //        ChangeWwiseState(PlayerLocationLevel_Level3);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("On Room Change was called in AudioStateManager, but no State was set!");
    //    }
    //}
    // Function to set State to Wwise
    private void ChangeWwiseState(AK.Wwise.State state)
    {
        if (state.IsValid())
        {
            state.SetValue();
        }
        else
        {
            Debug.LogWarning("Wwise State change for " + state.Name + "was called, but State is not valid. Is it assigned in the Inspector?");
        }
    }
}