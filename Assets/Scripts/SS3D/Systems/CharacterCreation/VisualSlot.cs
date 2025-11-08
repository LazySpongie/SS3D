
using FishNet.Object;
using SS3D.Core.Behaviours;
using UnityEngine;

namespace SS3D.Systems.CharacterCreation
{
    /// <summary>
    /// Generic script to control the rendering of hairstyles and clothing items
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [RequireComponent(typeof(RendererController))]
    public class VisualSlot : Actor
    {
        /// <summary>
        /// Cullable script on this object
        /// </summary>
        private RendererController _rendererController;

        /// <summary>
        /// SkinnedMeshRenderer script on this object
        /// </summary>
        private SkinnedMeshRenderer _renderer;

        /// <summary>
        /// Cullable script on this object
        /// </summary>
        public RendererController RendererController => _rendererController;

        /// <summary>
        /// SkinnedMeshRenderer script on this object
        /// </summary>
        public SkinnedMeshRenderer Renderer => _renderer;

        protected override void OnAwake()
        {
            base.OnAwake();
            _renderer = GetComponent<SkinnedMeshRenderer>();
            _rendererController = GetComponent<RendererController>();
        }

        /// <summary>
        /// Assign a mesh to the renderer 
        /// </summary>
        [Client]
        public void SetRendererMesh(Mesh mesh)
        {
            _renderer.sharedMesh = mesh;
            _rendererController.SetBlendShapesFromMesh();
            _rendererController.UpdateBlendShapes();
        }

        /// <summary>
        /// Assign materials to the renderer
        /// </summary>
        [Client]
        public void SetRendererMaterial(Material material)
        {
            _renderer.sharedMaterial = material;
        }
        
        /// <summary>
        /// Assign materials to the renderer
        /// </summary>
        [Client]
        public void SetRendererMaterials(Material[] materials)
        {
            _renderer.sharedMaterials = materials;
        }
    }
}