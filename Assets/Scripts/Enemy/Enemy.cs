using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public enum EnemyState
{
    idle,
    walk,
    attack,
    stagger,
}

public class Enemy : MonoBehaviour
{
    public EnemyState currentState;
    public Rigidbody2D rb;
    public EnemyAI enemyAI;
    public int maxHealth = 100;
    int currentHealth;
    public Animator animator;
    public float knockbackDistance = 3f;
    public float staggerTime = 1f;
    public bool isDead = false;
    public AIPath aiPath;

    [SerializeField]
    private FieldOfView fieldOfView;

    private AIDestinationSetter aiDestSetter;

    // Start is called before the first frame update
    public void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        enemyAI = this.GetComponent<EnemyAI>();
        aiDestSetter = GetComponent<AIDestinationSetter>();
    }

    private void Update()
    {
        fieldOfView.SetOrigin(transform.position);
        if (aiDestSetter.target != null)
        {
            if (this.transform.position.x < aiDestSetter.target.transform.position.x)
            {
                // Debug.Log("right");
                animator.SetFloat("facingRight", 1);
            }
            else
            {
                // Debug.Log("left");
                animator.SetFloat("facingRight", 0.1f);
            }
        }
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        Debug.Log(currentHealth);
        Vector3 dirFromAttacker = (transform.position - attackerPosition).normalized;
        transform.position += dirFromAttacker * knockbackDistance;
        currentHealth -= damage;
        //play hurt animation from here.
        animator.SetTrigger("Hurt");

        enemyAI.enemyState = EnemyAI.EnemyState.stagger;
        enemyAI.SetPlayerAsTarget();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine("Stagger");
        }
    }

    void Die()
    {
        animator.SetBool("IsDead", true);
        isDead = true;
        enemyAI.enabled = false;

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
