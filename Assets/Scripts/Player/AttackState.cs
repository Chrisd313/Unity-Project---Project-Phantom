using System.Collections;
using UnityEngine;
using System;

public class AttackState : PlayerState
{
    public override void Enter(PlayerController player)
    {
        PlayAttackAnimation(player);
        base.Enter(player);
        player.isAttacking = true;
        player.Stop();
    }

    public override void Update(PlayerController player)
    {
        if (justEntered)
        {
            justEntered = false;
            return;
        }

        if (Input.GetButtonDown("Attack"))
            PlayAttackAnimation(player);

        if (Input.GetButtonDown("Dash") && player.canDash)
        {
            player.TransitionToState(new DashState());
            player.EndCombo();
            return;
        }

        if (!player.isAttacking)
        {
            Vector2 move = player.GetMoveInput();
            if (move.sqrMagnitude > 0.01f)
                player.TransitionToState(new RunningState());
            else
                player.TransitionToState(new IdleState());
        }
    }

    public void PlayAttackAnimation(PlayerController player)
    {
        UnityEngine.Debug.Log("Play: Attack" + player.comboCount);
        var movement = player.direction;

        player.animator.SetTrigger("Attack" + player.comboCount);

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            player.animator.SetTrigger("Side");
            if (movement.x > 0)
                player.spriteRenderer.flipX = false;
            else
                player.spriteRenderer.flipX = true;
        }
        else
        {
            player.spriteRenderer.flipX = false;

            if (movement.y > 0)
                player.animator.SetTrigger("Up");
            else
                player.animator.SetTrigger("Down");
        }
    }
}
