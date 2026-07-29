using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("References")]
    public HeartUI heartUI;
    public GameManager gameManager;

    [Header("Damage Settings")]
    public float invincibleTime = 1f;

    private bool canTakeDamage = true;

    private void Start()
    {
        currentHealth = maxHealth;

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }
    }

    public void TakeDamage()
    {
        if (!canTakeDamage || currentHealth <= 0)
            return;

        canTakeDamage = false;

        currentHealth--;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHealth);
        }

        if (currentHealth <= 0)
        {
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }
        else
        {
            StartCoroutine(DamageCooldown());
        }
    }

    private IEnumerator DamageCooldown()
    {
        yield return new WaitForSeconds(invincibleTime);

        canTakeDamage = true;
    }
}