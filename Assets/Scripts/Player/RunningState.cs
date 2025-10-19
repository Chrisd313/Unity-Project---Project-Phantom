using UnityEngine;

public class RunningState : PlayerState
{
    public override void Enter(PlayerController player)
    {
        PlayDirectionalAnimation(player, "Run");
    }

    public override void HandleInput(PlayerController player)
    {
        Vector2 move = player.GetMoveInput();
        if (move.sqrMagnitude < 0.01f)
        {
            player.TransitionToState(new IdleState());
            return;
        }

        if (Input.GetButtonDown("Attack"))
        {
            player.TransitionToState(new AttackState());
            return;
        }

        if (Input.GetButtonDown("Cast"))
        {
            player.TransitionToState(new CastingState());
            return;
        }

        if (Input.GetButtonDown("Dash") && player.canDash)
        {
            player.TransitionToState(new DashState());
            return;
        }
    }

    public override void FixedUpdate(PlayerController player)
    {
        Vector2 move = player.GetMoveInput();
        PlayRunningAnimation(player, move);
        player.Move(move);
    }

    public void PlayRunningAnimation(PlayerController player, Vector2 movement)
    {
        Debug.Log("movement " + movement);

        // Determine dominant direction
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            // Horizontal movement is stronger
            if (movement.x > 0)
            {
                player.animator.Play("Run_Side");
                player.spriteRenderer.flipX = false;
            }
            else
            {
                player.animator.Play("Run_Side");
                player.spriteRenderer.flipX = true;
            }
        }
        else
        {
            player.spriteRenderer.flipX = false;

            // Vertical movement is stronger
            if (movement.y > 0)
            {
                player.animator.Play("Run_Up");
            }
            else
            {
                player.animator.Play("Run_Down");
            }
        }
    }
}
