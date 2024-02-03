using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Disposables;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using HJ.Scriptable;
using HJ.Runtime;
using HJ.Tools;

namespace HJ.Editors
{
    public class InventoryBuilder : EditorWindow
    {
        const float ITEMS_VIEW_WIDTH = 300f;
        const string NEW_ITEM_PREFIX = "NewItem";

        #region Structures
        [Serializable]
        public class ItemProperty
        {
            public bool isModified;
            public string GUID;

            public SerializedProperty icon;
            public SerializedProperty title;
            public SerializedProperty description;
            public SerializedProperty width;
            public SerializedProperty height;
            public SerializedProperty orientation;
            public SerializedProperty flipDirection;

            public SerializedProperty itemObject;
            public SerializedProperty settings;
            public SerializedProperty usableSettings;
            public SerializedProperty properties;
            public SerializedProperty combineSettings;
            public SerializedProperty localizationSettings;

            public ItemProperty(SerializedProperty guid, SerializedProperty item)
            {
                GUID = guid.stringValue;

                icon = item.FindPropertyRelative("Icon");
                title = item.FindPropertyRelative("Title");
                description = item.FindPropertyRelative("Description");
                width = item.FindPropertyRelative("Width");
                height = item.FindPropertyRelative("Height");
                orientation = item.FindPropertyRelative("Orientation");
                flipDirection = item.FindPropertyRelative("FlipDirection");

                itemObject = item.FindPropertyRelative("ItemObject");
                settings = item.FindPropertyRelative("Settings");
                usableSettings = item.FindPropertyRelative("UsableSettings");
                properties = item.FindPropertyRelative("Properties");
                combineSettings = item.FindPropertyRelative("CombineSettings");
                localizationSettings = item.FindPropertyRelative("LocalizationSettings");
            }
        }

        [Serializable]
        public class TempBuilderData
        {
            public SerializedObject AssetObject;
            public SerializedProperty ItemsArray;
            public Dictionary<string, ItemProperty> ItemProperties;

            public TempBuilderData(InventoryAsset asset)
            {
                AssetObject = new SerializedObject(asset);
                ItemsArray = AssetObject.FindProperty("Items");

                ItemProperties = new Dictionary<string, ItemProperty>();
                for (int i = 0; i < ItemsArray.arraySize; i++)
                {
                    SerializedProperty property = ItemsArray.GetArrayElementAtIndex(i);
                    Add(property);
                }
            }

            public void Add(SerializedProperty property, bool add = false)
            {
                SerializedProperty guid = property.FindPropertyRelative("Guid");
                SerializedProperty item = property.FindPropertyRelative("Item");
                ItemProperties.Add(guid.stringValue, new ItemProperty(guid, item) { isModified = add });
            }
        }
        #endregion

        private InventoryAsset _asset;
        private TempBuilderData _tempBuilderData;
        private PlayerItemsManager _playerItemsManager;
        private CompositeDisposable _disposables = new();

        private bool _builderDirty = false;
        private string _selectedGuid = "";
        private Vector2 _scrollPosition;

        [SerializeField]
        private TreeViewState _itemsViewState;
        private ItemsTreeView _itemsTreeView;

        private float _spacing => EditorGUIUtility.standardVerticalSpacing * 2;

        private bool ConfirmSaveChangesIfNeeded()
        {
            if (_builderDirty)
            {
                if (EditorUtility.DisplayDialog("Inventory data has been modified",
                    $"Do you want to save the changes? Your changes will be lost if you don't save them.", "Save", "Don't Save"))
                {
                    SaveAsset();
                }
                else
                {
                    Resources.UnloadAsset(_asset);
                }
            }

            return true;
        }

        private bool EditorWantsToQuit()
        {
            return ConfirmSaveChangesIfNeeded();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            ConfirmSaveChangesIfNeeded();
            EditorApplication.wantsToQuit -= EditorWantsToQuit;
        }

        private void OnEnable()
        {
            EditorApplication.wantsToQuit += EditorWantsToQuit;
        }

        private void SaveAsset()
        {
            _tempBuilderData.AssetObject.ApplyModifiedProperties();

            RebuildAssetItems();
            _tempBuilderData.AssetObject.Update();

            _disposables.Dispose();
            _disposables = new();
            InitializeTreeView();

            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssetIfDirty(_asset);
        }

