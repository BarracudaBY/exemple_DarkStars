using UnityEngine;

public class Enemy_DeadState : EnemyState
{

    private Collider2D col;
    private SpriteRenderer sr;
    private Entity_Health health;

    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        col = enemy.GetComponent<Collider2D>();
        sr = enemy.GetComponentInChildren<SpriteRenderer>();
        health = enemy.GetComponent<Entity_Health>();
    }

    public override void Enter()
    {
        anim.enabled = false;
        col.enabled = false;
        rb.simulated = false;

        stateMachine.SwitchOffStateMachine();

        PlayGoreEffects();
    }

    private void PlayGoreEffects()
    {
        if (GoreManager.Instance == null || enemy.GoreProfile == null) return;

        Vector2 hitDirection = health != null ? health.LastHitDirection : Vector2.up;

        GoreManager.Instance.PlayDeathSequence(
            enemy.transform.position,
            hitDirection,
            enemy.GoreProfile,
            sr
        );

        if (sr != null)
            sr.enabled = false; // прячем оригинальный спрайт — вместо него гибы
    }


}
