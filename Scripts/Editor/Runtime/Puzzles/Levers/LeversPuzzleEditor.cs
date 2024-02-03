using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Tools;
using Random = UnityEngine.Random;

namespace HJ.Editors
{
    [CustomEditor(typeof(LeversPuzzle))]
    public class LeversPuzzleEditor : InspectorEditor<LeversPuzzle>
    {
        private int _leversCount => Properties["_levers"].arraySize;
        private Vector2 _singleLeverSize => new Vector2(40, 60);
        private int _maxLevers => 6;
        private float _leversSpacing => 5f;
        private float _leftRightPadding => 10f;
        private float _leversPanelSize => (_leftRightPadding * 2) + (_maxLevers * _singleLeverSize.x) + ((_maxLevers - 1) * _leversSpacing);

        private Texture2D _leverOff => Resources.Load<Texture2D>("EditorIcons/Puzzles/Levers/lever_off");
        private Texture2D _leverOn => Resources.Load<Texture2D>("EditorIcons/Puzzles/Levers/lever_on");

        private PropertyCollection _leversOrderProperties;
        private PropertyCollection _leversStateProperties;
        private PropertyCollection _leversChainProperties;

        private LeversPuzzle.PuzzleType _puzzleTypeEnum;
        private int _selectedLeverIndex = -1;
        private int _mouseDownIndex = -1;
        private bool _mouseDown;

