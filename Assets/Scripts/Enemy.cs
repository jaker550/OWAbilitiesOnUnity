using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 3.5f; // Default speed
    [SerializeField] private float minSpeed = 0f; // Minimum speed
    [SerializeField] private float speedChangeRate = 1f; // Rate at which speed changes
    public float currentSpeed;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f; // Maximum health
    public float currentHealth { get; private set; } // Made property for more control over health

    [Header("Health Bar UI")]
    public GameObject healthBarPrefab; // Prefab for the health bar
    private GameObject healthBarInstance;
    private Slider healthSlider;

    private Rigidbody rb;
    private bool inSnow = false;

    [SerializeField] private Material frozenMaterial; // Material to use when the enemy is nearly frozen
    [SerializeField] private Material defaultMaterial; // Default material to use when not frozen
    private Renderer enemyRenderer;

    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA; // Patrol point A
    [SerializeField] private Transform pointB; // Patrol point B
    private Transform targetPoint; // Current target point

    [SerializeField] private bool canPatrol = true; // Checkbox to enable/disable patrolling
    private bool isPatrolling = true; // Patrol state control

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponent<Renderer>();

        currentSpeed = baseSpeed;
        currentHealth = maxHealth;

        // Set initial target point
        targetPoint = pointB;

        // Instantiate health bar and set its position above the enemy
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Only start patrolling if canPatrol is true
        if (!canPatrol)
        {
            isPatrolling = false;
        }
    }

    private void Update()
    {
        // Make sure the health bar follows the enemy
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + Vector3.up * 2f;
        }

        // Patrol between points if the enemy is patrolling
        if (isPatrolling && canPatrol)
        {
            Patrol();
        }
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void Patrol()
    {
        if (targetPoint == null)
            return;

        Vector3 direction = (targetPoint.position - transform.position).normalized;
        rb.linearVelocity = direction * currentSpeed;

        // If close to the target point, switch to the other point
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // Avoid applying damage if already dead

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the health bar
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die()
    {
        // Destroy the health bar
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
        
        // Destroy the enemy game object
        Destroy(gameObject);
    }

    public void PullWithVerticalLift(Vector3 targetPosition, float pullForce, float verticalLift, float liftDuration)
    {
        if (rb != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;

            // Apply the pulling force
            rb.AddForce(direction * pullForce, ForceMode.Impulse);

            // Apply the vertical lift force
            rb.AddForce(Vector3.up * verticalLift, ForceMode.Impulse);

            // Stop patrolling
            isPatrolling = false;

            // Start coroutine to re-enable the Rigidbody's kinematic property
            StartCoroutine(ReEnableAgentAfterDelay(liftDuration));
        }
    }

    private IEnumerator ReEnableAgentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isPatrolling = true;
    }

    public void EnterSnow()
    {
        if (!inSnow)
        {
            inSnow = true;
            StopCoroutine(nameof(RegainSpeed)); // Stop any ongoing speed regain coroutine
            StartCoroutine(SnowEffect());
        }
    }

    public void ExitSnow()
    {
        if (inSnow)
        {
            inSnow = false;
            StopCoroutine(nameof(SnowEffect)); // Stop any ongoing snow effect coroutine
            StartCoroutine(RegainSpeed());
        }
    }

    private IEnumerator SnowEffect()
    {
        while (inSnow)
        {
            if (currentSpeed > minSpeed)
            {
                currentSpeed -= speedChangeRate * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, minSpeed); // Ensure speed doesn't go below minSpeed
                // Change to frozen material if speed is less than 0.5
                if (currentSpeed < 0.5f && enemyRenderer.material != frozenMaterial)
                {
                    enemyRenderer.material = frozenMaterial;
                }
            }
            yield return null;
        }
    }

    private IEnumerator RegainSpeed()
    {
        while (!inSnow && currentSpeed < baseSpeed)
        {
            currentSpeed += speedChangeRate * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, baseSpeed); // Ensure speed doesn't exceed baseSpeed
            // Change back to default material if speed is 1 or more
            if (currentSpeed >= 1f && enemyRenderer.material != defaultMaterial)
            {
                enemyRenderer.material = defaultMaterial;
            }
            yield return null;
        }

        // Ensure the final speed is exactly baseSpeed
        currentSpeed = baseSpeed;
    }

}
