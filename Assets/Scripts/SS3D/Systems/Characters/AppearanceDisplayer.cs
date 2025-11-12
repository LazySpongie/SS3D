using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;
using SS3D.Systems.Characters.Preferences;
using System;
using Cysharp.Threading.Tasks.Triggers;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Client script that displays appearance on the player
    /// </summary>
    public class AppearanceDisplayer : NetworkActor
    {
        [Header("Bone Scaler")]
        [SerializeField] private BoneScaler _boneScaler;

        [Header("Style Slots")]
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

        [Header("Renderers")]
        /// <summary>
        /// Renderers that control the skin.
        /// </summary>
        [SerializeField] private List<SkinnedMeshRenderer> _skinRenderers;
        
        [SerializeField] private RendererController _headRenderer;

        [Header("Materials")]
        [SerializeField] private Material _hairMaterial;
        [SerializeField] private Material _eyeMaterial;
        [SerializeField] private Material _skinMaterial;

        private Material _hair;
        private Material _eye;
        private Material _skin;

        protected override void OnAwake()
        {
            base.OnAwake();

            if (IsServerOnly) return;

            _skin = new Material(_skinMaterial);
            _eye = new Material(_eyeMaterial);
            _hair = new Material(_hairMaterial);
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (IsServerOnly) return;
            SetupMaterials();
        }

        /// <summary>
        /// Create instanced materials so the color can be changed
        /// </summary>
        [Client]
        private void SetupMaterials()
        {
            Material[] skinMats = _skinRenderers[0].sharedMaterials;
            skinMats[0] = _skin;
            foreach (SkinnedMeshRenderer skin in _skinRenderers)
            {
                skin.sharedMaterials = skinMats;
            }

            _eyeSlot.SetRendererMaterial(_eye);
            _hairSlot.SetRendererMaterial(_hair);
            _beardSlot.SetRendererMaterial(_hair);
            _eyebrowSlot.SetRendererMaterial(_hair);
        }

        /// <summary>
        /// Sets color
        /// </summary>
        [Client]
        public void SetColor(ColorType type, Color color)
        {
            switch (type)
            {
                case ColorType.HairColor:
                    // code
                    _hair.SetColor("_Color", color);
                    break;
                case ColorType.EyeColor:
                    // code
                    _eye.SetColor("_Color", color);
                    break;
                case ColorType.SkinColor:
                    // code
                    _skin.SetColor("_Color", color);
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
        public void SetStyle(StyleType type, CustomizationSO customizationSO)
        {
            HairstyleSO hair = (HairstyleSO)customizationSO;
            switch (type)
            {
                case StyleType.Hairstyle:
                    _hairSlot.SetRendererMesh(hair.Model);
                    break;
                case StyleType.Beardstyle:
                    _beardSlot.SetRendererMesh(hair.Model);
                    break;
                case StyleType.Eyebrows:
                    _eyebrowSlot.SetRendererMesh(hair.Model);
                    break;
                default:
                    // error
                    break;
            }
        }

        /// <summary>
        /// Set body scales.
        /// </summary>
        [ServerOrClient]
        public void SetBody(BodyType type, float value)
        {
            value = Mathf.Clamp(value, 0.1f, 4f);

            if (_boneScaler == null) return;

            switch (type)
            {
                case BodyType.Height:
                    _boneScaler.heightInput = value;
                    break;
                case BodyType.Belly:
                    _boneScaler.bellyInput = value;
                    break;
                case BodyType.UpperBody:
                    _boneScaler.upperBodyInput = value;
                    break;
                case BodyType.Chest:
                    _boneScaler.chestInput = value;
                    break;
                case BodyType.LowerBody:
                    _boneScaler.lowerBodyInput = value;
                    break;
                case BodyType.Waist:
                    _boneScaler.waistInput = value;
                    break;
                case BodyType.Jaw:
                    BlendShape[] blends = { new BlendShape("Female", (1 - value) * 100f) };
                    _headRenderer.AddBlendShapeAffector(gameObject, blends);
                    break;
            }

            // when called on an ingame character the server needs to update physics but not cosmetic bones
            // has to be IsClient instead of IsServer otherwise the host will be affected
            _boneScaler.ScaleBones(IsServerOnly);
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

        [Client]
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