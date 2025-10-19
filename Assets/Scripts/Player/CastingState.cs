using System.Collections;
using UnityEngine;

public class CastingState : PlayerState
{
    private float _elapsed;

    public override void Enter(PlayerController player)
    {
        _elapsed = 0f;
        player.animator.Play("Cast");
        player.Stop();  // generally you don’t want movement while casting
    }

    public override void Update(PlayerController player)
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= player.castDuration)
        {
            Vector2 move = player.GetMoveInput();
            if (move.sqrMagnitude > 0.01f)
                player.TransitionToState(new RunningState());
            else
                player.TransitionToState(new IdleState());
        }
    }
}
