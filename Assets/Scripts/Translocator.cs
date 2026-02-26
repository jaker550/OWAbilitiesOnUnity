using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translocator : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    public GameObject player;
    public CharacterController characterController;
    public PlayerAbilityController playerAbilityController;
    public float destroyTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void Initialize(GameObject player, float destroyTime)
    {
        this.player = player;
        Debug.Log("Translocator initialized with player: " + player.name);
        characterController = player.GetComponent<CharacterController>();
        StartCoroutine(Teleport(destroyTime));
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Do not stick to the player
            return;
        }

        // Make the blade stick to the object it collided with
        transform.SetParent(collision.transform);

        // Stop the blade from moving and spinning further
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.enabled = false; // Disable collisions
        }

        Debug.Log($"Translocator collided with {collision.gameObject.name}");
        if (player != null)
        {
            characterController.enabled = false;
            Debug.Log("Teleporting player to translocator position");
            player.transform.position = transform.position;
            Debug.Log("Player teleported to: " + transform.position);
            characterController.enabled = true;
        }
        else
        {
            Debug.Log("Player not found!");
        }
        Destroy(gameObject);
    }

    IEnumerator Teleport(float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        characterController.enabled = false;
        Debug.Log("Teleporting player to translocator position");
        player.transform.position = transform.position;
        Debug.Log("Player teleported to: " + transform.position);
        characterController.enabled = true;
        Destroy(gameObject);
    }
}
