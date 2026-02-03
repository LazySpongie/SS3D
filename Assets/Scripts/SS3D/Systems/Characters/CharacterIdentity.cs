using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Inventory.Items;
using SS3D.Systems.Inventory.Items.Generic;

namespace SS3D.Systems.Characters
{

    /// <summary>
    /// Stores the name and appearance of a humanoid entity
    /// 
    /// TODO: Need to create setters at some point
    /// 
    /// </summary>
    public class CharacterIdentity : NetworkActor
    {
        /// <summary>
        /// The name that players will see.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncDisplayName))]
        private string _displayName = string.Empty;


        /// <summary>
        /// The name on the equipped ID card.
        /// 
        /// TODO: NEEDs TO BE LOCALISED STRINGS
        /// </summary>
        private string _idCardName = string.Empty;
        
        /// <summary>
        /// The role title on the equipped ID card.
        /// 
        /// TODO: NEEDs TO BE LOCALISED STRINGS
        /// </summary>
        private string _idCardRoleName = string.Empty;

        /// <summary>
        /// The real name of this character (only server should see this).
        /// </summary>
        private string _characterName = string.Empty;

        /// <summary>
        /// The flavor text that players will see.
        /// </summary>
        [SyncVar(OnChange = nameof(SyncDisplayFlavorText))]
        private string _displayFlavorText = string.Empty;

        /// <summary>
        /// The flavor text of this character.
        /// </summary>
        private string _flavorText = string.Empty;

        private HumanInventory _inventory;

        private bool _identityHidden = false;

        private int _identityHiders;

        /// <summary>
        /// The name that players will see.
        /// </summary>
        public string DisplayName => _displayName;


        /// <summary>
        /// The name of this character (only server should see this).
        /// </summary>
        public string Name => _characterName;

        /// <summary>
        /// The flavor text of this character.
        /// </summary>
        public string FlavorText => _flavorText;

        protected override void OnAwake()
        {
            base.OnAwake();
            _inventory = GetComponent<HumanInventory>();
            _inventory.OnContainerContentChanged += HandleInventoryChanged;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            _inventory.OnContainerContentChanged -= HandleInventoryChanged;
        }

        #region Setters

        /// <summary>
        /// Set the characters name
        /// </summary>
        [Server]
        public void SetName(string name)
        {
            _characterName = name;
            UpdateDisplayName();
        }
        
        /// <summary>
        /// Set the characters name
        /// </summary>
        [Server]
        public void SetFlavorText(string text)
        {
            _flavorText = text;
            UpdateFlavorText();
        }
    
        /// <summary>
        /// Set whether the characters real name is shown to players
        /// </summary>
        [Server]
        public void SetIdentityHidden(bool hidden)
        {
            if (hidden)
            {
                _identityHiders++;
            }
            else
            {
                _identityHiders--;
            }

            if (_identityHiders < 0) _identityHiders = 0;

            UpdateIdentity();
        }

        #endregion

        #region Displayed Identity

        /// <summary>
        /// Called when the characters inventory is changed
        /// If the item that changed was their id card we need to update their display.
        /// </summary>
        [Server]
        private void HandleInventoryChanged(AttachedContainer container, Item oldItem, Item newItem, ContainerChangeType type)
        {
            if (container.Type != ContainerType.Identification) return;

            _idCardName = string.Empty;
            _idCardRoleName = string.Empty;

            if (type != ContainerChangeType.Remove)
            {
                IDCard idCard = null;

                if (newItem is IDCard)
                {
                    idCard = (IDCard)newItem;
                }
                else if (newItem is PDA pda)
                {
                    idCard = (IDCard)pda.StartingIDCard;
                }
                
                _idCardName = idCard?.OwnerName;
                _idCardRoleName = idCard?.RoleName;
            }

            UpdateDisplayName();
        }

        /// <summary>
        /// Updates whether the character's identity is hidden or not
        /// </summary>
        [Server]
        private void UpdateIdentity()
        {
            _identityHidden = _identityHiders > 0;

            UpdateDisplayName();
        }

        /// <summary>
        /// Updates the currently displayed character name
        /// </summary>
        [Server]
        private void UpdateDisplayName()
        {
            string name = string.Empty;

            bool hasID = _idCardName != string.Empty;
            bool nameIsMismatched = _characterName != _idCardName;

            string fullIDCardName = _idCardName + " (" + _idCardRoleName + ")";

            // TODO: needs to move to the client at some point so it can be localised

            if (_identityHidden)
            {
                if (hasID)
                {
                    name = fullIDCardName;

                    // Name is Urist (Captain)
                }
                else
                {
                    name = string.Empty;
                    
                    // Name is Unknown
                }
            }
            else
            {
                if (hasID)
                {
                    if (nameIsMismatched)
                    {
                        name = _characterName + " as " + fullIDCardName;

                        // Name is Joe as Urist (Captain)
                    }
                    else
                    {
                        name = _characterName + " (" + _idCardRoleName + ")";

                        // Name is Joe (Assistant)
                    }
                }
                else
                {
                    name = _characterName;

                    // Name is Joe
                }
            }
            
            _displayName = name;
        }

        /// <summary>
        /// Updates the currently displayed flavor text
        /// </summary>
        [Server]
        private void UpdateFlavorText()
        {
            string text = string.Empty;

            if (!_identityHidden)
            {
                text = _flavorText;
            }

            _displayFlavorText = text;
        }

        #endregion

        #region Syncing

        /// <summary>
        /// Callback when the characters name is changed
        /// </summary>
        [ServerOrClient]
        private void SyncDisplayName(string oldName, string newName, bool asServer)
        {
            string name = "Unknown";
            if (_displayName != string.Empty) name = _displayName;

            gameObject.name = name;
        }

        [Client]
        private void SyncDisplayFlavorText(string oldChar, string newChar, bool asServer)
        {
            // throw new NotImplementedException();
        }

        #endregion

    }
}