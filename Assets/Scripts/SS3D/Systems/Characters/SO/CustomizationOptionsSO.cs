using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Characters
{
    
    [CreateAssetMenu(menuName = "SS3D/Customization/Customization List", fileName = "CustomizationList")]
    public class CustomizationOptionsSO : ScriptableObject
    {
        public List<CustomizationSO> Options;
    }
}