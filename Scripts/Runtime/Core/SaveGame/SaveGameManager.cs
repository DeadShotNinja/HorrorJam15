using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HJ.Runtime.Rendering;
using HJ.Scriptable;
using HJ.Tools;
//
namespace HJ.Runtime
{
    public class SaveGameManager : Singleton<SaveGameManager>
    {
        public enum LoadType
        {
            /// <summary>
            /// Normal scene loading.
            /// </summary>
            Normal,

            /// <summary>
            /// Load saved game from slot.
            /// </summary>
            LoadGameState,

            /// <summary>
            /// Load world state (Previous Scene Persistency).
            /// </summary>
            LoadWorldState,

            /// <summary>
            /// Load player data.
            /// </summary>
            LoadPlayer
        }

        public enum SaveType { Normal, Autosave, NextScene }
        
        [Header("References")]
        [SerializeField] private ObjectReferences _objectReferences;
        [SerializeField] private CanvasGroup _savingIcon;

        [Header("Saveable Lists")]
        [SerializeField] private List<SaveablePair> _worldSaveables = new();
        [SerializeField] private List<RuntimeSaveable> _runtimeSaveables = new();
        
        [Header("Debugging")]
        [SerializeField] private bool _debugging;
        
        private static SaveGameReader _saveGameReader;
        private static JObject _playerData;
        private static JObject _gameState;
        private static float _timePlayed;

        private SaveType _gameSaveType = SaveType.Normal;
        private Vector3 _customSavePosition;
        private Vector2 _customSaveRotation;
        private event Action _onWaitForSave;

        private string _currentScene;
        private bool _timerActive;
        private bool _isSaved;

        private SpritesheetAnimation _spritesheetAnimation;
        private PlayerPresenceManager _playerPresence;
        private ObjectiveManager _objectiveManager;
        private Inventory _inventory;
        
        #region Structures
        [Serializable]
        public struct SaveablePair
        {
            public string Token;
            public MonoBehaviour Instance;

            public SaveablePair(string token, MonoBehaviour instance)
            {
                Token = token;
                Instance = instance;
            }
        }

        [Serializable]
        public struct RuntimeSaveable
        {
            public string TokenGUID;
            public GameObject InstantiatedObject;
            public SaveablePair[] SaveablePairs;
        }
        #endregion

        public List<SaveablePair> WorldSaveables
        {
            get => _worldSaveables;
            set => _worldSaveables = value;
        }

        public List<RuntimeSaveable> RuntimeSaveables => _runtimeSaveables;
        public ObjectReferences ObjectReferences => _objectReferences;
        
        /// <summary>
        /// Check if the game will be loaded (<see cref="GameLoadType"/> is not set to Normal).
        /// </summary>
        public static bool IsGameJustLoaded => HasReference && GameLoadType != LoadType.Normal;

        /// <summary>
        /// Check if the game state of the level actually exists. Use with <see cref="IsGameJustLoaded"/> to check if the game state will be actually loaded.
        /// </summary>
        public static bool GameStateExist => _gameState != null;

        public static LoadType GameLoadType = LoadType.Normal;
        public static string LoadSceneName;
        public static string LoadFolderName;
        public static Dictionary<string, string> LastSceneSaves;
        
        public const char TOKEN_SEPARATOR = '.';
        
        public event Action<string> OnGameSaved;
        public event Action<bool> OnGameLoaded;

        public static SerializationAsset SerializationAsset
            => SerializationUtillity.SerializationAsset;
        
        public static SaveGameReader SaveGameReader
            => _saveGameReader ??= new SaveGameReader(SerializationAsset);

        /// <summary>
        /// Shortcut to Level Manager Scene/Scene Loader
        /// </summary>
        public static string LMS => SerializationAsset.LevelManagerScene;

        /// <summary>
        /// Shortcut to MainMenu Scene
        /// </summary>
        public static string MM => SerializationAsset.MainMenuScene;

