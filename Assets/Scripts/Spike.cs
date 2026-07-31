using UnityEngine;

// Attach to any spike/obstacle GameObject.
// Requires a Collider2D on this object set to "Is Trigger".
public class Spike : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}