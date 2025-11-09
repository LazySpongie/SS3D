using UnityEngine;

namespace SS3D.Systems.Characters
{
    
    /// <summary>
    /// SO for hairstyles and facial hair
    /// </summary>
    [CreateAssetMenu(menuName = "SS3D/Customization/Hairstyle", fileName = "Hairstyle")]
    public class HairstyleSO : CustomizationSO
    {

        [Header("Models")]

        [Tooltip("Mesh displayed on player model.")]
        public Mesh HairModel;
        
    }
}