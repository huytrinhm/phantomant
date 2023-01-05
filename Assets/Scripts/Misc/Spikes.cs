using UnityEngine;

public class Spikes : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float hitPerSec = 2f;
    [SerializeField] private PlayerStat player;

    private float hitCooldown = 0f;

    private void Update()
    {
        if (hitCooldown > 0)
            hitCooldown -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.GetComponent<EnemyHealth>().Kill();
        }

        if (collision.CompareTag("Player") && hitCooldown <= 0)
        {
            player.Damage(0, 0, damage, Vector2.zero);
            hitCooldown = 1 / hitPerSec;
        }
    }
}