        private static string SavedGamePath
        {
            get
            {
                string savesPath = SerializationAsset.GetSavesPath();
                if (!Directory.Exists(savesPath))
                    Directory.CreateDirectory(savesPath);

                return savesPath;
            }
        }

        private void Awake()
        {
            if (_savingIcon != null) 
                _spritesheetAnimation = _savingIcon.GetComponent<SpritesheetAnimation>();

            _playerPresence = GetComponent<PlayerPresenceManager>();
            _objectiveManager = GetComponent<ObjectiveManager>();
            _inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            if (_debugging)
            {
                if (GameLoadType == LoadType.LoadGameState)
                    Debug.Log($"[SaveGameManager] Attempting to load game state.");
                else if (GameLoadType == LoadType.LoadWorldState)
                    Debug.Log($"[SaveGameManager] Attempting to load world state.");
            }

            bool shouldLoadPlayer = false;
            if (GameLoadType == LoadType.LoadGameState || (GameLoadType == LoadType.LoadWorldState && SerializationAsset.PreviousScenePersistency))
            {
                if (_gameState != null)
                {
                    // load game state and clear load type
                    LoadGameState(_gameState);
                    StartCoroutine(ClearLoad());

                    // show debug log
                    if (_debugging) Debug.Log("[SaveGameManager] The game state has been loaded.");

                    // invoke game loaded event
                    OnGameLoaded?.Invoke(true);
                }
                else if (GameLoadType == LoadType.LoadWorldState)
                {
                    shouldLoadPlayer = true;
                    if (_debugging) Debug.Log($"[SaveGameManager] The last world state does not exist. Loading is prevented.");
                }
            }
            else
            {
                shouldLoadPlayer = true;
            }

            if (GameLoadType == LoadType.LoadPlayer || shouldLoadPlayer)
            {
                // load player data
                if (_playerData != null)
                {
                    LoadPlayerData(_playerData);
                    if (_debugging) Debug.Log($"[SaveGameManager] Player data has been loaded.");
                }

                OnGameLoaded?.Invoke(false);
                StartCoroutine(ClearLoad());
            }

            // get current scene name
            _currentScene = SceneManager.GetActiveScene().name;

            // start time played timer at start
            _timerActive = true;
        }

        private void Update()
        {
            if(_timerActive) _timePlayed += Time.deltaTime;
        }

        IEnumerator ClearLoad()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(2f);
            ClearLoadType();
        }

        #region Exposed Methods
        /// <summary>
        /// Clear the load type for normal scene loading.
        /// <br>The game will not be loaded.</br>
        /// </summary>
        public static void ClearLoadType()
        {
            GameLoadType = LoadType.Normal;
            _playerData = null;
            _gameState = null;
        }

