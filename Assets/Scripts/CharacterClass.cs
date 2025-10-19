using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterClass : MonoBehaviour
{
    public float speed;
    public Rigidbody2D rb;
    public Animator animator;
    public float attackRange;
    public int attackDamage;
    public int health;
    public float facingDirection;

    public WeaponManager weaponManager;
}
