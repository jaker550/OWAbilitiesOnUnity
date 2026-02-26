using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "InfraSightAbility", menuName = "Abilities/InfraSight")]
public class InfraSightAbility : Ability
{
    public float duration = 10f;  // Duration of InfraSight ability
    public Material silhouetteMaterial;  // Material to use for InfraSight

    private Coroutine infraSightCoroutine = null;  // Coroutine reference

    [Header("UI Settings")]
    public GameObject sliderPrefab;  // Prefab for the slider
    public string canvasTag = "UICanvas";  // Tag for the canvas (make sure your Canvas has this tag)

    private GameObject sliderInstance;

    public override void Activate(GameObject player)
    {
        // Check if the ability is already active
        if (infraSightCoroutine != null)
        {
            Debug.Log("InfraSight is already active!");
            return;  // Prevent activation while the ability is still active
        }

        // Start the InfraSight ability
        infraSightCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(ActivateInfraSight(player));
    }

    private IEnumerator ActivateInfraSight(GameObject player)
    {
        // Find all enemies in the scene by tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Store the original materials to revert later
        Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

        // Apply the silhouette material to each enemy and store their original material
        foreach (GameObject enemy in enemies)
        {
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Store the original material
                originalMaterials[enemy] = renderer.material;

                // Apply the silhouette material
                renderer.material = silhouetteMaterial;
            }
        }

        // Instantiate the slider prefab under the canvas
        GameObject canvas = GameObject.FindGameObjectWithTag(canvasTag);
        if (canvas != null && sliderPrefab != null)
        {
            sliderInstance = GameObject.Instantiate(sliderPrefab, canvas.transform);

            // Initialize the slider
            Slider abilitySlider = sliderInstance.GetComponentInChildren<Slider>();
            if (abilitySlider != null)
            {
                abilitySlider.maxValue = duration;  // Set max value to ability duration
                abilitySlider.value = duration;    // Start with the full value
                abilitySlider.gameObject.SetActive(true);  // Make sure the slider is visible
            }
        }
        else
        {
            Debug.LogError("Canvas or sliderPrefab not found!");
        }

        float elapsedTime = 0f;

        // Update the slider as the ability progresses
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Update slider value
            Slider abilitySlider = sliderInstance.GetComponentInChildren<Slider>();
            if (abilitySlider != null)
            {
                abilitySlider.value = duration - elapsedTime;  // Update slider to reflect remaining time
            }

            yield return null;  // Wait until the next frame
        }

        // Reset the enemies' materials back to their original state
        foreach (var enemyMaterialPair in originalMaterials)
        {
            Renderer renderer = enemyMaterialPair.Key.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = enemyMaterialPair.Value;
            }
        }

        // Destroy the entire slider prefab when the ability ends
        if (sliderInstance != null)
        {
            GameObject.Destroy(sliderInstance);  // Destroy the entire prefab (along with the slider and any other components)
        }

        // Mark the coroutine as finished
        infraSightCoroutine = null;

        Debug.Log("InfraSight ability ended, ready to be activated again.");
    }
}