        /// <summary>
        /// Clear the load scene and folder name.
        /// </summary>
        public static void ClearLoadName()
        {
            LoadSceneName = string.Empty;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Set the load type to load the game state.
        /// <br>The game state and player data will be loaded from a saved game.</br>
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        /// <param name="folderName">The name of the saved game folder.</param>
        public static void SetLoadGameState(string sceneName, string folderName)
        {
            GameLoadType = LoadType.LoadGameState;
            LoadSceneName = sceneName;
            LoadFolderName = folderName;
        }

        /// <summary>
        /// Set the load type to load the last game state of the world.
        /// <br>The game will be loaded from the last save of the scene. The player data will be transferred.</br>
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        public static void SetLoadWorldState(string sceneName)
        {
            GameLoadType = LoadType.LoadWorldState;
            LoadSceneName = sceneName;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Set the load type to load the player data only.
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        public static void SetLoadPlayerData(string sceneName)
        {
            GameLoadType = LoadType.LoadPlayer;
            LoadSceneName = sceneName;
            LoadFolderName = string.Empty;
        }

        /// <summary>
        /// Save player data only.
        /// <br>The world state will not be saved.</br>
        /// </summary>
        public static void SavePlayer()
        {
            Instance.SetStaticPlayerData();
        }

        /// <summary>
        /// Save game state normally. 
        /// <br>The world state and player data will be saved.</br>
        /// </summary>
        public static void SaveGame(bool autosave)
        {
            Instance._gameSaveType = autosave ? SaveType.Autosave : SaveType.Normal;
            Instance.PrepareAndSaveGameState();
        }

        /// <summary>
        /// Save game state normally. 
        /// <br>The world state and player data will be saved.</br>
        /// </summary>
        /// <param name="onSaved">Event when the game is successfully saved.</param>
        public static void SaveGame(Action onSaved)
        {
            Instance._gameSaveType = SaveType.Normal;
            Instance.PrepareAndSaveGameState();
            Instance._onWaitForSave += onSaved;
        }

        /// <summary>
        /// Save game state with custom player position and rotation. 
        /// <br>The world state and player data will be saved.</br>
        /// <br>Usability: If you need to have a different player position and rotation when returning to the previous scene.</br>
        /// </summary>
        /// <param name="onSaved">Event when the game is successfully saved.</param>
        public static void SaveGame(Vector3 position, Vector2 rotation, Action onSaved)
        {
            Instance._gameSaveType = SaveType.NextScene;
            Instance._customSavePosition = position;
            Instance._customSaveRotation = rotation;
            Instance.PrepareAndSaveGameState();
            Instance._onWaitForSave += onSaved;
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        /// <remarks>
        /// The object is instantiated and added to the list of saveable objects so that it can be saved and loaded later. The object must be stored in the ObjectReferences list.
        /// </remarks>
        public static GameObject InstantiateSaveable(ObjectReference reference, Vector3 position, Vector3 rotation, string name = null)
        {
            GameObject instantiate = Instantiate(reference.Object, position, Quaternion.Euler(rotation));
            instantiate.name = name ?? reference.Object.name;
            Instance.AddRuntimeSaveable(instantiate, reference.GUID);
            return instantiate;
        }

        /// <summary>
        /// Instantiate Runtime Saveable.
        /// </summary>
        /// <remarks>
        /// The object is instantiated and added to the list of saveable objects so that it can be saved and loaded later. The object must be stored in the ObjectReferences list.
        /// </remarks>
        public static GameObject InstantiateSaveable(string referenceGUID, Vector3 position, Vector3 rotation, string name = null)
        {
            var reference = Instance._objectReferences.GetObjectReference(referenceGUID);
            if (reference.HasValue)
            {
                GameObject instantiate = Instantiate(reference?.Object, position, Quaternion.Euler(rotation));
                instantiate.name = name ?? reference?.Object.name;
                Instance.AddRuntimeSaveable(instantiate, referenceGUID);
                return instantiate;
            }

            return null;
        }

        /// <summary>
        /// Remove Runtime Saveable.
        /// </summary>
        public static void RemoveSaveable(GameObject obj)
        {
            Instance._runtimeSaveables.RemoveAll(x => x.InstantiatedObject == obj);
        }

        /// <summary>
        /// Set time played timer active state.
        /// </summary>
        public static void SetTimePlayedTimer(bool state)
        {
            Instance._timerActive = state;
        }
        #endregion

        #region SAVING GAME STATE
        IEnumerator ShowSavingIcon()
        {
            if (Instance._savingIcon == null)
                yield return null;

            // show saving icon
            if (_spritesheetAnimation != null && !_spritesheetAnimation.PlayOnStart)
                _spritesheetAnimation.SetAnimationStatus(true);

            CanvasGroupFader.StartFadeInstance(Instance._savingIcon, true, 3f);

            yield return new WaitForSeconds(2f);
            yield return new WaitUntil(() => _isSaved);
            yield return CanvasGroupFader.StartFade(Instance._savingIcon, false, 3f);

            // hide saving icon
            if (_spritesheetAnimation != null)
                _spritesheetAnimation.SetAnimationStatus(false);

            _isSaved = false;
        }

        private async void PrepareAndSaveGameState()
        {
            // show saving icon
            _isSaved = false;
            StopCoroutine(ShowSavingIcon());
            StartCoroutine(ShowSavingIcon());

            // create player state and data buffers
            StorableCollection worldStatePlayerData = new();

            // store player position and rotation
            if (SerializationAsset.PreviousScenePersistency && _gameSaveType == SaveType.NextScene)
            {
                worldStatePlayerData.Add("position", _customSavePosition.ToSaveable());
                worldStatePlayerData.Add("rotation", _customSaveRotation.ToSaveable());
            }
            else
            {
                var (position, rotation) = _playerPresence.GetPlayerTransform();
                worldStatePlayerData.Add("position", position.ToSaveable());
                worldStatePlayerData.Add("rotation", rotation.ToSaveable());
            }

            // add player data to player data buffer
            StorableCollection globalPlayerData = GetPlayerDataBuffer();
            StorableCollection localPlayerData = _playerPresence.PlayerManager.OnCustomSave();
            StorableCollection playerData = new();

            playerData.Add("localData", localPlayerData);
            worldStatePlayerData.Add("localData", localPlayerData);

            playerData.Add("globalData", globalPlayerData);
            worldStatePlayerData.Add("globalData", globalPlayerData);

            // create saveable buffer with world data
            string saveId = GameTools.GetGuid();
            StorableCollection saveInfoData = new()
            {
                { "id", saveId },
                { "scene", _currentScene },
                { "dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "timePlayed", _timePlayed },
                { "saveType", (int)_gameSaveType },
                { "data", "" },
                { "thumbnail", "" }
            };

            // add player data and world state data to saveables buffer
            StorableCollection saveablesBuffer = new()
            {
                { "id", saveId },
                { "playerData", worldStatePlayerData },
                { "worldState", GetWorldStateBuffer() }
            };

            // store player data to static variable
            _playerData = JObject.FromObject(playerData);

            // get save folder name
            GetSaveFolder(out string saveFolderName);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);

            // create save folder
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);

            // if thumbnail creation is enabled, take a screenshot and save it to the thumbnail file
            if (SerializationAsset.CreateThumbnails)
            {
                string thumbnailName = SerializationAsset.SaveThumbnailName + ".png";
                string thumbnailPath = Path.Combine(saveFolderPath, thumbnailName);
                saveInfoData["thumbnail"] = thumbnailName;
                CreateThumbnail(thumbnailPath);
            }

            // serialize save info and world state
            await SerializeGameState(saveInfoData, saveablesBuffer, saveFolderName, saveFolderPath);
        }

        private StorableCollection GetPlayerDataBuffer()
        {
            // Generate Player Data Buffer: Inventory, Objectives, Other stuff...
            StorableCollection buffer = new StorableCollection();

            buffer.Add("inventory", (_inventory as ISaveableCustom).OnCustomSave());
            buffer.Add("objectives", (_objectiveManager as ISaveableCustom).OnCustomSave());

            return buffer;
        }

        private StorableCollection GetWorldStateBuffer()
        {
            // Generate World State Buffer: World Saveables, Runtime Saveables
            StorableCollection saveablesBuffer = new StorableCollection();
            StorableCollection worldBuffer = new StorableCollection();
            StorableCollection runtimeBuffer = new StorableCollection();

            foreach (var saveable in _worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.Token))
                    continue;

                var saveableData = ((ISaveable)saveable.Instance).OnSave();
                worldBuffer.Add(saveable.Token, saveableData);
            }

            foreach (var saveable in _runtimeSaveables)
            {
                if (saveable.InstantiatedObject == null || string.IsNullOrEmpty(saveable.TokenGUID))
                    continue;

                StorableCollection instantiateSaveables = new StorableCollection();
                foreach (var saveablePair in saveable.SaveablePairs)
                {
                    if (saveablePair.Instance == null || string.IsNullOrEmpty(saveablePair.Token))
                        continue;

                    var saveableData = ((ISaveable)saveablePair.Instance).OnSave();
                    instantiateSaveables.Add(saveablePair.Token, saveableData);
                }

                runtimeBuffer.Add(saveable.TokenGUID, instantiateSaveables);
            }

            saveablesBuffer.Add("worldSaveables", worldBuffer);
            saveablesBuffer.Add("runtimeSaveables", runtimeBuffer);
            return saveablesBuffer;
        }

