using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalEnemyWeapon : MonoBehaviour
{
    private NormalEnemy thisEnemy;

    private void Awake()
    {
        thisEnemy = this.GetComponentInParent<NormalEnemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandleCollision(collision.GetComponent<PlayerStat>());
        }
    }

    private void HandleCollision(PlayerStat objHealth)
    {
        Vector2 direction;
        if (this.transform.position.x - objHealth.transform.position.x >= 0)
            direction = Vector2.right;
        else
            direction = Vector2.left;

        objHealth.Damage(thisEnemy.knockbackHorizontalForce, thisEnemy.knockbackVerticalForce, thisEnemy.strength, -direction);
    }
}
