using SS3D.Core;
using SS3D.Core.Behaviours;
using UnityEngine;
using SS3D.Attributes;
using TMPro;
using System.Collections.Generic;
using SS3D.Data;
using System;
using UnityEngine.UI;
using SS3D.Systems.Characters.Preferences;
using SS3D.Systems.Screens.Events;
using SS3D.Systems.Screens;
using SS3D.Systems.Characters.Events;
using Coimbra.Services.Events;

namespace SS3D.Systems.Characters.UI.View
{
    /// <summary>
    /// this demon controls the appearance tab in character creation
    /// </summary>
    public sealed class AppearanceTabView : Actor
    {
        [Header("Color Picker")]
        [SerializeField] [NotNull] private ColorPickerControl _colorPicker;

        [Header("Selections")]
        [SerializeField] [NotNull] private List<StyleGrid> _styleSelections = new();
        
        [Header("Color Choices")]
        [SerializeField] [NotNull] private ColorGradientSO _skinToneGradient;
        [SerializeField] [NotNull] private BodySlider _skintoneSlider;
        [SerializeField] [NotNull] private List<ColorSelection> _colorSelections;

        [Header("Body Sliders")]
        [SerializeField] [NotNull] private List<BodySlider> _sliders;

        private ClientPreferencesSubSystem _preferences;

        private CharacterProfile _character;

        #region Setup

        protected override void OnStart()
        {
            base.OnStart();
            _preferences = SubSystems.Get<ClientPreferencesSubSystem>();
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            AddHandle(GameScreenChanged.AddListener(HandleChangeGameScreen));

            AddHandle(LocalLobbyCharacterChanged.AddListener(HandleCharacterChanged));
            
            _styleSelections.ForEach(grid =>
            {
                grid.OnStarted += HandleStyleGridStarted;
                grid.OnSelected += HandleStyleSelected;
            });

            _colorSelections.ForEach(colorSelection =>
            {
                colorSelection.OnPressed += HandleColorSelectionPressed;
            });

            _sliders.ForEach(slider =>
            {
                slider.OnValueChanged += HandleBodySliderChanged;
                slider.OnStarted += HandleBodySliderStarted;
            });

            _skintoneSlider.OnValueChanged += HandleSkinToneChanged;

            _colorPicker.OnValueChanged += HandleColorSelectionChanged;
        }

        protected override void OnDestroyed()
        {
            base.OnDestroyed();

            _styleSelections.ForEach(grid =>
            {
                grid.OnStarted -= HandleStyleGridStarted;
                grid.OnSelected -= HandleStyleSelected;
            });

            _colorSelections.ForEach(colorSelection =>
            {
                colorSelection.OnPressed -= HandleColorSelectionPressed;
            });

            _sliders.ForEach(slider =>
            {
                slider.OnValueChanged -= HandleBodySliderChanged;
                slider.OnStarted -= HandleBodySliderStarted;
            });

            _skintoneSlider.OnValueChanged -= HandleSkinToneChanged;

            _colorPicker.OnValueChanged -= HandleColorSelectionChanged;
        }

        #endregion

        #region Update UI

        /// <summary>
        /// Method called when the game screen is changed
        /// </summary>
        private void HandleChangeGameScreen(ref EventContext context, in GameScreenChanged e)
        {
            ScreenType screenType = e.ActiveScreen;
            switch (screenType)
            {
                case ScreenType.CharacterCreation:
                    // switched to char creation
                    SetStyleSelections();
                    break;
            }
        }

        /// <summary>
        /// Set the ui when the character is modified.
        /// </summary>
        private void HandleCharacterChanged(ref EventContext context, in LocalLobbyCharacterChanged e)
        {
            _character = e.Character;
            switch (e.ChangeType)
            {
                case CharacterChangeType.Load:
                    SetStyleSelections();
                    SetColorPickers();
                    SetBodySliders();
                    SetSkintoneSlider();
                    break;
                case CharacterChangeType.Appearance:
                    SetStyleSelections();
                    SetColorPickers();
                    SetBodySliders();
                    SetSkintoneSlider();
                    break;
            }
        }

        private void SetSkintoneSlider()
        {
            _skintoneSlider.SetValue(_character.SkinTone);
        }

        private void SetBodySliders()
        {
            _sliders.ForEach(slider =>
            {
                slider.SetValue(float.Parse(_character?.GetBody(slider.Type)));
            });
        }

        private void SetStyleSelections()
        {
            _styleSelections.ForEach(grid =>
            {
                grid.SetSelectedOptionByName(_character?.GetStyle(grid.Type), false);
            });
        }

        private void SetColorPickers()
        {
            foreach (int i in Enum.GetValues(typeof(ColorType)))
            {
                ColorType type = (ColorType)i;
                switch (type)
                {
                    case ColorType.HairColor:
                    case ColorType.EyeColor:
                    case ColorType.SkinColor:
                        if (!ColorUtility.TryParseHtmlString("#" + _character?.GetColor(type), out Color color)) break;
                        SetColorPickerButton(type, color);
                        break;
                }
            }
        }

        private void SetColorPickerButton(ColorType type, Color color)
        {
            _colorSelections.ForEach(colorSelection =>
            {
                if (colorSelection.Type != type) return;
                colorSelection.SetColor(color);
            });
        }

        /// <summary>
        /// Method called when a grid is loaded so the correct option can be set as selected in the ui.
        /// </summary>
        private void HandleStyleGridStarted(CustomizationGrid grid)
        {
            if (_character == null) return;
            if (grid is StyleGrid styleGrid)
            {
                StyleType type = styleGrid.Type;
                grid.SetSelectedOptionByName(_character?.GetStyle(type), false);
            }
        }

        /// <summary>
        /// Method called when a slider is loaded so the correct value can be set in the ui.
        /// </summary>
        private void HandleBodySliderStarted(BodySlider slider)
        {
            if (_character == null) return;
            slider.SetValue(float.Parse(_character.GetBody(slider.Type)));
        }

        #endregion

        #region Color Picking

        /// <summary>
        /// Callback when a color button is pressed.
        /// </summary>
        private void HandleColorSelectionPressed(ColorType type)
        {
            _colorPicker.SetActive(true);
            _colorPicker.Type = type;
            _colorPicker.SetColorFromHex(_character.GetColor(type));
        }

        #endregion

        #region Set Character

        /// <summary>
        /// Callback when an appearance option is selected.
        /// </summary>
        private void HandleStyleSelected(CustomizationGrid grid, CustomizationSlot option)
        {
            if (grid is not StyleGrid styleGrid) return;
            _preferences.SetStyle(styleGrid.Type, option.CustomizationSO.name);
        }

        /// <summary>
        /// Callback when a color is chosen.
        /// </summary>
        private void HandleColorSelectionChanged(ColorType type, Color color)
        {
            _preferences.SetColor(type, ColorUtility.ToHtmlStringRGB(color));
        }

        /// <summary>
        /// Callback when a color is chosen.
        /// </summary>
        private void HandleSkinToneChanged(BodyType type, float value)
        {
            Color color = _skinToneGradient.Gradient.Evaluate(value);
            _preferences.SetSkinTone(value, ColorUtility.ToHtmlStringRGB(color));
        }

        /// <summary>
        /// Callback when a body slider is changed.
        /// </summary>
        private void HandleBodySliderChanged(BodyType type, float value)
        {
            _preferences.SetBody(type, value.ToString());
        }
        
        #endregion
        
    }
}
