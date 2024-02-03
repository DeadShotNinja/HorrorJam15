using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using HJ.Scriptable;
using HJ.Runtime;

namespace HJ.Input
{
    public class InputManager : Singleton<InputManager>
    {
        [Header("Input Setup")]
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private InputSpritesAsset _inputSpritesAsset;
        [SerializeField] private bool _debugMode;
        
        public const string EXTENSION = ".xml";
        public const string NULL = "null";
        private const string IGNORE_TAG = "*";

        public Dictionary<string, ActionMap> ActionMaps = new();
        public List<RebindContext> PreparedRebinds = new();
        public CompositeDisposable Disposables = new();

        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        private readonly Dictionary<string, bool> _toggledActions = new();
        private readonly List<string> _pressedActions = new();

        public InputActionAsset InputActions => _inputActions;
        public InputSpritesAsset InputSpritesAsset => _inputSpritesAsset;
        public bool DebugMode => _debugMode;

        public ReplaySubject<Unit> OnInputsInit = new();
        public Subject<bool> OnApply = new();

        public Subject<RebindContext> OnRebindPrepare = new();
        public Subject<Unit> OnRebindStart = new();
        public Subject<bool> OnRebindEnd = new();

        public static InputActionAsset ActionsAsset
        {
            get => Instance._inputActions;
        }

        public static InputSpritesAsset SpritesAsset
        {
            get => Instance._inputSpritesAsset;
        }

        public Lazy<IList<HJ.Input.Action>> Actions { get; } = new(() =>
        {
            var actions = new List<HJ.Input.Action>();
            var playerMap = Instance.ActionMaps.First();

            foreach (var action in playerMap.Value.Actions)
            {
                if (!action.Key.Contains(IGNORE_TAG))
                    actions.Add(action.Value);
            }

            return actions;
        });

        private string _inputsFilename => SerializationUtillity.SerializationAsset.InputsFilename + EXTENSION;

        private string _inputsPath
        {
            get
            {
                string configPath = SerializationUtillity.SerializationAsset.GetConfigPath();
                if (!Directory.Exists(configPath))
                    Directory.CreateDirectory(configPath);

                return configPath + "/" + _inputsFilename;
            }
        }

        private async void Awake()
        {
            if (!_inputActions) throw new NullReferenceException("InputActionAsset is not assigned!");

            foreach (var map in _inputActions.actionMaps)
            {
                ActionMaps.Add(map.name, new ActionMap(map));
            }

            if (File.Exists(_inputsPath))
            {
                await ReadInputOverrides();
                if (_debugMode) Debug.Log($"[InputManager] {_inputsFilename} readed successfully.");
            }

            OnInputsInit.OnNext(Unit.Default);
            _inputActions.Enable();
        }

        private void OnDestroy()
        {
            Disposables.Dispose();
        }

        /// <summary>
        /// Subscribe listening to the performed input action event.
        /// </summary>
        public static void Performed(string name, Action<InputAction.CallbackContext> performed)
        {
            InputAction action = Action(name);
            var observable = PerformedObservable(name).Subscribe(performed);
            Instance.Disposables.Add(observable);
        }

        /// <summary>
        /// Get observable from performed input action event.
        /// </summary>
        public static IObservable<InputAction.CallbackContext> PerformedObservable(string name)
        {
            InputAction action = Action(name);
            return Observable.FromEvent<InputAction.CallbackContext>(
                handler => action.performed += handler,
                handler => action.performed -= handler);
        }

        /// <summary>
        /// Observe the input binding path change.
        /// </summary>
        public static IObservable<(bool apply, string path)> ObserveBindingPath(string actionName, int bindingIndex)
        {
            return Observable.Merge
            (
                Instance.OnRebindPrepare
                    .Where(ctx => ctx.Action.name == actionName && ctx.BindingIndex == bindingIndex)
                    .Select(ctx => (false, overridePath: ctx.OverridePath)),
                Instance.OnApply.Select(_ => (true, GetBindingPath(actionName, bindingIndex).EffectivePath)),
                Instance.OnInputsInit.Select(_ => (true, GetBindingPath(actionName, bindingIndex).EffectivePath))
            );
        }

