using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Burst.CompilerServices;
using UnityEditor.Rendering.BuiltIn.ShaderGraph;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Interact : MonoBehaviour
{
    #region Variables

    [Header("Player")]
    public GameObject player;

    [Header("Interact")]
    public float interactDistance = 1f;
    public float interractCooldown = 0.5f; // Adjust the cooldown duration as needed

    [Header("Target position")]
    public Vector3 targetPosition; // The destination for the player

    [Header("Misc")]
    public Rigidbody2D rb;
    public InventoryManager inventoryManager;
    public Sprite litCandle;
    
    
    private float lastInteractionTime = 0f;

    #endregion

    private void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            if (Time.time - lastInteractionTime >= interractCooldown)
            {
                // Update the last interaction time
                lastInteractionTime = Time.time;

                interact();
            }

            else
            {
                // The player cannot go through the door yet due to the cooldown
                Debug.Log("Interact on cooldown.");
            }
        }
    }

    private void interact()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, interactDistance);
        //Debug.Log("Number of colliders detected: " + hitColliders.Length);

        foreach (Collider2D collider in hitColliders)
        {
            // Pick up items
            if (collider.CompareTag("item"))
            {
                if(collider.name == "Lit candle")
                {
                    if (inventoryManager.HasItem("Lit candle"))
                    {

                    }
                    else if(inventoryManager.HasItem("Unlit candle"))
                    {
                        inventoryManager.RemoveItem("Unlit candle", 1);
                        inventoryManager.AddItem("Lit candle", 1, litCandle);
                    }
                    else
                    {
                        PickUp pickUp = collider.gameObject.GetComponent<PickUp>();
                        pickUp.Item();
                    }
                }
                else if(collider.name == "Wall candle")
                {
                    if (inventoryManager.HasItem("Unlit candle"))
                    {
                        inventoryManager.RemoveItem("Unlit candle", 1);
                        inventoryManager.AddItem("Lit candle", 1, litCandle);
                    }
                }
                else
                {
                    PickUp pickUp = collider.gameObject.GetComponent<PickUp>();
                    pickUp.Item();
                }
            }

            // Hide the player
            if (collider.CompareTag("hide") || collider.CompareTag("hide dark"))
            {
                Hide hide = player.GetComponent<Hide>();
                hide.hidePlayer(collider);
            }

            // Move the player to another door
            if (collider.CompareTag("door"))
            {
                LockedItem lockedItem = collider.gameObject.GetComponent<LockedItem>();
                
                if (!lockedItem.Unlock())
                {
                    InteractableObject interactableObject = collider.gameObject.GetComponent<InteractableObject>();
                    interactableObject.Door();
                }
            }

            // Open a chest and take the items inside
            if (collider.CompareTag("chest"))
            {
                LockedItem lockedItem = collider.gameObject.GetComponent<LockedItem>();

                if (!lockedItem.Unlock())
                {
                    SearchItem searchItem = collider.gameObject.GetComponent<SearchItem>();
                    searchItem.Search();
                }
            }

            // Save the game
            if (collider.CompareTag("diary"))
            {
                SaveGame saveGame = collider.gameObject.GetComponent<SaveGame>();
                saveGame.Save();
            }
        }
    }
}
