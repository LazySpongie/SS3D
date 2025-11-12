using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;
using SS3D.Systems.Characters.Preferences;

namespace SS3D.Systems.Characters
{
    // TODO: THIS NEEDS TO BE NETWORKED !!!
    public class BoneScaler : NetworkActor
    {

        [SerializeField]
        private Transform
            head,
            forearmL, handL,
            forearmR, handR,
            lowerLegL, footL,
            lowerLegR, footR;

        [SerializeField]
        private Transform
            waist, belly, butt, breastL, breastR, muscleChest,
            muscleShoulderL, muscleShoulderR, bicepL, bicepR,
            muscleNeck, torsoWidth, thighWidthL, thighWidthR;
        
        [Header("Body")]
        [Range(0.9f, 1.1f)] public float heightInput = 1.0f;
        [Range(0.1f, 1.9f)] public float weightInput = 1.0f;
        [Range(0f, 0.4f)] public float muscleInput = 0f;
        [Range(1f, 3f)] public float chestInput = 0f;
        [Range(0.6f, 1.5f)] public float buttInput = 1.0f;
        [Range(0.7f, 1.2f)] public float waistInput = 1.0f;

        public void ScaleBones()
        {
            // this one needs to be networked
            transform.localScale = heightInput * Vector3.one;
            head.localScale = Vector3.one / heightInput;

            float muscleValue = muscleInput + 1.0f;

            float forearmValue = muscleValue * 0.8f;
            forearmL.localScale = new Vector3(forearmValue, 1, forearmValue);
            forearmR.localScale = new Vector3(forearmValue, 1, forearmValue);

            if (IsServer) return;

            // waist
            waist.localScale = waistInput * Vector3.one;
            torsoWidth.localScale = waistInput * Vector3.one;

            // butt
            butt.localScale = Vector3.one * buttInput;

            // weight
            // need to work on this more
            float weightValue = weightInput * 2;
            belly.localScale = Vector3.one * weightValue;
            
            // chest
            breastL.localScale = Vector3.one * chestInput;
            breastR.localScale = Vector3.one * chestInput;

            // muscle

            muscleChest.localScale = new Vector3(muscleValue * 0.9f, muscleValue, muscleValue);
            // torsoWidth.localScale = Vector3.one * muscleValue;
            muscleNeck.localScale = Vector3.one * muscleValue;
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