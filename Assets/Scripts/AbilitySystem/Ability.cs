using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public string abilityName = "New Ability";
    public Sprite abilityIcon;
    public float abilityDuration;

    public abstract void Activate(GameObject player);
}