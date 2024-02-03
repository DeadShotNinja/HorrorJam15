using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using HJ.Tools;
using HJ.Input;
using HJ.Scriptable;
using Action = System.Action;

namespace HJ.Runtime
{
    public partial class OptionsManager : Singleton<OptionsManager>
    {
        public const char NAME_SEPARATOR = '.';
        public const string EXTENSION = ".json";

        public enum OptionTypeEnum { Custom, Monitor, Resolution, Fullscreen, FrameRate, VSync, RenderScale, FSRSharpness, Antialiasing, Anisotropic, TextureQuality, ShadowDistance, GlobalVolume }
        public enum OptionValueEnum { Boolean, Integer, Float, String }

        private static readonly int[] _framerates = { 30, 60, 120, -1 };
        private static readonly int[] _antialiasing = { 1, 2, 4, 8 };
        private static readonly int[] _shadowDistances = { 0, 25, 40, 55, 70, 85, 100 };

        [Serializable]
        public struct OptionObject
        {
            public string Name;
            public OptionBehaviour Option;
            public OptionTypeEnum OptionType;
            public OptionValueEnum OptionValue;
            public string DefaultValue;
        }

        [Serializable]
        public struct OptionSection
        {
            public string Section;
            public List<OptionObject> Options;
        }

        [SerializeField] private List<OptionSection> _options = new();
        [SerializeField] private bool _applyAndSaveInputs = true;
        [SerializeField] private bool _showDebug = true;

