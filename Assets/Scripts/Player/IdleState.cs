using UnityEngine;
using System;

public class IdleState : PlayerState
{
    public override void Enter(PlayerController player)
    {
        player.UpdateFacingDirection(player.lastNonZeroInput);
        PlayDirectionalAnimation(player, "Idle");
        player.Stop();
    }

    public override void Update(PlayerController player)
    {
        if (player.useGamepad) { }
    }

    public override void HandleInput(PlayerController player)
    {
        Vector2 move = player.GetMoveInput();

        if (move.sqrMagnitude > 0.01f)
        {
            player.TransitionToState(new RunningState());
            return;
        }

        if (Input.GetButtonDown("Attack"))
        {
            UnityEngine.Debug.Log("Attack button pressed from Idle " + DateTime.Now);
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
}
