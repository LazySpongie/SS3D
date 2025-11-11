using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters
{
        
    public class BoneScaler : Actor
    {
        [SerializeField]
        private Transform
            hips, spine, chest, upperChest, neck, head,
            leftArm, rightArm, leftLowerArm, rightLowerArm,
            leftLeg, rightLeg, leftLowerLeg, rightLowerLeg,
            leftHand, rightHand, leftFoot, rightFoot,
            leftShoulder, rightShoulder;
        
        [Header("Body")]
        [Range(0.5f, 1.5f)] public float heightInput = 1.0f;
        [Range(0.5f, 1.5f)] public float weightInput = 1.0f;
        [Range(0.5f, 1.5f)] public float muscleInput = 1.0f;
        [Range(0.5f, 1.5f)] public float chestInput = 1.0f;
        [Range(0.5f, 1.5f)] public float buttInput = 1.0f;
        [Range(0.5f, 1.5f)] public float waistInput = 1.0f;

        public void ScaleBones()
        {
            hips.transform.localScale = heightInput * Vector3.one;
            float spineScale = heightInput;
        }

        [ContextMenu("Scale")]
        public void EditorScale()
        {
            ScaleBones();
        }

    }
}