        private void SetStaticPlayerData()
        {
            StorableCollection globalPlayerData = GetPlayerDataBuffer();
            StorableCollection localPlayerData = _playerPresence.PlayerManager.OnCustomSave();
            StorableCollection playerDataBuffer = new StorableCollection
            {
                { "localData", localPlayerData },
                { "globalData", globalPlayerData }
            };

            JObject playerData = JObject.FromObject(playerDataBuffer);
            _playerData = playerData;
        }

        private void GetSaveFolder(out string saveFolderName)
        {
            saveFolderName = SerializationAsset.SaveFolderPrefix;

            if (!SerializationAsset.SingleSave)
            {
                string[] directories = Directory.GetDirectories(SavedGamePath, $"{saveFolderName}*");
                saveFolderName += directories.Length.ToString("D3");
            }
            // if single save and use of scene names is enabled, the scene name is used as the save name
            else if (SerializationAsset.UseSceneNames)
            {
                saveFolderName += _currentScene.Replace(" ", "");
            }
            // if single save and previous scene persistency is enabled
            else if (SerializationAsset.PreviousScenePersistency)
            {
                if (LastSceneSaves == null)
                {
                    // set save name to save prefix + 000
                    saveFolderName += "000";
                }
                else if (LastSceneSaves.ContainsKey(_currentScene))
                {
                    // set save name to last scene save name
                    saveFolderName = LastSceneSaves[_currentScene];
                }
                else
                {
                    // set save name to save prefix + count of saves
                    saveFolderName += LastSceneSaves.Count().ToString("D3");
                }
            }
        }

