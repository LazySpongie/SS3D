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
            hips, spine, chest, neck, head,
            shoulderL, armL, forearmL, handL,
            shoulderR, armR, forearmR, handR,
            thighL, lowerLegL, footL,
            thighR, lowerLegR, footR;

        [SerializeField]
        private Transform
            waist, belly, butt, breastL, breastR, muscleChest,
            muscleShoulderL, muscleShoulderR, bicepL, bicepR,
            muscleNeck, torsoWidth, thighWidthL, thighWidthR;
        
        [Header("Body")]
        [Range(0.9f, 1.1f)] public float heightInput = 1.0f;
        [Range(0.5f, 1.5f)] public float weightInput = 1.0f;
        [Range(0f, 1f)] public float muscleInput = 0f;
        [Range(0f, 1f)] public float chestInput = 0f;
        [Range(0f, 1)] public float buttInput = 1.0f;
        [Range(0.5f, 1.5f)] public float waistInput = 1.0f;

        public void ScaleBones()
        {

            transform.localScale = heightInput * Vector3.one;

            head.localScale = Vector3.one / heightInput;
            
            // waist
            waist.localScale = Vector3.one * waistInput;

            // butt
            float buttValue = buttInput + 1.0f;
            butt.localScale = Vector3.one * buttValue;

            // weight
            belly.localScale = Vector3.one * weightInput;
            torsoWidth.localScale = Vector3.one * weightInput;
            
            // chest
            float chestValue = chestInput + 1.0f;
            breastL.localScale = Vector3.one * chestValue;
            breastR.localScale = Vector3.one * chestValue;
            
            // muscle
            float muscleValue = muscleInput + 1.0f;
            muscleChest.localScale = Vector3.one * muscleValue;
            muscleShoulderL.localScale = Vector3.one * muscleValue;
            muscleShoulderR.localScale = Vector3.one * muscleValue;
            thighWidthL.localScale = Vector3.one * muscleValue;
            thighWidthR.localScale = Vector3.one * muscleValue;
            bicepL.localScale = Vector3.one * muscleValue;
            bicepR.localScale = Vector3.one * muscleValue;
            bicepL.localScale = Vector3.one * muscleValue;
            bicepR.localScale = Vector3.one * muscleValue;
            
        }

        [ContextMenu("Scale")]
        public void EditorScale()
        {
            ScaleBones();
        }

    }
}