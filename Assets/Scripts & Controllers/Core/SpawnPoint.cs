using UnityEngine;

// Where the player appears in this scene. Doors point at a spawn point by its id.
// Tick "Is Default" on the one to use when you press Play directly in this scene.
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId;
    [SerializeField] private bool isDefault;

    public string SpawnId => spawnId;

    public static SpawnPoint Find(string id)
    {
        foreach (SpawnPoint point in FindObjectsByType<SpawnPoint>())
        {
            if (point.spawnId == id) return point;
        }
        return null;
    }

    // The point marked Is Default, or any spawn point if none is marked. Null if the scene has none.
    public static SpawnPoint FindDefault()
    {
        SpawnPoint[] points = FindObjectsByType<SpawnPoint>();
        foreach (SpawnPoint point in points)
        {
            if (point.isDefault) return point;
        }
        return points.Length > 0 ? points[0] : null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isDefault ? Color.green : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
    }
}
