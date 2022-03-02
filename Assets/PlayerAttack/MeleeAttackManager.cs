using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackManager : MonoBehaviour
{
    public float hitPerSec = 2;
    public float defaultForce = 300;
    public float movementTime = .1f;
    private bool meleeAttack;
   
    private Animator _playerAnimation;
    private float attackCooldown;

    private void Awake()
    {
        _playerAnimation = GetComponent<Animator>();
    }

    private void Start()
    {
        attackCooldown = 0;
    }

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (Input.GetButtonDown("Attack") && attackCooldown <= 0)
        {
            meleeAttack = true;
            attackCooldown = 1 / hitPerSec;
        }
        else
            meleeAttack = false;

        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (meleeAttack)
        {
            _playerAnimation.SetTrigger("Attack");
        }
    }
}