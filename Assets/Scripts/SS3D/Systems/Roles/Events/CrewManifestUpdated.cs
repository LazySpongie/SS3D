using Coimbra.Services.Events;
using SS3D.Systems.PlayerControl;
using SS3D.Systems.Roles;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SS3D.Systems.Rounds.Events
{
    public partial struct CrewManifestUpdated : IEvent
    {
        public readonly ReadOnlyDictionary<string, RoleCounter> CrewManifest;

        public CrewManifestUpdated(ReadOnlyDictionary<string, RoleCounter> crewManifest)
        {
            CrewManifest = crewManifest;
        }
    }
}