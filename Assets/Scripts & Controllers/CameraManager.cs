using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject player; // Reference to the player GameObject

    private Camera cam;
    private RoomBounds bounds;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
        bounds = FindAnyObjectByType<RoomBounds>();
        SnapToPlayer();
    }

    private void LateUpdate()
    {
        SnapToPlayer();
    }

    public void SnapToPlayer()
    {
        if (player == null) return;

        Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        transform.position = ClampToBounds(target);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (bounds == null) bounds = FindAnyObjectByType<RoomBounds>();
        if (bounds == null || cam == null || !cam.orthographic) return position;

        Rect rect = bounds.Rect;
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        position.x = rect.width > halfWidth * 2f ? Mathf.Clamp(position.x, rect.xMin + halfWidth, rect.xMax - halfWidth) : rect.center.x;
        position.y = rect.height > halfHeight * 2f ? Mathf.Clamp(position.y, rect.yMin + halfHeight, rect.yMax - halfHeight) : rect.center.y;
        return position;
    }
}
