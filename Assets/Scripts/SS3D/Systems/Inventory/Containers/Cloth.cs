using SS3D.Systems.Inventory.Containers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Inventory.Containers
{
    /// <summary>
    /// Script to put on cloth, which are gameobjects going to the clothes slots.
    /// In the future, should be the folded models, and could contain a reference to the worn mesh version (and maybe torn mesh version, stuff like that...)
    /// </summary>
    public class Cloth : MonoBehaviour
    {
        /// <summary>
        /// Mesh displayed on the player model so items like jumpsuits can use the folded model when on the ground
        /// Should be changed later to be able to include different meshes like for different races or equipping a headset on each ear
        /// </summary>
        [Tooltip("Mesh displayed on player model. If left blank it will grab the mesh from the MeshFilter component instead.")]
        [SerializeField]
        private Mesh _wornMesh;

        /// <summary>
        /// Sets which clothing slot the item can be equipped in 
        /// </summary>
        [SerializeField]
        private ClothType _clothType;

        public Mesh WornMesh => _wornMesh;

        public ClothType Type => _clothType;
    }
}
