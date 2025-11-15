using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Characters
{
    
    [CreateAssetMenu(menuName = "SS3D/Customization/Color Gradient", fileName = "Gradient")]
    public class ColorGradientSO : ScriptableObject
    {
        public Gradient Gradient;
    }
}