        private async Task SerializeGameState(StorableCollection saveInfo, StorableCollection worldState, string folderName, string folderPath)
        {
            string saveInfoFilename = SerializationAsset.SaveInfoName + SerializationAsset.SaveExtension;
            string saveDataFilename = SerializationAsset.SaveDataName + SerializationAsset.SaveExtension;
            saveInfo["data"] = saveDataFilename;

            // if previous scene persistency is enabled, store or update last scene save
            if (SerializationAsset.PreviousScenePersistency)
            {
                // if last scene saves dictionary is null, create a new instance
                if (LastSceneSaves == null) await LoadLastSceneSaves();

                // change the name of the last saved scene to a new one
                if (LastSceneSaves.ContainsKey(_currentScene))
                {
                    LastSceneSaves[_currentScene] = folderName;
                }
                else
                {
                    LastSceneSaves.Add(_currentScene, folderName);
                }
            }

            // serialize save info to file
            string saveInfoPath = Path.Combine(folderPath, saveInfoFilename);
            await SerializeData(saveInfo, saveInfoPath);

            // serialize save data to file
            string saveDataPath = Path.Combine(folderPath, saveDataFilename);
            await SerializeData(worldState, saveDataPath);

            // show debug log
            if (_debugging) Debug.Log($"[SaveGameManager] The game state has been saved to the '{folderName}' folder.");

            // invoke game saved events
            OnGameSaved?.Invoke(folderName);
            _onWaitForSave?.Invoke();
            _isSaved = true;
        }
        #endregion

        #region LOAD GAME STATE
        /// <summary>
        /// Try to Deserialize and Validate Game State
        /// </summary>
        public static async Task TryDeserializeGameStateAsync(string folderName)
        {
            // get saves path
            string savesPath = SerializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);

