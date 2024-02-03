using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(DynamicObject))]
    public class DynamicObjectEditor : Editor
    {
        private DynamicObject _target;

        private SerializedProperty _dynamicType;
        private SerializedProperty _dynamicStatus;
        private SerializedProperty _interactType;
        private SerializedProperty _statusChange;

        private SerializedProperty _mTarget;
        private SerializedProperty _animator;
        private SerializedProperty _joint;
        private SerializedProperty _rigidbody;

        private SerializedProperty _unlockScript;
        private SerializedProperty _keepUnlockItem;
        private SerializedProperty _unlockItem;
        private SerializedProperty _showLockedText;
        private SerializedProperty _lockedText;

        // ignore colliders
        private SerializedProperty _ignoreColliders;
        private ReorderableList _ignoreCollidersList;

        // animation triggers
        private SerializedProperty _useTrigger1;
        private SerializedProperty _useTrigger2;
        private SerializedProperty _useTrigger3;

        // dynamic types
        private SerializedProperty _openable;
        private PropertyCollection _openableProperties;

        private SerializedProperty _pullable;
        private PropertyCollection _pullableProperties;

        private SerializedProperty _switchable;
        private PropertyCollection _switchableProperties;

        private SerializedProperty _rotable;
        private PropertyCollection _rotableProperties;

        private SerializedProperty _useEvent1;
        private SerializedProperty _useEvent2;
        private SerializedProperty _onValueChange;
        private SerializedProperty _lockedEvent;
        private SerializedProperty _unlockedEvent;

        private SerializedProperty _lockPlayer;

        private DynamicObject.DynamicType _dynamicTypeEnum;
        private DynamicObject.InteractType _interactTypeEnum;
        private DynamicObject.DynamicStatus _dynamicStatusEnum;
        private DynamicObject.StatusChange _statusChangeEnum;

        private void OnEnable()
        {
            _target = target as DynamicObject;

            _dynamicType = serializedObject.FindProperty("_dynamicType");
            _dynamicStatus = serializedObject.FindProperty("_dynamicStatus");
            _interactType = serializedObject.FindProperty("_interactType");
            _statusChange = serializedObject.FindProperty("_statusChange");

            _mTarget = serializedObject.FindProperty("_target");
            _animator = serializedObject.FindProperty("_animator");
            _joint = serializedObject.FindProperty("_joint");
            _rigidbody = serializedObject.FindProperty("_rigidbody");

            _unlockScript = serializedObject.FindProperty("_unlockScript");
            _keepUnlockItem = serializedObject.FindProperty("KeepUnlockItem");
            _unlockItem = serializedObject.FindProperty("_unlockItem");
            _showLockedText = serializedObject.FindProperty("_showLockedText");
            _lockedText = serializedObject.FindProperty("_lockedText");

            _ignoreColliders = serializedObject.FindProperty("_ignoreColliders");
            _ignoreCollidersList = new ReorderableList(serializedObject, _ignoreColliders, true, false, true, true);
            _ignoreCollidersList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = _ignoreColliders.GetArrayElementAtIndex(index);
                rect.y += EditorGUIUtility.standardVerticalSpacing;
                ReorderableList.defaultBehaviours.DrawElement(rect, element, null, isActive, isFocused, true, true);
            };

            _useTrigger1 = serializedObject.FindProperty("_useTrigger1");
            _useTrigger2 = serializedObject.FindProperty("_useTrigger2");
            _useTrigger3 = serializedObject.FindProperty("_useTrigger3");

            // dynamic types
            {
                _openable = serializedObject.FindProperty("_openable");
                _openableProperties = EditorDrawing.GetAllProperties(_openable);

                _pullable = serializedObject.FindProperty("_pullable");
                _pullableProperties = EditorDrawing.GetAllProperties(_pullable);

                _switchable = serializedObject.FindProperty("_switchable");
                _switchableProperties = EditorDrawing.GetAllProperties(_switchable);

                _rotable = serializedObject.FindProperty("_rotable");
                _rotableProperties = EditorDrawing.GetAllProperties(_rotable);
            }

            _useEvent1 = serializedObject.FindProperty("_useEvent1");
            _useEvent2 = serializedObject.FindProperty("_useEvent2");
            _onValueChange = serializedObject.FindProperty("_onValueChange");
            _lockedEvent = serializedObject.FindProperty("_lockedEvent");
            _unlockedEvent = serializedObject.FindProperty("_unlockedEvent");

            _lockPlayer = serializedObject.FindProperty("_lockPlayer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDynamicTypeGroup();
            EditorGUILayout.Space();
            EditorDrawing.Separator();
            EditorGUILayout.Space();

            _dynamicTypeEnum = (DynamicObject.DynamicType)_dynamicType.enumValueIndex;
            _interactTypeEnum = (DynamicObject.InteractType)_interactType.enumValueIndex;
            _dynamicStatusEnum = (DynamicObject.DynamicStatus)_dynamicStatus.enumValueIndex;
            _statusChangeEnum = (DynamicObject.StatusChange)_statusChange.enumValueIndex;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.PropertyField(_interactType);
                EditorGUILayout.PropertyField(_dynamicStatus);

                if (_dynamicStatusEnum != DynamicObject.DynamicStatus.Normal)
                    EditorGUILayout.PropertyField(_statusChange);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (_dynamicStatusEnum == DynamicObject.DynamicStatus.Locked)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Status Change", EditorStyles.boldLabel);

                if (_statusChangeEnum == DynamicObject.StatusChange.InventoryItem)
                {
                    EditorGUILayout.PropertyField(_unlockItem, new GUIContent("Unlock Item"));
                    EditorGUILayout.PropertyField(_keepUnlockItem);
                }
                else if (_statusChangeEnum == DynamicObject.StatusChange.CustomScript)
                {
                    EditorGUILayout.Space(1f);
                    EditorGUILayout.PropertyField(_unlockScript);
                }

                EditorGUILayout.PropertyField(_showLockedText);
                if (_showLockedText.boolValue)
                {
                    EditorGUILayout.PropertyField(_lockedText);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);

            switch (_dynamicTypeEnum)
            {
                case DynamicObject.DynamicType.Openable:
                    DrawOpenableProperties();
                    break;
                case DynamicObject.DynamicType.Pullable:
                    DrawPullableProperties();
                    break;
                case DynamicObject.DynamicType.Switchable:
                    DrawSwitchableProperties();
                    break;
                case DynamicObject.DynamicType.Rotable:
                    DrawRotableProperties();
                    break;
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_useEvent1, new GUIContent("Event Settings")))
            {
                EditorGUILayout.PropertyField(_useEvent1, new GUIContent("OnOpen"));
                EditorGUILayout.PropertyField(_useEvent2, new GUIContent("OnClose"));

                if(_interactTypeEnum != DynamicObject.InteractType.Animation)
                    EditorGUILayout.PropertyField(_onValueChange, new GUIContent("OnValueChange"));

                if (_dynamicStatusEnum == DynamicObject.DynamicStatus.Locked)
                {
                    EditorGUILayout.PropertyField(_lockedEvent, new GUIContent("OnLocked"));
                    EditorGUILayout.PropertyField(_unlockedEvent, new GUIContent("OnUnlocked"));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDynamicTypeGroup()
        {
            GUIContent[] toolbarContent = {
                new GUIContent(Resources.Load<Texture>("EditorIcons/icon_openable"), "Openable"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/icon_pullable"), "Pullable"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/icon_switchable"), "Switchable"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/icon_rotable"), "Rotable")
            };

            Vector2 prevIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(18, 18));

            GUIStyle toolbarButtons = new GUIStyle(GUI.skin.button);
            toolbarButtons.fixedHeight = 0;
            toolbarButtons.fixedWidth = 40;

            Rect toolbarRect = EditorGUILayout.GetControlRect(false, 25);
            toolbarRect.width = 40 * toolbarContent.Length;
            toolbarRect.x = EditorGUIUtility.currentViewWidth / 2 - toolbarRect.width / 2 + 7f;

            _dynamicType.enumValueIndex = GUI.Toolbar(toolbarRect, _dynamicType.enumValueIndex, toolbarContent, toolbarButtons);

            EditorGUIUtility.SetIconSize(prevIconSize);
        }

        private void DrawOpenableProperties()
        {
            switch (_interactTypeEnum)
            {
                case DynamicObject.InteractType.Dynamic:
                    EditorGUILayout.PropertyField(_mTarget);
                    break;
                case DynamicObject.InteractType.Mouse:
                    EditorGUILayout.PropertyField(_mTarget);
                    EditorGUILayout.PropertyField(_joint);
                    EditorGUILayout.PropertyField(_rigidbody);
                    break;
                case DynamicObject.InteractType.Animation:
                    EditorGUILayout.PropertyField(_mTarget);
                    EditorGUILayout.PropertyField(_animator); 
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            if (_interactTypeEnum != DynamicObject.InteractType.Animation)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_openableProperties["_openLimits"], new GUIContent("Dynamic Limits")))
                {
                    _openableProperties.Draw("_openLimits");

                    float minLimit = _openableProperties["_openLimits"].FindPropertyRelative("Min").floatValue;
                    float maxLimit = _openableProperties["_openLimits"].FindPropertyRelative("Max").floatValue;
                    SerializedProperty startAngle = _openableProperties["_startingAngle"];
                    startAngle.floatValue = EditorGUILayout.Slider(new GUIContent(startAngle.displayName), startAngle.floatValue, minLimit, maxLimit);
                    EditorGUILayout.Space(1f);

                    _openableProperties.Draw("_limitsForward");
                    _openableProperties.Draw("_limitsUpward");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
            }
            else
            {
                _target.Openable.DragSounds = false;
                if (EditorDrawing.BeginFoldoutBorderLayout(_openable, new GUIContent("Animation Settings")))
                {
                    EditorGUILayout.PropertyField(_useTrigger1, new GUIContent("Open Trigger Name"));
                    EditorGUILayout.PropertyField(_useTrigger2, new GUIContent("Close Trigger Name"));
                    if (_openableProperties["_bothSidesOpen"].boolValue)
                        EditorGUILayout.PropertyField(_useTrigger3, new GUIContent("OpenSide Name"));

                    EditorGUILayout.Space();
                    _openableProperties.Draw("_playCloseSound");
                    if (_openableProperties.DrawGetBool("_bothSidesOpen"))
                        _openableProperties.Draw("_openableForward", new GUIContent("Frame Forward"));

                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            if (_interactTypeEnum == DynamicObject.InteractType.Dynamic)
            {
                _target.Openable.DragSounds = false;
                if (EditorDrawing.BeginFoldoutBorderLayout(_openable, new GUIContent("Dynamic Settings")))
                {
                    _openableProperties.Draw("_openSpeed");
                    _openableProperties.Draw("_openCurve");
                    _openableProperties.Draw("_closeCurve");

                    if (_openableProperties.BoolValue("_bothSidesOpen"))
                        _openableProperties.Draw("_openableForward", new GUIContent("Frame Forward"));

                    if (_openableProperties.BoolValue("_useUpwardDirection"))
                        _openableProperties.Draw("_openableUp", new GUIContent("Openable Upward"));

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
                    _openableProperties.Draw("_flipOpenDirection");
                    _openableProperties.Draw("_flipForwardDirection");
                    _openableProperties.Draw("_useUpwardDirection");
                    _openableProperties.Draw("_bothSidesOpen");
                    _openableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            else if(_interactTypeEnum == DynamicObject.InteractType.Mouse)
            {
                _target.Openable.BothSidesOpen = true;
                if (EditorDrawing.BeginFoldoutBorderLayout(_openable, new GUIContent("Mouse Settings")))
                {
                    _openableProperties.Draw("_openableForward", new GUIContent("Target Forward"));
                    _openableProperties.Draw("_openableUp", new GUIContent("Target Upward"));
                    EditorGUILayout.Space();

                    _openableProperties.Draw("_openSpeed");
                    _openableProperties.Draw("_damper");
                    if(_openableProperties["_dragSounds"].boolValue)
                        _openableProperties.Draw("_dragSoundPlay");
                    EditorGUILayout.Space();

                    _openableProperties.Draw("_dragSounds");
                    _openableProperties.Draw("_flipMouse");
                    _openableProperties.Draw("_flipValue");
                    _openableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_openableProperties["_useLockedMotion"], new GUIContent("Locked Settings")))
            {
                _openableProperties.Draw("_useLockedMotion");
                _openableProperties.Draw("_lockedPattern");
                _openableProperties.Draw("_lockedMotionAmount");
                _openableProperties.Draw("_lockedMotionTime");
                EditorDrawing.EndBorderHeaderLayout();
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_ignoreColliders, new GUIContent("Ignore Colliders")))
            {
                _ignoreCollidersList.DoLayoutList();
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawPullableProperties()
        {
            switch (_interactTypeEnum)
            {
                case DynamicObject.InteractType.Dynamic:
                case DynamicObject.InteractType.Mouse:
                    EditorGUILayout.PropertyField(_mTarget);
                    break;
                case DynamicObject.InteractType.Animation:
                    EditorGUILayout.PropertyField(_animator);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            if (_interactTypeEnum != DynamicObject.InteractType.Animation)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_pullableProperties["_openLimits"], new GUIContent("Dynamic Limits")))
                {
                    _pullableProperties.Draw("_openLimits");
                    _pullableProperties.Draw("_pullAxis");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
            }
            else
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_pullable, new GUIContent("Animation Settings")))
                {
                    EditorGUILayout.PropertyField(_useTrigger1, new GUIContent("Open Trigger Name"));
                    EditorGUILayout.PropertyField(_useTrigger2, new GUIContent("Close Trigger Name"));
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            if (_interactTypeEnum == DynamicObject.InteractType.Dynamic)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_pullable, new GUIContent("Dynamic Settings")))
                {
                    _pullableProperties.Draw("_openCurve");
                    _pullableProperties.Draw("_openSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            else if(_interactTypeEnum == DynamicObject.InteractType.Mouse)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_pullable, new GUIContent("Mouse Settings")))
                {
                    _pullableProperties.Draw("_openSpeed");
                    _pullableProperties.Draw("_damping");
                    if (_pullableProperties["_dragSounds"].boolValue)
                        _pullableProperties.Draw("_dragSoundPlay");
                    EditorGUILayout.Space();

                    _pullableProperties.Draw("_dragSounds");
                    _pullableProperties.Draw("_flipMouse");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_ignoreColliders, new GUIContent("Ignore Colliders")))
            {
                _ignoreCollidersList.DoLayoutList();
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawSwitchableProperties()
        {
            switch (_interactTypeEnum)
            {
                case DynamicObject.InteractType.Dynamic:
                case DynamicObject.InteractType.Mouse:
                    EditorGUILayout.PropertyField(_mTarget);
                    break;
                case DynamicObject.InteractType.Animation:
                    EditorGUILayout.PropertyField(_animator);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            if (_interactTypeEnum != DynamicObject.InteractType.Animation)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_switchableProperties["_switchLimits"], new GUIContent("Dynamic Limits")))
                {
                    _switchableProperties.Draw("_switchLimits");

                    float minLimit = _switchableProperties["_switchLimits"].FindPropertyRelative("Min").floatValue;
                    float maxLimit = _switchableProperties["_switchLimits"].FindPropertyRelative("Max").floatValue;
                    SerializedProperty startAngle = _switchableProperties["_startingAngle"];
                    startAngle.floatValue = EditorGUILayout.Slider(new GUIContent(startAngle.displayName), startAngle.floatValue, minLimit, maxLimit);

                    _switchableProperties.Draw("_limitsForward");
                    _switchableProperties.Draw("_limitsUpward");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
            }
            else
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_switchable, new GUIContent("Animation Settings")))
                {
                    EditorGUILayout.PropertyField(_useTrigger1, new GUIContent("SwitchOn Trigger Name"));
                    EditorGUILayout.PropertyField(_useTrigger2, new GUIContent("SwitchOff Trigger Name"));
                    EditorGUILayout.Space();

                    _switchableProperties.Draw("_lockOnSwitch");

                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            if (_interactTypeEnum == DynamicObject.InteractType.Dynamic)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_switchable, new GUIContent("Dynamic Settings")))
                {
                    _switchableProperties.Draw("_rootObject");
                    _switchableProperties.Draw("_switchOnCurve");
                    _switchableProperties.Draw("_switchOffCurve");
                    _switchableProperties.Draw("_switchSpeed");
                    EditorGUILayout.Space();

                    _switchableProperties.Draw("_flipSwitchDirection");
                    _switchableProperties.Draw("_lockOnSwitch");
                    _switchableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            else if (_interactTypeEnum == DynamicObject.InteractType.Mouse)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_switchable, new GUIContent("Mouse Settings")))
                {
                    _switchableProperties.Draw("_rootObject");
                    _switchableProperties.Draw("_switchSpeed");
                    _switchableProperties.Draw("_damping");
                    EditorGUILayout.Space();

                    _switchableProperties.Draw("_lockOnSwitch");
                    _switchableProperties.Draw("_flipMouse");
                    _switchableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_ignoreColliders, new GUIContent("Ignore Colliders")))
            {
                _ignoreCollidersList.DoLayoutList();
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawRotableProperties()
        {
            switch (_interactTypeEnum)
            {
                case DynamicObject.InteractType.Dynamic:
                case DynamicObject.InteractType.Mouse:
                    EditorGUILayout.PropertyField(_mTarget);
                    break;
                case DynamicObject.InteractType.Animation:
                    EditorGUILayout.PropertyField(_animator);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            if (_interactTypeEnum != DynamicObject.InteractType.Animation)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_rotableProperties["_rotationLimit"], new GUIContent("Dynamic Limits")))
                {
                    _rotableProperties.Draw("_rotationLimit");
                    _rotableProperties.Draw("_rotateAroundAxis");
                    _rotableProperties.Draw("_rotationOffset");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
            }
            else
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_rotable, new GUIContent("Animation Settings")))
                {
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            if (_interactTypeEnum == DynamicObject.InteractType.Dynamic)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_rotable, new GUIContent("Dynamic Settings")))
                {
                    _rotableProperties.Draw("_rotateCurve");
                    _rotableProperties.Draw("_rotationSpeed");
                    EditorGUILayout.Space();

                    _rotableProperties.Draw("_holdToRotate");
                    _rotableProperties.Draw("_lockOnRotate");
                    EditorGUILayout.PropertyField(_lockPlayer);
                    _rotableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            else if (_interactTypeEnum == DynamicObject.InteractType.Mouse)
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(_rotable, new GUIContent("Mouse Settings")))
                {
                    _rotableProperties.Draw("_rotationSpeed");
                    _rotableProperties.Draw("_mouseMultiplier");
                    _rotableProperties.Draw("_damping");
                    EditorGUILayout.Space();

                    _rotableProperties.Draw("_lockOnRotate");
                    _rotableProperties.Draw("_showGizmos");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }

            EditorGUILayout.Space(1f);
            if (EditorDrawing.BeginFoldoutBorderLayout(_ignoreColliders, new GUIContent("Ignore Colliders")))
            {
                _ignoreCollidersList.DoLayoutList();
                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}