        public void Show(InventoryAsset asset)
        {
            this._asset = asset;
            _selectedGuid = "";
            InitializeTreeView();

            if(PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
                _playerItemsManager = PlayerPresenceManager.Instance.Player.GetComponentInChildren<PlayerItemsManager>();
        }

        private void InitializeTreeView()
        {
            _tempBuilderData = new TempBuilderData(_asset);
            _itemsViewState = new TreeViewState();
            _itemsTreeView = new ItemsTreeView(_itemsViewState, _tempBuilderData);
            _itemsTreeView.OnItemSelect.Subscribe(OnItemSelect).AddTo(_disposables);
            _itemsTreeView.OnAddNewItem.Subscribe(_ => OnAddNewItem()).AddTo(_disposables);
            _itemsTreeView.OnDeleteItem.Subscribe(_ => OnDeleteItem()).AddTo(_disposables);
        }

        private void OnItemSelect(string guid)
        {
            _selectedGuid = guid;
        }

        private void OnAddNewItem()
        {
            string newGuid = GameTools.GetGuid();
            _asset.Items.Add(new InventoryAsset.ReferencedItem()
            {
                Guid = newGuid,
                Item = new Item() 
                { 
                    Title = NEW_ITEM_PREFIX,
                    UsableSettings = new Item.ItemUsableSettings() { PlayerItemIndex = -1 }
                }
            });

            _tempBuilderData.AssetObject.Update();
            _tempBuilderData.AssetObject.ApplyModifiedProperties();

            int itemIndex = _tempBuilderData.ItemsArray.arraySize - 1;
            SerializedProperty property = _tempBuilderData.ItemsArray.GetArrayElementAtIndex(itemIndex);
            SerializedProperty propertyGuid = property.FindPropertyRelative("Guid");

            if (propertyGuid.stringValue == newGuid)
            {
                _tempBuilderData.Add(property, true);
                _itemsTreeView.Reload();
                _builderDirty = true;
            }
        }

        private void OnDeleteItem()
        {
            _selectedGuid = string.Empty;
            _builderDirty = true;
        }

        private void RebuildAssetItems()
        {
            Dictionary<string, InventoryAsset.ReferencedItem> itemsDict = _asset.Items.ToDictionary(x => x.Guid, x => x);
            List<InventoryAsset.ReferencedItem> newItemsList = new();
            var actualItems = _tempBuilderData.ItemProperties.Keys.ToArray();

            foreach (var guid in actualItems)
            {
                var item = itemsDict[guid];
                newItemsList.Add(item);
            }

            _asset.Items = newItemsList;
        }

        private void OnGUI()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, 20f);
            GUI.Box(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            Rect saveBtn = toolbarRect;
            saveBtn.xMin = saveBtn.xMax - 100f;

            if (GUI.Button(saveBtn, "Save Asset", EditorStyles.toolbarButton))
            {
                SaveAsset();
                foreach (var item in _tempBuilderData.ItemProperties)
                    item.Value.isModified = false;

                _builderDirty = false;
            }

            Rect exportBtn = saveBtn;
            exportBtn.x -= 100f;

            if (GUI.Button(exportBtn, "Localize Items", EditorStyles.toolbarButton))
            {
                EditorWindow browser = GetWindow<InventoryItemsExport>(true, "Localize Inventory Items", true);
                browser.minSize = new Vector2(500, 185);
                browser.maxSize = new Vector2(500, 185);
                ((InventoryItemsExport)browser).Show(_asset);
            }

            Rect itemsRect = new Rect(5f, 25f, ITEMS_VIEW_WIDTH, position.height - 30f);
            _itemsTreeView.OnGUI(itemsRect);

            if(!string.IsNullOrEmpty(_selectedGuid))
            {
                ItemProperty itemProperty = _tempBuilderData.ItemProperties[_selectedGuid];
                Rect itemInspectorRect = new Rect(ITEMS_VIEW_WIDTH + 10f, 25f, position.width - ITEMS_VIEW_WIDTH - 15f, position.height - 30f);

                GUIContent itemInspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" ITEM INSPECTOR ({itemProperty.title.stringValue})", "PrefabVariant On Icon");
                EditorDrawing.DrawHeaderWithBorder(ref itemInspectorRect, itemInspectorTitle, 20f, false);

                Rect inspectorViewRect = itemInspectorRect;
                inspectorViewRect.y += _spacing;
                inspectorViewRect.yMax -= _spacing;
                inspectorViewRect.xMin += _spacing;
                inspectorViewRect.xMax -= _spacing;

                OnDrawItemInspector(inspectorViewRect, itemProperty);
            }
        }