        private UniversalRenderPipelineAsset _URPAsset => (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;

        private SerializationAsset _serializationAsset
            => SerializationUtillity.SerializationAsset;

        private string _optionsFilename => SerializationUtillity.SerializationAsset.OptionsFilename + EXTENSION;

        private string _optionsPath
        {
            get
            {
                string configPath = _serializationAsset.GetConfigPath();
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath + "/" + _optionsFilename;
            }
        }

        public static bool IsLoaded { get; private set; }

        private readonly Dictionary<string, BehaviorSubject<object>> _optionSubjects = new();
        private static Dictionary<string, JValue> _serializableData = new();

        private readonly CompositeDisposable _disposables = new();
        private readonly List<DisplayInfo> _displayInfos = new();

        private List<Resolution> _resolutions;
        private readonly ObservableValue<DisplayInfo> _currentDisplay = new();
        private readonly ObservableValue<FullScreenMode> _currentFullscreen = new();
        private readonly ObservableValue<Resolution> _currentResolution = new();

        public List<OptionSection> Options => _options;
        
        [UnityEngine.ContextMenu("Reset Loaded Options")]
        private void ResetOptions()
        {
            IsLoaded = false;
            _serializableData.Clear();
        }

        private void Awake()
        {
            foreach (var section in _options)
            {
                foreach (var option in section.Options)
                {
                    string name = option.Name.ToLower();
                    string _val = string.IsNullOrEmpty(option.DefaultValue) ? "0" : option.DefaultValue;
                    _optionSubjects[name] = new BehaviorSubject<object>(option.OptionValue switch
                    {
                        OptionValueEnum.Boolean => int.Parse(_val) == 1,
                        OptionValueEnum.Integer => int.Parse(_val),
                        OptionValueEnum.Float => float.Parse(_val),
                        _ => _val,
                    });
                }
            }
        }

        private void Start()
        {
            SetOptionDatas();
            LoadOptions();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private void SetOptionDatas()
        {
            var monitor = GetOption(OptionTypeEnum.Monitor);
            var resolution = GetOption(OptionTypeEnum.Resolution);

            Screen.GetDisplayLayout(_displayInfos);
            string[] displays = _displayInfos.Select(x => x.name).ToArray();

            this._resolutions = Screen.resolutions.ToList();
            string[] _resolutions = this._resolutions.Select(x => $"{x.width}x{x.height}@{x.refreshRateRatio}").ToArray();

            monitor?.Option.SetOptionData(displays);
            resolution?.Option.SetOptionData(_resolutions);
        }

        public static void ObserveOption(string name, Action<object> onChange)
        {
            if(Instance._optionSubjects.TryGetValue(name, out var subject))
                subject.Subscribe(onChange).AddTo(Instance._disposables);
        }

        public OptionObject? GetOption(OptionTypeEnum optionType)
        {
            foreach (var section in _options)
            {
                foreach (var option in section.Options)
                {
                    if (option.OptionType == optionType)
                        return option;
                }
            }

            return null;
        }

        public OptionObject? GetOption(string optionName)
        {
            string[] path = optionName.Split(NAME_SEPARATOR);
            foreach (var section in _options)
            {
                if (section.Section != path[0])
                    continue;

                foreach (var option in section.Options)
                {
                    if (option.Name == path[1])
                        return option;
                }
            }

            return null;
        }

        public async void ApplyOptions()
        {
            foreach (var section in _options)
            {
                foreach (var option in section.Options)
                {
                    ApplyOptionsRealtime(option);
                }
            }

            ApplyResolution();
            await SerializeOptions();

            if(_applyAndSaveInputs) InputManager.ApplyInputRebindOverrides();
            if(_showDebug) Debug.Log($"[OptionsManager] The option values have been saved to '{_optionsFilename}'.");
        }

        public void DiscardChanges()
        {
            bool anyDiscard = false;
            foreach (var section in _options)
            {
                foreach (var option in section.Options)
                {
                    if (!option.Option.IsChanged)
                        continue;

                    LoadOptions(option, false);
                    anyDiscard = true;
                }
            }

            if(_applyAndSaveInputs) InputManager.ResetInputsToDefaults();
            if(_showDebug && anyDiscard) Debug.Log("[OptionsManager] Options Discarded");
        }

        private async void LoadOptions()
        {
            bool fromFile = IsLoaded || File.Exists(_optionsPath);

            if (fromFile && !IsLoaded)
            {
                await DeserializeOptions();
                if (_showDebug) Debug.Log("[OptionsManager] The options have been loaded.");
            }

            foreach (var section in _options)
            {
                foreach (var option in section.Options)
                {
                    LoadOptions(option, fromFile);
                }
            }

            if(!IsLoaded) ApplyResolution();
            IsLoaded = true;
        }

        private void ApplyResolution()
        {
            int screenWidth = _currentResolution.Value.width;
            int screenHeight = _currentResolution.Value.height;
            var fullscreen = _currentFullscreen.Value;

            if (_currentResolution.IsChanged && _currentFullscreen.IsChanged)
            {
                Screen.SetResolution(screenWidth, screenHeight, fullscreen);
            }
            else if (_currentResolution.IsChanged)
            {
                Screen.SetResolution(screenWidth, screenHeight, Screen.fullScreenMode);
            }
            else if (_currentFullscreen.IsChanged)
            {
                Screen.fullScreenMode = fullscreen;
            }
        }

        private void ApplyOptionsRealtime(OptionObject option)
        {
            OptionTypeEnum optionType = option.OptionType;
            string name = option.Name.ToLower();

            bool isChanged = option.Option.IsChanged;
            object obj = option.Option.GetOptionValue();

            Dictionary<OptionTypeEnum, Action> options = new()
            {
                { OptionTypeEnum.Custom, () => ApplyCustomOption(option, name, obj) },
                { OptionTypeEnum.Monitor, () => ApplyMonitorOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.Resolution, () => ApplyResolutionOption((int)obj, isChanged) },
                { OptionTypeEnum.Fullscreen, () => ApplyFullscreenOption((int)obj, isChanged) },
                { OptionTypeEnum.FrameRate, () => ApplyFramerateOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.VSync, () => ApplyVSyncOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.RenderScale, () => ApplyRenderScaleOption(name, (float)obj, isChanged) },
                { OptionTypeEnum.FSRSharpness, () => ApplyFSRSharpnessOption(name, (float)obj, isChanged) },
                { OptionTypeEnum.Antialiasing, () => ApplyAntialiasingOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.Anisotropic, () => ApplyAnisotropicOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.TextureQuality, () => ApplyTextureQualityOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.ShadowDistance, () => ApplyShadowDistanceOption(name, (int)obj, isChanged) },
                { OptionTypeEnum.GlobalVolume, () => ApplyGlobalVolumeOption(name, (float)obj, isChanged) }
            };

            options[optionType].Invoke();
            option.Option.IsChanged = false;
        }

        private void ApplyCustomOption(OptionObject option, string name, object obj)
        {
            object convertedValue = option.OptionValue switch
            {
                OptionValueEnum.Boolean => Convert.ToBoolean(obj),
                OptionValueEnum.Integer => Convert.ToInt32(obj),
                OptionValueEnum.Float => Convert.ToSingle(obj),
                OptionValueEnum.String => obj.ToString(),
                _ => obj.ToString(),
            };

            if (option.Option.IsChanged && _optionSubjects.TryGetValue(name, out var subject))
                subject.OnNext(convertedValue);

            _serializableData[name] = new(convertedValue);
        }

        private void LoadOptions(OptionObject option, bool fromFile)
        {
            var behaviour = option.Option;
            var optionType = option.OptionType;
            string name = option.Name.ToLower();

            Dictionary<OptionTypeEnum, Action> options = new()
            {
                { OptionTypeEnum.Custom, () => LoadCustomOption(name, fromFile, behaviour, option) },
                { OptionTypeEnum.Monitor, () => LoadMonitorOption(name, fromFile, behaviour) },
                { OptionTypeEnum.Resolution, () => LoadResoltionOption(name, fromFile, behaviour) },
                { OptionTypeEnum.Fullscreen, () => LoadFullscreenOption(name, fromFile, behaviour) },
                { OptionTypeEnum.FrameRate, () => LoadFramerateOption(name, fromFile, behaviour) },
                { OptionTypeEnum.VSync, () => LoadVSyncOption(name, fromFile, behaviour) },
                { OptionTypeEnum.RenderScale, () => LoadRenderScaleOption(name, fromFile, behaviour) },
                { OptionTypeEnum.FSRSharpness, () => LoadFSRSharpnessOption(name, fromFile, behaviour) },
                { OptionTypeEnum.Antialiasing, () => LoadAntialiasingOption(name, fromFile, behaviour) },
                { OptionTypeEnum.Anisotropic, () => LoadAnisotropicOption(name, fromFile, behaviour) },
                { OptionTypeEnum.TextureQuality, () => LoadTextureQualityOption(name, fromFile, behaviour) },
                { OptionTypeEnum.ShadowDistance, () => LoadShadowDistanceOption(name, fromFile, behaviour) },
                { OptionTypeEnum.GlobalVolume, () => LoadGlobalVolumeOption(name, fromFile, behaviour) }
            };

            options[optionType].Invoke();
        }

        private void LoadCustomOption(string name, bool fromFile, OptionBehaviour behaviour, OptionObject option)
        {
            object value = string.IsNullOrEmpty(option.DefaultValue) ? "0" : option.DefaultValue;
            object optionValue, subjectValue;

            if (fromFile && _serializableData.TryGetValue(name, out JValue jValue))
            {
                optionValue = option.OptionValue switch
                {
                    OptionValueEnum.Boolean => jValue.ToObject<bool>() ? 1 : 0,
                    OptionValueEnum.Integer => jValue.ToObject<int>(),
                    OptionValueEnum.Float => jValue.ToObject<float>(),
                    _ => jValue.ToString(),
                };

                subjectValue = option.OptionValue switch
                {
                    OptionValueEnum.Boolean => jValue.ToObject<bool>(),
                    OptionValueEnum.Integer => jValue.ToObject<int>(),
                    OptionValueEnum.Float => jValue.ToObject<float>(),
                    _ => jValue.ToString(),
                };
            }
            else
            {
                optionValue = option.OptionValue switch
                {
                    OptionValueEnum.Boolean => Convert.ToInt32(value),
                    OptionValueEnum.Integer => Convert.ToInt32(value),
                    OptionValueEnum.Float => Convert.ToSingle(value),
                    _ => value.ToString(),
                };

                subjectValue = option.OptionValue switch
                {
                    OptionValueEnum.Boolean => Convert.ToInt32(value) == 1,
                    OptionValueEnum.Integer => Convert.ToInt32(value),
                    OptionValueEnum.Float => Convert.ToSingle(value),
                    _ => value.ToString(),
                };
            }

            if (_optionSubjects.TryGetValue(name, out var subject))
                subject.OnNext(subjectValue);

            behaviour.SetOptionValue(optionValue);
        }

        private async Task SerializeOptions()
        {
            string json = JsonConvert.SerializeObject(_serializableData, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await File.WriteAllTextAsync(_optionsPath, json);
        }

        private async Task DeserializeOptions()
        {
            string json = await File.ReadAllTextAsync(_optionsPath);
            _serializableData = JsonConvert.DeserializeObject<Dictionary<string, JValue>>(json);
        }
    }
}