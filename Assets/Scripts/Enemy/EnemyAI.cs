using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    // private EnemyMovement enemyMovement;


    float reachedPositionDistance = 1.5f;
    public FieldOfView fov;
    public Rigidbody2D rb;
    private AIDestinationSetter aiDestSetter;
    private Transform player;
    public float staggerTime = 1f;
    bool playerInSight = false;
    public GameObject excMark;
    public EnemyState enemyState;
    public GameObject targetPosition;
    public Animator animator;
    public Vector3 lastPosition;
    public Transform enemyTransform;
    public bool isMoving;

    // private TileBase[] tileMapArray;

    IAstarAI ai;

    public enum EnemyState
    {
        idle,
        chasing,
        attack,
        stagger,
        randomPatrol,
        dead,
        highAlert,
    }

    private void Awake()
    {
        // enemyMovement = GetComponent<EnemyMovement>();
        this.player = GameObject.FindWithTag("Player").transform;
        aiDestSetter = GetComponent<AIDestinationSetter>();
        ai = GetComponent<IAstarAI>();
        // targetPosition = transform.Find("TargetPosition").gameObject;
    }

    // Start is called before the first frame update
    private void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();

        // roamPosition = GetRoamingPosition();
        this.player = GameObject.FindWithTag("Player").transform;
        animator = GetComponent<Animator>();

        enemyTransform = transform;
        lastPosition = enemyTransform.position;
        isMoving = false;
    }

    private void Update()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) < 10f)
        {
            enemyState = EnemyState.chasing;
            aiDestSetter.target = player;
        }
        if (ai.velocity.magnitude > 0)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        switch (enemyState)
        {
            case EnemyState.chasing:
                Chasing();
                break;
            case EnemyState.idle:
                Idle();
                break;
            case EnemyState.stagger:
                StartCoroutine("Stagger");
                break;
            // case EnemyState.randomPatrol:
            //     RandomRoaming();
            //     break;
            // case EnemyState.highAlert:
            //     RandomRoaming();
            //     break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Walls" && enemyState == EnemyState.randomPatrol)
        {
            StartCoroutine(patrolPause());
        }
    }

    // private void LateUpdate()
    // {
    //     FindTargetPlayer();
    // }

    // Update is called once per frame
    private GameObject RoamingPosition()
    {
        var targetTile = GameObject
            .Find("Tilemap_Ground")
            .GetComponent<TilemapManager>()
            .GetRandomTile();

        Debug.Log("RoamingPosition: " + targetTile);
        targetPosition.transform.position = targetTile;

        return targetPosition;
    }

    private Vector3 GetRandomDir()
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;
    }

    // Detecting for player within the field of view.
    private void FindTargetPlayer()
    {
        //Continuously detecting for Player if they enter the FOV.
        if (Vector3.Distance(this.transform.position, player.transform.position) < fov.viewDistance)
        {
            // If player is within viewing distance
            Vector3 dirToPlayer = (player.transform.position - this.transform.position).normalized;
            if (
                Vector3.Angle(fov.GetVectorFromAngle(fov.startingAngle - fov.fov / 2), dirToPlayer)
                < fov.fov / 2
            )
            {
                // If player is within viewing angle
                RaycastHit2D raycastHit2D = Physics2D.Raycast(
                    this.transform.position,
                    dirToPlayer,
                    fov.viewDistance,
                    fov.layerMask
                );
                if (raycastHit2D.collider != null)
                {
                    // If raycast detects a hit
                    if (raycastHit2D.transform.gameObject.tag == "Player")
                    {
                        // If the detected hit is the Player
                        enemyState = EnemyState.chasing;
                        playerInSight = true;
                        aiDestSetter.target = player;
                    }
                }
            }
        }
        else
        {
            if (playerInSight)
            {
                playerInSight = false;
                StartCoroutine(patrolPause());
            }
        }
    }

    public void SetPlayerAsTarget()
    {
        aiDestSetter.target = player;
        enemyState = EnemyState.chasing;
    }

    private void Chasing()
    {
        excMark.SetActive(true);

        fov.viewDistance = 10f;
        // fov.fov = 180f;
        fov.SetAimDirection(player.position - transform.position);
        // animator.SetBool("isMoving", true);
    }

    private void Idle()
    {
        excMark.SetActive(false);
        aiDestSetter.target = null;
    }

    private IEnumerator Stagger()
    {
        yield return new WaitForSeconds(staggerTime);
        //rb.velocity = Vector2.zero;
        enemyState = EnemyState.idle;
        // animator.SetBool("isMoving", false);
    }

    private void RandomRoaming()
    {
        // Debug.Log("RANDOM ROAMING CALLED 193");
        // animator.SetBool("isMoving", true);

        if (aiDestSetter.target == null)
        {
            aiDestSetter.target = RoamingPosition().transform;
        }
        else
        {
            if (
                Vector3.Distance(transform.position, targetPosition.transform.position)
                < reachedPositionDistance
            )
            {
                // Debug.Log("Start Cor from RandomRoaming");

                StartCoroutine(patrolPause());
            }
        }
        adjustFOVDirection();
    }

    IEnumerator patrolPause()
    {
        enemyState = EnemyState.idle;
        yield return new WaitForSeconds(Random.Range(3, 10));
        enemyState = EnemyState.randomPatrol;
    }

    private void adjustFOVDirection()
    {
        fov.SetAimDirection(aiDestSetter.target.position - transform.position);
    }
}
