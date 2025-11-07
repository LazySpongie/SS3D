using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;

namespace SS3D.Systems.CharacterCreation
{

    /// <summary>
    /// Client script that displays appearance on the player
    /// </summary>
    public class AppearanceDisplayer : Actor
    {
        /// <summary>
        /// Renderer that will display the hair.
        /// </summary>
        [SerializeField] private SkinnedMeshRenderer _hairRenderer;

        /// <summary>
        /// Renderer that will display the beard.
        /// </summary>
        [SerializeField] private SkinnedMeshRenderer _beardRenderer;

        /// <summary>
        /// Renderer that will display the eyebrows.
        /// </summary>
        [SerializeField] private SkinnedMeshRenderer _eyebrowRenderer;

        /// <summary>
        /// Renderer that will display the eyes.
        /// </summary>
        [SerializeField] private SkinnedMeshRenderer _eyeRenderer;

        /// <summary>
        /// Renderers that control the skin.
        /// </summary>
        [SerializeField] private List<SkinnedMeshRenderer> _skinRenderers;

        private Material _hairMaterial;
        private Material _eyeMaterial;
        private Material _skinMaterial;

        protected override void OnAwake()
        {
            base.OnAwake();
            SetupMaterials();
        }

        /// <summary>
        /// Create instanced materials so the color can be changed
        /// </summary>
        private void SetupMaterials()
        {
            _skinMaterial = new Material(_skinRenderers[0].sharedMaterials[0]);
            Material[] skinMats = _skinRenderers[0].sharedMaterials;
            skinMats[0] = _skinMaterial;
            foreach (SkinnedMeshRenderer skin in _skinRenderers)
            {
                skin.sharedMaterials = skinMats;
            }

            _eyeMaterial = new Material(_eyeRenderer.sharedMaterial);
            _eyeRenderer.sharedMaterial = _eyeMaterial;

            _eyeMaterial = new Material(_eyeRenderer.sharedMaterial);
            _eyeRenderer.sharedMaterial = _eyeMaterial;
            
            _hairMaterial = new Material(_hairRenderer.sharedMaterial);
            _hairRenderer.sharedMaterial = _hairMaterial;
            _beardRenderer.sharedMaterial = _hairMaterial;
            _eyebrowRenderer.sharedMaterial = _hairMaterial;
        }

        /// <summary>
        /// Sets hair color
        /// </summary>
        [Client]
        public void SetHairColor(Color color)
        {
            _hairMaterial.SetColor("_Color", color);
        }
        
        /// <summary>
        /// Sets skin color
        /// </summary>
        [Client]
        public void SetSkinColor(Color color)
        {
            _skinMaterial.SetColor("_Color", color);
        }

        /// <summary>
        /// Sets eye color
        /// </summary>
        [Client]
        public void SetEyeColor(Color color)
        {
            _eyeMaterial.SetColor("_Color", color);
        }

        /// <summary>
        /// Set hairstyle mesh.
        /// </summary>
        [Client]
        public void SetHairstyle(CustomizationSO customizationSO)
        {
            HairstyleSO hair = (HairstyleSO)customizationSO;
            _hairRenderer.sharedMesh = hair.HairModel;
        }

        /// <summary>
        /// Set hairstyle mesh.
        /// </summary>
        [Client]
        public void SetBeardstyle(CustomizationSO customizationSO)
        {
            HairstyleSO hair = (HairstyleSO)customizationSO;
            _beardRenderer.sharedMesh = hair.HairModel;
        }
        
		/// <summary>
        /// Set hairstyle mesh.
        /// </summary>
        [Client]
        public void SetEyebrows(CustomizationSO customizationSO)
        {
            HairstyleSO hair = (HairstyleSO)customizationSO;
            _eyebrowRenderer.sharedMesh = hair.HairModel;
        }

        /// <summary>
        /// Get a body part that is referenced in the given culling data
        /// </summary>
        [Client]
        private void SetCullingOnAppearance(ClothingItemCullingData cullingData, bool addCulling)
        {
            
            // clothingVisualSlot.Cullable?.AddCuller(gameObject);
        }

    }
}