using System.Collections.Generic;
using HJ.Runtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace HJ
{
    public static class GlobalGameSettings
    {
        private static readonly List<int> _framerateTargets = new() { 20, 30, 60, 100, 120, 144 };
        
        /// <returns>All resolutions that display supports in fullscreen mode.
        /// It's basically Screen.resolutions without duplicates because of
        /// refresh rate differences.</returns>
        public static List<Vector2Int> GetFullscreenResolutions()
        {
            var list = new List<Vector2Int>();
            Resolution? lastResolution = null;

            foreach (var res in Screen.resolutions)
            {
                if (
                    !lastResolution.HasValue
                    || lastResolution.Value.width != res.width
                    || lastResolution.Value.height != res.height
                ) {
                    list.Add(new Vector2Int(res.width, res.height));
                    lastResolution = res;
                }
            }
            
            return list;
        }

        public static List<int> GetFramerateTargets()
        {
            return _framerateTargets;
        }

        public static int CurrentFramerateTargetIndex
        {
            get {
                var fps = PlayerPrefs.GetInt("fps", 60);
                var index = _framerateTargets.FindIndex(elem => elem == fps);

                if (index < 0) 
                    return 2;
            
                return index;
            }
            set {
                Assert.IsTrue(value >= 0);
                Assert.IsTrue(value < _framerateTargets.Count);
            
                var targetFrameRate = _framerateTargets[value];
                PlayerPrefs.SetInt("fps", targetFrameRate);
                ApplyTargetFramerate(targetFrameRate);
            }
        }

        public static bool VSync
        {
            get => PlayerPrefs.GetInt("vsync", 0) == 1;
            set
            {
                PlayerPrefs.SetInt("vsync", 1);
                ApplyVSync(value);
            }
        }

        public static void ApplySettingsOnStart()
        {
            // NOTE(Hulvdan): There will also probably be setups for Wwise bus volumes
            // TODO(Hulvdan): Check Gaia.FrameRateManager...
            ApplyTargetFramerate(_framerateTargets[CurrentFramerateTargetIndex]);
            ApplyVSync(VSync);
        }

        private static void ApplyTargetFramerate(int targetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }

        private static void ApplyVSync(bool value)
        {
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
    }
    
    public class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private bool _debug;
        
        [Header("Screens")]
        [SerializeField] private List<string> _screenNames;
        [SerializeField] private List<GameObject> _screens;
        
        [Header("Controls")]
        [SerializeField] private LeftRightChooser _resolutionsChooser;
        [SerializeField] private LeftRightChooser _framerateChooser;
        [SerializeField] private Toggle _toggleFullscreen;
        [SerializeField] private Toggle _toggleVSync;

        private List<Vector2Int> _fullscreenResolutions;
        
        private void Awake()
        {
            _fullscreenResolutions = GlobalGameSettings.GetFullscreenResolutions();
        }

        private void Start()
        {
            SetupResolutionChooser();
            SetupFramerateChooser();

            _toggleFullscreen.isOn = Screen.fullScreen;
            _toggleFullscreen.onValueChanged.AddListener(SetFullscreen);

            _toggleVSync.isOn = GlobalGameSettings.VSync;
            _toggleVSync.onValueChanged.AddListener(SetVSync);
            SetVSync(_toggleVSync.isOn);
        }

        public void ShowScreen(string val)
        {
            for (var i = 0; i < _screenNames.Count; i++)
                _screens[i].SetActive(_screenNames[i] == val);
        }

        private void SetupResolutionChooser()
        {
            // NOTE(Hulvdan): It shows the wrong resolution
            // if the game was closed while being in windowed mode 
            var currentResolutionIndex = 0;
            var resolutionTexts = new List<string>();

            for (var i = 0; i < _fullscreenResolutions.Count; i++)
            {
                var res = _fullscreenResolutions[i];
                resolutionTexts.Add($"{res.x}x{res.y}");

                var isCurrent = Screen.currentResolution.width == res.x && Screen.currentResolution.height == res.y;
                if (isCurrent)
                    currentResolutionIndex = i;
            } 

            _resolutionsChooser.Init(resolutionTexts, currentResolutionIndex);
            _resolutionsChooser.OnCurrentChanged += SetResolutionByIndex;
        }

        private void SetResolutionByIndex(int index)
        {
            var res = _fullscreenResolutions[index];
            if (_debug)
                Debug.Log($"SetResolutionByIndex index={index} ({res.x}x{res.y})");
            
            Screen.SetResolution(res.x, res.y, Screen.fullScreenMode);
        }

        private void SetupFramerateChooser()
        {
            var labels = new List<string> { Capacity = 0 };
            foreach (var label in GlobalGameSettings.GetFramerateTargets())
                labels.Add(label.ToString());

            _framerateChooser.Init(labels, GlobalGameSettings.CurrentFramerateTargetIndex);
            _framerateChooser.OnCurrentChanged += SetCurrentFramerateTargetByIndex;
        }

        private void SetFullscreen(bool fullscreen)
        {
            if (_debug)
                Debug.Log($"SetFullscreen fullscreen={fullscreen}");
            
            Screen.fullScreen = fullscreen;
        }

        private void SetCurrentFramerateTargetByIndex(int index)
        {
            if (_debug)
            {
                var fps = GlobalGameSettings.GetFramerateTargets()[index];
                Debug.Log($"SetCurrentFramerateTargetByIndex index={index} ({fps} FPS)");
            }
            
            GlobalGameSettings.CurrentFramerateTargetIndex = index;
        }

        private void SetVSync(bool vsync)
        {
            _framerateChooser.SetInteractable(!vsync);
            
            GlobalGameSettings.VSync = vsync;
        }
    }
}
