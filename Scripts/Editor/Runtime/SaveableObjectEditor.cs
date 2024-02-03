using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using static HJ.Runtime.SaveableObject;

namespace HJ.Editors
{
    [CustomEditor(typeof(SaveableObject))]
    public class SaveableObjectEditor : InspectorEditor<SaveableObject>
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This script is used to save basic object properties such as position, " +
                                    "rotation, scale, etc.. To save custom object properties, use the ISaveable interface " +
                                    "in any custom script.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("_saveableFlags");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                bool rendererFlag = Target.SaveableFlags.HasFlag(SaveableFlagsEnum.RendererActive);
                bool referencesFlag = Target.SaveableFlags.HasFlag(SaveableFlagsEnum.ReferencesActive);

                if(rendererFlag || referencesFlag)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Object Properties", EditorStyles.boldLabel);
                }

                if (rendererFlag)
                {
                    Properties.Draw("_meshRenderer");
                }

                if (referencesFlag)
                {
                    Properties.Draw("_references");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}