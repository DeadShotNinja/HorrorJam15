using System;
using System.Linq;
using UnityEngine;
using HJ.Tools;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public partial class OptionsManager
    {
         // 0 - First, N - Last
        private void LoadMonitorOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                if (value < Display.displays.Length)
                {
                    _currentDisplay.SilentValue = _displayInfos[value];
                    Display.displays[value].Activate();
                    behaviour.SetOptionValue(value);
                    return;
                }
            }

            _currentDisplay.SilentValue = Screen.mainWindowDisplayInfo;
            int display = _displayInfos.IndexOf(_currentDisplay.Value);
            behaviour.SetOptionValue(display);
        }

        // 0 - Min Resolution, N - Max Resolution
        private void LoadResoltionOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile)
            {
                bool val1 = CheckOption("screen_width", JTokenType.Integer, out int width);
                bool val2 = CheckOption("screen_height", JTokenType.Integer, out int height);

                if (val1 && val2)
                {
                    int index = _resolutions.FindIndex(x => x.width == width && x.height == height);
                    if (index <= -1) index = _resolutions.Count - 1;

                    _currentResolution.SilentValue = Screen.currentResolution;
                    _currentResolution.Value = _resolutions[index];
                    behaviour.SetOptionValue(index);
                    return;
                }
            }

            _currentResolution.SilentValue = Screen.currentResolution;
            int value = _resolutions.IndexOf(_currentResolution.Value);
            behaviour.SetOptionValue(value);
        }

        // 0 - Windowed, 1 - Fullscreen
        private void LoadFullscreenOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption("screen_fullscreen", JTokenType.Boolean, out bool fullscreen))
            {
                _currentFullscreen.SilentValue = Screen.fullScreenMode;
                _currentFullscreen.Value = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
                behaviour.SetOptionValue(fullscreen ? 1 : 0);
                return;
            }

            _currentFullscreen.Value = Screen.fullScreenMode;
            int value = _currentFullscreen.Value == FullScreenMode.FullScreenWindow ? 1 : 0;
            behaviour.SetOptionValue(value);
        }

        // 0 - 30FPS, 1 - 60FPS, 2 - 120FPS, 3 - Variable
        private void LoadFramerateOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                if (_framerates.Any(x => value == x))
                {
                    Application.targetFrameRate = value;
                    value = Array.IndexOf(_framerates, value);
                    behaviour.SetOptionValue(value);
                    return;
                }
            }

            int framerate = Application.targetFrameRate;
            int fIndex = Array.IndexOf(_framerates, framerate);
            behaviour.SetOptionValue(fIndex);
        }

        // 0 - Don't Sync, 1 = Every V Blank
        private void LoadVSyncOption(string name, bool fromFile, OptionBehaviour behaviour)
        {           
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 1);
                QualitySettings.vSyncCount = value;
                behaviour.SetOptionValue(value);
                return;
            }

            int vsync = QualitySettings.vSyncCount;
            behaviour.SetOptionValue(vsync);
        }

        // 0.1 - Min Resolution, 2 - Max Resolution
        private void LoadRenderScaleOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Float, out float value))
            {
                value = Mathf.Clamp(value, 0.1f, 2f);
                _URPAsset.renderScale = value;
                behaviour.SetOptionValue(value);
                return;
            }

            float renderScale = _URPAsset.renderScale;
            behaviour.SetOptionValue(renderScale);
        }

        // 0.1 - Min Resolution, 2 - Max Resolution
        private void LoadFSRSharpnessOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Float, out float value))
            {
                value = Mathf.Clamp01(value);
                _URPAsset.fsrSharpness = value;
                behaviour.SetOptionValue(value);
                return;
            }

            float fsrSharpness = _URPAsset.fsrSharpness;
            behaviour.SetOptionValue(fsrSharpness);
        }

        // 0 - Disabled, 1 - 2x, 2 - 4x, 3 - 8x
        private void LoadAntialiasingOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 3);
                _URPAsset.msaaSampleCount = _antialiasing[value];
                behaviour.SetOptionValue(value);
                return;
            }

            int antialiasing = _URPAsset.msaaSampleCount;
            antialiasing = Array.IndexOf(_antialiasing, antialiasing);
            behaviour.SetOptionValue(antialiasing);
        }

        // 0 - Disable, 1 - Enable, 2 - Force Enable
        private void LoadAnisotropicOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 2);
                QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
                behaviour.SetOptionValue(value);
                return;
            }

            int anisotropic = (int)QualitySettings.anisotropicFiltering;
            behaviour.SetOptionValue(anisotropic);
        }

        // 0 - Eighth Size, 1 - Quarter Size, 2 - Half Size, 3 - Normal
        private void LoadTextureQualityOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 3);
                QualitySettings.globalTextureMipmapLimit = 3 - value;
                behaviour.SetOptionValue(value);
                return;
            }

            int texQuality = QualitySettings.globalTextureMipmapLimit;
            texQuality = 3 - texQuality;
            behaviour.SetOptionValue(texQuality);
        }

        // 0 - 0m (Disabled), 1 - 25m (Very Low), 2 - 40m (Low), 3 - 55m (Medium), 4 - 70m (High), 5 - 85m (Very High), 6 - 100m (Max)
        private void LoadShadowDistanceOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Integer, out int value))
            {
                value = Mathf.Clamp(value, 0, 6);
                _URPAsset.shadowDistance = _shadowDistances[value];
                behaviour.SetOptionValue(value);
                return;
            }

            float shadowDistance = _URPAsset.shadowDistance;
            int distance = _shadowDistances.ClosestIndex((int)shadowDistance);
            behaviour.SetOptionValue(distance);
        }

        // 0 - Min Volume, 1 - Max Volume
        private void LoadGlobalVolumeOption(string name, bool fromFile, OptionBehaviour behaviour)
        {
            if (fromFile && CheckOption(name, JTokenType.Float, out float value))
            {
                value = Mathf.Clamp01(value);
                AudioListener.volume = value;
                behaviour.SetOptionValue(value);
                return;
            }

            float globalVolume = AudioListener.volume;
            behaviour.SetOptionValue(globalVolume);
        }

        private bool CheckOption<T>(string name, JTokenType type, out T value) where T : struct
        {
            if (_serializableData.TryGetValue(name, out JValue jValue) && jValue.Type == type)
            {
                value = jValue.ToObject<T>();
                return true;
            }

            value = default;
            return false;
        }
    }
}