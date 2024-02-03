using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HJ.Runtime
{
    public class LeversPuzzle : MonoBehaviour, ISaveable
    {
        public enum PuzzleType { LeversOrder, LeversState, LeversChain }

        [SerializeField] private PuzzleType _leversPuzzleType;
        [SerializeField] private List<LeversPuzzleLever> _levers = new();

        [SerializeField] private LeversPuzzleOrder _leversOrder = new();
        [SerializeField] private LeversPuzzleState _leversState = new();
        [SerializeField] private LeversPuzzleChain _leversChain = new();

        [SerializeField] private float _leverSwitchSpeed = 2.5f;

        [SerializeField] private UnityEvent _onLeversCorrect;
        [SerializeField] private UnityEvent _onLeversWrong;
        [SerializeField] private UnityEvent<int, bool> _onLeverChanged;

        #region Properties
       
        public PuzzleType LeversPuzzleType => _leversPuzzleType;
        public List<LeversPuzzleLever> Levers => _levers;
        public LeversPuzzleOrder LeversOrder => _leversOrder;
        public LeversPuzzleState LeversState => _leversState;
        public LeversPuzzleChain LeversChain => _leversChain;
        public float LeverSwitchSpeed => _leverSwitchSpeed;
        
        #endregion

        public LeversPuzzleType CurrentLeverPuzzle
        {
            get => _leversPuzzleType switch
            {
                PuzzleType.LeversOrder => _leversOrder,
                PuzzleType.LeversState => _leversState,
                PuzzleType.LeversChain => _leversChain,
                _ => null,
            };
        }

        private void OnValidate()
        {
            _leversOrder.LeversPuzzle = this;
            _leversState.LeversPuzzle = this;
            _leversChain.LeversPuzzle = this;
        }

        private void Update()
        {
            CurrentLeverPuzzle?.OnLeverUpdate();
        }

        public void OnLeverInteract(LeversPuzzleLever lever)
        {
            if (CurrentLeverPuzzle == null)
                return;

            int leverIndex = _levers.IndexOf(lever);
            _onLeverChanged?.Invoke(leverIndex, _leversPuzzleType == PuzzleType.LeversOrder || lever.LeverState);
            CurrentLeverPuzzle.OnLeverInteract(lever);
        }

        public void ValidateLevers()
        {
            if (CurrentLeverPuzzle == null)
                return;

            if (CurrentLeverPuzzle.OnValidate())
                _onLeversCorrect.Invoke();
            else
                _onLeversWrong.Invoke();
        }

        public void ResetLevers()
        {
            foreach (var lever in _levers)
            {
                lever.ResetLever();
            }
        }

        public void DisableLevers()
        {
            foreach (var lever in _levers)
            {
                lever.SetInteractState(false);
            }
        }

        public StorableCollection OnSave()
        {
            StorableCollection leverStates = new StorableCollection();
            StorableCollection storableCollection = new StorableCollection();

            for (int i = 0; i < _levers.Count; i++)
            {
                string leverName = "lever_" + i;
                bool leverState = _levers[i].LeverState;
                leverStates.Add(leverName, leverState);
            }

            storableCollection.Add("leverStates", leverStates);
            storableCollection.Add("leverData", CurrentLeverPuzzle.OnSave());

            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            JToken leverStates = data["leverStates"];
            for (int i = 0; i < _levers.Count; i++)
            {
                string leverName = "lever_" + i;
                bool leverState = (bool)leverStates[leverName];
                _levers[i].SetLeverState(leverState);
            }

            JToken leverData = data["leverData"];
            CurrentLeverPuzzle.OnLoad(leverData);
            CurrentLeverPuzzle.TryToValidate();
        }
    }
}