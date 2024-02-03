using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(InteractableItem))]
    public class InteractableItemEditor : InspectorEditor<InteractableItem>
    {
        private readonly bool[] foldout = new bool[6];

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                InteractableItem.InteractableTypeEnum interactableTypeEnum = (InteractableItem.InteractableTypeEnum)Properties["_interactableType"].enumValueIndex;
                InteractableItem.MessageTypeEnum messageTypeEnum = (InteractableItem.MessageTypeEnum)Properties["_messageType"].enumValueIndex;
                InteractableItem.ExamineTypeEnum examineTypeEnum = (InteractableItem.ExamineTypeEnum)Properties["_examineType"].enumValueIndex;

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Interactable Properties")))
                {
                    if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.InventoryItem)
                    {
                        Properties["_useInventoryTitle"].boolValue = false;
                        Properties["_examineInventoryTitle"].boolValue = false;
                    }

                    // enums
                    {
                        Properties.Draw("_interactableType");
                        Properties.Draw("_examineType");

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                            Properties.Draw("_examineRotate");

                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                        {
                            Properties.Draw("_messageType");
                            Properties.Draw("_disableType");
                        }
                    }
                }
                EditorGUILayout.Space();

                // draw inventory item field
                if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem)
                {
                    Properties.Draw("_pickupItem");
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space(1f);

                if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem || interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryExpand)
                {
                    // draw item settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Item Settings"), ref foldout[0]))
                    {
                        if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryExpand)
                        {
                            if (Properties.DrawGetBool("_expandRows"))
                            {
                                Properties.Draw("_slotsToExpand", new GUIContent("Rows To Expand"));
                            }
                            else
                            {
                                Properties.Draw("_slotsToExpand");
                            }
                        }
                        else
                        {
                            Properties.Draw("_quantity");
                            Properties.Draw("_useInventoryTitle");
                            if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                                Properties.Draw("_examineInventoryTitle");
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                {
                    // draw examine settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Examine Settings"), ref foldout[1]))
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            if (Properties.DrawToggleLeft("_useExamineZooming"))
                            {
                                Properties.Draw("_examineZoomLimits");
                                float minLimit = Properties["_examineZoomLimits"].FindPropertyRelative("Min").floatValue;
                                float maxLimit = Properties["_examineZoomLimits"].FindPropertyRelative("Max").floatValue;
                                SerializedProperty examineDistance = Properties["_examineDistance"];
                                examineDistance.floatValue = EditorGUILayout.Slider(new GUIContent(examineDistance.displayName), examineDistance.floatValue, minLimit, maxLimit);
                            }
                            else
                            {
                                Properties.Draw("_examineDistance");
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            using (new EditorGUI.DisabledGroupScope(!Properties.DrawToggleLeft("_useFaceRotation")))
                            {
                                Properties.Draw("_faceRotation");
                            }
                        }
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        {
                            using (new EditorGUI.DisabledGroupScope(!Properties.DrawToggleLeft("_useControlPoint")))
                            {
                                Properties.Draw("_controlPoint");
                            }
                        }
                        EditorGUILayout.EndVertical();

                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                            Properties.Draw("_takeFromExamine");

                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.InventoryExpand)
                        {
                            Properties.Draw("_isPaper");
                            Properties.Draw("_allowCursorExamine");
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                // draw message settings
                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Message Settings"), ref foldout[2]))
                {
                    if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                        Properties.Draw("_showExamineTitle");

                    if (!Properties["_useInventoryTitle"].boolValue)
                        Properties.Draw("_interactTitle");

                    if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None && Properties["_showExamineTitle"].boolValue && !Properties["_examineInventoryTitle"].boolValue)
                        Properties.Draw("_examineTitle");

                    if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None && Properties["_isPaper"].boolValue)
                        Properties.Draw("_paperText");

                    if (messageTypeEnum == InteractableItem.MessageTypeEnum.Hint)
                        Properties.Draw("_hintMessage");

                    if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem && messageTypeEnum != InteractableItem.MessageTypeEnum.None)
                        Properties.Draw("_messageTime");

                    EditorDrawing.EndBorderHeaderLayout();
                }
                EditorGUILayout.Space(1f);

                if (interactableTypeEnum == InteractableItem.InteractableTypeEnum.InventoryItem)
                {
                    // draw custom item data
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_itemCustomData"], new GUIContent("Item Custom Data")))
                    {
                        SerializedProperty jsonData = Properties["_itemCustomData"].FindPropertyRelative("JsonData");
                        EditorGUILayout.PropertyField(jsonData);
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.GenericItem && examineTypeEnum == InteractableItem.ExamineTypeEnum.CustomObject)
                {
                    // draw custom examine settings
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Custom Examine Settings"), ref foldout[3]))
                    {
                        EditorGUI.indentLevel++;
                        {
                            Properties.Draw("_collidersEnable");
                            EditorGUILayout.Space(1f);
                            Properties.Draw("_collidersDisable");
                            EditorGUILayout.Space(1f);
                            Properties.Draw("_examineHotspot");
                        }
                        EditorGUI.indentLevel--;
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                // draw events settings
                if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None || interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                {
                    EditorGUILayout.Space(1f);
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Event Settings"), ref foldout[5]))
                    {
                        if (interactableTypeEnum != InteractableItem.InteractableTypeEnum.ExamineItem)
                            Properties.Draw("_onTakeEvent");

                        if (examineTypeEnum != InteractableItem.ExamineTypeEnum.None)
                        {
                            Properties.Draw("_onExamineStartEvent");
                            Properties.Draw("_onExamineEndEvent");
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                if (Properties["_quantity"].intValue < 1) Properties["_quantity"].intValue = 1;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}