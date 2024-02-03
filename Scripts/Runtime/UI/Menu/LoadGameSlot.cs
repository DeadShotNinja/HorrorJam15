using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HJ.Runtime
{
    public class LoadGameSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _indexText;
        [SerializeField] private RawImage _thumbnail;
        [SerializeField] private TMP_Text _saveTypeText;
        [SerializeField] private TMP_Text _sceneNameText;
        [SerializeField] private TMP_Text _timeSavedText;
        [SerializeField] private TMP_Text _playtimeText;

        public void Initialize(int index, SavedGameInfo info)
        {
            _indexText.text = index.ToString();
            _thumbnail.texture = info.Thumbnail;
            _saveTypeText.text = info.IsAutosave ? "Autosave" : "Manual Save";
            _sceneNameText.text = info.Scene;
            _timeSavedText.text = info.TimeSaved.ToString("dd/MM/yyyy HH:mm:ss");
            _playtimeText.text = info.TimePlayed.ToString(@"hh\:mm\:ss");
        }
    }
}