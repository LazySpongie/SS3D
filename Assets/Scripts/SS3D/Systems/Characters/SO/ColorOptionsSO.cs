using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Characters
{
    
    [CreateAssetMenu(menuName = "SS3D/Customization/Color List", fileName = "Colors")]
    public class ColorOptionsSO : ScriptableObject
    {
        public List<Color> Colors;
    }
}