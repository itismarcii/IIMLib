using System;
using IIMLib.Dialogue.Authoring;
using UnityEngine;

namespace IIMLib.Dialogue.Localization
{
    [Serializable]
    public struct DialogueLocalizationParameter
    {
        [SerializeField] private string _key;
        [SerializeField] private AbstractDialogueMethodStringAsset _value;

        public string Key => _key;
        public AbstractDialogueMethodStringAsset Value => _value;
    }
}
