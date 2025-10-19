using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] Weapons;

    public string currentWeapon;

    private Animator WeaponAnimator;

    private float facingDirection;

    private GameObject parentObject;

    private CharacterClass characterClass;

    void Start()
    {
        SetWeapon(0);

        WeaponAnimator = this.GetComponent<Animator>();

        parentObject = transform.parent.gameObject;
        characterClass = parentObject.GetComponent<CharacterClass>();
    }

    // Update is called once per frame
    void Update()
    {
        if (parentObject.tag == "Player")
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetWeapon(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetWeapon(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetWeapon(2);
            }

        // Debug.Log("Facing Direction: " + facingDirection);
        WeaponAnimator.SetFloat("AttackDirection", characterClass.facingDirection);
    }

    void SetWeapon(int weaponID)
    {
        // Loop through all the weapons in the array
        for (int i = 0; i < Weapons.Length; i++)
        {
            // Set the weapon active if its index matches the weaponID
            // Otherwise, set it inactive
            Weapons[i].SetActive(i == weaponID);
        }

        currentWeapon = Weapons[weaponID].name;
    }

    public void AttackAnimation()
    {
        Debug.Log("AttackAnimation | Weapon Manager");
        WeaponAnimator.SetTrigger("Attack");
    }
}
