using Coimbra.Services.Events;
using SS3D.Systems.Characters.Preferences;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SS3D.Systems.Characters.Events
{
    public partial struct LocalLobbyCharacterChanged : IEvent
    {
        public readonly int Index;
        public readonly CharacterProfile Character;
        public readonly CharacterChangeType ChangeType;

        public LocalLobbyCharacterChanged(int index, CharacterProfile character, CharacterChangeType changeType)
        {
            Index = index;
            Character = character;
            ChangeType = changeType;
        }
    }
}