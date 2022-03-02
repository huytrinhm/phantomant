using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private bool damageable = true;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float invulnerabilityTime = .2f;
    [SerializeField] [Range(0, 1)] private float fadeAmount = .2f;
    [SerializeField] private Slider healthBar;

    public bool giveUpwardForce = true;
    private bool hit;
    private float currentHealth;

    private SpriteRenderer spriteRenderer;
    private Color faded;
    private Color originalColor;

    private void Awake()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
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
    }

    public void Damage(int amount)
    {
        if (damageable && !hit && currentHealth > 0)
        {
            spriteRenderer.color = faded;
            hit = true;
            currentHealth -= amount;
            UpdateHealthBar();
            if (currentHealth <= 0)
            {
                currentHealth = 0;   
                gameObject.SetActive(false);
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
