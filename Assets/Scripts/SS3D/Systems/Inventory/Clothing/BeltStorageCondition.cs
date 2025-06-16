using SS3D.Core.Behaviours;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Inventory.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Inventory.Containers.Clothing
{
    /// <summary>
    /// So belts can only be worn if there is a jumpsuit equipped
    /// </summary>
    public class BeltStorageCondition : Actor, IStorageCondition
    {
        [SerializeField]
        private AttachedContainer _jumpsuitContainer;

        public bool CanRemove(AttachedContainer container, Item item)
        {
            return true;
        }

        public bool CanStore(AttachedContainer container, Item item)
        {
            return !_jumpsuitContainer.Empty;
        }
    }
}
