using Coimbra;
using SS3D.Attributes;
using SS3D.Systems.CharacterCreation.Preferences;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Actor = SS3D.Core.Behaviours.Actor;

namespace SS3D.Systems.CharacterCreation
{
    public class AppearanceGrid : CustomizationGrid
    {
        [SerializeField][NotNull] private AppearanceType _appearanceType;
        
        public AppearanceType AppearanceType => _appearanceType;
    }
}