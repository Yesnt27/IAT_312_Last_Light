using UnityEngine;
using UnityEngine.Rendering;

// Builds the ocean's flat grid mesh, dense near the middle and sparse towards the horizon, so the
// waves from the Lighthouse/Ocean shader look detailed near the tower. Runs in edit mode too.
// The mesh is rebuilt on load (it isn't saved in the scene); change the numbers to resize it.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OceanSurface : MonoBehaviour
{
    [Header("Size")]
    [SerializeField] private float halfSize = 3000f;
    [SerializeField, Range(16, 300)] private int resolution = 240;
    [Tooltip("Higher = more detail near the middle, less towards the edge.")]
    [SerializeField, Range(1f, 4f)] private float centreDetail = 2.6f;

    private Mesh mesh;
    private bool needsRebuild = true;

    private void OnEnable()
    {
        needsRebuild = true;
        Rebuild();
    }

    private void OnValidate()
    {
        needsRebuild = true;
    }

    private void Update()
    {
        if (needsRebuild) Rebuild();
    }

    private void OnDestroy()
    {
        if (mesh == null) return;
        if (Application.isPlaying) Destroy(mesh);
        else DestroyImmediate(mesh);
    }

    private void Rebuild()
    {
        needsRebuild = false;
        if (mesh == null)
        {
            mesh = new Mesh { name = "Ocean (generated)", hideFlags = HideFlags.DontSave };
        }

        int side = resolution + 1;
        var vertices = new Vector3[side * side];
        for (int z = 0; z < side; z++)
        {
            for (int x = 0; x < side; x++)
            {
                vertices[z * side + x] = new Vector3(Spread(x / (float)resolution), 0f, Spread(z / (float)resolution)) * halfSize;
            }
        }

        var triangles = new int[resolution * resolution * 6];
        int t = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * side + x;
                triangles[t++] = i;
                triangles[t++] = i + side;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + side;
                triangles[t++] = i + side + 1;
            }
        }

        mesh.Clear();
        mesh.indexFormat = vertices.Length > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // Waves move the surface up and down in the shader, so give culling some headroom.
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(halfSize * 2f, 20f, halfSize * 2f));
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    // Maps 0..1 to -1..1 with more points packed near the middle.
    private float Spread(float t)
    {
        float centred = t * 2f - 1f;
        return Mathf.Sign(centred) * Mathf.Pow(Mathf.Abs(centred), centreDetail);
    }
}
