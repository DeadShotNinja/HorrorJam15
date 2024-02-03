using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(ObjectReferences))]
    public class ObjectReferencesEditor : Editor
    {
        private static ObjectReferences _target;

        private void OnEnable()
        {
            _target = target as ObjectReferences;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if (obj != null && obj is ObjectReferences)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        static void OpenWindow()
        {
            if (_target != null)
            {
                ObjectReferencesWindow objRefWindow = EditorWindow.GetWindow<ObjectReferencesWindow>(false, _target.name, true);

                Rect position = objRefWindow.position;
                position.width = 800;
                position.height = 450;

                objRefWindow.minSize = new Vector2(800, 450);
                objRefWindow.position = position;
                objRefWindow.Init(_target);
            }
            else
            {
                Debug.LogError("[OpenDatabaseEditor] Scriptable object is not initialized!");
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Contains references to objects that can be instantiated and saved at runtime.", MessageType.Info, true);
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox("Assign this asset to SaveGameManager script to enable reference picker with this asset.", MessageType.Warning, true);
                EditorGUILayout.Space(10);

                if (GUILayout.Button("Open Object References Window", GUILayout.Height(30)))
                {
                    OpenWindow();
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("References Count: " + _target.References.Count, EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}