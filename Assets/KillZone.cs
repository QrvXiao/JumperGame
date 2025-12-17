using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZone : MonoBehaviour
{
    void Reset()
    {
        // Ensure collider is trigger by default when first added
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other) return;
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }
            else
            {
                Debug.LogWarning("GameManager.Instance is null — disabling player as fallback.");
                other.gameObject.SetActive(false);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(col.offset, col.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(col.offset, col.size);
        Gizmos.matrix = old;
    }
}
