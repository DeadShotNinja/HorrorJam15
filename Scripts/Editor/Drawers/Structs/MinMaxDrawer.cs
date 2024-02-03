using HJ.Runtime;
using UnityEditor;
using UnityEngine;

namespace HJ.Editors
{
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty min = prop.FindPropertyRelative("Min");
            SerializedProperty max = prop.FindPropertyRelative("Max");

            EditorGUI.BeginProperty(pos, label, prop);
            pos = EditorGUI.PrefixLabel(pos, label);
            pos.xMax -= EditorGUIUtility.singleLineHeight + 2f;

            float[] values = new float[2];
            values[0] = min.floatValue;
            values[1] = max.floatValue;
            
            EditorGUI.MultiFloatField(pos, new GUIContent[]
            {
                new GUIContent("Min"),
                new GUIContent("Max"),
            }, values);

            min.floatValue = values[0];
            max.floatValue = values[1];

            Rect flipRect = pos;
            flipRect.width = EditorGUIUtility.singleLineHeight;
            flipRect.x = pos.xMax + 2f;

            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));

            GUIContent flipIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Flip Min & Max values.");
            if (GUI.Button(flipRect, flipIcon, EditorStyles.iconButton))
            {
                float tempMin = min.floatValue;
                min.floatValue = max.floatValue;
                max.floatValue = tempMin;

                if (prop.serializedObject != null)
                    prop.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