        public override void OnEnable()
        {
            base.OnEnable();
            _mouseDown = false;
            _mouseDownIndex = -1;
            _selectedLeverIndex = -1;

            _leversOrderProperties = EditorDrawing.GetAllProperties(Properties["_leversOrder"]);
            _leversStateProperties = EditorDrawing.GetAllProperties(Properties["_leversState"]);
            _leversChainProperties = EditorDrawing.GetAllProperties(Properties["_leversChain"]);
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Levers Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                DrawLeversPuzzleTypeGroup();
                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                DrawLeversList();
                EditorGUILayout.Space(2f);

                DrawLeverSetup();
                EditorGUILayout.Space(2f);

                _puzzleTypeEnum = (LeversPuzzle.PuzzleType)Properties["_leversPuzzleType"].enumValueIndex;
                switch (_puzzleTypeEnum)
                {
                    case LeversPuzzle.PuzzleType.LeversOrder:
                        DrawLeversOrderProperties();
                        break;
                    case LeversPuzzle.PuzzleType.LeversState:
                        DrawLeversStateProperties();
                        break;
                    case LeversPuzzle.PuzzleType.LeversChain:
                        DrawLeversChainProperties();
                        break;
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lever Settings")))
                {
                    Properties.Draw("_leverSwitchSpeed");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_onLeversCorrect"], new GUIContent("Events")))
                {
                    Properties.Draw("_onLeversCorrect");
                    Properties.Draw("_onLeversWrong");
                    Properties.Draw("_onLeverChanged");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLeversPuzzleTypeGroup()
        {
            GUIContent[] toolbarContent = {
                new GUIContent(Resources.Load<Texture>("EditorIcons/Puzzles/Levers/levers_order"), "Levers Order"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Puzzles/Levers/levers_state"), "Levers State"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Puzzles/Levers/levers_chain"), "Levers Chain"),
            };

            using (new EditorDrawing.IconSizeScope(25))
            {
                GUIStyle toolbarButtons = new GUIStyle(GUI.skin.button);
                toolbarButtons.fixedHeight = 0;
                toolbarButtons.fixedWidth = 50;

                Rect toolbarRect = EditorGUILayout.GetControlRect(false, 30);
                toolbarRect.width = toolbarButtons.fixedWidth * toolbarContent.Length;
                toolbarRect.x = EditorGUIUtility.currentViewWidth / 2 - toolbarRect.width / 2 + 7f;

                EditorGUI.BeginChangeCheck();
                SerializedProperty puzzleType = Properties["_leversPuzzleType"];
                puzzleType.enumValueIndex = GUI.Toolbar(toolbarRect, puzzleType.enumValueIndex, toolbarContent, toolbarButtons);
                if (EditorGUI.EndChangeCheck()) OnLeversPuzzleTypeChanged();
            }
        }
    
        private void DrawLeversList()
        {
            SerializedProperty levers = Properties["_levers"];
            if (EditorDrawing.BeginFoldoutBorderLayout(levers, new GUIContent("Levers List")))
            {
                EditorGUI.BeginChangeCheck();
                levers.arraySize = EditorGUILayout.IntSlider(new GUIContent("Levers Count"), levers.arraySize, 0, _maxLevers);
                if (EditorGUI.EndChangeCheck()) OnLeversCountChanged();

                for (int i = 0; i < levers.arraySize; i++)
                {
                    SerializedProperty leverElement = levers.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(leverElement, new GUIContent("Lever " + i));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawLeverSetup()
        {
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Puzzle Setup")))
            {
                Rect leversPanelRect = GUILayoutUtility.GetRect(_leversPanelSize, 100f);
                Rect maskRect = leversPanelRect;

                leversPanelRect.y = 0f;
                leversPanelRect.x = (leversPanelRect.width / 2) - (_leversPanelSize / 2);
                leversPanelRect.width = _leversPanelSize;

                GUI.BeginGroup(maskRect);
                DrawLevers(leversPanelRect);
                GUI.EndGroup();

                EditorGUILayout.Space();
                if (Target.Levers.Any(x => x == null))
                {
                    EditorGUILayout.HelpBox("Some lever references are not assigned. Please assign lever references first to make the levers clickable.", MessageType.Error);
                }
            }
        }

        private void DrawLeversOrderProperties()
        {
            SerializedProperty leversOrder = _leversOrderProperties["_leversOrder"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Order")))
            {
                EditorGUILayout.HelpBox("Define the order in which the levers interact by clicking on the levers at the top. Changing the number of levers will clear the currently defined order.", MessageType.Info);
                EditorGUILayout.Space(2f);

                EditorGUILayout.TextField(new GUIContent("Levers Order"), leversOrder.stringValue);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    leversOrder.stringValue = "";
                    for (int i = 0; i < _leversCount; i++)
                    {
                        int random = Random.Range(0, _leversCount);
                        leversOrder.stringValue += random.ToString();
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    leversOrder.stringValue = "";
                }
            }
        }

        private void DrawLeversStateProperties()
        {
            SerializedProperty leverStates = _leversStateProperties["_leverStates"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers State")))
            {
                EditorGUILayout.HelpBox("Define the state of the levers, which is correct, by clicking on the levers at the top. Changing the number of levers will clear the currently defined levers state.", MessageType.Info);
                EditorGUILayout.Space(2f);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    for (int i = 0; i < _leversCount; i++)
                    {
                        SerializedProperty lever = leverStates.GetArrayElementAtIndex(i);
                        lever.boolValue = Random.Range(0, 2) != 0;
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    for (int i = 0; i < _leversCount; i++)
                    {
                        SerializedProperty lever = leverStates.GetArrayElementAtIndex(i);
                        lever.boolValue = false;
                    }
                }
            }
        }

        private void DrawLeversChainProperties()
        {
            SerializedProperty leversChains = _leversChainProperties["_leversChains"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Chain")))
            {
                EditorGUILayout.HelpBox("Define the levers chain reaction by selecting the first lever and clicking on the other levers at the top. Changing the number of levers will clear the currently defined levers state.", MessageType.Info);
                EditorGUILayout.Space(2f);

                if (Target.LeversChain.LeversChains.Any(x => x.ChainIndex.Count > 0))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        for (int i = 0; i < _leversCount; i++)
                        {
                            var leverChain = Target.LeversChain.LeversChains[i];
                            if (leverChain.ChainIndex.Count > 0)
                            {
                                EditorGUILayout.LabelField($"<b>[Lever {i}]</b>: {string.Join(", ", leverChain.ChainIndex)}", EditorDrawing.Styles.RichLabel);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2f);
                }

                SerializedProperty maxLeverReactions = _leversChainProperties["_maxLeverReactions"];
                SerializedProperty maxReactiveLevers = _leversChainProperties["_maxReactiveLevers"];

                EditorGUILayout.IntSlider(maxLeverReactions, 1, _leversCount - 1);
                EditorGUILayout.IntSlider(maxReactiveLevers, 1, _leversCount - 1);
                EditorGUILayout.Space(2f);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    leversChains.arraySize = 0;
                    leversChains.arraySize = _leversCount;
                    List<int> randomLever = new();

                    for (int i = 0; i < maxReactiveLevers.intValue; i++)
                    {
                        int leverIndex = GameTools.RandomExclude(0, _leversCount, randomLever.ToArray());
                        randomLever.Add(leverIndex);

                        SerializedProperty lever = leversChains.GetArrayElementAtIndex(leverIndex);
                        SerializedProperty chainIndex = lever.FindPropertyRelative("ChainIndex");

                        int chainsCount = Random.Range(1, maxLeverReactions.intValue + 1);
                        chainIndex.arraySize = chainsCount;

                        for (int j = 0; j < chainsCount; j++)
                        {
                            List<int> current = new();
                            for (int k = 0; k < j; k++)
                            {
                                SerializedProperty currChain = chainIndex.GetArrayElementAtIndex(k);
                                current.Add(currChain.intValue);
                            }

                            SerializedProperty chain = chainIndex.GetArrayElementAtIndex(j);
                            chain.intValue = GameTools.RandomExcludeUnique(0, _leversCount, new int[] { leverIndex }, current.Distinct().ToArray());
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    foreach (var lever in Target.LeversChain.LeversChains)
                    {
                        lever.ChainIndex = lever.ChainIndex.OrderBy(x => x).ToList();
                    }

                    _mouseDown = false;
                    _mouseDownIndex = -1;
                    _selectedLeverIndex = -1;
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    leversChains.arraySize = 0;
                    leversChains.arraySize = _leversCount;

                    _mouseDown = false;
                    _mouseDownIndex = -1;
                    _selectedLeverIndex = -1;
                }
            }
        }

        private void DrawLevers(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float Y = rect.height / 2 - _singleLeverSize.y / 2 - 10f;
            float X = (rect.width / 2) - ((_leftRightPadding * 2) + (_leversCount * _singleLeverSize.x) + ((_leversCount - 1) * _leversSpacing)) / 2;

            GUI.BeginGroup(rect);
            {
                if (_leversCount > 0)
                {
                    for (int x = 0; x < _leversCount; x++)
                    {
                        Vector2 leverPos = new Vector2(X + _leftRightPadding + (x * _singleLeverSize.x) + x * _leversSpacing, Y);
                        DrawLever(new Rect(leverPos, _singleLeverSize), x);
                    }
                }
                else
                {
                    GUIContent labelText = new GUIContent("Change the Levers Count");
                    Vector2 labelSize = EditorStyles.label.CalcSize(labelText);
                    float xPos = (rect.width / 2) - (labelSize.x / 2);
                    EditorGUI.LabelField(new Rect(xPos, 0, labelSize.x, rect.height), labelText);
                }
            }
            GUI.EndGroup();
            Repaint();
        }

        private void DrawLever(Rect rect, int index)
        {
            SerializedProperty lever = Properties["_levers"].GetArrayElementAtIndex(index);
            bool leverState = false;
            bool onHover = false;

            Color backgroundColor = Color.black.Alpha(0.5f);
            Event e = Event.current;

            if (lever.objectReferenceValue != null && rect.Contains(e.mousePosition))
            {
                backgroundColor = Color.white.Alpha(0.35f);
                onHover = true;

                if (!_mouseDown && e.type == EventType.MouseDown && e.button == 0)
                {
                    _mouseDown = true;
                    OnLeverMouseDown(index);
                }
                else if(_mouseDown && e.type == EventType.MouseUp)
                {
                    _mouseDown = false;
                    _mouseDownIndex = -1;
                }

                if(_mouseDown) backgroundColor = Color.black.Alpha(0.5f);
            }

            leverState = OnDrawLever(rect, index, leverState, backgroundColor, onHover);

            Vector2 labelOffset = new Vector2(0, _singleLeverSize.y + _leversSpacing);
            Rect labelPos = new Rect(rect.position + labelOffset, new Vector2(_singleLeverSize.x, 15f));
            Color labelColor = Color.black.Alpha(0.5f);

            // draw lever label
            EditorGUI.DrawRect(labelPos, labelColor);
            GUI.Label(labelPos, index.ToString(), EditorDrawing.CenterStyle(EditorStyles.miniBoldLabel));

            // draw lever texture
            Texture2D leverStateTex = leverState ? _leverOn : _leverOff;
            EditorDrawing.DrawTransparentTexture(rect, leverStateTex);
        }

        private bool OnDrawLever(Rect rect, int index, bool leverState, Color backgroundColor, bool onHover)
        {
            if(_puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversChain)
            {
                if(_selectedLeverIndex >= 0 && !(_mouseDownIndex == index && _mouseDown))
                {
                    if (_selectedLeverIndex == index || onHover)
                    {
                         backgroundColor = Color.white.Alpha(0.35f);
                    }
                    else
                    {
                        var leversChain = Target.LeversChain.LeversChains[_selectedLeverIndex];
                        if (leversChain.ChainIndex.Contains(index))
                            backgroundColor = Color.cyan.Alpha(0.35f);
                    }
                }
            }

            EditorGUI.DrawRect(rect, backgroundColor);

            if (_puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversState)
            {
                leverState = Target.LeversState.LeverStates[index];

                Rect stateRect = rect;
                stateRect.height = 2f;
                stateRect.xMin += 5f;
                stateRect.xMax -= 5f;
                stateRect.y = rect.yMax - 4f;

                Color stateColor = Color.red.Alpha(0.6f);
                if (leverState) stateColor = Color.green.Alpha(0.6f);

                EditorGUI.DrawRect(stateRect, stateColor);
            }

            return leverState;
        }

        private void OnLeversCountChanged()
        {
            _leversOrderProperties["_leversOrder"].stringValue = "";
            _leversStateProperties["_leverStates"].arraySize = _leversCount;
            _leversChainProperties["_leversChains"].arraySize = _leversCount;
            serializedObject.ApplyModifiedProperties();

            _mouseDown = false;
            _mouseDownIndex = -1;
            _selectedLeverIndex = -1;
        }

        private void OnLeversPuzzleTypeChanged()
        {
            _mouseDown = false;
            _mouseDownIndex = -1;
            _selectedLeverIndex = -1;
        }

        private void OnLeverMouseDown(int index)
        {
            _mouseDownIndex = index;

            if (_puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversOrder)
            {
                if(Target.LeversOrder.LeversOrder.Length < _leversCount)
                    Target.LeversOrder.LeversOrder += index.ToString();
            }
            else if(_puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversState)
            {
                var leverState = Target.LeversState.LeverStates[index];
                Target.LeversState.LeverStates[index] = !leverState;
            }
            else if(_puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversChain)
            {
                if(_selectedLeverIndex == index)
                {
                    _selectedLeverIndex = -1;
                    return;
                }

                if (_selectedLeverIndex < 0)
                {
                    _selectedLeverIndex = index;
                }
                else
                {
                    var leversChain = Target.LeversChain.LeversChains[_selectedLeverIndex];
                    leversChain.ChainIndex = leversChain.ChainIndex.Contains(index)
                        ? leversChain.ChainIndex.Except(new int[] { index }).ToList()
                        : leversChain.ChainIndex.Concat(new int[] { index }).ToList();
                }
            }
        }
    }
}