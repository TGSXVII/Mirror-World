using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGame : MonoBehaviour
{
    #region Variables

    // Reference to the player game object
    public GameObject player;
    public InventoryManager inventoryManager;
    public GameObject eventSystem;
    public AudioListener audioListener;
    public InventoryManager mirrorInventory;

    #endregion

    [System.Serializable]
    
    public class PlayerData
    {
        public string level;
        public Vector3 position;
        public List<InventoryManager.Item> inventory;
        public List<InventoryManager.Item> mirror;
        public List<GameWorldState> items;
    }

    [System.Serializable]
    public class GameWorldState
    {
        public string name;
        public int objectID;
        public bool isActive;
        public bool isLocked;
        public bool collider2D;
        // Add any additional fields or data structures as needed
    }

    // Save the player's progress
    public void Save()
    {
        // Create a data model to hold the relevant game data
        PlayerData data = new PlayerData();
        data.level = SceneManager.GetActiveScene().name;
        data.position = player.transform.position;
        data.inventory = inventoryManager.GetInventoryData();
        data.mirror = mirrorInventory.GetInventoryData();
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        Debug.Log(objects.Length);
        List<GameWorldState> worldstate = new List<GameWorldState>(); 
        // Iterate over each object and add its state to the Items list
        foreach (GameObject obj in objects)
        {
            //needs to check on an ID on a scrypt
            if (obj.GetComponent<ID>())
            {
                ID itemIDs = obj.GetComponent<ID>();
                GameWorldState objectState = new GameWorldState();
                objectState.name = obj.name;
                objectState.objectID = itemIDs.itemID;
                // Check if the game object is active
                objectState.isActive = obj.activeSelf;
                if (obj.GetComponent<LockedItem>())
                {
                    LockedItem lockedItem = obj.GetComponent<LockedItem>();
                    objectState.isLocked = lockedItem.isLocked;
                    objectState.collider2D = obj.GetComponent<BoxCollider2D>().enabled;
                }
                if (objectState != null)
                {
                    // Set other relevant state properties as needed
                    worldstate.Add(objectState);
                }
            }
        }
        data.items = worldstate;

        // Serialize the data to JSON
        string json = JsonUtility.ToJson(data);

        // Save the JSON to a local file
        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        File.WriteAllText(savePath, json);

    }

    // Load the player's progress
    public void LoadGame()
    {
        // Load the JSON data from the local file
        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        if (File.Exists(savePath))
        {
            
            string json = File.ReadAllText(savePath);

            // Deserialize the JSON to retrieve the saved data
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            
            EventManager.StartListening(EventName.Load, LoadProgress);

            eventSystem.SetActive(false);
            audioListener.enabled = false;
            
            // Load the specified level
            SceneManager.LoadScene(data.level, LoadSceneMode.Additive);
            
        }

        else
        {
            Debug.Log("No save file found.");
        }
    }
    
    public void LoadProgress()
    {
        //Debug.Log("start of load");
        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log(savePath);
        string json = File.ReadAllText(savePath);

        // Deserialize the JSON to retrieve the saved data
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);   
        
        EventManager.StopListening(EventName.Load, LoadProgress);

        // Find the player object in the new scene
        player = GameObject.FindGameObjectWithTag("Player");

        // Find the player object in the new scene
        inventoryManager = GameObject.FindGameObjectWithTag("inventory").GetComponent<InventoryManager>();
        mirrorInventory = GameObject.FindGameObjectWithTag("mirrorInventory").GetComponent<InventoryManager>();
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        Debug.Log(data.inventory);
        // Apply the saved data to the player
        player.transform.position = data.position;
        inventoryManager.LoadInventoryData(data.inventory);
        mirrorInventory.LoadInventoryData(data.mirror);
        //needs to check on an ID on a scrypt
        foreach (GameWorldState state in data.items)
        {
            foreach (GameObject obj in allObjects)
            {
                if (obj.GetComponent<ID>())
                {
                    ID itemIDs = obj.GetComponent<ID>();
                    if (itemIDs.itemID == state.objectID)
                    {
                        Debug.Log(state.name + " " + state.isActive);
                        obj.SetActive(state.isActive);
                        if (obj.GetComponent<LockedItem>())
                        {
                            obj.GetComponent<LockedItem>().isLocked = state.isLocked;
                            obj.GetComponent<BoxCollider2D>().enabled = state.collider2D;
                        }
                        // Apply other relevant state properties to the object
                    }
                }
            }
        }
        SceneManager.UnloadSceneAsync("Main Menu");
        Debug.Log("Game loaded.");
    }
}
