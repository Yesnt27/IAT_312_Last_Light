using UnityEngine;

// A visible shaft of light for a Spot Light. URP has no volumetric light, so this fakes it with a soft
// cone mesh drawn by the additive "Lighthouse/Light Beam" shader. Put it on a child of the Spot Light:
// it turns with the light and follows its colour and Spot Angle. Builds its mesh in the editor too.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LightBeam : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int StrengthId = Shader.PropertyToID("_Strength");

    [Header("References")]
    [SerializeField] private Light spotLight;

    [Header("Shape")]
    [Tooltip("How far the visible beam reaches, in metres.")]
    [SerializeField] private float length = 600f;
    [SerializeField] private int segments = 40;

    [Header("Look")]
    [Tooltip("How bright the beam looks in the air (doesn't change how much it lights).")]
    [SerializeField] private float strength = 0.35f;

    private Mesh mesh;
    private float builtAngle = -1f;
    private float builtLength = -1f;
    private MaterialPropertyBlock block;

    private void OnDisable()
    {
        if (mesh != null) DestroyImmediate(mesh);
        mesh = null;
        builtAngle = -1f;
    }

    private void Update()
    {
        if (spotLight == null) return;
        if (mesh == null || builtAngle != spotLight.spotAngle || builtLength != length) Build();

        if (block == null) block = new MaterialPropertyBlock();
        block.SetColor(ColorId, spotLight.color);
        block.SetFloat(StrengthId, spotLight.isActiveAndEnabled ? strength : 0f);
        GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    // A cone along +Z: a tiny ring at the light, a wide ring at Length. UV.y = 0 at the light, 1 at the end.
    private void Build()
    {
        builtAngle = spotLight.spotAngle;
        builtLength = length;
        float radius = Mathf.Tan(builtAngle * 0.5f * Mathf.Deg2Rad) * length;
        int count = Mathf.Max(3, segments);

        Vector3[] vertices = new Vector3[(count + 1) * 2];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i <= count; i++)
        {
            float angle = i / (float)count * Mathf.PI * 2f;
            Vector3 around = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 normal = (around - Vector3.forward * (radius / length)).normalized;
            vertices[i * 2] = around * 0.2f;
            vertices[i * 2 + 1] = around * radius + Vector3.forward * length;
            normals[i * 2] = normals[i * 2 + 1] = normal;
            uvs[i * 2] = new Vector2(i / (float)count, 0f);
            uvs[i * 2 + 1] = new Vector2(i / (float)count, 1f);
        }

        int[] triangles = new int[count * 6];
        for (int i = 0; i < count; i++)
        {
            int a = i * 2;
            triangles[i * 6] = a;
            triangles[i * 6 + 1] = a + 2;
            triangles[i * 6 + 2] = a + 1;
            triangles[i * 6 + 3] = a + 1;
            triangles[i * 6 + 4] = a + 2;
            triangles[i * 6 + 5] = a + 3;
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Light Beam", hideFlags = HideFlags.DontSave };
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
