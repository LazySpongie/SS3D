using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Video;
using System.Collections.Generic;
using FishNet.Object.Synchronizing;
using SS3D.Data;

namespace SS3D.Systems.CharacterCreation
{

    /// <summary>
    /// Client script that displays appearance on the player
    /// </summary>
    public class UniqueIdentifiers : NetworkActor
    {

        /// <summary>
        /// Renderer that will display the characters appearance.
        /// </summary>
        [SerializeField] private AppearanceDisplayer _appearance;
        
        [SyncObject]
        private readonly SyncDictionary<CustomizationType, string> _currentCustomization = new();

        protected override void OnAwake()
        {
            base.OnAwake();
            _currentCustomization.OnChange += SyncCustomization;
        }

        #region Syncing

        public void SyncCustomization(SyncDictionaryOperation op, CustomizationType type, string value, bool asServer)
        {
            // if (asServer) return;
            if (op != SyncDictionaryOperation.Set) return;

            switch (type)
            {
                case CustomizationType.Hairstyle:
                case CustomizationType.Beardstyle:
                case CustomizationType.Eyebrows:
                    CustomizationSO option = Assets.Get<CustomizationSO>("Customization", value);
                    _appearance.SetStyle(type, option);
                    break;

                case CustomizationType.HairColor:
                case CustomizationType.EyeColor:
                case CustomizationType.SkinColor:
                    if (!ColorUtility.TryParseHtmlString("#" + value, out Color color)) break;
                    _appearance.SetColor(type, color);
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
        /// Sets customization
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetCustomization(Dictionary<CustomizationType, string> customization)
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