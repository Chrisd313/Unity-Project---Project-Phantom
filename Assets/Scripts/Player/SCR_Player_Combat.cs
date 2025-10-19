using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_Player_Combat : MonoBehaviour
{
    public Animator animator;
    Rigidbody2D rb;
    
    void Update()
    {
        if (Input.GetButtonDown("Attack") /*&& this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Grounded")*/)
        {
            animator.SetTrigger("Attack");
        }
            
    }
}
    

//{

//    public Animator animator;

//    public float attackRate = 5f;
//    float nextAttackTime = 0f;
//    public int noOfClicks = 0;
//    float lastClickedTime = 0;
//    public float maxComboDelay = 0.9f;

//    Rigidbody2D rb;

//    public Transform attackPoint;
//    public float attackRange = 0.5f;
//    public LayerMask enemyLayers;

//    public int attackDamage = 20;

//    public GameObject enemy;



//    void Update()
//    {
//        if (Time.time - lastClickedTime > maxComboDelay)
//        {
//            noOfClicks = 0;
//        }

//        if (Time.time >= nextAttackTime)
//        {
//            if (Input.GetButtonDown("Attack") /*&& this.animator.GetCurrentAnimatorStateInfo(0).IsTag("Grounded")*/)
//            {
//                lastClickedTime = Time.time;
//                noOfClicks++;
//                //Attack();
//                if (noOfClicks == 1)
//                {
//                    animator.SetBool("Attack1", true);
//                    Attack();
//                }
//                noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);
//                nextAttackTime = Time.time + 1f / attackRate;
//                rb.velocity = Vector2.zero;
//            }
//        }

//    }

//    public void return1() //Check for second click, then action return2 if detected
//    {
//        if (noOfClicks >= 2)
//        {
//            animator.SetBool("Attack2", true);
//            animator.SetBool("Attack1", false);
//            Attack();
//        }
//        else
//        {
//            animator.SetBool("Attack1", false);
//            noOfClicks = 0;
//        }
//    }

//    public void return2() //Check for third click, then action return3 if detected
//    {
//        if (noOfClicks >= 3)
//        {
//            animator.SetBool("Attack3", true);
//            Attack();
//        }
//        else
//        {
//            animator.SetBool("Attack2", false);
//            noOfClicks = 0;
//        }
//    }

//    public void return3() //Reset "Attack" anim bools, goes back to idle.
//    {
//        animator.SetBool("Attack1", false);
//        animator.SetBool("Attack2", false);
//        animator.SetBool("Attack3", false);
//        noOfClicks = 0;
//    }

//    void Attack()
//    {
//        //Detect enemies
//        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

//        //Damage enemies
//        foreach (Collider2D enemy in hitEnemies)
//        {
//            enemy.GetComponent<SCR_Enemy>().TakeDamage(attackDamage);
//            Debug.Log("We hit " + enemy.name);
//        }
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (attackPoint == null)
//            return;

//        //Gizmos.DrawWireSphere(attackPoint.position, attackRange);
//    }

//}
