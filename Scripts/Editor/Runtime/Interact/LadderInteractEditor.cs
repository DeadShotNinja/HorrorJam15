using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LadderInteract))]
    public class LadderInteractEditor : InspectorEditor<LadderInteract>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Ladder Offsets")))
                {
                    Properties.Draw("_ladderUpOffset");
                    Properties.Draw("_ladderExitOffset");
                    Properties.Draw("_ladderArcOffset");
                    Properties.Draw("_centerOffset");
                }

                EditorGUILayout.Space();

                if(EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Use Mouse Limits"), Properties["_useMouseLimits"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties["_useMouseLimits"].boolValue))
                    {
                        Properties.Draw("_mouseVerticalLimits");
                        Properties.Draw("_mouseHorizontalLimits");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_ladderPart"], new GUIContent("Ladder Builder")))
                {
                    Properties.Draw("_ladderPart");
                    Properties.Draw("_verticalIncrement");

                    EditorGUILayout.Space();
                    if (GUILayout.Button("Build Ladder", GUILayout.Height(25f)))
                    {
                        GenerateLadder();
                    }

                    if (GUILayout.Button("Calculate Bounds", GUILayout.Height(25f)))
                    {
                        GenerateCollider();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
                Properties.Draw("_drawGizmos");
                Properties.Draw("_drawGizmosSteps");
                Properties.Draw("_drawGizmosLabels");
                if (Properties.DrawGetBool("_drawPlayerPreview"))
                {
                    Properties.Draw("_drawPlayerAtEnd");
                    Properties.Draw("_playerRadius");
                    Properties.Draw("_playerHeight");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateLadder()
        {
            float increment = Target.VerticalIncrement;
            int steps = Mathf.RoundToInt(Target.LadderUpOffset.y / increment);

            Transform oldMesh = Target.transform.Find("LadderMesh");
            if (oldMesh != null) DestroyImmediate(oldMesh.gameObject);

            GameObject ladder = new GameObject("LadderMesh");
            ladder.transform.SetParent(Target.transform);
            ladder.transform.localPosition = Vector3.zero;

            float y = 0;
            for (int i = 0; i < steps; i++)
            {
                GameObject part = Instantiate(Target.LadderPart, ladder.transform);
                part.name = "LadderPart_" + i;
                Vector3 pos = part.transform.localPosition;
                pos.y += y; 
                y += increment;
                part.transform.localPosition = pos;
            }
        }

        private void GenerateCollider()
        {
            if (Target.GetComponentsInChildren<Renderer>().Length > 0)
            {
                BoxCollider collider = Target.GetComponent<BoxCollider>();
                if (collider == null) collider = Target.gameObject.AddComponent<BoxCollider>();

                Bounds bounds = CalculateBounds();
                collider.size = bounds.size;
                collider.center = bounds.center - Target.transform.position;
                collider.isTrigger = true;
            }
        }

        private Bounds CalculateBounds()
        {
            Quaternion oldRotation = Target.transform.rotation;
            Target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Bounds bounds = new Bounds(Target.transform.position, Vector3.zero);
            Renderer[] renderers = Target.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Target.transform.rotation = oldRotation;
            return bounds;
        }
    }
}