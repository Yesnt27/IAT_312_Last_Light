using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId;

    public string SpawnId => spawnId;

    public static SpawnPoint Find(string id)
    {
        foreach (SpawnPoint point in FindObjectsByType<SpawnPoint>())
        {
            if (point.spawnId == id) return point;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
    }
}
