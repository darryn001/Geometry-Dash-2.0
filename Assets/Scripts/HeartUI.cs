using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [Header("Heart Images")]
    public Image[] hearts;

    public void UpdateHearts(int health)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].enabled = (i < health);
            }
        }
    }

    public void ResetHearts()
    {
        UpdateHearts(hearts.Length);
    }
}