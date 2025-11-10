using System;
using System.Collections.Generic;

namespace SS3D.Systems.Characters.Preferences
{

    /// <summary>
    /// This class is used by ClientPreferencesSubSystem to keep track of saved characters
    /// </summary>
    [Serializable]
    public class CharacterProfileManifest
    {
        
        public int LastSelected = 0;
        public List<string> Characters = new();

        public CharacterProfileManifest()
        {
        }

        public CharacterProfileManifest(int lastSelected, List<string> characters)
        {
            LastSelected = lastSelected;
            Characters = characters;
        }
    }
}