            // check if directory exists
            if (!Directory.Exists(saveFolderPath)) return;

            // deserialize saved game info
            var saveInfo = await SaveGameReader.ReadSave(folderName);

            // get data path and check if the file exists
            string dataFilepath = Path.Combine(saveFolderPath, saveInfo.Dataname);
            if (!File.Exists(dataFilepath)) throw new FileNotFoundException("Serialized file with saved data could not be found!");

            // deserialize game data state
            string gameStateJson = await DeserializeData(dataFilepath);
            JObject gameState = JObject.Parse(gameStateJson);

            // validate id of the save info and data
            if (saveInfo.Id != (string)gameState["id"])
                throw new DataException("Saved Info and the Saved Data do not match!");

            // assign time played
            _timePlayed = (float)saveInfo.TimePlayed.TotalSeconds;

            // assign deserialized game state
            _gameState = gameState;
        }

        /// <summary>
        /// Try to Deserialize Last Scene Saves
        /// </summary>
        public static async Task LoadLastSceneSaves()
        {
            LastSceneSaves = new Dictionary<string, string>();

            SavedGameInfo[] savedGames = (await SaveGameReader.ReadSavesMeta())
                .GroupBy(x => x.Scene)
                .Select(s => s.OrderByDescending(x => x.TimeSaved).FirstOrDefault())
                .ToArray();

            foreach (var save in savedGames)
            {
                LastSceneSaves.Add(save.Scene, save.Foldername);
            }
        }

        /// <summary>
        /// Remove all saved games.
        /// </summary>
        public static async Task RemoveAllSaves()
        {
            LastSceneSaves?.Clear();
            await SaveGameReader.RemoveAllSaves();
        }

        private void LoadGameState(JObject gameState)
        {
            if (gameState != null)
            {
                // load world state
                if (gameState.ContainsKey("worldState"))
                {
                    JToken worldState = gameState["worldState"];
                    LoadSaveables(worldState);
                }

                // load player data
                if (gameState.ContainsKey("playerData"))
                {
                    // parse player data from game state
                    JObject playerData = JObject.FromObject(gameState["playerData"]);

                    // set player position and rotation from game state
                    Vector3 playerPos = playerData["position"].ToObject<Vector3>();
                    Vector2 playerRot = playerData["rotation"].ToObject<Vector2>();
                    _playerPresence.SetPlayerTransform(playerPos, playerRot);

                    if (GameLoadType == LoadType.LoadWorldState && _playerData != null)
                    {
                        // load static player data
                        LoadPlayerData(_playerData);
                    }
                    else if (GameLoadType == LoadType.LoadGameState || _playerData == null)
                    {
                        // load player data from game state
                        LoadPlayerData(playerData);
                    }
                }
            }
        }

        private void LoadSaveables(JToken worldState)
        {
            bool isTokenError = false;
            JToken worldSaveablesData = worldState["worldSaveables"];
            JToken runtimeSaveablesData = worldState["runtimeSaveables"];

            // iterate every world saveable
            foreach (var saveable in _worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.Token))
                    continue;

                JToken token = worldSaveablesData[saveable.Token];
                if (token == null)
                {
                    Debug.LogError($"Could not find saveable with token '{saveable.Token}'.");
                    isTokenError = true;
                    continue;
                }
                ((ISaveable)saveable.Instance).OnLoad(token);
            }

            if (isTokenError)
            {
                Debug.LogError("[Token Error] Try to save your scene before loading game.");
                return;
            }

            // iterate every runtime saveable
            foreach (JProperty saveable in runtimeSaveablesData.Cast<JProperty>())
            {
                string tokenGUID = saveable.Name.Split('.')[0];
                var reference = _objectReferences.GetObjectReference(tokenGUID);

                if (reference.HasValue)
                {
                    // instantiate saved object
                    GameObject instantiate = Instantiate(reference?.Object, Vector3.zero, Quaternion.identity);
                    instantiate.name = "Instance_" + reference?.Object.name;

                    // get saveables from instantiated object
                    SaveablePair[] saveablePairs = FindSaveablesInChildren(instantiate);

                    // add instantiated object to runtime saveables again
                    AddRuntimeSaveable(instantiate, tokenGUID, saveablePairs);

                    // iterate every saveable
                    foreach (JProperty saveableToken in saveable.Value.Cast<JProperty>())
                    {
                        // get saveable uniqueID and token
                        string uniqueID = saveableToken.Name.Split(TOKEN_SEPARATOR)[1];
                        JToken token = saveableToken.Value;
                        bool isUIDError = true;

                        // iterate every saveable pair in instantiated object
                        foreach (var saveablePair in saveablePairs)
                        {
                            // check if saveable uid is equal with uid in instantiated object
                            string uID = saveablePair.Token.Split(TOKEN_SEPARATOR)[1];
                            if (uniqueID == uID)
                            {
                                // load token
                                ((ISaveable)saveablePair.Instance).OnLoad(token);
                                isUIDError = false;
                            }
                        }

                        if (isUIDError) Debug.LogError($"[UniqueID Error] Could not find script with Unique ID: {uniqueID}.");
                    }
                }
            }
        }

        private void LoadPlayerData(JObject playerData)
        {
            // get local and global player data
            JToken localData = playerData["localData"];
            JToken globalData = playerData["globalData"];

            // load player local data
            _playerPresence.PlayerManager.OnCustomLoad(localData);

            // load player global data
            LoadPlayerGlobalData(globalData);
        }

        private void LoadPlayerGlobalData(JToken globalData)
        {
            // load inventory
            JToken inventoryData = globalData["inventory"];
            (_inventory as ISaveableCustom).OnCustomLoad(inventoryData);

            // load objectives
            JToken objectivesData = globalData["objectives"];
            (_objectiveManager as ISaveableCustom).OnCustomLoad(objectivesData);

        }
        #endregion

        private void CreateThumbnail(string path)
        {
            ScreenshotFeature screenshot = ScreenshotFeature.Instance;
            StartCoroutine(screenshot.Pass.CaptureScreen(path));
        }

        private async Task SerializeData(StorableCollection buffer, string path)
        {
            string json = JsonConvert.SerializeObject(buffer, Formatting.Indented, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await SerializableEncryptor.Encrypt(SerializationAsset, path, json);
        }

        private static async Task<string> DeserializeData(string path)
        {
            return await SerializableEncryptor.Decrypt(SerializationAsset, path);
        }

        private GameObject AddRuntimeSaveable(GameObject instantiate, string guid)
        {
            SaveablePair[] saveablePairs = FindSaveablesInChildren(instantiate);
            return AddRuntimeSaveable(instantiate, guid, saveablePairs);
        }

        private GameObject AddRuntimeSaveable(GameObject instantiate, string guid, SaveablePair[] saveablePairs)
        {
            int count = _runtimeSaveables.Count(x => x.TokenGUID.Contains(guid));
            string newGuid = guid + $".id[{count}]";

            _runtimeSaveables.Add(new RuntimeSaveable()
            {
                TokenGUID = newGuid,
                InstantiatedObject = instantiate,
                SaveablePairs = saveablePairs
            });

            return instantiate;
        }

        private SaveablePair[] FindSaveablesInChildren(GameObject root)
        {
            return (from mono in root.GetComponentsInChildren<MonoBehaviour>(true)
                    let type = mono.GetType()
                    where typeof(IRuntimeSaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                    let saveable = mono as IRuntimeSaveable
                    let token = $"{type.Name}{TOKEN_SEPARATOR}{saveable.UniqueID}"
                    select new SaveablePair(token, mono)).ToArray();
        }
    }
}