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
        public string Name = "New Character";
        public string FlavorText = string.Empty;
        public int Age = 18;
        public Sex Sex = Sex.Male;
        public Gender Gender = Gender.Male;

        // [NonSerialized]
        public Dictionary<StyleType, string> Styles = new Dictionary<StyleType, string>
            {
                { StyleType.Hairstyle,     "Bald" },
                { StyleType.Beardstyle,    "Bald" },
                { StyleType.Eyebrows,      "Default" },

            };

        public Dictionary<ColorType, string> Colors = new Dictionary<ColorType, string>
            {
                { ColorType.HairColor,     "804831" },
                { ColorType.EyeColor,      "000000" },
                { ColorType.SkinColor,     "FFBD99" },

            };

        public Dictionary<BodyType, string> Body = new Dictionary<BodyType, string>
            {
                { BodyType.Height,        "1" },
                { BodyType.Belly,         "1" },
                { BodyType.Jaw,           "1" },
                { BodyType.UpperBody,     "1" },
                { BodyType.Chest,         "0" },
                { BodyType.Waist,         "1" },
                { BodyType.LowerBody,     "1" },
            };

        // [NonSerialized]
        public Dictionary<RoleData, JobPriority> Jobs = new();

        public List<string> Antags = new();

        public List<string> Traits = new();

        #region Constructors

        /// <summary>
        /// Create a character profile with default values
        /// </summary>
        public CharacterProfile()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public CharacterProfile(CharacterProfile character)
        {
            if (character == null) character = new CharacterProfile();
            Name = character.Name;
            FlavorText = character.FlavorText;
            Age = character.Age;
            Sex = character.Sex;
            Gender = character.Gender;
            Styles = new Dictionary<StyleType, string>(character.Styles);
            Colors = new Dictionary<ColorType, string>(character.Colors);
            Body = new Dictionary<BodyType, string>(character.Body);
            Jobs = new Dictionary<RoleData, JobPriority>(character.Jobs);
            Antags = new List<string>(character.Antags);
            Traits = new List<string>(character.Traits);
        }
        #endregion

        #region Getters

        public string GetName()
        {
            if (Name != string.Empty)
            {
                return Name;
            }
            else
            {
                return new CharacterProfile().Name;
            }
        }

        public string GetStyle(StyleType type)
        {
            if (Styles.ContainsKey(type))
            {
                return Styles[type];
            }
            else
            {
                return new CharacterProfile().Styles[type];
            }
        }

        public string GetColor(ColorType type)
        {
            if (Colors.ContainsKey(type))
            {
                return Colors[type];
            }
            else
            {
                return new CharacterProfile().Colors[type];
            }
        }

        public string GetBody(BodyType type)
        {
            if (Body.ContainsKey(type))
            {
                return Body[type];
            }
            else
            {
                return new CharacterProfile().Body[type];
            }
        }
        
        #endregion

        #region Serialization

        [SerializeField] private SerializableDictionary<StyleType, string> _styles;
        [SerializeField] private SerializableDictionary<ColorType, string> _Colors;
        [SerializeField] private SerializableDictionary<BodyType, string> _Body;
        [SerializeField] private SerializableDictionary<RoleData, JobPriority> _jobs;

        public void OnAfterDeserialize()
        {
            Styles = new Dictionary<StyleType, string>(_styles);
            Colors = new Dictionary<ColorType, string>(_Colors);
            Body = new Dictionary<BodyType, string>(_Body);
            Jobs = new Dictionary<RoleData, JobPriority>(_jobs);

            _styles.Clear();
            _Colors.Clear();
            _Body.Clear();
            _jobs.Clear();
        }

        public void OnBeforeSerialize()
        {
            _styles = new SerializableDictionary<StyleType, string>(Styles);
            _Colors = new SerializableDictionary<ColorType, string>(Colors);
            _Body = new SerializableDictionary<BodyType, string>(Body);
            _jobs = new SerializableDictionary<RoleData, JobPriority>(Jobs);
        }

        #endregion
    }
}