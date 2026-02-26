using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    public Ability ability1;
    public Ability ability2;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (ability1 != null)
            {
                ActivateAbility(ability1);
            }
            else
            {
                Debug.LogWarning("ability1 is not assigned.");
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (ability2 != null)
            {
                ActivateAbility(ability2);
            }
            else
            {
                Debug.LogWarning("ability2 is not assigned.");
            }
        }
    }

    void ActivateAbility(Ability ability)
    {
        if (ability != null)
        {
            ability.Activate(gameObject);
        }
        else
        {
            Debug.LogWarning("Ability is null.");
        }
    }
}