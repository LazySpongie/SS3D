using FishNet.Broadcast;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters.Messages
{
    public struct ClientSendCharacterMessage : IBroadcast
    {
        public readonly string Ckey;
        public readonly CharacterProfile Character;

        public ClientSendCharacterMessage(string ckey, CharacterProfile character)
        {
            Ckey = ckey;
            Character = character;
        }
    }
}