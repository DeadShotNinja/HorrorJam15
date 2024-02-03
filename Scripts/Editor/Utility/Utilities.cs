using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using HJ.Runtime;
using Object = UnityEngine.Object;

namespace HJ.Editors
{
    public class Utilities 
    {
        public const string ROOT_PATH = "Assets";
        public const string TOOLS_PATH = "Tools/" + ROOT_PATH;
        public const string GAMEMANAGER_PATH = "Setup/GameManager";
        public const string PLAYER_PATH = "Setup/Player";

        [MenuItem("Tools/HJ/Setup Scene")]  //[MenuItem(TOOLS_PATH + "/Setup Scene")]
        private static void SetupScene()
        {
            // load GameManager and Player prefab from Resources
            GameObject gameManagerPrefab = Resources.Load<GameObject>(GAMEMANAGER_PATH);
            GameObject playerPrefab = Resources.Load<GameObject>(PLAYER_PATH);

            // add GameManager and Player to scene
            GameObject gameManager = PrefabUtility.InstantiatePrefab(gameManagerPrefab) as GameObject;
            GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;

            // get required components
            Camera mainCameraBrain = gameManager.GetComponentInChildren<Camera>();
            PlayerPresenceManager playerPresence = gameManager.GetComponent<PlayerPresenceManager>();
            PlayerManager playerManager = player.GetComponent<PlayerManager>();

            // assign missing references
            player.transform.position = new Vector3(0f, 0f, 0f);
            playerManager.MainCamera = mainCameraBrain;
            playerPresence.Player = player;
        }

        [MenuItem("Tools/HJ/Utilities/Select GameObjects With Missing Scripts")]
        private static void SelectGameObjects()
        {
            //Get the current scene and all top-level GameObjects in the scene hierarchy
            Scene currentScene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = currentScene.GetRootGameObjects();

            List<Object> objectsWithDeadLinks = new List<Object>();
            foreach (GameObject g in rootObjects)
            {
                //Get all components on the GameObject, then loop through them 
                Component[] components = g.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    Component currentComponent = components[i];

                    //If the component is null, that means it's a missing script!
                    if (currentComponent == null)
                    {
                        //Add the sinner to our naughty-list
                        objectsWithDeadLinks.Add(g);
                        Selection.activeGameObject = g;
                        Debug.Log(g + " has a missing script!");
                        break;
                    }
                }
            }
            if (objectsWithDeadLinks.Count > 0)
            {
                //Set the selection in the editor
                Selection.objects = objectsWithDeadLinks.ToArray();
            }
            else
            {
                Debug.Log("No GameObjects in '" + currentScene.name + "' have missing scripts.");
            }
        }

        [MenuItem("Tools/HJ/Utilities/Find Missing Scripts In Prefabs")]
        private static void FindMissingScriptsInProjectMenuItem()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                foreach (Component component in prefab.GetComponentsInChildren<Component>())
                {
                    if (component == null)
                    {
                        Debug.Log("Prefab found with missing script " + assetPath, prefab);
                        break;
                    }
                }
            }
        }
        
        [MenuItem("Tools/HJ/Utilities/Find Missing Scripts In ScriptableObjects")]
        private static void FindMissingScriptsInScriptableObjectsMenuItem()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject soAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (soAsset == null)
                {
                    Debug.Log("ScriptableObject found with missing script " + assetPath);
                }
            }
        }
    }
}