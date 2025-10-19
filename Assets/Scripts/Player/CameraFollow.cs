using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //private Transform playerTransform;
    //void Start()
    //{
    //    playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    //}

    //// Update is called once per frame
    //void LateUpdate()
    //{
    //    // store current camera's position in variable temp
    //    Vector3 temp = transform.position;

    //    // set the camera's x position to be equal to player
    //    temp.x = playerTransform.position.x;
    //    temp.y = playerTransform.position.y;

    //    // set back the cameras temp position, to the cameras current position
    //    transform.position = temp;
    //}

    public Transform target;
    public Vector3 offset;
    [Range(1, 10)]
    public float smoothFactor;

    private void FixedUpdate()
    {
        Follow();
    }

    void Follow()
    {
        Vector3 targetPosition = target.position + offset;
        Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, smoothFactor * Time.fixedDeltaTime);
        transform.position = smoothPosition;
    }
}