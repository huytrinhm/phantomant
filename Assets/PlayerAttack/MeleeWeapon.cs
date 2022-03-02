using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private int damageAmount = 20;
    private PlayerController playerController;
    
    private Rigidbody2D rb;
    private MeleeAttackManager meleeAttackManager;
    private Vector2 direction;
   
    private bool collided;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        playerController = GetComponentInParent<PlayerController>();
        meleeAttackManager = GetComponentInParent<MeleeAttackManager>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

     private void HandleMovement()
    {
        if (collided)
        {
            rb.AddForce(direction * meleeAttackManager.defaultForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyHealth>())
        {
            HandleCollision(collision.GetComponent<EnemyHealth>());
        }
    }

    private void HandleCollision(EnemyHealth objHealth)
    {
        if (playerController.IsFacingRight)
            direction = Vector2.left;
        else
            direction = Vector2.right;
        collided = true;
        
        objHealth.Damage(damageAmount);
        StartCoroutine(NoLongerColliding());
    }

     private IEnumerator NoLongerColliding()
    {
        yield return new WaitForSeconds(meleeAttackManager.movementTime);
        collided = false;
    }




}


