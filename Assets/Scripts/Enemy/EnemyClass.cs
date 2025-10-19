using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyClass : MonoBehaviour
{
    public Rigidbody2D rb;
    public EnemyState enemyState;

    public enum EnemyState
    {
        idle,
        chasing,
        attack,
        stagger,
        dead,
    }

    public Animator animator;

    public int maxHealth = 100;
    int currentHealth;
    public float knockbackDistance = 1000f;
    public float staggerTime = 1f;
    public bool isDead = false;
    public LayerMask knockbackLayerMask;

    public float attackRange = 2f;
    public int attackDamage = 20;

    public float attackCooldown = 1f; //seconds
    private float lastAttackedAt = -9999f;
    private AIDestinationSetter aiDestSetter;
    IAstarAI ai;
    private Transform player;
    private GameObject playerObject;
    private bool playerIsDead = false;

    private WeaponManager weaponManager;

    // Start is called before the first frame update
    void Start()
    {
        this.player = GameObject.FindWithTag("Player").transform;
        this.playerObject = GameObject.FindWithTag("Player");

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        aiDestSetter = GetComponent<AIDestinationSetter>();
        ai = GetComponent<IAstarAI>();
        Debug.Log(currentHealth);

        weaponManager = GetComponentInChildren<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (enemyState)
        {
            case EnemyState.chasing:
                // Chasing();
                break;
            case EnemyState.idle:
                Idle();
                break;
            case EnemyState.stagger:
                StartCoroutine("Stagger");
                break;
            case EnemyState.attack:
                Attack();
                break;
        }

        if (ai.velocity.magnitude > 0)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        if (aiDestSetter.target != null)
        {
            if (this.transform.position.x < aiDestSetter.target.transform.position.x)
            {
                // Debug.Log("right");
                animator.SetFloat("facingDirection", 1);
            }
            else
            {
                // Debug.Log("left");
                animator.SetFloat("facingDirection", 0.1f);
            }
        }

        if (Vector3.Distance(this.transform.position, player.transform.position) < attackRange)
        {
            if (Time.time > lastAttackedAt + attackCooldown & !playerIsDead)
            {
                // enemyState = EnemyState.attack;
                weaponManager.AttackAnimation();

                lastAttackedAt = Time.time;
            }
            // aiDestSetter.target = player;
        }
        // playerIsDead = playerObject.GetComponent<PlayerCombat>().isDead;
    }

    public void Attack()
    {
        Debug.Log("In attack range");
        if (Vector3.Distance(this.transform.position, player.transform.position) < attackRange)
        {
            // player.GetComponent<PlayerCombat>().TakeDamage(attackDamage, transform.position);
        }
        enemyState = EnemyState.idle;
    }

    public void Idle()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) < 10f)
        {
            enemyState = EnemyState.chasing;
            aiDestSetter.target = player;
        }
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        Debug.Log(currentHealth);
        Vector2 dirFromAttacker = (transform.position - attackerPosition).normalized;
        Vector2 knockback = dirFromAttacker * knockbackDistance;

        rb.AddForce(knockback, ForceMode2D.Force);

        currentHealth -= damage;
        //play hurt animation from here.
        animator.SetTrigger("Hurt");
        Debug.Log(currentHealth);

        enemyState = EnemyState.stagger;
        // enemyAI.SetPlayerAsTarget();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine("Stagger");
        }
    }

    private IEnumerator Stagger()
    {
        yield return new WaitForSeconds(staggerTime);
        //rb.velocity = Vector2.zero;
        enemyState = EnemyState.idle;
        // animator.SetBool("isMoving", false);
    }

    void Die()
    {
        animator.SetTrigger("isDead");
        isDead = true;
        // enemyAI.enabled = false;

        GetComponent<AIPath>().enabled = false;
        this.GetComponent<Collider2D>().enabled = false;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        // this.GetComponent<Pathfinding>().enabled = false;
        // GetComponent
        this.enabled = false;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            var child = this.transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(false);
        }
    }
}
