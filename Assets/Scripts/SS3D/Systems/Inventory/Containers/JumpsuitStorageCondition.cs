using SS3D.Core.Behaviours;
using SS3D.Systems.Inventory.Containers;
using SS3D.Systems.Inventory.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Inventory.Containers
{
    /// <summary>
    /// So jumpsuits can only be removed if the belt slot is empty
    /// </summary>
    public class JumpsuitStorageCondition : Actor, IStorageCondition
    {
        [SerializeField]
        private AttachedContainer _beltContainer;
        public bool CanRemove(AttachedContainer container, Item item)
        {
            return _beltContainer.Empty;
        }

        public bool CanStore(AttachedContainer container, Item item)
        {
            return true;
        }


    }
}
