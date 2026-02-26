using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount); // Applies damage to the entity
    bool IsDead(); // Checks if the entity is dead
    float GetHealth(); // Gets the current health
    float GetMaxHealth(); // Gets the maximum health
}