        /// <summary>
        /// Find InputAction reference by name.
        /// </summary>
        public static InputAction FindAction(string name)
        {
            return Instance._inputActions.FindAction(name);
        }

        /// <summary>
        /// Get InputAction reference by name.
        /// </summary>
        public static InputAction Action(string name)
        {
            foreach (var map in Instance.ActionMaps)
            {
                if (map.Value.Actions.TryGetValue(name, out HJ.Input.Action action))
                    return action.InputAction;
            }

            Debug.LogError(new NullReferenceException($"[InputManager] Could not find input action with name \"{name}\"!").ToString());
            return null;
        }

        /// <summary>
        /// Get action map action by name.
        /// </summary>
        public static HJ.Input.Action ActionMapAction(string name)
        {
            foreach (var map in Instance.ActionMaps)
            {
                if (map.Value.Actions.TryGetValue(name, out HJ.Input.Action action))
                    return action;
            }

            Debug.LogError(new NullReferenceException($"[InputManager] Could not find action map action with name \"{name}\"!").ToString());
            return null;
        }

        /// <summary>
        /// Read input value as Type.
        /// </summary>
        public static T ReadInput<T>(string actionName) where T : struct
        {
            InputAction inputAction = Action(actionName);
            return inputAction.ReadValue<T>();
        }

        /// <summary>
        /// Read input value as object.
        /// </summary>
        public static object ReadInput(string actionName)
        {
            InputAction inputAction = Action(actionName);
            return inputAction.ReadValueAsObject();
        }

