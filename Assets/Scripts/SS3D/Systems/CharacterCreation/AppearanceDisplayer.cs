using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;
using SS3D.Systems.CharacterCreation.Preferences;

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
        [SerializeField] private VisualSlot _hairSlot;

        /// <summary>
        /// Renderer that will display the beard.
        /// </summary>
        [SerializeField] private VisualSlot _beardSlot;

        /// <summary>
        /// Renderer that will display the eyebrows.
        /// </summary>
        [SerializeField] private VisualSlot _eyebrowSlot;

        /// <summary>
        /// Renderer that will display the eyes.
        /// </summary>
        [SerializeField] private VisualSlot _eyeSlot;

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

            _eyeMaterial = new Material(_eyeSlot.Renderer.sharedMaterial);
            _eyeSlot.Renderer.sharedMaterial = _eyeMaterial;
            _eyeSlot.SetRendererMaterial(_eyeMaterial);

            _hairMaterial = new Material(_hairSlot.Renderer.sharedMaterial);
            _hairSlot.SetRendererMaterial(_hairMaterial);
            _beardSlot.SetRendererMaterial(_hairMaterial);
            _eyebrowSlot.SetRendererMaterial(_hairMaterial);
        }

        /// <summary>
        /// Sets color
        /// </summary>
        [Client]
        public void SetColor(AppearanceType type, Color color)
        {
            switch (type)
            {
                case AppearanceType.HairColor:
                    // code
                    _hairMaterial.SetColor("_Color", color);
                    break;
                case AppearanceType.EyeColor:
                    // code
                    _eyeMaterial.SetColor("_Color", color);
                    break;
                case AppearanceType.SkinColor:
                    // code
                    _skinMaterial.SetColor("_Color", color);
                    break;
                default:
                    // error
                    break;
            }
        }
        
        /// <summary>
        /// Set hair beard eyebrows.
        /// </summary>
        [Client]
        public void SetStyle(AppearanceType type, CustomizationSO customizationSO)
        {
            HairstyleSO hair = (HairstyleSO)customizationSO;
            switch (type)
            {
                case AppearanceType.Hairstyle:
                    _hairSlot.SetRendererMesh(hair.HairModel);
                    break;
                case AppearanceType.Beardstyle:
                    _beardSlot.SetRendererMesh(hair.HairModel);
                    break;
                case AppearanceType.Eyebrows:
                    _eyebrowSlot.SetRendererMesh(hair.HairModel);
                    break;
                default:
                    // error
                    break;
            }
        }

        /// <summary>
        /// Set culling on hair
        /// </summary>
        [Client]
        public void SetCullingOnAppearance(GameObject culler, ClothingItemCullingData cullingData, bool addCulling)
        {

            BlendShape[] blends = { };
            if (cullingData)
            {
                blends = new BlendShape[] {
                                    new("Hat", cullingData.Hat),
                                    new("Helmet", cullingData.Helmet),
                                    new("Mask", cullingData.Mask),
                                    new("Hood", cullingData.Hood),
                                    };
            }

            SetCullingOnSlot(culler, _hairSlot, blends, addCulling, cullingData.HideHair);
            SetCullingOnSlot(culler, _beardSlot, blends, addCulling, cullingData.HideBeard);
            SetCullingOnSlot(culler, _eyebrowSlot, blends, addCulling, cullingData.HideEyebrows);
            SetCullingOnSlot(culler, _eyeSlot, blends, addCulling, cullingData.HideEyes);
        }

        public void SetCullingOnSlot(GameObject culler, VisualSlot slot, BlendShape[] blends, bool addCulling, bool hideSlot)
        {
            if (addCulling)
            {
                if (hideSlot) slot.RendererController.AddCuller(culler.gameObject);
                slot.RendererController.AddBlendShapeAffector(culler.gameObject, blends);
            }
            else
            {
                if (hideSlot) slot.RendererController.RemoveCuller(culler.gameObject);
                slot.RendererController.RemoveBlendShapeAffector(culler.gameObject);
            }
        }
    }
}