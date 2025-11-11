using Coimbra.Services.Events;
using SS3D.Systems.Characters.Preferences;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SS3D.Systems.Characters.Events
{
    public partial struct LocalLobbyCharacterListChanged : IEvent
    {
        public readonly List<CharacterProfile> Characters;
        public readonly List<string> Names;

        public LocalLobbyCharacterListChanged(List<CharacterProfile> characters, List<string> names)
        {
            Characters = characters;
            Names = names;
        }
    }
}