using UnityEngine;

[CreateAssetMenu(fileName = "RewindAbility", menuName = "Abilities/Rewind")]
public class RewindAbility : Ability
{
    public float rewindDuration = 3f;

    public override void Activate(GameObject player)
    {
        PlayerRewindController rewindController = player.GetComponent<PlayerRewindController>();
        if (rewindController != null)
        {
            rewindController.StartRewind(rewindDuration);
        }
    }
}
