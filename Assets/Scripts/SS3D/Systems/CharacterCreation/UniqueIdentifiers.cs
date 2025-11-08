using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;
using SS3D.Data;
using SS3D.Attributes;

namespace SS3D.Systems.CharacterCreation
{

    /// <summary>
    /// Script on an entity that stores the current appearance and sends it to AppearanceDisplayer when changed
    /// </summary>
    public class UniqueIdentifiers : NetworkActor
    {

        /// <summary>
        /// Renderer that will display the characters appearance.
        /// </summary>
        [SerializeField] [NotNull] private AppearanceDisplayer _appearanceDisplayer;
        
        [SyncObject]
        private readonly SyncDictionary<CustomizationType, string> _currentCustomization = new();

        protected override void OnAwake()
        {
            base.OnAwake();
            _currentCustomization.OnChange += SyncAppearance;
        }

        #region Syncing

        public void SyncAppearance(SyncDictionaryOperation op, CustomizationType type, string value, bool asServer)
        {
            if (asServer) return;
            
            if (op != SyncDictionaryOperation.Set) return;

            switch (type)
            {
                case CustomizationType.Hairstyle:
                case CustomizationType.Beardstyle:
                case CustomizationType.Eyebrows:
                    CustomizationSO option = Assets.Get<CustomizationSO>("Customization", value);
                    _appearanceDisplayer.SetStyle(type, option);
                    break;

                case CustomizationType.HairColor:
                case CustomizationType.EyeColor:
                case CustomizationType.SkinColor:
                    if (!ColorUtility.TryParseHtmlString("#" + value, out Color color)) break;
                    _appearanceDisplayer.SetColor(type, color);
                    break;

                case CustomizationType.CharacterName:
                    gameObject.name = value;
                    break;
                default:
                    // no code for other customisation types has been added yet
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Called by client when their player entity is spawned to set their appearance
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetAppearance(Dictionary<CustomizationType, string> customization)
        {
            foreach (KeyValuePair<CustomizationType, string> entry in customization)
            {
                _currentCustomization[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Sets customization option
        /// </summary>
        [Server]
        public void SetCustomizationOption(CustomizationType type, string option)
        {
            _currentCustomization[type] = option;
        }
    }
}