using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private bool damageable = true;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float invulnerabilityTime = .2f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private float knockbackHorizontalForce = 3f;
    [SerializeField] private float knockbackVerticalForce = 1.5f;
    [SerializeField] private ParticleSystem damagedParticle;
    private Rigidbody2D rb;

    private bool hit;
    private Vector2 knockbackForce;
    private float currentHealth;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        rb = this.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        knockbackForce = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (hit && knockbackForce != Vector2.zero)
        {
            rb.AddForce(knockbackForce, ForceMode2D.Impulse);
            knockbackForce = Vector2.zero;
        }
    }

    public void Damage(int amount, Vector2 direction)
    {
        if (damageable && !hit && currentHealth > 0)
        {
            spriteRenderer.material.SetFloat("_FadeAmount", GameMaster.Instance.HitFadeAmount);
            damagedParticle.Play();
            hit = true;
            currentHealth -= amount;
            UpdateHealthBar();
            knockbackForce = direction * knockbackHorizontalForce + Vector2.up * knockbackVerticalForce;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Kill();
                // gameObject.SetActive(false);
            }
            else
            {
                StartCoroutine(TurnOffHit());
            }
        }
    }

    private IEnumerator TurnOffHit()
    {
        yield return new WaitForSeconds(invulnerabilityTime);
        hit = false;
        spriteRenderer.material.SetFloat("_FadeAmount", 0);
    }

    private void UpdateHealthBar()
    {
        healthBar.value = Mathf.Clamp01(currentHealth / maxHealth);
    }

    public void Kill()
    {
        GameMaster.Instance.KillCount++;
        Instantiate(GameMaster.Instance.DeathParticle, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
