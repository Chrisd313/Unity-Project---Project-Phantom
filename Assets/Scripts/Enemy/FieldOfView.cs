using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

//https://www.youtube.com/watch?v=3-jPo2wzvdw&t=185s


public class FieldOfView : MonoBehaviour
{
    [SerializeField]
    public LayerMask layerMask;
    public float fov = 80f;
    public float viewDistance = 5f;
    public float speed = 100f;
    private Vector3 origin;
    private Mesh mesh;
    public float startingAngle;
    private Transform player;
    public bool playerInSight;

    public float lowAlert = 50f;
    public float highAlert = 120f;

    public float angle;

    RaycastHit2D[] hit;

    public List<string> raycastHits;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        this.player = GameObject.FindWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        // Debug.Log(player.position);
        // float targetDirection = Mathf.Atan2(player.y, player.x) * Mathf.Rad2Deg;
        // float targetDirection = Mathf.Atan2(player.position.y - transform.position.y, player.position.x -transform.position.x ) * Mathf.Rad2Deg;
        // Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, targetDirection));
        // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);

        Vector3 offsetPos = transform.parent.position;

        float angle = startingAngle;
        //Vector3 origin = Vector3.zero;
        int rayCount = 30;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin - offsetPos;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            // RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorFromAngle(angle), viewDistance, layerMask);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(
                origin,
                GetVectorFromAngle(angle),
                viewDistance,
                layerMask
            );
            hit = Physics2D.RaycastAll(origin, GetVectorFromAngle(angle), viewDistance, layerMask);

            if (raycastHit2D.collider == null)
            {
                vertex = (origin - offsetPos) + GetVectorFromAngle(angle) * viewDistance;
                // raycastHits = null;
            }
            else
            {
                // Debug.Log("Hit: " + raycastHit2D.collider.name);
                vertex = raycastHit2D.point - (Vector2)offsetPos;

                // if (!raycastHits.Contains(raycastHit2D.collider.name)){
                //     raycastHits.Add(raycastHit2D.collider.name) ;
                // }


                // if(raycastHit2D.transform.gameObject.tag == "Player"){
                //     playerInSight = true;
                // }
            }

            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);

        // Debug.Log("FOV: " + fov);
    }

   



    public Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        // Debug.Log("GetAngleFromVect " + dir);

        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0)
            n += 360;
        return n;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        // Debug.Log("Set aim direction" + aimDirection);
        startingAngle = GetAngleFromVectorFloat(aimDirection) + fov / 2;
    }

    public void SetEnemyAimDirection(Vector3 aimDirection)
    {
        // Debug.Log("Set enemy aim direction" + aimDirection);
        startingAngle = GetAngleFromVectorFloat(aimDirection) + fov / 2;
    }

    public float GetAimDirection()
    {
        return startingAngle;
    }
}




//SOURCE: https://www.youtube.com/watch?v=rQG9aUWarwE
//Need to find a way to make the cone/angle follow the enemies last facing direction.
//Possible solution: https://www.youtube.com/watch?v=3-jPo2wzvdw
/*

{
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine("FindTargetWithDelay", .2f);
    }

    IEnumerator FindTargetWithDelay (float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider2D[] targetInViewRadius = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), viewRadius, targetMask);

        for (int i = 0; i < targetInViewRadius.Length; i++)
        {
            Transform target = targetInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            if (Vector2.Angle (transform.up, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector2.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    Debug.Log("Player spotted!");
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}*/
