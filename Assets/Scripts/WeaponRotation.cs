using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRotation : MonoBehaviour
{
    public SpriteRenderer characterRenderer,
        weaponRenderer;
    public Vector2 PointerPosition { get; set; }
    public Transform rotationTarget;

    public bool rotateWithDirection;
    private GameObject parentObject;
    public CharacterClass characterClass;

    // void Start()
    // {
    //     parentObject = transform.parent.gameObject;
    //     characterClass = parentObject.GetComponent<CharacterClass>();
    // }

    // Update is called once per frame
    void Update()
    {
        if (rotateWithDirection)
        {
            PointerPosition = rotationTarget.position;
            Vector2 direction = (PointerPosition - (Vector2)transform.position).normalized;
            transform.right = direction;

            Vector2 scale = transform.localScale;
            if (direction.x < 0)
            {
                scale.y = -1;
            }
            else if (direction.x > 0)
            {
                scale.y = 1;
            }

            transform.localScale = scale;

            if (transform.eulerAngles.z > 0 && transform.eulerAngles.z < 180)
            {
                weaponRenderer.sortingOrder = characterRenderer.sortingOrder - 1;
            }
            else
            {
                weaponRenderer.sortingOrder = characterRenderer.sortingOrder + 1;
            }
        }
        else
        {
            if (characterClass.facingDirection < 0)
            {
                weaponRenderer.flipX = false;
                weaponRenderer.sortingOrder = characterRenderer.sortingOrder + 1;
            }
            else
            {
                weaponRenderer.flipX = true;
                weaponRenderer.sortingOrder = characterRenderer.sortingOrder - 1;
            }
        }
    }
}
