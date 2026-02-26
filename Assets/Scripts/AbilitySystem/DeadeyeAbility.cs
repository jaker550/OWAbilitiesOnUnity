using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DeadeyeAbility", menuName = "Abilities/Deadeye")]
public class DeadeyeAbility : Ability
{
    [Header("Player Settings")]
    [Tooltip("Multiplier to slow down the player's speed while Deadeye is active.")]
    public float speedMultiplier = 0.5f;
    
    [Tooltip("Duration in seconds for smooth FOV transition.")]
    public float fovTransitionDuration = 1f;

    [Header("Deadeye Settings")]
    [Tooltip("Total time allowed for locking on to enemies during Deadeye mode.")]
    public float lockOnTime = 5f;

    [Tooltip("Damage per second dealt during the first phase of Deadeye.")]
    public float firstPhaseDamagePerSecond = 150f;

    [Tooltip("Damage per second dealt during the second phase of Deadeye.")]
    public float secondPhaseDamagePerSecond = 300f;

    [Header("Detection Settings")]
    [Tooltip("Layer to identify enemy targets during Deadeye.")]
    public LayerMask enemyLayer;

    [Tooltip("Layer used to identify obstacles that block visibility to enemies.")]
    public LayerMask obstacleLayer;

    [Tooltip("Field of View (FOV) angle in degrees for detecting enemies.")]
    public float fovAngle = 60f;

    [Tooltip("Maximum radius around the player in which enemies can be detected for lock-on.")]
    public float detectionRadius = 50f;