        /// <summary>
        /// Check whether the button is pressed and return its value.
        /// </summary>
        public static bool ReadInput<T>(string actionName, out T value) where T : struct
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.IsPressed())
            {
                value = inputAction.ReadValue<T>();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Check whether the button is pressed once and return its value.
        /// </summary>
        public static bool ReadInputOnce<T>(UnityEngine.Object obj, string actionName, out T value) where T : struct
        {
            string inputKey = actionName + "." + obj.GetInstanceID().ToString();
            InputAction inputAction = Action(actionName);

            if (inputAction.IsPressed())
            {
                if (!Instance._pressedActions.Contains(inputKey))
                {
                    Instance._pressedActions.Add(inputKey);
                    value = inputAction.ReadValue<T>();
                    return true;
                }
            }
            else
            {
                Instance._pressedActions.Remove(inputKey);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Check whether the button is pressed and return its value.
        /// </summary>
        public static bool ReadInput(string actionName, out object value)
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.IsPressed())
            {
                value = inputAction.ReadValueAsObject();
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Read input value as Button.
        /// </summary>
        public static bool ReadButton(string actionName)
        {
            InputAction inputAction = Action(actionName);
            if (inputAction.type == InputActionType.Button)
                return Convert.ToBoolean(inputAction.ReadValueAsObject());

            throw new NotSupportedException("[InputManager] The Input Action must be a button type!");
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(UnityEngine.Object obj, string actionName)
        {
            return ReadButtonOnce(obj.GetInstanceID().ToString(), actionName);
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(int instanceID, string actionName)
        {
            return ReadButtonOnce(instanceID.ToString(), actionName);
        }

        /// <summary>
        /// Read input as a button once.
        /// </summary>
        public static bool ReadButtonOnce(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButton(actionName))
            {
                if (!Instance._pressedActions.Contains(inputKey))
                {
                    Instance._pressedActions.Add(inputKey);
                    return true;
                }
            }
            else
            {
                Instance._pressedActions.Remove(inputKey);
            }

            return false;
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(UnityEngine.Object obj, string actionName)
        {
            return ReadButtonToggle(obj.GetInstanceID().ToString(), actionName);
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(int instanceID, string actionName)
        {
            return ReadButtonToggle(instanceID.ToString(), actionName);
        }

        /// <summary>
        /// Read input button as toggle (on/off).
        /// </summary>
        public static bool ReadButtonToggle(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButtonOnce(key, actionName))
            {
                if (!Instance._toggledActions.ContainsKey(inputKey))
                {
                    Instance._toggledActions.Add(inputKey, true);
                }
                else if (!Instance._toggledActions[inputKey])
                {
                    Instance._toggledActions.Remove(inputKey);
                }
            }
            else if (Instance._toggledActions.ContainsKey(inputKey))
            {
                Instance._toggledActions[inputKey] = false;
            }

            return Instance._toggledActions.ContainsKey(inputKey);
        }

        /// <summary>
        /// Reset toggled button.
        /// </summary>
        public static void ResetToggledButton(string key, string actionName)
        {
            string inputKey = actionName + "." + key;
            if (Instance._toggledActions.ContainsKey(inputKey))
                Instance._toggledActions.Remove(inputKey);
        }

        /// <summary>
        /// Reset toggled button.
        /// </summary>
        public static void ResetToggledButton(string actionName)
        {
            foreach (var toggledAction in Instance._toggledActions)
            {
                if (toggledAction.Key.Contains(actionName))
                {
                    Instance._toggledActions.Remove(toggledAction.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Reset all toggled buttons.
        /// </summary>
        public static void ResetToggledButtons()
        {
            Instance._toggledActions.Clear();
        }

        /// <summary>
        /// Check if any button is pressed.
        /// </summary>
        public static bool AnyKeyPressed()
        {
            Mouse mouse = Mouse.current;
            return Keyboard.current.anyKey.isPressed
                || mouse.leftButton.isPressed
                || mouse.rightButton.isPressed;
        }

        /// <summary>
        /// Get binding path of action.
        /// </summary>
        public static BindingPath GetBindingPath(string actionName, int bindingIndex = 0)
        {
            HJ.Input.Action action = ActionMapAction(actionName);

            if (action == null) 
                return null;

            HJ.Input.Binding binding = action.Bindings[bindingIndex];
            BindingPath bindingPath = binding.BindingPath;
            bindingPath.GetGlyphPath();
            return bindingPath;
        }

        /// <summary>
        /// Start action rebinding operation.
        /// </summary>
        public static void StartRebindOperation(string actionName, int bindingIndex = 0)
        {
            InputAction action = Action(actionName);

            Instance._inputActions.Disable();
            Instance.PerformInteractiveRebinding(action, bindingIndex);
            Instance.OnRebindStart.OnNext(Unit.Default);
            if (Instance._debugMode) Debug.Log("[InputManager] Rebind Started - Press any control.");
        }

        /// <summary>
        /// Start action rebinding operation.
        /// </summary>
        public static void StartRebindOperation(InputActionReference actionReference, int bindingIndex = 0)
        {
            InputAction action = actionReference.action;

            Instance._inputActions.Disable();
            Instance.PerformInteractiveRebinding(action, bindingIndex);
            Instance.OnRebindStart.OnNext(Unit.Default);
            if (Instance._debugMode) Debug.Log("[InputManager] Rebind Started - Press any control.");
        }

        /// <summary>
        /// Final apply of input overrides. (Without serialization)
        /// </summary>
        public static void SetInputRebindOverrides()
        {
            if (Instance.PreparedRebinds.Count > 0)
            {
                foreach (var rebind in Instance.PreparedRebinds)
                {
                    if (rebind.Action.bindings[rebind.BindingIndex].path == rebind.OverridePath || string.IsNullOrEmpty(rebind.OverridePath))
                    {
                        rebind.Action.RemoveBindingOverride(rebind.BindingIndex);
                    }
                    else
                    {
                        rebind.Action.ApplyBindingOverride(rebind.BindingIndex, rebind.OverridePath);
                    }

                    GetBindingPath(rebind.Action.name, rebind.BindingIndex)
                        .EffectivePath = rebind.OverridePath;
                }

                // write overrides
                Instance.PreparedRebinds.Clear();
            }

            if (Instance._debugMode) Debug.Log("[InputManager] Bindings Applied");
            Instance.OnApply.OnNext(true);
        }

        /// <summary>
        /// Final apply and serialization of input overrides.
        /// </summary>
        public static async void ApplyInputRebindOverrides()
        {
            if (Instance.PreparedRebinds.Count > 0)
            {
                foreach (var rebind in Instance.PreparedRebinds)
                {
                    if (rebind.Action.bindings[rebind.BindingIndex].path == rebind.OverridePath || string.IsNullOrEmpty(rebind.OverridePath))
                    {
                        rebind.Action.RemoveBindingOverride(rebind.BindingIndex);
                    }
                    else
                    {
                        rebind.Action.ApplyBindingOverride(rebind.BindingIndex, rebind.OverridePath);
                    }

                    GetBindingPath(rebind.Action.name, rebind.BindingIndex)
                        .EffectivePath = rebind.OverridePath;
                }

                // write overrides
                Instance.PreparedRebinds.Clear();
                await Instance.PackAndWriteOverrides();
            }

            if (Instance._debugMode) Debug.Log("[InputManager] Bindings Applied");
            Instance.OnApply.OnNext(true);
        }

        /// <summary>
        /// Discard prepared input overrides.
        /// </summary>
        public static void DiscardInputRebindOverrides()
        {
            if (Instance._debugMode) Debug.Log("[InputManager] Bindings Discarded");
            Instance.PreparedRebinds.Clear();
            Instance.OnApply.OnNext(false);
        }

        /// <summary>
        /// Discard prepared input overrides and reset them to default values.
        /// </summary>
        public static void ResetInputsToDefaults()
        {
            Instance.PreparedRebinds.Clear();

            foreach (var map in Instance.ActionMaps)
            {
                foreach (var action in map.Value.Actions)
                {
                    foreach (var binding in action.Value.Bindings)
                    {
                        Instance.PrepareRebind(new(action.Value.InputAction, binding.Value.BindingIndex, null));
                    }
                }
            }
        }

        private void PerformInteractiveRebinding(InputAction action, int bindingIndex)
        {
            _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnApplyBinding((operation, path) =>
                {
                    Debug.Log("Rebind Path: " + path);

                    // if there is a prepared binding with the same override path
                    if (AnyPreparedRebind(path, action, bindingIndex, out var dupPath))
                    {
                        Debug.Log("Any Prepared");
                        PrepareRebind(new(action, bindingIndex, path));
                        PrepareRebind(new(dupPath.Action, dupPath.BindingIndex, NULL));
                    }
                    // if a binding path with the same override path exists in the action map
                    else if (AnyBindingPath(path, action, bindingIndex, out var duplicate))
                    {
                        Debug.Log("Any Binding");
                        PrepareRebind(new(action, bindingIndex, path));
                        if (!PreparedRebinds.Any(x => x.BindingIndex == duplicate.bindingIndex))
                            PrepareRebind(new(duplicate.action, duplicate.bindingIndex, NULL));
                    }
                    // normal rebind
                    else
                    {
                        Debug.Log("Normal");
                        PrepareRebind(new(action, bindingIndex, path));
                    }
                })
                .OnComplete(_ =>
                {
                    if (_debugMode) Debug.Log("[InputManager] Rebind Completed");
                    _inputActions.Enable();
                    OnRebindEnd.OnNext(true);
                    CleanRebindOperation();
                })
                .OnCancel(_ =>
                {
                    if (_debugMode) Debug.Log("[InputManager] Rebind Cancelled");
                    _inputActions.Enable();
                    OnRebindEnd.OnNext(false);
                    CleanRebindOperation();
                })
                .WithCancelingThrough("<Keyboard>/escape")
                .Start();
        }

        private bool AnyPreparedRebind(string bindingPath, InputAction currentAction, int currentIndex, out RebindContext duplicate) 
        {
            foreach (var context in PreparedRebinds)
            {
                if (bindingPath == context.OverridePath && (context.Action != currentAction || context.Action == currentAction && context.BindingIndex != currentIndex))
                {
                    duplicate = context;
                    return true;
                }
            }

            duplicate = null;
            return false;
        }

        private bool AnyBindingPath(string bindingPath, InputAction currentAction, int currentIndex, out (InputAction action, int bindingIndex) duplicate)
        {
            foreach (var map in ActionMaps)
            {
                foreach (var action in map.Value.Actions)
                {
                    foreach (var binding in action.Value.Bindings)
                    {
                        if (action.Value.InputAction == currentAction && binding.Value.BindingIndex == currentIndex)
                            continue;

                        if (binding.Value.BindingPath.EffectivePath == bindingPath)
                        {
                            duplicate = (action.Value.InputAction, binding.Value.BindingIndex);
                            return true;
                        }
                    }
                }
            }

            duplicate = default;
            return false;
        }

        private void PrepareRebind(RebindContext context)
        {
            PreparedRebinds.RemoveAll(x => x == context);
            var bindingPath = GetBindingPath(context.Action.name, context.BindingIndex);

            // if context override path is null, set override path to default binding path
            if (string.IsNullOrEmpty(context.OverridePath))
                context.OverridePath = bindingPath.BindedPath;

            // if context override path is not same as binding effective path, add prepared rebind
            if (bindingPath.EffectivePath != context.OverridePath)
                PreparedRebinds.Add(context);

            // send prepare rebind event
            OnRebindPrepare.OnNext(context);
        }

        private XmlDocument WriteOverridesToXML()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("InputBinding");

            foreach (var map in ActionMaps.Take(2))
            {
                XmlNode node_map = xmlDoc.CreateElement("Map");

                foreach (var action in map.Value.Actions.Values)
                {
                    XmlNode node_action = xmlDoc.CreateElement("Action");
                    XmlAttribute attr_name = xmlDoc.CreateAttribute("Name");

                    attr_name.Value = action.InputAction.name;
                    node_action.Attributes.Append(attr_name);

                    foreach (var binding in action.Bindings)
                    {
                        XmlNode node_binding = xmlDoc.CreateElement("Binding");
                        XmlAttribute attr_index = xmlDoc.CreateAttribute("Index");
                        XmlAttribute attr_value = xmlDoc.CreateAttribute("Path");

                        attr_index.Value = binding.Value.BindingIndex.ToString();
                        node_binding.Attributes.Append(attr_index);

                        attr_value.Value = binding.Value.BindingPath.EffectivePath;
                        node_binding.Attributes.Append(attr_value);

                        node_action.AppendChild(node_binding);
                    }

                    if (node_action.HasChildNodes)
                    {
                        node_map.AppendChild(node_action);
                    }
                }

                if (node_map.HasChildNodes)
                {
                    rootNode.AppendChild(node_map);
                }
            }

            xmlDoc.AppendChild(rootNode);
            return xmlDoc;
        }

        private async Task PackAndWriteOverrides()
        {
            XmlDocument xml = WriteOverridesToXML();
            StringWriter sw = new();
            XmlTextWriter xtw = new(sw);

            xtw.Formatting = Formatting.Indented;
            xml.WriteTo(xtw);

            if (!Directory.Exists(SerializationUtillity.SerializationAsset.GetConfigPath()))
                Directory.CreateDirectory(SerializationUtillity.SerializationAsset.GetConfigPath());

            using StreamWriter stream = new(_inputsPath);
            await stream.WriteAsync(sw.ToString());
        }

        private async Task ReadInputOverrides()
        {
            using StreamReader sr = new(_inputsPath);
            string xmlData = await sr.ReadToEndAsync();

            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlData);

            foreach (XmlNode mapNode in xmlDoc.DocumentElement.ChildNodes)
            {
                foreach (XmlNode actionNode in mapNode.ChildNodes)
                {
                    string actionName = actionNode.Attributes["Name"].Value;

                    foreach (XmlNode bindingNode in actionNode.ChildNodes)
                    {
                        int bindingIndex = int.Parse(bindingNode.Attributes["Index"].Value);
                        string bindingPath = bindingNode.Attributes["Path"].Value;

                        var action = ActionMapAction(actionName);
                        var binding = action.Bindings[bindingIndex];

                        if (binding.BindingPath.EffectivePath != bindingPath)
                        {
                            action.InputAction.ApplyBindingOverride(bindingIndex, bindingPath);
                            binding.BindingPath.EffectivePath = bindingPath;
                        }
                    }
                }
            }
        }

        private void CleanRebindOperation()
        {
            _rebindingOperation?.Dispose();
            _rebindingOperation = null;
        }
    }
}
