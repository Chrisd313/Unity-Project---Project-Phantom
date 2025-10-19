using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Input Mode")]
    [Tooltip("Switch between Keyboard+Mouse and Gamepad controls.")]
    public bool useGamepad = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float minInputThreshold = 0.01f;
    public float attackDuration = 0.3f;
    public float castDuration = 0.5f;
    private Vector2 movementInput;

    [System.Serializable]
    public class AttackPhase
    {
        public string animationTrigger = "Attack1";
        public float damage = 10f;
        public float pushForce = 4f;
        public float pushDuration = 0.1f;
        public float lockDuration = 0.2f;
    }

    [Header("Combo Attack Settings")]
    public AttackPhase[] comboPhases = new AttackPhase[3];

    public int currentComboIndex = 0;

    public bool isAttacking;
    public bool isAttackLocked;
    public bool isAttackPushing;

    [Header("General Attack Settings")]
    public GameObject attackHitboxPrefab;
    public float attackRange = 1.0f;
    public Vector2 attackOffset = new Vector2(0.6f, 0f);

    [Header("Attack Movement")]
    [Range(0f, 1f)]
    public float attackMoveSlowdown = 0f;

    [Header("Dash")]
    public float dashDistance = 6f;
    public float dashDuration = 0.16f;
    public bool canDash = true;
    public float dashCooldown = 1.0f; // set this in Inspector
    public float postDashPause = 0.15f;

    public float dashInvulnTime = 0.12f;
    public AnimationCurve dashEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool isDashing;
    private float lastDashTime;

    [Header("Crosshair")]
    public GameObject crossHair;
    public float crossHairDistance = 3f;

    [Header("Layers")]
    public LayerMask hitMask;

    [HideInInspector]
    public Vector2 direction;

    [HideInInspector]
    public Animator animator;

    [HideInInspector]
    public Rigidbody2D rb;

    [HideInInspector]
    float invulnUntil;

    private PlayerState _currentState;

    [HideInInspector]
    public Vector2 lastNonZeroInput = Vector2.down;

    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    public int comboCount;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (crossHair != null)
            crossHair.SetActive(!useGamepad);
    }

    private void Start()
    {
        TransitionToState(new IdleState());
    }

    private void Update()
    {
        GetDirection();
        if (crossHair != null)
        {
            crossHair.SetActive(!useGamepad);
            if (!useGamepad)
            {
                SetCrosshair();
            }
        }

        UnityEngine.Debug.Log($"Current State: {_currentState.GetType().Name}");
        _currentState.HandleInput(this);
        _currentState.Update(this);
    }

    private void FixedUpdate()
    {
        _currentState.FixedUpdate(this);
    }

    private void SetCrosshair()
    {
        if (crossHair == null)
            return;
        crossHair.transform.position =
            transform.position + (Vector3)(direction * crossHairDistance);
    }

    public void TransitionToState(PlayerState newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit(this);
        }
        _currentState = newState;
        _currentState.Enter(this);
    }

    public Vector2 GetMoveInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (movementInput.sqrMagnitude > minInputThreshold * minInputThreshold)
            lastNonZeroInput = movementInput.normalized;

        return movementInput;
    }

    void GetDirection()
    {
        if (useGamepad)
        {
            direction =
                (movementInput.sqrMagnitude > 0.01f) ? movementInput.normalized : lastNonZeroInput;
        }
        else
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
            Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
            direction = (worldMouse - transform.position).normalized;
        }
    }

    public void Move(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * moveSpeed;
        if (dir == Vector2.zero)
            rb.linearVelocity = Vector2.zero;
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    #region --- Attack Methods ---

    public void StartCombo()
    {
        // isAttacking = false;
        UnityEngine.Debug.Log("StartCombo, combo count: " + comboCount + " | " + DateTime.Now);
        if (comboCount < 3)
        {
            comboCount++;
        }
    }

    public void EndCombo()
    {
        UnityEngine.Debug.Log("EndCombo " + DateTime.Now);

        isAttacking = false;
        comboCount = 0;
    }

    public void OnAttackHitFrame()
    {
        var phase = comboPhases[Mathf.Clamp(currentComboIndex, 0, comboPhases.Length - 1)];
        SpawnAttackHitbox(phase.damage);
    }

    public void TriggerAttackMomentum()
    {
        var phase = comboPhases[Mathf.Clamp(comboCount, 0, comboPhases.Length - 1)];
        UnityEngine.Debug.Log("TriggerAttackMomentum, pushForce" + phase.pushForce);
        // Vector2 dir = GetDirection();
        StartCoroutine(ApplyAttackMomentum(direction, phase.pushForce, phase.pushDuration));
    }

    private IEnumerator ApplyAttackMomentum(Vector2 dir, float force, float duration)
    {
        UnityEngine.Debug.Log("ApplyAttackMomentum");
        // Cancel any previous momentum coroutine cleanly
        if (momentumCoroutine != null)
            StopCoroutine(momentumCoroutine);

        // momentumCoroutine = StartCoroutine(AttackMomentumRoutine(dir, force, duration));

        isAttackPushing = true;

        Vector2 startPos = rb.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentForce = Mathf.Lerp(force, 0f, t);
            rb.linearVelocity = dir * currentForce;

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Manually zero velocity at end
        rb.linearVelocity = Vector2.zero;
        isAttackPushing = false;
        yield break;
    }

    private Coroutine momentumCoroutine;

    void SpawnAttackHitbox(float damage)
    {
        if (attackHitboxPrefab == null)
            return;
        Vector2 spawnPos =
            (Vector2)transform.position + RotateOffsetByFacing(attackOffset, direction);
        var go = Instantiate(attackHitboxPrefab, spawnPos, Quaternion.identity);
        var hb = go.GetComponent<AttackHitbox>();
        hb.Init(damage, direction, attackRange, hitMask, gameObject);
    }

    Vector2 RotateOffsetByFacing(Vector2 offset, Vector2 facing)
    {
        if (facing == Vector2.zero)
            return offset;
        float angle = Mathf.Atan2(facing.y, facing.x);
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(offset.x * cos - offset.y * sin, offset.x * sin + offset.y * cos);
    }

    #endregion

    #region --- Dash Methods ---

    public void PerformDash()
    {
        if (!canDash || isDashing)
            return;

        Vector2 move = GetMoveInput();
        if (move.sqrMagnitude > 0.01f)
        {
            direction = move.normalized;
            lastNonZeroInput = direction;
        }

        canDash = false;
        isDashing = true;
        lastDashTime = Time.time;

        Vector2 dashDir = direction;
        StartCoroutine(DashCoroutine(dashDir));
    }

    public IEnumerator DashCoroutine(Vector2 dashDir)
    {
        float baseSpeed = dashDistance / dashDuration;
        invulnUntil = Time.time + dashInvulnTime;

        float start = Time.time;
        while (Time.time < start + dashDuration)
        {
            float t = (Time.time - start) / dashDuration;
            rb.linearVelocity = dashDir * baseSpeed * dashEasing.Evaluate(t);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        StartCoroutine(DashCooldownCoroutine());
    }

    private IEnumerator DashCooldownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public bool IsInvulnerable() => Time.time <= invulnUntil;

    #endregion

    public void UpdateFacingDirection(Vector2 facing)
    {
        if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
        {
            // Horizontal facing
            spriteRenderer.flipX = facing.x < 0;
        }
        else
        {
            // Vertical facing (don’t flip horizontally)
            spriteRenderer.flipX = false;
        }
    }
}
