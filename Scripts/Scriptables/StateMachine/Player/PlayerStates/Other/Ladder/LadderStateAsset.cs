using UnityEngine;
using HJ.Scriptable;
using HJ.Input;

namespace HJ.Runtime.States
{
    public class LadderStateAsset : BasicStateAsset
    {
        [SerializeField] private LadderStateData _stateData;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new LadderPlayerState(machine, _stateData);
        }

        public override string GetStateKey() => PlayerStateMachine.LADDER_STATE;

        public override string ToString() => "Ladder";

        public class LadderPlayerState : FSMPlayerState
        {
            //private readonly AudioSource _audioSource;
            private Collider _interactCollider;
            private Transform _ladder;
            private LadderController _ladderController;
            
            private float _stepTime;
            private int _lastStep;
            
            public readonly LadderStateData Data;
            public LadderPositions LadderPositions;

            public Vector3 StartPosition;
            public Vector3 MovePosition;

            public float BezierEval;
            public bool PlayerMoved;
            public bool ExitLadder;
            public bool ClimbDown;
            public bool ExitState;

            public Vector3 OffsetedCenter
            {
                get
                {
                    Vector3 center = _centerPosition;
                    center.y += Data.PlayerCenterOffset;
                    return center;
                }
            }

            public LadderPlayerState(PlayerStateMachine machine, LadderStateData stateData) : base(machine)
            {
                Data = stateData;
                Data.ControlExit.SubscribeGloc();
                // _audioSource = machine.GetComponent<AudioSource>();

                _ladderController = new LadderController(this, _machine);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<IdleStateAsset>(() => ExitState || InputManager.ReadButtonOnce("Jump", Controls.JUMP)),
                    Transition.To<DeathStateAsset>(() => IsDead)
                };
            }

            public override void OnStateEnter()
            {
                InitializeLadder();
                InitializePlayerState();
            }
            
            private void InitializeLadder()
            {
                _ladder = (Transform)StateData["transform"];
                LadderPositions = _ladderController.GetLadderPositions(StateData);

                if (_ladder.TryGetComponent(out _interactCollider))
                    _interactCollider.enabled = false;
            }
            
            private void InitializePlayerState()
            {
                bool useMouseLimits = (bool)StateData["useLimits"];
                MinMax verticalLimits = (MinMax)StateData["verticalLimits"];
                MinMax horizontalLimits = (MinMax)StateData["horizontalLimits"];

                PlayerMoved = false;
                ExitLadder = false;
                ExitState = false;
                ClimbDown = false;
                BezierEval = 0f;

                // set look rotation and limits
                if (useMouseLimits)
                {
                    Vector3 ladderRotation = _ladder.rotation.eulerAngles;
                    _cameraLook.LerpClampRotation(ladderRotation, verticalLimits, horizontalLimits);
                }

                // set ladder climb direction
                if (ClimbDown = LadderUtility.LadderDotUp(LadderPositions.End, _centerPosition) < 0)
                {
                    StartPosition = _centerPosition;
                    MovePosition = LadderPositions.End;
                    MovePosition.y -= Data.PlayerCenterOffset + 0.1f;

                    // player is in front of ladder
                    ClimbDown = LadderUtility.LadderDotForward(_ladder, _centerPosition) < Data.LadderFrontAngle;
                }
                else
                {
                    float ladderT = LadderUtility.LadderEval(_centerPosition, LadderPositions.Start, LadderPositions.End);
                    MovePosition = Vector3.Lerp(LadderPositions.Start, LadderPositions.End, ladderT);
                    MovePosition.y += Data.GroundToLadderOffset;
                }

                _playerItems.DeactivateCurrentItem();
                _playerItems.IsItemsUsable = false;

                _gameManager.ShowControlsInfo(true, Data.ControlExit);
            }

            public override void OnStateUpdate()
            {
                _ladderController.UpdateLadderMotion(_centerPosition);
                _controllerState = _machine.StandingState;
                PlayerHeightUpdate();
            }
            
            public override void OnStateExit()
            {
                _playerItems.IsItemsUsable = true;
                _cameraLook.ResetLookLimits();
                if (_interactCollider != null) 
                    _interactCollider.enabled = true;

                _gameManager.ShowControlsInfo(false, null);
            }
        }
    }
}
