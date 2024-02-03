using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(CustomInteractTitle)), CanEditMultipleObjects]
    public class CustomInteractTitleEditor : InspectorEditor<CustomInteractTitle>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_overrideTitle");
                Properties.Draw("_overrideUseTitle");
                Properties.Draw("_overrideExamineTitle");

                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                bool overrideTitle = Properties.BoolValue("_overrideTitle");
                bool overrideUseTitle = Properties.BoolValue("_overrideUseTitle");
                bool overrideExamineTitle = Properties.BoolValue("_overrideExamineTitle");

                if(!overrideTitle && !overrideUseTitle && !overrideExamineTitle)
                {
                    EditorGUILayout.HelpBox("Nothing will be overridden. The interactive message will not be overridden.", MessageType.Info);
                }
                else
                {
                    if (overrideTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Title")))
                        {
                            if (!Properties.DrawGetBool("_useTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("_title");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("_dynamicTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("_trueTitle");
                                Properties.Draw("_falseTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.Title);
                                }
                            }
                        }

                        if (overrideUseTitle || overrideExamineTitle) EditorGUILayout.Space();
                    }

                    if (overrideUseTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Use Title")))
                        {
                            if (!Properties.DrawGetBool("_useUseTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("_useTitle");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("_dynamicUseTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("_trueUseTitle");
                                Properties.Draw("_falseUseTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.UseTitle);
                                }
                            }
                        }

                        if (overrideExamineTitle) EditorGUILayout.Space();
                    }

                    if (overrideExamineTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Examine Title")))
                        {
                            if (!Properties.DrawGetBool("_useExamineTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("_examineTitle");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("_dynamicExamineTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("_trueExamineTitle");
                                Properties.Draw("_falseExamineTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.ExamineTitle);
                                }
                            }
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}