using Coimbra;
using UnityEngine;
using UnityEngine.UI;
using SS3D.Core.Behaviours;
using SS3D.Systems.Characters.Preferences;
using Actor = SS3D.Core.Behaviours.Actor;
using System;

namespace SS3D.Systems.Characters.UI
{
    public class ColorSelection : Actor
    {
        public delegate void ColorSelectionPressed(ColorType type);
        
        public event ColorSelectionPressed OnPressed;

        [SerializeField] private ColorType _type;

        public ColorType Type => _type;

        public void SetColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        public void HandleButtonPressed()
        {
            OnPressed?.Invoke(_type);
        }
    }
}