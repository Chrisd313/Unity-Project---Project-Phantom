using UnityEngine;
using System.Collections;

public class DashState : PlayerState
{
    private float _elapsed;
    private bool _dashComplete;
    private bool _recoveryComplete;

    public override void Enter(PlayerController player)
    {
        Debug.Log("Entered dash state");
        _elapsed = 0f;
        _dashComplete = false;
        _recoveryComplete = false;

        player.PerformDash();

        Vector2 move = player.direction;
        PlayDashingAnimation(player, move);
    }

    public override void Update(PlayerController player)
    {
        _elapsed += Time.deltaTime;

        if (!_dashComplete && _elapsed >= player.dashDuration)
        {
            _dashComplete = true;
            _elapsed = 0f;
            player.Stop();
        }

        if (_dashComplete && !_recoveryComplete && _elapsed >= player.postDashPause)
        {
            _recoveryComplete = true;

            Vector2 move = player.GetMoveInput();
            if (move.sqrMagnitude > 0.01f)
                player.TransitionToState(new RunningState());
            else
                player.TransitionToState(new IdleState());
        }
    }

    public void PlayDashingAnimation(PlayerController player, Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            if (movement.x > 0)
            {
                player.animator.Play("Dash_Side");
                player.spriteRenderer.flipX = false;
            }
            else
            {
                player.animator.Play("Dash_Side");
                player.spriteRenderer.flipX = true;
            }
        }
        else
        {
            player.spriteRenderer.flipX = false;

            if (movement.y > 0)
            {
                player.animator.Play("Dash_Up");
            }
            else
            {
                player.animator.Play("Dash_Down");
            }
        }
    }
}
