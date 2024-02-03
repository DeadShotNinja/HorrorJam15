using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class SaveableObject : MonoBehaviour, ISaveable
    {
        [Flags]
        public enum SaveableFlagsEnum
        {        
            None = 0,
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale = 1 << 2,
            ObjectActive = 1 << 3,
            RendererActive = 1 << 4,
            ReferencesActive = 1 << 5
        }

        [SerializeField] private SaveableFlagsEnum _saveableFlags;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Behaviour[] _references;

        public SaveableFlagsEnum SaveableFlags => _saveableFlags;

        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new();

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Position))
            {
                storableCollection.Add("position", transform.position.ToSaveable());
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Rotation))
            {
                storableCollection.Add("rotation", transform.eulerAngles.ToSaveable());
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Scale))
            {
                storableCollection.Add("scale", transform.localScale.ToSaveable());
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive))
            {
                storableCollection.Add("objectActive", gameObject.activeSelf);
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && _meshRenderer != null)
            {
                storableCollection.Add("rendererEnabled", _meshRenderer.enabled);
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && _references.Length > 0)
            {
                for (int i = 0; i < _references.Length; i++)
                {
                    string name = "referenceId_" + i;
                    storableCollection.Add(name, _references[i].enabled);
                }
            }

            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Position))
            {
                Vector3 position = data["position"].ToObject<Vector3>();
                transform.position = position;
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Rotation))
            {
                Vector3 rotation = data["rotation"].ToObject<Vector3>();
                transform.eulerAngles = rotation;
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.Scale))
            {
                Vector3 scale = data["scale"].ToObject<Vector3>();
                transform.localScale = scale;
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive))
            {
                bool active = (bool)data["objectActive"];
                gameObject.SetActive(active);
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && _meshRenderer != null)
            {
                bool active = (bool)data["rendererEnabled"];
                _meshRenderer.enabled = active;
            }

            if (_saveableFlags.HasFlag(SaveableFlagsEnum.ObjectActive) && _references.Length > 0)
            {
                for (int i = 0; i < _references.Length; i++)
                {
                    string name = "referenceId_" + i;
                    bool active = (bool)data[name];
                    _references[i].enabled = active;
                }
            }
        }
    }
}