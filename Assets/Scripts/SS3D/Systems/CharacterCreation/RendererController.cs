using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SS3D.Core.Behaviours;

namespace SS3D.Systems.CharacterCreation
{
    /// <summary>
    /// Script to handle clothing items covering certain objects
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class RendererController : Actor
    {
        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// If there is no item in the slot or the bodypart has been severed the renderer should be hidden
        /// </summary>
        private bool _isHidden;

        /// <summary>
        /// List of items culling this slot
        /// </summary>
        private List<GameObject> _cullers = new();

        /// <summary>
        /// Dictionary of blend shape cullers
        /// </summary>
        private Dictionary<GameObject, BlendShape[]> _blendShapeAffectors = new();

        private Dictionary<string, float> _blendShapes = new();

        /// <summary>
        /// If any items are culling this slot the renderer should be hidden
        /// </summary>
        public bool IsCulled => _cullers.Count > 0;

        /// <summary>
        /// If there is no item in the slot or the bodypart has been severed the renderer should be hidden
        /// </summary>
        public bool IsHidden => _isHidden;

        protected override void OnAwake()
        {
            base.OnAwake();
            _renderer = GetComponent<SkinnedMeshRenderer>();
        }

        public void SetHidden(bool hidden)
        {
            _isHidden = hidden;
            UpdateRendererEnabled();
        }

        /// <summary>
        /// Add an item to the list of items attempting to hide this clothing slot
        /// </summary>
        public void AddCuller(GameObject obj)
        {
            _cullers.Add(obj);

            UpdateRendererEnabled();
        }

        /// <summary>
        /// Remove an item from the list of items attempting to hide this clothing slot
        /// </summary>
        public void RemoveCuller(GameObject obj)
        {
            _cullers.Remove(obj);
            UpdateRendererEnabled();
        }

        /// <summary>
        /// Set the renderer's visibility based on the culling and if it has an item assigned
        /// </summary>
        public void UpdateRendererEnabled()
        {
            if (IsCulled || IsHidden)
            {
                _renderer.enabled = false;
                return;
            }
            _renderer.enabled = true;
        }

        /// <summary>
        /// Add an item to the list of items attempting to hide this clothing slot
        /// </summary>
        public void AddBlendShapeAffector(GameObject obj, BlendShape[] blendShapes)
        {
            _blendShapeAffectors.Add(obj, blendShapes);
            UpdateBlendShapes();
        }

        /// <summary>
        /// Remove an item from the list of items attempting to hide this clothing slot
        /// </summary>
        public void RemoveBlendShapeAffector(GameObject obj)
        {
            _blendShapeAffectors.Remove(obj);
            UpdateBlendShapes();
        }

        public void UpdateBlendShapes()
        {
            // need to reset to 0
            SetBlendShapesFromMesh();

            // set the blendshape to the highest value in the affectors

            foreach (KeyValuePair<GameObject, BlendShape[]> affector in _blendShapeAffectors)
            {
                foreach (BlendShape blend in affector.Value)
                {
                    if (!_blendShapes.ContainsKey(blend.Name)) continue; 
                    if (_blendShapes[blend.Name] >= blend.Value) continue;
                    _blendShapes[blend.Name] = blend.Value;
                }

            }

            int i = 0;
            foreach (KeyValuePair<string, float> blend in _blendShapes)
            {
                SetBlendShape(i, blend.Value);
                i++;
            }
        }

        public void SetBlendShape(int index, float value)
        {
            _renderer.SetBlendShapeWeight(index, value);
        }

        public void SetBlendShapesFromMesh()
        {
            _blendShapes.Clear();
            Mesh m = _renderer.sharedMesh;
            if (m == null) return;

            for (int i = 0; i < m.blendShapeCount; i++)
            {
                _blendShapes.Add(m.GetBlendShapeName(i), 0f);
            }
        }

    }
}