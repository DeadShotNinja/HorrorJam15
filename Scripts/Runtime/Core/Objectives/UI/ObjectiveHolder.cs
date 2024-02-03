using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using UnityEngine;
using HJ.Tools;
using TMPro;
using static HJ.Runtime.ObjectiveManager;

namespace HJ.Runtime
{
    public class ObjectiveHolder : MonoBehaviour
    {
        [SerializeField] private Transform _subObjectives;
        [SerializeField] private TMP_Text _objectiveTitle;

        private ObjectiveManager _manager;
        private readonly CompositeDisposable _disposables = new();
        private readonly Dictionary<string, GameObject> _subObjectivesDict = new();
        private readonly Dictionary<string, CompositeDisposable> _subDisposables = new();

        private void OnDestroy()
        {
            _disposables.Dispose();
            foreach (var disposable in _subDisposables)
            {
                disposable.Value.Dispose();
            }
        }

        public void SetObjective(ObjectiveManager manager, ObjectiveData objective)
        {
            this._manager = manager;
            _objectiveTitle.text = objective.Objective.ObjectiveTitle;

            // subscribe listening to localization changes
            objective.Objective.ObjectiveTitle
                .ObserveText(text => _objectiveTitle.text = text)
                .AddTo(_disposables);

            // event when all objectives will be completed
            objective.IsCompleted.Subscribe(completed =>
            {
                if (completed)
                {
                    _disposables.Dispose();
                    Destroy(gameObject);
                }
            })
            .AddTo(_disposables);

            // event when sub objective will be added
            objective.AddSubObjective.Subscribe(data =>
            {
                CreateSubObjective(data);
            })
            .AddTo(_disposables);

            // event when sub objective will be removed
            objective.RemoveSubObjective.Subscribe(RemoveSubObjective)
            .AddTo(_disposables);

            // add starting objectives
            foreach (var subObj in objective.SubObjectives)
            {
                CreateSubObjective(subObj.Value);
            }
        }

        private void CreateSubObjective(SubObjectiveData data)
        {
            GameObject subObjective = Instantiate(_manager.SubObjectivePrefab, Vector3.zero, Quaternion.identity, _subObjectives);
            TMP_Text objectiveTitle = subObjective.GetComponentInChildren<TMP_Text>();
            data.SubObjectiveObject = subObjective;

            CompositeDisposable _disposables = new();
            _subDisposables.Add(data.SubObjective.SubObjectiveKey, _disposables);

            string subObjectiveText = data.SubObjective.ObjectiveText;
            objectiveTitle.text = FormatObjectiveText(subObjectiveText, data.CompleteCount.Value);

            // subscribe listening to localization changes
            data.SubObjective.ObjectiveText
                .ObserveText(text => objectiveTitle.text = FormatObjectiveText(text, data.CompleteCount.Value))
                .AddTo(_disposables);

            // event when sub objective will be completed
            data.IsCompleted.Subscribe(completed =>
            {
                if (completed)
                {
                    _disposables.Dispose();
                    Destroy(subObjective);
                }
            })
            .AddTo(_disposables);

            // event when sub objective complete count will be changed
            data.CompleteCount.Subscribe(count =>
            {
                objectiveTitle.text = FormatObjectiveText(subObjectiveText, count);
            })
            .AddTo(_disposables);

            // add sub objective to sub objectives dictionary
            _subObjectivesDict.Add(data.SubObjective.SubObjectiveKey, subObjective);
        }

        private void RemoveSubObjective(string key)
        {
            if(_subObjectivesDict.TryGetValue(key, out GameObject subObj))
            {
                Destroy(subObj);
                _subDisposables[key].Dispose();
                _subDisposables.Remove(key);
            }
        }

        private string FormatObjectiveText(string text, ushort count)
        {
            return text.RegexReplaceTag('[', ']', "count", count.ToString());
        }
    }
}