using Coimbra;
using SS3D.Systems.Roles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Characters.Preferences
{

    [Serializable]
    public class CharacterProfile : ISerializationCallbackReceiver
    {
        public string Name = "John Beep";
        public string FlavorText = string.Empty;
        public int Age = 18;
        public Sex Sex = Sex.Male;
        public Gender Gender = Gender.Male;

        [NonSerialized]
        public Dictionary<AppearanceType, string> Appearance = new();

        [NonSerialized]
        public Dictionary<RoleData, JobPriority> Jobs = new();

        public List<string> Antags = new();

        public List<string> Traits = new();

        #region Constructors

        /// <summary>
        /// Create a character profile with default values
        /// </summary>
        public CharacterProfile()
        {
            Appearance = new Dictionary<AppearanceType, string>
            {
                { AppearanceType.Hairstyle,     "Bald" },
                { AppearanceType.Beardstyle,    "Bald" },
                { AppearanceType.Eyebrows,      "Default" },

                { AppearanceType.HairColor,     "804831" },
                { AppearanceType.EyeColor,      "000000" },
                { AppearanceType.SkinColor,     "FFBD99" },

                // SLIDER DEFAULTS GO HERE
            };
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public CharacterProfile(CharacterProfile character)
        {
            Name = character.Name;
            FlavorText = character.FlavorText;
            Age = character.Age;
            Sex = character.Sex;
            Gender = character.Gender;
            Appearance = new Dictionary<AppearanceType, string>(character.Appearance);
            Jobs = new Dictionary<RoleData, JobPriority>(character.Jobs);
            Antags = new List<string>(character.Antags);
            Traits = new List<string>(character.Traits);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        // public CharacterProfile(string name, float value)
        // {
        //     Name = "John Beep";
        //     FlavorText = string.Empty;
        //     Age = 18;
        //     Sex = Sex.Male;
        //     Gender = Gender.Male;
        //     Appearance = new Dictionary<AppearanceType, string>()
        // }
        #endregion

        #region Serialization

        // i dont think SerializableDictionary is able to be sent over the network so i have to do this
        [SerializeField]
        private SerializableDictionary<AppearanceType, string> _appearance;
        [SerializeField]
        private SerializableDictionary<RoleData, JobPriority> _jobs;

        public void OnAfterDeserialize()
        {
            Appearance = new Dictionary<AppearanceType, string>(_appearance);
            _appearance.Clear();
            Jobs = new Dictionary<RoleData, JobPriority>(_jobs);
            _jobs.Clear();
        }

        public void OnBeforeSerialize()
        {
            _appearance = new SerializableDictionary<AppearanceType, string>(Appearance);
            _jobs = new SerializableDictionary<RoleData, JobPriority>(Jobs);
            // SerializableDictionary<AppearanceType, string> Appearance;
        }
        #endregion
    }
}