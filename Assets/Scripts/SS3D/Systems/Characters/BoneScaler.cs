using SS3D.Core.Behaviours;
using UnityEngine;
using FishNet.Object;
using SS3D.Systems.Inventory.Clothing;
using UnityEngine.Video;
using System.Collections.Generic;
using SS3D.Systems.Characters.Preferences;
using System;
using UnityEngine.Animations;

namespace SS3D.Systems.Characters
{
    /// <summary>
    /// This script takes in input values and then scales the bones of the character.
    /// </summary>
    public class BoneScaler : Actor
    {

        [SerializeField]
        private Transform head;

        [SerializeField]
        private Transform
            waist, belly, butt, breastL, breastR, muscleChest,
            muscleShoulderL, muscleShoulderR, bicepL, forearmL, bicepR, forearmR, 
            muscleNeck, torsoWidth, thighWidthL, thighWidthR;

        [SerializeField]
        private Transform
            idHold, holdR, holdL;
            
        [Header("Body")]
        [Range(0.9f, 1.1f)] public float heightInput = 1.0f;
        [Range(0.8f, 1.2f)] public float bellyInput = 1.0f;
        [Range(0.7f, 1.3f)] public float upperBodyInput = 0f;
        [Range(0.0f, 1.0f)] public float chestInput = 0f;
        [Range(0.6f, 1.5f)] public float lowerBodyInput = 1.0f;
        [Range(0.7f, 1.2f)] public float waistInput = 1.0f;
        [Range(0.5f, 1.5f)] public float headInput = 1.0f;

        public void ScaleBones(bool onlyPhysicsBones)
        {

            Height();
            Head();

            if (onlyPhysicsBones) return;

            Belly();
            Waist();
            LowerBody();
            Chest();
            Shoulder();
            ForeArm();
            Bicep();
            UpperBody();
        }

        private void Height()
        {
            transform.localScale = heightInput * Vector3.one;
        }

        private void Chest()
        {
            float chestScale = (chestInput * 3) + 1f;
            chestScale = chestScale - Reduce(upperBodyInput, 0.3f);
            chestScale = Mathf.Clamp(chestScale, 0.8f, 4f);
            Vector3 scale = new Vector3(chestScale, chestScale, chestScale);
            breastL.localScale = scale;
            breastR.localScale = scale;
        }

        private void LowerBody()
        {
            butt.localScale = new Vector3(1, lowerBodyInput, lowerBodyInput);
            thighWidthL.localScale = Vector3.one * lowerBodyInput;
            thighWidthR.localScale = Vector3.one * lowerBodyInput;
        }

        private void Waist()
        {
            waist.localScale = waistInput * Vector3.one;
            
            torsoWidth.localScale = Avg(waistInput, upperBodyInput) * Vector3.one;
        }

        private void UpperBody()
        {
            float muscleScale = upperBodyInput;
            float chestX = Reduce(upperBodyInput, 0.5f);
            muscleChest.localScale = new Vector3(chestX, muscleScale, muscleScale);
            muscleNeck.localScale = Vector3.one * muscleScale;
        }

        private void Bicep()
        {
            float bicepScale = Increase(upperBodyInput, 1.2f);
            bicepScale = Mathf.Clamp(bicepScale, 0.7f, 3f);
            Vector3 bicep = new Vector3(bicepScale, 1, bicepScale);
            bicepL.localScale = bicep;
            bicepR.localScale = bicep;
        }

        private void Shoulder()
        {
            float shoulderScale = Mathf.Clamp(upperBodyInput, 1f, 3f);
            Vector3 shoulder = new Vector3(shoulderScale, 1, shoulderScale);
            muscleShoulderL.localScale = shoulder;
            muscleShoulderR.localScale = shoulder;
        }

        private void ForeArm()
        {
            float foreArmScale = Increase(upperBodyInput, 0.7f);
            foreArmScale = Mathf.Clamp(foreArmScale, 0.3f, 1.1f);
            Vector3 forearm = new Vector3(foreArmScale, 1, foreArmScale);
            forearmL.localScale = forearm;
            forearmR.localScale = forearm;
        }

        private void Head()
        {
            head.localScale = Vector3.one / heightInput / headInput;
        }

        private void Belly()
        {
            float bellyScale = 1 + ((bellyInput - 1) * 5);
            belly.localScale = new Vector3(bellyScale, bellyScale, bellyScale);
        }

        /// <summary>
        /// If 1 is default value use this to get a number smaller than the input.
        /// </summary>
        private float Reduce(float input, float multiplier)
        {
            return input - ((input - 1f) * multiplier);
        }

        /// <summary>
        /// If 1 is default value use this to get a number larger than the input.
        /// </summary>
        private float Increase(float input, float multiplier)
        {
            return input + ((input - 1f) * multiplier);
        }

        /// <summary>
        /// Average of two floats.
        /// </summary>
        private float Avg(float inp1, float inp2)
        {
            return (inp1 + inp2) / 2;
        }
        
        [ContextMenu("Scale")]
        public void EditorScale()
        {
            ScaleBones(false);
        }

    }
}