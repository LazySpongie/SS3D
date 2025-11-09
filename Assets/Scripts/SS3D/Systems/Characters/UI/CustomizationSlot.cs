using TMPro;
using SS3D.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// Slot that holds information for each hairstyle option in the character creation UI.
    /// They get created when the character creation menu starts.
    /// </summary>
    public class CustomizationSlot : MonoBehaviour
    {
        [SerializeField] [NotNull] protected Button _button;

        public Button Button => _button;

        [SerializeField]
        protected Image Image;
        [SerializeField] [NotNull] protected TMP_Text OptionName;
        protected CustomizationSO _customizationSO;

        public CustomizationSO CustomizationSO => _customizationSO;

        /// <summary>
        /// Load a UI icon and string for the customization.
        /// </summary>
        public void Setup(CustomizationGrid customizationGrid, CustomizationSO customizationSO)
        {
            _customizationSO = customizationSO;
            Image.sprite = customizationSO.icon;
            OptionName.text = customizationSO.NameString;
            // _customizationGrid = GetComponentInParent<CustomizationGrid>(); 
        }
    }
}