        private void OnDrawItemInspector(Rect inspectorView, ItemProperty property)
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginArea(inspectorView);
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

                    // icon
                    Rect baseControlRect = EditorGUILayout.GetControlRect(false, 100);
                    Rect iconRect = baseControlRect;
                    iconRect.width = 100; iconRect.height = 100;
                    property.icon.objectReferenceValue = EditorDrawing.DrawLargeSpriteSelector(iconRect, property.icon.objectReferenceValue);

                    // title
                    Rect titleRect = baseControlRect;
                    titleRect.height = EditorGUIUtility.singleLineHeight;
                    titleRect.xMin = iconRect.xMax + EditorGUIUtility.standardVerticalSpacing * 2;
                    property.title.stringValue = EditorGUI.TextField(titleRect, property.title.stringValue);

                    // description
                    Rect descriptionRect = titleRect;
                    descriptionRect.y = 20f + _spacing;
                    descriptionRect.height = 80f - _spacing;
                    property.description.stringValue = EditorGUI.TextField(descriptionRect, property.description.stringValue);

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    
#if HJ_LOCALIZATION
                    EditorGUILayout.HelpBox("Game localization is enabled, the title and description text will be replaced from the localization asset. To change the text, go to the localization asset and change it from there. If the localization key in the localization section is incorrect, the current title and description will be used.", MessageType.Warning);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
#endif