    [Header("Control Settings")]
    [Tooltip("Key to fire Deadeye and execute damage on locked enemies.")]
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("Special Effects")]
    [Tooltip("Prefab for the tumbleweed effect during Deadeye activation.")]
    public GameObject tumbleweedPrefab;

    [Tooltip("Wind force applied to the tumbleweed during Deadeye.")]
    public float windForce = 10f;

    [Header("UI Settings")]
    [Tooltip("Prefab for the lock-on circle that appears over enemies.")]
    public GameObject lockOnCirclePrefab;

    [Tooltip("Prefab for the timer displayed on the screen during Deadeye.")]
    public GameObject deadeyeTimerPrefab;

    private Dictionary<GameObject, float> lockOnProgress = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, GameObject> lockOnCircles = new Dictionary<GameObject, GameObject>();
    private List<GameObject> lockedEnemies = new List<GameObject>();
    private Coroutine deadeyeCoroutine;
    private GameObject deadeyeTimerObject; // Reference to the instantiated Deadeye timer image
    private float currentLockOnTime;
    private float initialFOV;
    private Camera playerCamera;

    public List<Ray> visibilityRays = new List<Ray>();
    public List<bool> rayHits = new List<bool>();

    public override void Activate(GameObject player)
    {
        playerCamera = player.GetComponentInChildren<Camera>();
        initialFOV = playerCamera.fieldOfView;

        fovCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(SmoothFOV(fovAngle));
        
        if (deadeyeCoroutine == null)
        {
            Debug.Log("Deadeye activated. Starting lock-on process.");

            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(speedMultiplier);
            }

            SpawnTumbleweed(player);

            // Create and setup the Deadeye timer image
            SetupDeadeyeTimerImage();

            deadeyeCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(StartDeadeye(player));
        }
        else
        {
            Debug.Log("Deadeye is already active.");
        }
    }

    private void SetupDeadeyeTimerImage()
    {
        // Find the Canvas with the tag "Canvas"
        GameObject canvasObject = GameObject.FindGameObjectWithTag("Canvas");
        if (canvasObject != null)
        {
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Instantiate the Deadeye timer image prefab
                deadeyeTimerObject = Instantiate(deadeyeTimerPrefab, canvas.transform);
                Image timerImage = deadeyeTimerObject.GetComponent<Image>();
                if (timerImage != null)
                {
                    timerImage.fillAmount = 1f; // Set to full at the start
                }
            }
            else
            {
                Debug.LogError("The GameObject with tag 'Canvas' does not have a Canvas component.");
            }
        }
        else
        {
            Debug.LogError("No Canvas found with the tag 'Canvas'.");
        }
    }

    private IEnumerator StartDeadeye(GameObject player)
    {
        float startTime = Time.time;
        currentLockOnTime = lockOnTime;

        while (Time.time < startTime + lockOnTime)
        {
            LockOnTargets(player);

            if (Input.GetKeyDown(fireKey))
            {
                Debug.Log("Fire key pressed! Firing Deadeye early.");
                FireDeadeye(player);
                yield break;
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("Right click pressed! Canceling Deadeye.");
                EndDeadeye(player);
                yield break;
            }

            // Update the Deadeye timer UI
            if (deadeyeTimerObject != null)
            {
                currentLockOnTime = Mathf.Max(0, startTime + lockOnTime - Time.time);
                Image timerImage = deadeyeTimerObject.GetComponent<Image>();
                if (timerImage != null)
                {
                    timerImage.fillAmount = currentLockOnTime / lockOnTime;
                }
            }

            yield return null;
        }

        Debug.Log("Lock-on time completed. Canceling Deadeye.");
        EndDeadeye(player);
    }
    
    private Coroutine fovCoroutine;

    private IEnumerator SmoothFOV(float targetFOV)
    {
        float timeElapsed = 0f;
        float startFOV = playerCamera.fieldOfView;

        while (timeElapsed < fovTransitionDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fovTransitionDuration;
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        playerCamera.fieldOfView = targetFOV;
    }

    private void SpawnTumbleweed(GameObject player)
    {
        Vector3 playerPosition = player.transform.position;

        if (tumbleweedPrefab == null)
        {
            Debug.LogWarning("Tumbleweed prefab is not assigned.");
            return;
        }

        // Calculate spawn position slightly to the left and in front of the player
        Vector3 leftDirection = -player.transform.right; // Left relative to the player
        Vector3 forwardDirection = player.transform.forward;
        Vector3 spawnPosition = player.transform.position + leftDirection * 4f + forwardDirection * 1.8f;

        // Perform a raycast downward to detect the ground level
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 5f, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            spawnPosition.y = hit.point.y; // Set the spawn position to the ground level
        }
        else
        {
            Debug.LogWarning("No ground detected for tumbleweed spawn.");
            spawnPosition.y = playerPosition.y; // Default to player height if ground isn't detected
        }

        // Instantiate the tumbleweed at the calculated position
        GameObject tumbleweed = Instantiate(tumbleweedPrefab, spawnPosition, Quaternion.identity);

        // Get the TumbleweedMovement component and set wind direction from left to right
        TumbleweedMovement tumbleweedMovement = tumbleweed.GetComponent<TumbleweedMovement>();
        if (tumbleweedMovement != null)
        {
            // Set the wind direction from the left to right relative to the player
            Vector3 windDirection = player.transform.right; // Right relative to the player
            tumbleweedMovement.SetWindDirection(windDirection); // Apply wind direction
            tumbleweedMovement.SetWindForce(windForce); // Example wind force
        }

        Debug.Log("Tumbleweed spawned with wind force from left to right.");
    }

    private void LockOnTargets(GameObject player)
    {
        // Detect enemies in range
        Collider[] enemiesInRange = Physics.OverlapSphere(player.transform.position, detectionRadius, enemyLayer);

        foreach (Collider enemyCollider in enemiesInRange)
        {
            GameObject enemy = enemyCollider.gameObject;

            // Check if the enemy is in the player's FOV and unobstructed by obstacles
            if (IsInFOV(player, enemy) && IsVisible(player, enemy))
            {
                // Start locking on to enemies if not already locked
                if (!lockOnProgress.ContainsKey(enemy))
                {
                    lockOnProgress[enemy] = 0f; // Initialize lock-on time for this enemy
                    lockedEnemies.Add(enemy);
                    Debug.Log($"Locking on to new enemy: {enemy.name}");

                    // Instantiate the lock-on circle UI above the enemy
                    GameObject lockOnCircle = Instantiate(lockOnCirclePrefab, enemy.transform.position, Quaternion.identity);
                    lockOnCircle.transform.SetParent(enemy.transform); // Attach to enemy so it moves with them
                    lockOnCircle.transform.localPosition = new Vector3(-0.2f, 0f, 0); // Adjust position above enemy
                    lockOnCircles[enemy] = lockOnCircle;
                }

                // Increase the lock-on time for this enemy
                lockOnProgress[enemy] += Time.deltaTime;

                // Update the lock-on circle UI
                UpdateLockOnCircle(player, enemy, lockOnProgress[enemy]);

                // Cap lock-on time at the maximum (lockOnTime)
                if (lockOnProgress[enemy] > lockOnTime)
                {
                    lockOnProgress[enemy] = lockOnTime;
                }

                Debug.Log($"Lock-on progress for {enemy.name}: {lockOnProgress[enemy]}/{lockOnTime}");
            }
            else
            {
                Debug.Log($"Enemy {enemy.name} is not in FOV or obstructed.");
            }
        }
    }

    private bool IsInFOV(GameObject player, GameObject enemy)
    {
        Vector3 directionToEnemy = (enemy.transform.position - player.transform.position).normalized;
        float angleToEnemy = Vector3.Angle(player.transform.forward, directionToEnemy);

        if (angleToEnemy < fovAngle / 2)
        {
            Debug.Log($"Enemy {enemy.name} is within FOV.");
            return true;
        }

        return false;
    }

    private bool IsVisible(GameObject player, GameObject enemy)
    {
        Vector3 directionToEnemy = (enemy.transform.position - player.transform.position).normalized;
        Ray ray = new Ray(player.transform.position, directionToEnemy);
        RaycastHit hit;

        // Perform raycast to check if there's an obstacle between the player and enemy
        if (Physics.Raycast(ray, out hit, detectionRadius))
        {
            visibilityRays.Add(ray); // Store the ray for Gizmos drawing
            rayHits.Add(hit.collider.gameObject == enemy); // Store the hit result for Gizmos drawing

            if (hit.collider.gameObject == enemy)
            {
                Debug.Log($"Enemy {enemy.name} is visible.");
                return true;
            }
            else
            {
                Debug.Log($"Enemy {enemy.name} is obstructed by {hit.collider.name}.");
            }
        }

        return false;
    }

    private void UpdateLockOnCircle(GameObject player, GameObject enemy, float lockOnProgress)
    {
        if (lockOnCircles.ContainsKey(enemy))
        {
            GameObject lockOnCircle = lockOnCircles[enemy];
            Image circleFillImage = lockOnCircle.GetComponentInChildren<Image>();

            IDamageable damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float enemyHealth = damageable.GetHealth();
                float lockOnTimeElapsed = lockOnProgress;

                // Calculate potential damage based on the lock-on time
                float potentialDamage = CalculateDamage(lockOnTimeElapsed);

                // Calculate the fill amount based on the percentage of the enemy's health that would be damaged
                float fillAmount = Mathf.Clamp(potentialDamage / enemyHealth, 0, 1);
                circleFillImage.fillAmount = fillAmount;

                // Find the skull icon inside the lock-on circle
                Image skullIcon = lockOnCircle.transform.Find("SkullIcon").GetComponent<Image>();

                // Update the skull icon opacity based on the fill amount
                if (skullIcon != null)
                {
                    Color skullColor = skullIcon.color;

                    // If fillAmount is 100%, set opacity to 1; otherwise, keep it at 0
                    skullColor.a = fillAmount >= 1f ? 1f : 0f;
                    skullIcon.color = skullColor;
                }
            }

            // Make the circle a billboard (face the player)
            lockOnCircle.transform.LookAt(player.transform);
        }
    }

    private void FireDeadeye(GameObject player)
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("No main camera found. Cannot determine if enemies are visible on screen.");
            return;
        }

        Debug.Log($"Firing Deadeye. Locked onto {lockedEnemies.Count} enemies.");

        foreach (GameObject enemy in lockedEnemies)
        {
            // Check if the enemy is visible
            if (IsVisible(player, enemy))
            {
                // Check if the enemy is in the camera's viewport
                Vector3 screenPoint = playerCamera.WorldToViewportPoint(enemy.transform.position);
                if (screenPoint.z > 0 && screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1)
                {
                    // Get the total lock-on time for this enemy
                    float lockOnTimeElapsed = lockOnProgress[enemy];

                    // Calculate the damage based on how long this enemy has been locked onto
                    float damage = CalculateDamage(lockOnTimeElapsed);

                    // Apply damage to the enemy
                    Debug.Log($"Firing at {enemy.name}. Lock-on time: {lockOnTimeElapsed}s, Damage: {damage}");
                    DealDamage(enemy, damage);
                }
                else
                {
                    Debug.Log($"Enemy {enemy.name} is not on screen and will not be damaged.");
                }
            }
            else
            {
                Debug.Log($"Enemy {enemy.name} is not visible and will not be damaged.");
            }

            // Destroy the lock-on circle after firing
            if (lockOnCircles.ContainsKey(enemy))
            {
                Destroy(lockOnCircles[enemy]);
                lockOnCircles.Remove(enemy);
            }
        }

        // Clear lock-on data
        EndDeadeye(player);
    }

    private float CalculateDamage(float lockOnTimeElapsed)
    {
        float damage = 0f;

        // First 2 seconds: 150 damage per second
        if (lockOnTimeElapsed <= 2f)
        {
            damage = lockOnTimeElapsed * firstPhaseDamagePerSecond;
        }
        else
        {
            // After 2 seconds: 150 damage per second for the first 2 seconds + 300 damage per second thereafter
            damage = (2f * firstPhaseDamagePerSecond) + ((lockOnTimeElapsed - 2f) * secondPhaseDamagePerSecond);
        }

        Debug.Log($"Damage calculated based on lock-on time: {lockOnTimeElapsed}s. Total damage: {damage}");
        return damage;
    }

    private void DealDamage(GameObject enemy, float damage)
    {
        IDamageable damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage); // Apply calculated damage
            Debug.Log($"Applied {damage} damage to {enemy.name}");
        }
        else
        {
            Debug.LogWarning($"{enemy.name} does not implement IDamageable.");
        }
    }

    private void EndDeadeye(GameObject player)
    {
        Debug.Log("Ending Deadeye. Clearing lock-on data and resetting.");
        
        lockOnProgress.Clear();
        lockedEnemies.Clear();

        visibilityRays.Clear();
        rayHits.Clear();
        
        MonoBehaviour mono = player.GetComponent<MonoBehaviour>();
        if (mono == null)
        {
            Debug.LogError("Player lacks MonoBehaviour! Can't start FOV revert.");
            return;
        }

        if (fovCoroutine != null)
        {
            mono.StopCoroutine(fovCoroutine);
            fovCoroutine = null;
        }

        foreach (var circle in lockOnCircles.Values)
        {
            Destroy(circle);
        }
        lockOnCircles.Clear();

        if (deadeyeTimerObject != null)
        {
            Destroy(deadeyeTimerObject); // Remove the Deadeye timer image
        }

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ResetSpeedMultiplier();
        }

        if (deadeyeCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(deadeyeCoroutine);
            deadeyeCoroutine = null;
        }

        if (fovCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(fovCoroutine);
            fovCoroutine = null;
        }

        mono.StartCoroutine(SmoothFOV(initialFOV));

        Debug.Log("Deadeye deactivated.");
    }
}