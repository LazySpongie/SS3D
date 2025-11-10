using TMPro;
using SS3D.Attributes;
using UnityEngine;
using UnityEngine.UI;
using SS3D.Core.Behaviours;

namespace SS3D.Systems.Characters
{
    public class CharacterSlot : Actor
    {
        [SerializeField] [NotNull] private Button _characterButton;

        [SerializeField] [NotNull] private Button _deleteButton;
        [SerializeField] [NotNull] private TMP_Text _text;

        public Button CharacterButton => _characterButton;

        public Button DeleteButton => _deleteButton;

        public TMP_Text Text => _text;

        public void SetName(string text)
        {
            _text.text = text;
        }

        public void SetSelected(bool selected)
        {
            _characterButton.interactable = !selected;

            // _deleteButton.interactable = !selected;
            _deleteButton.gameObject.SetActive(!selected);
        }
    }
}