using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackManager : MonoBehaviour
{
    public float hitPerSec = 2;
    //public float defaultForce = 300;
    public float movementTime = .1f;
    public float hitShakeAmount = .5f;
    public float hitShakeFrequency = .01f;
    private bool meleeAttack;
   
    private Animator _playerAnimation;
    private float attackCooldown;
    public bool IsEndRoom = false;

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
        if(!GameMaster.Instance.IsPaused && !IsEndRoom)
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
            AudioManager.Instance.PlaySoundEffect("hero_attack");
            _playerAnimation.SetTrigger("Attack");
            StartCoroutine(AttackAnimator());
        }
    }

    IEnumerator AttackAnimator()
    {
        _playerAnimation.SetBool("isAttacking", true);
        yield return new WaitForSeconds(movementTime);
        _playerAnimation.SetBool("isAttacking", false);
    }
}