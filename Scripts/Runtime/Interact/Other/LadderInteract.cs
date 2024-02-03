using UnityEngine;
using HJ.Tools;
using HJ.Input;

namespace HJ.Runtime
{
    public class LadderInteract : MonoBehaviour, IStateInteract
    {
        [SerializeField] private GameObject _ladderPart;
        [SerializeField] private float _verticalIncrement;

        [SerializeField] private Vector3 _ladderUpOffset;
        [SerializeField] private Vector3 _ladderExitOffset;
        [SerializeField] private Vector3 _ladderArcOffset;
        [SerializeField] private Vector3 _centerOffset;

        [SerializeField] private bool _useMouseLimits = true;
        [SerializeField] private MinMax _mouseVerticalLimits = new MinMax(-60, 90);
        [SerializeField] private MinMax _mouseHorizontalLimits = new MinMax(-80, 80);

        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private bool _drawGizmosSteps = true;
        [SerializeField] private bool _drawGizmosLabels = true;
        [SerializeField] private bool _drawPlayerPreview = true;
        [SerializeField] private bool _drawPlayerAtEnd = true;
        [SerializeField] private float _playerRadius = 0.3f;
        [SerializeField] private float _playerHeight = 1.8f;

        #region Properties

        public GameObject LadderPart => _ladderPart;
        public float VerticalIncrement => _verticalIncrement;
        public Vector3 LadderUpOffset => _ladderUpOffset;
        
        public Vector3 StartPos => transform.TransformPoint(_centerOffset);
        public Vector3 EndPos => transform.TransformPoint(_ladderUpOffset + _centerOffset);
        public Vector3 ExitPos => transform.TransformPoint(_ladderUpOffset + _centerOffset + _ladderExitOffset);
        public Vector3 ArcPos => transform.TransformPoint(_ladderUpOffset + _centerOffset + _ladderArcOffset);
        
        #endregion

        public StateParams OnStateInteract()
        {
            return new StateParams()
            {
                StateKey = PlayerStateMachine.LADDER_STATE,
                StateData = new StorableCollection()
                {
                    { "transform", transform },
                    { "start", StartPos },
                    { "end", EndPos },
                    { "exit", ExitPos },
                    { "arc", ArcPos },
                    { "useLimits", _useMouseLimits },
                    { "verticalLimits", _mouseVerticalLimits },
                    { "horizontalLimits", _mouseHorizontalLimits },
                }
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (_drawGizmos)
            {
                Gizmos.color = Color.green.Alpha(0.5f);
                Gizmos.DrawSphere(StartPos, 0.1f);

                Gizmos.color = Color.yellow.Alpha(0.5f);
                Gizmos.DrawSphere(EndPos, 0.1f);

                Gizmos.color = Color.white.Alpha(0.5f);
                Gizmos.DrawLine(StartPos, EndPos);

                Gizmos.color = Color.red.Alpha(0.5f);
                Gizmos.DrawSphere(ExitPos, 0.1f);

                float radius = 0.75f;
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green.Alpha(0.01f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, radius);
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, radius);
#endif
                Gizmos.color = Color.white;
                GizmosE.DrawGizmosArrow(transform.position, transform.forward * radius);

                if (_drawPlayerPreview)
                {
                    Vector3 center = ExitPos;
                    if (!_drawPlayerAtEnd) center = Vector3.Lerp(StartPos, EndPos, 0.5f);

                    float height = (_playerHeight - 0.6f) / 2f;
                    Vector3 p1 = new Vector3(center.x, center.y - height, center.z);
                    Vector3 p2 = new Vector3(center.x, center.y + height, center.z);
                    Gizmos.color = Color.green;
                    GizmosE.DrawWireCapsule(p1, p2, _playerRadius);
                }

                if (_drawGizmosLabels)
                {
                    GizmosE.DrawCenteredLabel(StartPos, "Start");
                    if (_ladderUpOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(EndPos, "End");
                    if (_ladderExitOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(ExitPos, "Exit");
                }

                if (_drawGizmosSteps)
                {
                    Gizmos.color = new Color(1f, 0.65f, 0f, 0.5f);
                    Gizmos.DrawSphere(ArcPos, 0.05f);

#if UNITY_EDITOR
                    if (_drawGizmosLabels && _ladderArcOffset != Vector3.zero)
                        GizmosE.DrawCenteredLabel(ArcPos, "Arc Point");
#endif

                    Vector3 llp = VectorExtension.QuadraticBezier(EndPos, ExitPos, ArcPos, 0);
                    Gizmos.color = Color.white.Alpha(0.5f);

                    int steps = 20;
                    for (int i = 1; i <= steps; i++)
                    {
                        float t = i / (float)steps;
                        Vector3 lp = VectorExtension.QuadraticBezier(EndPos, ExitPos, ArcPos, t);
                        Gizmos.DrawLine(llp, lp);
                        llp = lp;
                    }
                }
            }
        }
    }
}