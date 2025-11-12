using UnityEngine;
using SS3D.Systems.Characters;
using System;

namespace SS3D.Systems.Health
{
    /// <summary>
    /// Script to handle clothing items covering certain objects
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class BodyPartRenderer : RendererController
    {

        [SerializeField]
        private BodyPartType _bodyPartType;
        
        public BodyPartType Type => _bodyPartType;

    }
}