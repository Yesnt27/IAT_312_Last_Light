using UnityEngine;

// The area the camera may show in this scene. Resize it freely when the room art changes.
public class RoomBounds : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(20f, 10f);

    public Rect Rect => new Rect((Vector2)transform.position - size / 2f, size);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
