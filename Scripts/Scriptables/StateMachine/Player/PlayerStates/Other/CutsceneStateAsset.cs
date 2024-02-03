using System;
using UnityEngine;
using UnityEngine.Playables;
using HJ.Input;
using HJ.Scriptable;

namespace HJ.Runtime.States
{
    public class CutsceneStateAsset : PlayerStateAsset
    {
        public float ToCutsceneTime = 0.3f;

        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new CutscenePlayerState(machine, this);
        }

        public override string GetStateKey() => "Cutscene";

        public override string ToString() => "Cutscene";

        public class CutscenePlayerState : FSMPlayerState
        {
            private readonly CutsceneStateAsset _state;
            private CutsceneModule _cutsceneModule;

            private Vector3 _targetPosition;
            private Vector2 _targetLook;
            private PlayableDirector _cutscene;
            private System.Action _completedEvent;

            private Vector3 _startingPosition;
            private bool _cutsceneStart;
            private bool _cutsceneEnd;

            private float _tFactor = 0f;
            private float _tVel;

            public CutscenePlayerState(PlayerStateMachine machine, CutsceneStateAsset stateAsset) : base(machine) 
            {
                _state = stateAsset;
            }

            public override void OnStateEnter()
            {
                _cutsceneModule = GameManager.Module<CutsceneModule>();
                _targetPosition = (Vector3)StateData["position"];
                _targetLook = (Vector2)StateData["look"];
                _cutscene = (PlayableDirector)StateData["cutscene"];
                _completedEvent = (System.Action)StateData["event"];

                _cutsceneStart = false;
                _cutsceneEnd = false;

                InputManager.ResetToggledButtons();
                _startingPosition = Position;
                _machine.Motion = Vector3.zero;
                _tFactor = 0f;

                _playerItems.IsItemsUsable = false;
            }

            public override void OnStateExit()
            {
                _playerItems.IsItemsUsable = true;
                _cameraLook.ResetCustomLerp();
                _machine.Motion = Vector3.zero;
            }

            public override void OnStateUpdate()
            {
                if (_cutsceneStart) return;
                _tFactor = Mathf.SmoothDamp(_tFactor, 1.001f, ref _tVel, _state.ToCutsceneTime);

                if (_tFactor < 1f)
                {
                    Position = Vector3.Lerp(_startingPosition, _targetPosition, _tFactor);
                    _cameraLook.CustomLerp(_targetLook, _tFactor);
                }
                else if(!_cutsceneStart)
                {
                    _cutsceneModule.PlayCutscene(_cutscene, () => 
                    {
                        _cutsceneEnd = true;
                        _completedEvent.Invoke();
                    });
                    _cutsceneStart = true;
                }

                _controllerState = _machine.StandingState;
                PlayerHeightUpdate();
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<WalkingStateAsset>(() => _cutsceneEnd)
                };
            }
        }
    }
}