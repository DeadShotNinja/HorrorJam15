using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HJ.Runtime
{
    public class SavesUILoader : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private BackgroundFader _backgroundFader;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _loadButton;

        [Header("Save Slot")]
        [SerializeField] private Transform _saveSlotsParent;
        [SerializeField] private GameObject _saveSlotPrefab;

        [Header("Settings")]
        [SerializeField] private bool _fadeOutAtStart;
        [SerializeField] private bool _loadAtStart;
        [SerializeField] private float _fadeSpeed;

        [Header("Events")]
        [SerializeField] private UnityEvent _onSavesBeingLoaded;
        [SerializeField] private UnityEvent _onSavesLoaded;
        [SerializeField] private UnityEvent _onSavesEmpty;

        private readonly Dictionary<GameObject, SavedGameInfo> _saveSlots = new();
        private SavedGameInfo? _lastSave;
        private SavedGameInfo? _selected;

        private bool _isLoading;

        private async void Start()
        {
            if (_loadAtStart)
            {
                // load saves process
                await LoadAllSaves();

                // enable or disable continue button when last save exists
                if(_continueButton != null)
                    _continueButton.gameObject.SetActive(_lastSave.HasValue);

                if (_fadeOutAtStart) 
                    StartCoroutine(_backgroundFader.StartBackgroundFade(true));
            }
        }

        public async void LoadSavedGames()
        {
            foreach (var slot in _saveSlots)
            {
                Destroy(slot.Key);
            }

            _saveSlots.Clear();
            _onSavesBeingLoaded?.Invoke();

            await LoadAllSaves();
        }

        public void LoadLastSave()
        {
            if (!_lastSave.HasValue || _isLoading) 
                return;

            SaveGameManager.SetLoadGameState(_lastSave.Value.Scene, _lastSave.Value.Foldername);
            StartCoroutine(FadeAndLoadGame());

            _selected = null;
            _isLoading = true;
        }

        public void LoadSelectedSave()
        {
            if (!_selected.HasValue || _isLoading)
                return;

            SaveGameManager.SetLoadGameState(_selected.Value.Scene, _selected.Value.Foldername);
            StartCoroutine(FadeAndLoadGame());

            _selected = null;
            _isLoading = true;
        }

        public void ResetSaves()
        {
            if (_loadButton != null) _loadButton.gameObject.SetActive(false);
            foreach (var slot in _saveSlots)
            {
                UIButton slotButton = slot.Key.GetComponent<UIButton>();
                slotButton.DeselectButton();
            }

            _selected = null;
        }

        private async Task LoadAllSaves()
        {
            // load saves in another thread
            var savedGames = await SaveGameManager.SaveGameReader.ReadAllSaves();

            // set last saved game
            if(savedGames.Length > 0)
                _lastSave = savedGames[0];

            // instantiate saves in main thread
            for (int i = 0; i < savedGames.Length; i++)
            {
                SavedGameInfo saveInfo = savedGames[i];
                GameObject slotGO = Instantiate(_saveSlotPrefab, _saveSlotsParent);
                slotGO.name = "Slot" + i.ToString();

                LoadGameSlot loadGameSlot = slotGO.GetComponent<LoadGameSlot>();
                loadGameSlot.Initialize(i, saveInfo);

                UIButton loadButton = slotGO.GetComponent<UIButton>();
                loadButton.OnClick.AddListener((_) => 
                {
                    _selected = saveInfo;
                    if (_loadButton != null) _loadButton.gameObject.SetActive(true);
                });

                _saveSlots.Add(slotGO, saveInfo);
            }

            if(savedGames.Length > 0)
            {
                _onSavesLoaded?.Invoke();
            }
            else
            {
                _onSavesEmpty?.Invoke();
            }
        }

        IEnumerator FadeAndLoadGame()
        {
            if(_backgroundFader != null) yield return _backgroundFader.StartBackgroundFade(false, fadeSpeed: _fadeSpeed);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }
    }
}