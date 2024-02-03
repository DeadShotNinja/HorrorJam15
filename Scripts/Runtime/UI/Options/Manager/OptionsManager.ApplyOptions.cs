using UnityEngine;

namespace HJ.Runtime
{
    public partial class OptionsManager
    {
        // 0 - First, N - Last
        private void ApplyMonitorOption(string optionName, int value, bool isChanged)
        {
            if(isChanged) _currentDisplay.Value = _displayInfos[value];
            _serializableData[optionName] = new(value);
        }

        // 0 - Min Resolution, N - Max Resolution
        private void ApplyResolutionOption(int value, bool isChanged)
        {
            var resolution = _resolutions[value];
            if (isChanged) _currentResolution.Value = resolution;
            _serializableData["screen_width"] = new(resolution.width);
            _serializableData["screen_height"] = new(resolution.height);
        }

        // 0 - Windowed, 1 - Fullscreen
        private void ApplyFullscreenOption(int value, bool isChanged)
        {
            if (isChanged) _currentFullscreen.Value = value == 1 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            _serializableData["screen_fullscreen"] = new(value == 1);
        }

        // 0 - 30FPS, 1 - 60FPS, 2 - 120FPS, 3 - Variable
        private void ApplyFramerateOption(string optionName, int value, bool isChanged)
        {
            int framerate = _framerates[value];
            if (isChanged) Application.targetFrameRate = framerate;
            _serializableData[optionName] = new(framerate);
        }

        // 0 - Don't Sync, 1 = Every V Blank
        private void ApplyVSyncOption(string optionName, int value, bool isChanged)
        {
            if (isChanged) QualitySettings.vSyncCount = value;
            _serializableData[optionName] = new(value);
        }

        // 0.1 - Min Resolution, 2 - Max Resolution
        private void ApplyRenderScaleOption(string optionName, float value, bool isChanged)
        {
            Debug.Log(value);

            value = Mathf.Clamp(value, 0.1f, 2f);
            if (isChanged) _URPAsset.renderScale = value;
            _serializableData[optionName] = new(value);
        }

        // 0 - Min Sharpness, 1 - Max Sharpness
        private void ApplyFSRSharpnessOption(string optionName, float value, bool isChanged)
        {
            value = Mathf.Clamp01(value);
            if (isChanged) _URPAsset.fsrSharpness = value;
            _serializableData[optionName] = new(value);
        }

        // 1 - Disabled, 2 - 2x, 3 - 4x, 4 - 8x
        private void ApplyAntialiasingOption(string optionName, int value, bool isChanged)
        {
            int antialiasing = _antialiasing[value];
            if (isChanged) _URPAsset.msaaSampleCount = antialiasing;
            _serializableData[optionName] = new(value);
        }

        // 0 - Disable, 1 - Enable, 2 - Force Enable
        private void ApplyAnisotropicOption(string optionName, int value, bool isChanged)
        {
            if (isChanged) QualitySettings.anisotropicFiltering = (AnisotropicFiltering)value;
            _serializableData[optionName] = new(value);
        }

        // 0 - Eighth Size, 1 - Quarter Size, 2 - Half Size, 3 - Normal
        private void ApplyTextureQualityOption(string optionName, int value, bool isChanged)
        {
            int quality = 3 - value;
            if (isChanged) QualitySettings.globalTextureMipmapLimit = quality;
            _serializableData[optionName] = new(value);
        }

        // 0 - 0m (Disabled), 1 - 25m (Very Low), 2 - 40m (Low), 3 - 55m (Medium), 4 - 70m (High), 5 - 85m (Very High), 6 - 100m (Max)
        private void ApplyShadowDistanceOption(string optionName, int value, bool isChanged)
        {
            float distance = _shadowDistances[value];
            if (isChanged) _URPAsset.shadowDistance = distance;
            _serializableData[optionName] = new(value);
        }

        // 0 - Min Volume, 1 - Max Volume
        private void ApplyGlobalVolumeOption(string optionName, float value, bool isChanged)
        {
            value = Mathf.Clamp01(value);
            if (isChanged) AudioListener.volume = value;
            _serializableData[optionName] = new(value);
        }
    }
}