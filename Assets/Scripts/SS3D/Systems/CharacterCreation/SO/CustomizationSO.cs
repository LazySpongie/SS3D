using UnityEngine;

namespace SS3D.Systems.CharacterCreation
{
    
    /// <summary>
    /// Base SO for all customization options that can be selected in character creation (hairstyles, loadout item.)
    /// </summary>
    public class CustomizationSO : ScriptableObject
    {
        public string NameString;
        public Sprite icon;
        
    }
}