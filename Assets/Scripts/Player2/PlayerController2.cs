using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController2 : MonoBehaviour
{
    [Header("Input Mode")]
    [Tooltip("Switch between Keyboard+Mouse and Gamepad controls.")]
    public bool useGamepad = true;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 40f;
    public float deceleration = 40f;
    public float minInputThreshold = 0.01f;

    [Header("Dash")]
    public float dashDistance = 6f;
    public float dashDuration = 0.16f;
    public float dashCooldown = 0.6f;
    public float dashInvulnTime = 0.12f;
    public AnimationCurve dashEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);

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

    public int combo;

    [Header("General Attack Settings")]
    public GameObject attackHitboxPrefab;
    public float attackRange = 1.0f;
    public Vector2 attackOffset = new Vector2(0.6f, 0f);
    public float comboResetTime = 1.0f; // resets combo if player waits too long

    [Header("Attack Movement")]
    [Range(0f, 1f)]
    public float attackMoveSlowdown = 0f;

    [Header("Skill (Projectile)")]
    public GameObject projectilePrefab;
    public float skillDamage = 12f;
    public float projectileSpeed = 12f;
    public float skillCooldown = 1.2f;

    [Header("Layers")]
    public LayerMask hitMask;

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;
    Vector2 lastNonZeroInput = Vector2.down;
    Vector2 velocity;

    [Header("Crosshair")]
    public GameObject crossHair;
    public float crossHairDistance = 3f;

    bool isAttacking;
    bool isCasting;
    bool isDashing;
    bool isAttackLocked;
    bool isAttackPushing;
    bool queuedNextAttack;

    float lastAttackTime;
    float lastSkillTime;
    float lastDashTime;
    float invulnUntil;

    int currentComboIndex = 0;
    float comboTimer;

    [Header("Debug Gizmos")]
    public bool showGizmos = true;
    public bool showAttackRange = true;
    public bool showDashDistance = true;
    public bool showCrosshairDistance = true;
    public bool showProjectileDirection = true;
    public bool showFacingDirection = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (crossHair != null)
            crossHair.SetActive(!useGamepad);
    }

    void Update()
    {
        ReadInput();
        HandleActions();
        UpdateAnimationParameters();

        if (crossHair != null)
        {
            crossHair.SetActive(!useGamepad);
            if (!useGamepad)
                SetCrosshair();
        }

        // Combo timeout reset
        if (isAttacking && Time.time > comboTimer)
        {
            ResetCombo();
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
            ApplyMovementPhysics();
    }

    void ReadInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude > minInputThreshold * minInputThreshold)
            lastNonZeroInput = moveInput.normalized;
    }

    void HandleActions()
    {
        if (Input.GetButtonDown("Dash"))
            TryDash();
        if (Input.GetButtonDown("Fire1"))
            TryAttack();
        if (Input.GetButtonDown("Fire2"))
            TryCastSkill();
    }

    void ApplyMovementPhysics()
    {
        if (isAttackPushing)
            return;

        Vector2 target = moveInput.normalized * moveSpeed;
        if (isAttackLocked)
            target *= attackMoveSlowdown;

        float accel = (target.sqrMagnitude > velocity.sqrMagnitude) ? acceleration : deceleration;
        velocity = Vector2.MoveTowards(velocity, target, accel * Time.fixedDeltaTime);
        rb.linearVelocity = velocity;
    }

    private Vector2 GetDirection()
    {
        Vector2 direction;
        if (useGamepad)
        {
            direction = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastNonZeroInput;
        }
        else
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
            Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
            direction = (worldMouse - transform.position).normalized;
        }
        return direction;
    }

    private void SetCrosshair()
    {
        if (crossHair == null)
            return;
        Vector2 direction = GetDirection();
        crossHair.transform.position =
            transform.position + (Vector3)(direction * crossHairDistance);
    }

    #region --- Attack Logic ---
    // void TryAttack()
    // {
    //     if (isDashing || isCasting)
    //         return;

    //     // Start from scratch if not attacking
    //     if (!isAttacking)
    //     {
    //         currentComboIndex = 0;
    //         PlayComboAttack();
    //         return;
    //     }

    //     // Queue next attack if player presses again during combo window
    //     if (isAttacking && currentComboIndex < comboPhases.Length - 1)
    //     {
    //         queuedNextAttack = true;
    //     }
    // }

    void TryAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack" + combo);
    }

    public void StartCombo()
    {
        UnityEngine.Debug.Log("StartCombo " + combo);
        isAttacking = false;
        if (combo < 3)
        {
            combo++;
        }
    }

    public void FinishAnim()
    {
        UnityEngine.Debug.Log("FinishAnim");
        isAttacking = false;
        combo = 0;
    }

    void PlayComboAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        comboTimer = Time.time + comboResetTime;

        var phase = comboPhases[currentComboIndex];
        UnityEngine.Debug.Log("phase" + phase.animationTrigger);
        anim.SetTrigger(phase.animationTrigger);
        StartCoroutine(AttackLockRoutine(phase.lockDuration));
    }

    #region --- Animation Event Hooks ---

    public void OnDashStart()
    {
        if (isDashing)
            return;
        StartCoroutine(PerformDash());
    }

    public void OnDashEnd()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }

    public void OnAttackHitFrame()
    {
        var phase = comboPhases[Mathf.Clamp(currentComboIndex, 0, comboPhases.Length - 1)];
        SpawnAttackHitbox(phase.damage);
    }

    public void TriggerAttackMomentum()
    {
        var phase = comboPhases[Mathf.Clamp(currentComboIndex, 0, comboPhases.Length - 1)];
        Vector2 dir = GetDirection();
        StartCoroutine(ApplyAttackMomentum(dir, phase.pushForce, phase.pushDuration));
    }

    public void OnAttackEnd()
    {
        // If player queued next attack, advance combo
        if (queuedNextAttack && currentComboIndex < comboPhases.Length - 1)
        {
            queuedNextAttack = false;
            currentComboIndex++;
            PlayComboAttack();
        }
        else
        {
            ResetCombo();
        }
    }

    public void OnCastRelease()
    {
        SpawnProjectile(GetDirection());
    }

    public void OnCastEnd()
    {
        isCasting = false;
    }
    #endregion

    void ResetCombo()
    {
        isAttacking = false;
        queuedNextAttack = false;
        currentComboIndex = 0;
    }

    public void EnableComboInputWindow()
    {
        queuedNextAttack = false; // open input window
    }

    #endregion

    #region --- Other Actions ---
    void TryCastSkill()
    {
        if (Time.time < lastSkillTime + skillCooldown || isCasting || isDashing)
            return;
        isCasting = true;
        lastSkillTime = Time.time;
        anim.SetTrigger("Cast");
    }

    void TryDash()
    {
        FinishAnim();
        if (Time.time < lastDashTime + dashCooldown || isDashing)
            return;
        lastDashTime = Time.time;
        anim.SetTrigger("Dash");
    }
    #endregion

    #region --- Coroutines & Helpers ---
    IEnumerator PerformDash()
    {
        UnityEngine.Debug.Log("Perform dash");
        isDashing = true;
        Vector2 dir = GetDirection();
        float baseSpeed = dashDistance / dashDuration;
        invulnUntil = Time.time + dashInvulnTime;

        float start = Time.time;
        while (Time.time < start + dashDuration)
        {
            float t = (Time.time - start) / dashDuration;
            rb.linearVelocity = dir * baseSpeed * dashEasing.Evaluate(t);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    IEnumerator AttackLockRoutine(float duration)
    {
        isAttackLocked = true;
        yield return new WaitForSeconds(duration);
        isAttackLocked = false;
    }

    // private IEnumerator ApplyAttackMomentum(Vector2 dir, float force, float duration)
    // {
    //     isAttackPushing = true;
    //     float timer = 0f;
    //     while (timer < duration)
    //     {
    //         float t = timer / duration;
    //         float currentForce = Mathf.Lerp(force, 0f, t);
    //         UnityEngine.Debug.Log("Velocity: " + dir * currentForce);
    //         rb.linearVelocity = dir * currentForce;
    //         timer += Time.fixedDeltaTime;
    //         yield return new WaitForFixedUpdate();
    //     }
    //     isAttackPushing = false;
    // }

    private IEnumerator ApplyAttackMomentum(Vector2 dir, float force, float duration)
    {
        // Cancel any previous momentum coroutine cleanly
        if (momentumCoroutine != null)
            StopCoroutine(momentumCoroutine);

        momentumCoroutine = StartCoroutine(AttackMomentumRoutine(dir, force, duration));
        yield break;
    }

    private Coroutine momentumCoroutine;

    private IEnumerator AttackMomentumRoutine(Vector2 dir, float force, float duration)
    {
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
    }

    void SpawnAttackHitbox(float damage)
    {
        if (attackHitboxPrefab == null)
            return;
        Vector2 dir = GetDirection();
        Vector2 spawnPos = (Vector2)transform.position + RotateOffsetByFacing(attackOffset, dir);
        var go = Instantiate(attackHitboxPrefab, spawnPos, Quaternion.identity);
        var hb = go.GetComponent<AttackHitbox>();
        hb.Init(damage, dir, attackRange, hitMask, gameObject);
    }

    void SpawnProjectile(Vector2 dir)
    {
        if (projectilePrefab == null)
            return;
        var p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = p.GetComponent<Projectile>();
        proj.Init(dir);
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

    void UpdateAnimationParameters()
    {
        anim.SetFloat("Speed", rb.linearVelocity.magnitude);
        anim.SetFloat("MoveX", lastNonZeroInput.x);
        anim.SetFloat("MoveY", lastNonZeroInput.y);
        anim.SetBool("IsDashing", isDashing);
        anim.SetBool("IsAttacking", isAttacking);
        anim.SetBool("IsCasting", isCasting);
        anim.SetBool("IsAttackLocked", isAttackLocked);
    }

    public bool IsInvulnerable() => Time.time <= invulnUntil;

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        Vector3 origin = transform.position;
        Vector2 dir = Vector2.right;

        if (Application.isPlaying)
            dir = GetDirection();

        if (showFacingDirection)
        {
            Gizmos.color = Color.yellow;
            Vector3 end = origin + (Vector3)(dir * 1.5f);
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawSphere(end, 0.05f);
        }

        if (showAttackRange)
        {
            Gizmos.color = Color.red;
            Vector2 attackPos =
                (Vector2)transform.position + RotateOffsetByFacing(attackOffset, dir);
            Gizmos.DrawWireSphere(attackPos, attackRange);
        }

        if (showDashDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin + (Vector3)(dir * dashDistance), 0.15f);
        }

        if (showCrosshairDistance)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(origin + (Vector3)(dir * crossHairDistance), 0.1f);
        }

        if (showProjectileDirection)
        {
            Gizmos.color = Color.magenta;
            Vector3 projPos = origin + (Vector3)(dir * 0.5f);
            Gizmos.DrawLine(projPos, projPos + (Vector3)(dir * 1.5f));
            Gizmos.DrawSphere(projPos, 0.05f);
        }
    }
}
