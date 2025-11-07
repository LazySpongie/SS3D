using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using SS3D.Core;
using SS3D.Core.Behaviours;
using SS3D.Attributes;

namespace SS3D.Systems.CharacterCreation
{
    /// <summary>
    /// 
    /// store current character and their selected roles
    /// 
    /// saved character which will be sent over network when spawning
    /// local unsaved character when making changes in the menu
    /// 
    /// save character button - save character to file and store networked version
    /// load character button - menu of character slots which you can switch between
    /// 
    /// when a round is started the server should ask this script to give it the players appearance and selected jobs
    ///  
    /// character creation view should read from this script to preview character
    /// 
    /// </summary>
    public class CharacterCreationMenuSubSystem : NetworkSubSystem
    {

        [SerializeField] [NotNull] private CustomizationGrid _hairSelection;
        [SerializeField] [NotNull] private CustomizationGrid _beardselection;
        [SerializeField] [NotNull] private CustomizationGrid _eyebrowSelection;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            _hairSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _beardselection.OnCustomizationGridStarted += HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted += HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelectionChanged += HandleHairBeardBrowChanged;
            _beardselection.OnCustomizationGridSelectionChanged += HandleHairBeardBrowChanged;
            _eyebrowSelection.OnCustomizationGridSelectionChanged += HandleHairBeardBrowChanged;
        }
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            _hairSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _beardselection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;
            _eyebrowSelection.OnCustomizationGridStarted -= HandleCustomizationGridStarted;

            _hairSelection.OnCustomizationGridSelectionChanged -= HandleHairBeardBrowChanged;
            _beardselection.OnCustomizationGridSelectionChanged -= HandleHairBeardBrowChanged;
            _eyebrowSelection.OnCustomizationGridSelectionChanged -= HandleHairBeardBrowChanged;
        }

        /// <summary>
        /// Method called when the load character button is clicked.
        /// </summary>
        public void HandleLoadButton()
        {
            // set hairstyle selection here
        }

        /// <summary>
        /// Method called when the save character button is clicked.
        /// </summary>
        public void HandleSaveButton()
        {

        }

        /// <summary>
        /// Method called when a grid is loaded in the menu so the default option can be selected.
        /// </summary>
        public void HandleCustomizationGridStarted(CustomizationType type, CustomizationGrid grid)
        {
            // Need to load data and send it
            switch (type)
            {
                case CustomizationType.Hairstyle:
                    // code
                    grid.SetSelectedOptionByName("Beep");
                    break;
                case CustomizationType.Beardstyle:
                    // code
                    grid.SetSelectedOptionByName("Danish");
                    break;
                case CustomizationType.Eyebrow:
                    // code
                    grid.SetSelectedOptionByName("Circles");
                    break;
                default:
                    // error
                    break;
            }
        }
        
        /// <summary>
        /// Method called when a hair or beard or brow is selected.
        /// </summary>
        public void HandleHairBeardBrowChanged(CustomizationType type, CustomizationSlot option)
        {
            // Debug.Log(type);
            // Debug.Log(option.CustomizationSO.NameString);

            switch (type)
            {
                case CustomizationType.Hairstyle:
                    // code
                    break;
                case CustomizationType.Beardstyle:
                    // code
                    break;
                case CustomizationType.Eyebrow:
                    // code
                    break;
                default:
                    // error
                    break;
            }
        }

    }
}