                    // item grid view
                    using (new EditorDrawing.IconSizeScope(14))
                    {
                        GUIContent itemViewTitle = EditorGUIUtility.TrTextContentWithIcon("Item Grid View", "GridLayoutGroup Icon");
                        float previewBoxSize = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
                        Rect itemViewRect = EditorGUILayout.GetControlRect(false, 18f + 13f + previewBoxSize);
                        EditorDrawing.DrawHeaderWithBorder(ref itemViewRect, itemViewTitle, 18f, true);
                        {
                            Rect insideItemView = itemViewRect;
                            insideItemView.width -= 10f;
                            insideItemView.height -= 10f;
                            insideItemView.x += 5f;
                            insideItemView.y += 5f;

                            Rect previewControlRect = insideItemView;
                            previewControlRect.height = EditorGUIUtility.singleLineHeight;
                            previewControlRect.xMin += previewBoxSize + EditorGUIUtility.standardVerticalSpacing;

                            // width
                            Rect widthRect = previewControlRect;
                            EditorGUI.LabelField(widthRect, "Width");
                            widthRect.xMin += 50f;
                            property.width.intValue = (ushort)EditorGUI.Slider(widthRect, property.width.intValue, 1, 4);

                            // height
                            Rect heightRect = previewControlRect;
                            heightRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            EditorGUI.LabelField(heightRect, "Height");
                            heightRect.xMin += 50f;
                            property.height.intValue = (ushort)EditorGUI.Slider(heightRect, property.height.intValue, 1, 4);

                            // orientation
                            Rect orientationRect = previewControlRect;
                            orientationRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                            EditorGUI.LabelField(orientationRect, "Image Orientation");
                            orientationRect.xMin += 120f;
                            orientationRect.xMax -= 100f;
                            EditorGUI.PropertyField(orientationRect, property.orientation, GUIContent.none);

                            using (new EditorGUI.DisabledGroupScope(property.orientation.enumValueIndex == 0))
                            {
                                Rect flipDirectionRect = previewControlRect;
                                flipDirectionRect.y = orientationRect.y;
                                flipDirectionRect.xMin = orientationRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                                EditorGUI.PropertyField(flipDirectionRect, property.flipDirection, GUIContent.none);
                            }

                            // inventory preview
                            insideItemView.width = previewBoxSize;
                            DrawGLInventoryPreview(insideItemView, property.width.intValue, property.height.intValue);
                        }

                        EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                        // drop object
                        GUIContent itemObjectTitle = EditorGUIUtility.TrTextContentWithIcon("Item Object Reference", "Prefab On Icon");
                        using (new EditorDrawing.BorderBoxScope(itemObjectTitle, roundedBox: true))
                        {
                            EditorGUILayout.PropertyField(property.itemObject);
                        }
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // settings
                    EditorDrawing.DrawClassBorderFoldout(property.settings, new GUIContent("Settings"));

                    // properties
                    EditorDrawing.DrawClassBorderFoldout(property.properties, new GUIContent("Properties"));

                    // usable settings
                    bool isUsable = property.settings.FindPropertyRelative("IsUsable").boolValue;
                    if(isUsable && EditorDrawing.BeginFoldoutBorderLayout(property.usableSettings, new GUIContent("Usable Settings")))
                    {
                        SerializedProperty usableType = property.usableSettings.FindPropertyRelative("UsableType");
                        UsableType usableTypeEnum = (UsableType)usableType.enumValueIndex;

                        EditorGUILayout.PropertyField(usableType);
                        if(usableTypeEnum == UsableType.PlayerItem)
                        {
                            SerializedProperty playerItemIndex = property.usableSettings.FindPropertyRelative("PlayerItemIndex");
                            DrawPlayerItemPicker(playerItemIndex, new GUIContent("Player Item"));
                        }
                        else if(usableTypeEnum == UsableType.HealthItem)
                        {
                            SerializedProperty healthPoints = property.usableSettings.FindPropertyRelative("HealthPoints");
                            EditorGUILayout.PropertyField(healthPoints);
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    // combine settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(property.combineSettings, new GUIContent("Combine Settings"), 18f))
                    {
                        EditorGUILayout.LabelField("Combinations: " + property.combineSettings.arraySize, EditorStyles.miniBoldLabel);

                        for (int i = 0; i < property.combineSettings.arraySize; i++)
                        {
                            DrawCombination(property, i);
                        }

                        EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                        if (GUILayout.Button("Add Combination"))
                        {
                            int size = property.combineSettings.arraySize++;
                            SerializedProperty partnerElement = property.combineSettings.GetArrayElementAtIndex(size);
                            SerializedProperty partnerID = partnerElement.FindPropertyRelative("CombineWithID");
                            partnerID.stringValue = string.Empty;
                            MirrorCombination(null, partnerElement); // clear new combination values
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    // properties
                    EditorDrawing.DrawClassBorderFoldout(property.localizationSettings, new GUIContent("Localization Settings"));

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // reference id
                    using (new EditorGUI.DisabledGroupScope(true))
                        EditorGUILayout.LabelField("Reference ID: " + property.GUID);

                    EditorGUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }
            if (EditorGUI.EndChangeCheck())
            {
                _itemsTreeView.ChangeTitle(_selectedGuid, property.title.stringValue);
                property.isModified = true;
                _builderDirty = true;
            }
        }

        private void DrawPlayerItemPicker(SerializedProperty playerItemProperty, GUIContent title)
        {
            if (_playerItemsManager != null)
            {
                Rect playerItemPickerRect = EditorGUILayout.GetControlRect();
                playerItemPickerRect = EditorGUI.PrefixLabel(playerItemPickerRect, title);

                PlayerItemsPicker.PlayerItem[] playerItems = _playerItemsManager.PlayerItems
                    .Select((x, i) => new PlayerItemsPicker.PlayerItem() { Name = x.Name, Index = i }).ToArray();

                GUIContent playerItemFieldContent = new GUIContent("None");
                foreach (var item in playerItems)
                {
                    if (item.Index == playerItemProperty.intValue)
                    {
                        playerItemFieldContent = EditorGUIUtility.TrTextContentWithIcon(item.Name, "Prefab On Icon");
                        break;
                    }
                }

                if (EditorDrawing.ObjectField(playerItemPickerRect, playerItemFieldContent))
                {
                    PlayerItemsPicker playerItemsPicker = new PlayerItemsPicker(new AdvancedDropdownState(), playerItems);
                    playerItemsPicker.OnItemPressed += index =>
                    {
                        playerItemProperty.intValue = index;
                        _tempBuilderData.ItemsArray.serializedObject.ApplyModifiedProperties();
                    };

                    Rect playerItemsRect = playerItemPickerRect;
                    playerItemPickerRect.width = 250;
                    playerItemsPicker.Show(playerItemPickerRect);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("To enable player item picker, add a GameManager and a Player to the scene using the Scene Setup option from Tools. Because of this, the property is switched to the classic int property.", MessageType.Warning);
                EditorGUILayout.PropertyField(playerItemProperty, title);
            }
        }

        private void DrawCombination(ItemProperty property, int index)
        {
            SerializedProperty arrayProperty = property.combineSettings;
            SerializedProperty element = arrayProperty.GetArrayElementAtIndex(index);
            SerializedProperty combineWithID = element.FindPropertyRelative("CombineWithID");
            SerializedProperty resultCombineID = element.FindPropertyRelative("ResultCombineID");
            SerializedProperty playerItemIndex = element.FindPropertyRelative("PlayerItemIndex");
            SerializedProperty keepAfterCombine = element.FindPropertyRelative("KeepAfterCombine");
            SerializedProperty eventAfterCombine = element.FindPropertyRelative("EventAfterCombine");
            SerializedProperty selectAfterCombine = element.FindPropertyRelative("SelectAfterCombine");

            // partner property reference
            ItemProperty partnerProperty = null;
            if (!string.IsNullOrEmpty(combineWithID.stringValue))
                _tempBuilderData.ItemProperties.TryGetValue(combineWithID.stringValue, out partnerProperty);

            GUIContent headerGUI = new GUIContent($"Combination {index}");
            if (EditorDrawing.BeginFoldoutBorderLayout(element, headerGUI, out Rect headerRect, 18f, false))
            {
                // combine with id field
                Rect combineWithRect = EditorGUILayout.GetControlRect();
                combineWithRect = EditorGUI.PrefixLabel(combineWithRect, new GUIContent("Combine With Item"));
                combineWithRect.xMax -= 80f;

                GUIContent combineWithGUI = new GUIContent("Set Combination Partner");
                if(partnerProperty != null) combineWithGUI = new GUIContent(partnerProperty.title.stringValue);

                // combine with button
                if (GUI.Button(combineWithRect, combineWithGUI, EditorStyles.miniButton))
                {
                    ItemProperty[] itemProperties = _tempBuilderData.ItemProperties.Select(x => x.Value).ToArray();
                    ItemPicker itemPicker = new ItemPicker(new AdvancedDropdownState(), itemProperties);
                    itemPicker.OnItemPressed += obj =>
                    {
                        combineWithID.stringValue = obj.GUID;
                        _tempBuilderData.ItemsArray.serializedObject.ApplyModifiedProperties();
                    };

                    Rect dropdownRect = combineWithRect;
                    dropdownRect.width = 250;
                    itemPicker.Show(dropdownRect);
                }

                // combine with mirror button
                Rect mirrorBtnRect = combineWithRect;
                mirrorBtnRect.xMin = mirrorBtnRect.xMax;
                mirrorBtnRect.xMax += 80f;

                using (new EditorGUI.DisabledScope(partnerProperty == null || property.GUID == combineWithID.stringValue))
                {
                    if (GUI.Button(mirrorBtnRect, new GUIContent("Mirror", "Mirror combination with partner item."), EditorStyles.miniButton))
                    {
                        bool addNew = true;
                        for (int i = 0; i < partnerProperty.combineSettings.arraySize; i++)
                        {
                            SerializedProperty partnerElement = partnerProperty.combineSettings.GetArrayElementAtIndex(i);
                            SerializedProperty partnerID = partnerElement.FindPropertyRelative("CombineWithID");
                            if (partnerID.stringValue.Equals(property.GUID))
                            {
                                MirrorCombination(element, partnerElement);
                                addNew = false;
                                break;
                            }
                        }

                        if (addNew)
                        {
                            int size = partnerProperty.combineSettings.arraySize++;
                            SerializedProperty partnerElement = partnerProperty.combineSettings.GetArrayElementAtIndex(size);
                            SerializedProperty partnerID = partnerElement.FindPropertyRelative("CombineWithID");
                            partnerID.stringValue = property.GUID;
                            MirrorCombination(element, partnerElement);
                        }

                        Debug.Log($"[Inventory Builder] {headerGUI.text} was mirrored with {partnerProperty.title.stringValue}");
                    }
                }

                if (!selectAfterCombine.boolValue)
                {
                    // combine result field
                    Rect combineResultRect = EditorGUILayout.GetControlRect();
                    combineResultRect = EditorGUI.PrefixLabel(combineResultRect, new GUIContent("Combine Result Item"));

                    GUIContent combineResultGUI = new GUIContent("Set Combination Result");
                    if (!string.IsNullOrEmpty(resultCombineID.stringValue))
                    {
                        if (_tempBuilderData.ItemProperties.TryGetValue(resultCombineID.stringValue, out ItemProperty item))
                        {
                            combineResultGUI = new GUIContent(item.title.stringValue);
                        }
                    }

                    // combine result button
                    if (GUI.Button(combineResultRect, combineResultGUI, EditorStyles.miniButton))
                    {
                        ItemProperty[] itemProperties = _tempBuilderData.ItemProperties.Select(x => x.Value).ToArray();
                        ItemPicker itemPicker = new ItemPicker(new AdvancedDropdownState(), itemProperties);
                        itemPicker.OnItemPressed += obj =>
                        {
                            resultCombineID.stringValue = obj.GUID;
                            _tempBuilderData.ItemsArray.serializedObject.ApplyModifiedProperties();
                        };

                        Rect dropdownRect = combineResultRect;
                        dropdownRect.width = 250;
                        itemPicker.Show(dropdownRect);
                    }
                }
                else
                {
                    DrawPlayerItemPicker(playerItemIndex, new GUIContent("Result Player Item"));
                }

                // combination settings
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                EditorGUILayout.PropertyField(keepAfterCombine);
                EditorGUILayout.PropertyField(eventAfterCombine);
                EditorGUILayout.PropertyField(selectAfterCombine);
                EditorDrawing.EndBorderHeaderLayout();
            }

            Rect minusRect = headerRect;
            minusRect.xMin = minusRect.xMax - EditorGUIUtility.singleLineHeight;
            minusRect.y += 3f;
            minusRect.x -= 2f;

            if (GUI.Button(minusRect, EditorUtils.Styles.TrashIcon, EditorStyles.iconButton))
            {
                GenericMenu popup = new GenericMenu();

                popup.AddItem(new GUIContent("Delete"), false, () =>
                {
                    arrayProperty.DeleteArrayElementAtIndex(index);
                });

                if (partnerProperty != null)
                {
                    popup.AddItem(new GUIContent("Delete With Mirrored"), false, () =>
                    {
                        for (int i = 0; i < partnerProperty.combineSettings.arraySize; i++)
                        {
                            SerializedProperty partnerElement = partnerProperty.combineSettings.GetArrayElementAtIndex(i);
                            SerializedProperty partnerID = partnerElement.FindPropertyRelative("CombineWithID");

                            if(partnerID.stringValue == property.GUID)
                            {
                                partnerProperty.combineSettings.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        arrayProperty.DeleteArrayElementAtIndex(index);
                    });
                }
                else
                {
                    popup.AddDisabledItem(new GUIContent("Delete With Mirrored"));
                }

                popup.ShowAsContext();
            }
        }

        private void MirrorCombination(SerializedProperty current, SerializedProperty partner)
        {
            string resultID = current != null ? current.FindPropertyRelative("ResultCombineID").stringValue : "";
            partner.FindPropertyRelative("ResultCombineID").stringValue = resultID;

            bool keepAfterCombine = current != null && current.FindPropertyRelative("KeepAfterCombine").boolValue;
            partner.FindPropertyRelative("KeepAfterCombine").boolValue = keepAfterCombine;

            bool eventAfterCombine = current != null && current.FindPropertyRelative("EventAfterCombine").boolValue;
            partner.FindPropertyRelative("EventAfterCombine").boolValue = eventAfterCombine;
        }

        private void DrawGLInventoryPreview(Rect rect, int w, int h)
        {
            Material _uiMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float spacing = 5f;
            int slots = Math.Clamp(Math.Max(w, h), 2, 4);
            float slotSize = (rect.width - spacing * (slots + 1)) / slots;

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                {
                    GL.PushMatrix();
                    GL.LoadPixelMatrix();
                    _uiMaterial.SetPass(0);

                    Vector2 slotStart = new Vector2(spacing + 0.5f, spacing);

                    GL.Begin(GL.LINES);
                    {
                        // draw slots
                        Vector2 _tSlotStart = slotStart;
                        for (int y = 0; y < slots; y++)
                        {
                            for (int x = 0; x < slots; x++)
                            {
                                Vector2 slotLU = _tSlotStart + x * new Vector2(slotSize + spacing, 0);
                                Vector2 slotRU = slotLU + new Vector2(slotSize, 0);
                                Vector2 slotLD = slotLU + new Vector2(0, slotSize);
                                Vector2 slotRD = slotLU + new Vector2(slotSize, slotSize);

                                Line(slotLU, slotRU);
                                Line(slotRU, slotRD);
                                Line(slotRD, slotLD);
                                Line(slotLD, slotLU);
                            }

                            _tSlotStart.y += slotSize + spacing;
                        }
                    }
                    GL.End();

                    GL.Begin(GL.QUADS);
                    {
                        // draw item preview
                        Vector2 _tItemStart = slotStart;
                        _tItemStart.y -= 1;
                        GL.Color(Color.red.Alpha(0.35f));

                        Vector2 itemPrewWidth = w * new Vector2(slotSize, 0) + (w - 1) * new Vector2(spacing, 0) + new Vector2(1, 0);
                        Vector2 itemPrewHeight = h * new Vector2(0, slotSize) + (h - 1) * new Vector2(0, spacing) + new Vector2(0, 1);

                        Vector2 itemLU = _tItemStart;
                        Vector2 itemRU = itemLU + itemPrewWidth;
                        Vector2 itemLD = itemLU + itemPrewHeight;
                        Vector2 itemRD = itemLD + itemPrewWidth;

                        Line(itemLU, itemRU);
                        Line(itemRU, itemRD);
                        Line(itemRD, itemLD);
                        Line(itemLD, itemLU);
                    }
                    GL.End();
                    GL.PopMatrix();
                }
                GUI.EndClip();
            }
        }

        private void Line(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        internal class ItemPicker : AdvancedDropdown
        {
            private class ItemPickerDropdownItem : AdvancedDropdownItem
            {
                public ItemProperty itemProperty;

                public ItemPickerDropdownItem(ItemProperty itemProperty) : base(itemProperty.title.stringValue)
                {
                    this.itemProperty = itemProperty;
                }
            }

            private readonly ItemProperty[] properties;
            public event Action<ItemProperty> OnItemPressed;

            public ItemPicker(AdvancedDropdownState state, ItemProperty[] properties) : base(state)
            {
                this.properties = properties;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Inventory Items");

                if(properties.Length > 0)
                {
                    foreach (var property in properties)
                    {
                        var dropdownItem = new ItemPickerDropdownItem(property);
                        dropdownItem.icon = (Texture2D)EditorGUIUtility.TrIconContent("Prefab On Icon").image;
                        root.AddChild(dropdownItem);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                OnItemPressed?.Invoke((item as ItemPickerDropdownItem).itemProperty);
            }
        }

        internal class PlayerItemsPicker : AdvancedDropdown
        {
            public class PlayerItem
            {
                public string Name;
                public int Index;
            }

            private class PlayerItemsDropdownItem : AdvancedDropdownItem
            {
                public PlayerItem PlayerItem;

                public PlayerItemsDropdownItem(PlayerItem playerItem) : base(playerItem.Name)
                {
                    PlayerItem = playerItem;
                }
            }

            private readonly PlayerItem[] _playerItems;
            public event Action<int> OnItemPressed;

            public PlayerItemsPicker(AdvancedDropdownState state, PlayerItem[] playerItems) : base(state)
            {
                _playerItems = playerItems;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Player Items");

                if (_playerItems.Length > 0)
                {
                    root.AddChild(new PlayerItemsDropdownItem(new PlayerItem() { Name = "None", Index = -1}));

                    foreach (var item in _playerItems)
                    {
                        var dropdownItem = new PlayerItemsDropdownItem(item);
                        dropdownItem.icon = (Texture2D)EditorGUIUtility.TrIconContent("Prefab On Icon").image;
                        root.AddChild(dropdownItem);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                OnItemPressed?.Invoke((item as PlayerItemsDropdownItem).PlayerItem.Index);
            }
        }
    }
}