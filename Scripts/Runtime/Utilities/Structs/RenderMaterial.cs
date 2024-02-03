using System;
using UnityEngine;

namespace HJ.Runtime
{
    [Serializable]
    public struct RendererMaterial
    {
        public MeshRenderer MeshRenderer;
        public Material Material;
        public int MaterialIndex;

        public bool IsAssigned => MeshRenderer != null && Material != null;

        public Material ClonedMaterial
        {
            get => Material = MeshRenderer.materials[MaterialIndex];
            set
            {
                Material[] materials = MeshRenderer.materials;
                materials[MaterialIndex] = value;
                MeshRenderer.materials = materials;
            }
        }
    }
}