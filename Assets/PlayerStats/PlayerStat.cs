using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStat : MonoBehaviour
{
    [SerializeField] private bool damageable = true;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float invulnerabilityTime = .2f;
    [SerializeField] [Range(0, 1)] private float fadeAmount = .2f;
    [SerializeField] private Slider healthBar;
    // [SerializeField] private float knockbackHorizontalForce = 3f;
    // [SerializeField] private float knockbackVerticalForce = 1.5f;
    private Rigidbody2D rb;

    private bool hit;
    private Vector2 knockbackForce;
    private float currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color faded;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        rb = this.GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        originalColor = spriteRenderer.color;
        faded = new Color(
            Mathf.Clamp01(originalColor.r + fadeAmount),
            Mathf.Clamp01(originalColor.g + fadeAmount),
            Mathf.Clamp01(originalColor.b + fadeAmount)
        );
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
            spriteRenderer.color = faded;
            hit = true;
            currentHealth -= amount;
            UpdateHealthBar();
            knockbackForce = direction * knockbackHorizontalForce + Vector2.up * knockbackVerticalForce;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Debug.Log("GameOver");
                GameMaster.Instance.ReloadLevel();
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
        spriteRenderer.color = originalColor;
    }

    private void UpdateHealthBar()
    {
        healthBar.value = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
