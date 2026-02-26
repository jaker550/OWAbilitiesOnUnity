using System.Collections.Generic;
using UnityEngine;

public class AbilityStateManager : MonoBehaviour
{
    public static AbilityStateManager Instance;

    public List<Ability> abilities;
    public AbilityUIManager abilityUIManager;

    private Dictionary<Ability, bool> abilityStates = new Dictionary<Ability, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterAbility(Ability ability)
    {
        if (!abilityStates.ContainsKey(ability))
        {
            abilityStates[ability] = false;
        }
    }
}
