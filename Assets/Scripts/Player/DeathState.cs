using UnityEngine;

public class DeathState : PlayerState
{
    public override void Enter(PlayerController player)
    {
        player.animator.Play("Death");
        player.Stop();
        // Disable input, collision, etc.
    }

    public override void HandleInput(PlayerController player)
    {
        // Do nothing — dead player doesn’t respond
    }

    public override void Update(PlayerController player)
    {
        // Optionally, wait for respawn or replay action
    }
}
