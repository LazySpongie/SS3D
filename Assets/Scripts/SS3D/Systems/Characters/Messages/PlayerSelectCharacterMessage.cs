using FishNet.Broadcast;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters.Messages
{
    public struct PlayerSelectCharacterMessage : IBroadcast
    {
        public readonly string Ckey;
        public readonly CharacterProfile Character;

        public PlayerSelectCharacterMessage(string ckey, CharacterProfile character)
        {
            Ckey = ckey;
            Character = character;
        }
    }
}