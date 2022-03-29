using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour
{
    public bool damageable = true;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float invulnerabilityTime = .2f;
    [SerializeField] private Slider healthBar;
    [SerializeField] private ParticleSystem damagedParticle;
    private Rigidbody2D rb;
    private PlayerController cc;
    
    private bool hit;
    private Vector2 knockbackForce;
    private float currentHealth;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        rb = this.GetComponent<Rigidbody2D>();
        cc = GetComponent<PlayerController>();
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

    public void Damage(float knockbackHorizontalForce, float knockbackVerticalForce, int amount, Vector2 direction)
    {
        if (damageable && !hit && currentHealth > 0)
        {
            cc.enabled = false;
            rb.velocity = new Vector2(0.0f, 0.0f);

            spriteRenderer.material.SetFloat("_FadeAmount", GameMaster.Instance.HitFadeAmount);
            damagedParticle.Play();
            hit = true;
            currentHealth -= amount;
            UpdateHealthBar();
            knockbackForce = direction * knockbackHorizontalForce + Vector2.up * knockbackVerticalForce;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                GameMaster.Instance.GameOver();
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
        cc.enabled = true;
    }

    private void UpdateHealthBar()
    {
        healthBar.value = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
