using UnityEngine;

public abstract class PlayerState
{
    protected bool justEntered;

    public virtual void Enter(PlayerController player)
    {
        justEntered = true;
        UnityEngine.Debug.Log("JustEntered should be true " + justEntered);
    }

    public virtual void Exit(PlayerController player) { }

    public virtual void HandleInput(PlayerController player) { }

    public virtual void Update(PlayerController player)
    {
        if (justEntered)
        {
            UnityEngine.Debug.Log("JustEntered false");
            justEntered = false;
        }
    }

    public virtual void FixedUpdate(PlayerController player) { }

    protected void PlayDirectionalAnimation(PlayerController player, string baseName)
    {
        Vector2 dir = player.lastNonZeroInput;

        if (dir.y > 0)
        {
            player.animator.Play($"{baseName}_Up");
        }
        else if (dir.y < 0)
        {
            player.animator.Play($"{baseName}_Down");
        }
        else
        {
            player.animator.Play($"{baseName}_Side");

            // Flip sprite if facing left/right
            if (dir.x < 0)
                player.spriteRenderer.flipX = true;
            else if (dir.x > 0)
                player.spriteRenderer.flipX = false;
        }
    }
}
