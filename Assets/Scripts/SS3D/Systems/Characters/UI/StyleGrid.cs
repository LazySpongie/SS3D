using SS3D.Attributes;
using SS3D.Systems.Characters.Preferences;
using UnityEngine;

namespace SS3D.Systems.Characters.UI
{
    public class StyleGrid : CustomizationGrid
    {
        [SerializeField][NotNull] private StyleType _type;
        
        public StyleType Type => _type;
    }
}