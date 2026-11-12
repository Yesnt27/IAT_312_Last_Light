using System.Collections.Generic;
using UnityEngine;

// One lightning bolt, built the way Gabriel Aguiar's "Lightning Strike" tutorial builds it: a zigzag ribbon mesh,
// thick at the top and thin at the bottom, drawn twice with the Lighthouse/Lightning erode shader: Bright (the painted
// bolt) over Blur (its soft glow). The mesh is made fresh for every strike, so no two bolts look the same.
// Over a strike's life the colour gradients make it blink once and then flare, Clip erodes it in and out, Scroll slides
// the texture down the bolt, and the Flash light lights the sea for a moment. LightningStorm calls Strike().
public class LightningStrike : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int ClipId = Shader.PropertyToID("_Clip");
    private static readonly int MainTexStId = Shader.PropertyToID("_MainTex_ST");

    [Header("Bolt Shape")]
    [SerializeField] private int segments = 16;
    [Tooltip("How far the bolt wanders sideways, as a part of its length.")]
    [SerializeField] private float zigzag = 0.05f;
    [Tooltip("Width of the ribbon in metres: X at the top, Y at the bottom. The bolt itself is a thin line down its middle.")]
    [SerializeField] private Vector2 width = new Vector2(90f, 40f);
    [SerializeField] private int branches = 2;
    [Tooltip("How many times the texture repeats along the bolt.")]
    [SerializeField] private float textureRepeats = 1.5f;

    [Header("Over The Strike's Life (left = start, right = end)")]
    [Tooltip("Seconds one strike lasts.")]
    [SerializeField] private float lifetime = 1f;
    [Tooltip("The glow lingers a little longer than the bolt.")]
    [SerializeField] private float blurLifetime = 1.2f;
    [Tooltip("The bolt's colour: a small blink at the start, dark, then the flare, fading to black.")]
    [SerializeField] private Gradient brightColor = new Gradient();
    [Tooltip("The bolt's colour is multiplied by this (above 1 = a white-hot core).")]
    [SerializeField] private float brightIntensity = 6f;
    [SerializeField] private Gradient blurColor = new Gradient();
    [Tooltip("The glow's colour is multiplied by this.")]
    [SerializeField] private float blurIntensity = 2.5f;
    [Tooltip("0 = the whole bolt, 1 = eroded away.")]
    [SerializeField] private AnimationCurve clip = new AnimationCurve();
    [Tooltip("How far the texture has slid down the bolt (1 = one whole texture).")]
    [SerializeField] private AnimationCurve scroll = new AnimationCurve();

    [Header("Flash")]
    [Tooltip("Lights the sea and the ships while the bolt is bright. Empty = no flash.")]
    [SerializeField] private Light flash;
    [Tooltip("The flash light's intensity at the bolt's brightest moment.")]
    [SerializeField] private float flashIntensity = 1.2f;

    [Header("References")]
    [SerializeField] private MeshRenderer bright;
    [SerializeField] private MeshRenderer blur;

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<Color> colors = new List<Color>();
    private readonly List<int> triangles = new List<int>();
    private Mesh mesh;
    private MaterialPropertyBlock block;
    private float age = -1f;
    private float peakGlow;

    public bool IsStriking => age >= 0f;

    // The tutorial's curves, so a new component (or Reset in the Inspector) starts with a working bolt.
    private void Reset()
    {
        Color blue = new Color(0.45f, 0.65f, 1f);
        GradientColorKey[] colorKeys = { new GradientColorKey(blue * 0.4f, 0f), new GradientColorKey(Color.black, 0.12f), new GradientColorKey(blue, 0.56f), new GradientColorKey(Color.black, 1f) };
        GradientAlphaKey[] alphaKeys = { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.03f), new GradientAlphaKey(0f, 0.45f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(1f, 1f) };
        brightColor = new Gradient();
        brightColor.SetKeys(colorKeys, alphaKeys);
        blurColor = new Gradient();
        blurColor.SetKeys(colorKeys, alphaKeys);

        clip = new AnimationCurve(new Keyframe(0f, 0.85f), new Keyframe(0.3f, 0f, 0f, 0f), new Keyframe(0.55f, 0f, 0f, 0f), new Keyframe(1f, 1f));
        scroll = new AnimationCurve(new Keyframe(0.3f, 0f, 0f, 0f), new Keyframe(0.6f, 0.4f), new Keyframe(1f, 0.5f, 0f, 0f));
    }

    private void Awake()
    {
        mesh = new Mesh { name = "Lightning Bolt" };
        mesh.MarkDynamic();
        bright.GetComponent<MeshFilter>().sharedMesh = mesh;
        blur.GetComponent<MeshFilter>().sharedMesh = mesh;
        block = new MaterialPropertyBlock();

        for (int i = 0; i <= 64; i++) peakGlow = Mathf.Max(peakGlow, Glow(brightColor.Evaluate(i / 64f)));
        Hide();
    }

    private void OnDestroy()
    {
        Destroy(mesh);
    }

    private void Update()
    {
        if (age < 0f) return;

        age += Time.deltaTime;
        if (age >= Mathf.Max(lifetime, blurLifetime))
        {
            Hide();
            return;
        }

        float life = age / lifetime;
        Show(bright, brightColor, brightIntensity, life, clip.Evaluate(life), scroll.Evaluate(life));
        Show(blur, blurColor, blurIntensity, age / blurLifetime, 0f, 0f);

        if (flash == null) return;
        flash.enabled = life < 1f;
        flash.intensity = life < 1f && peakGlow > 0f ? flashIntensity * Glow(brightColor.Evaluate(life)) / peakGlow : 0f;
    }

    // A new bolt from top to bottom, turned to face the viewer.
    public void Strike(Vector3 top, Vector3 bottom, Vector3 viewer)
    {
        BuildMesh(top, bottom, viewer);
        if (flash != null) flash.transform.rotation = Quaternion.LookRotation(viewer - Vector3.Lerp(top, bottom, 0.5f));
        Flash();
    }

    // The same bolt again (the tutorial's second burst: a strike that blinks twice).
    public void Flash()
    {
        age = 0f;
    }

    private void Hide()
    {
        age = -1f;
        bright.enabled = false;
        blur.enabled = false;
        if (flash != null) flash.enabled = false;
    }

    private void Show(MeshRenderer target, Gradient gradient, float intensity, float life, float clipAmount, float scrollAmount)
    {
        target.enabled = life < 1f;
        if (life >= 1f) return;

        Color color = gradient.Evaluate(life);
        block.SetColor(ColorId, new Color(color.r * intensity, color.g * intensity, color.b * intensity, color.a));
        block.SetFloat(ClipId, clipAmount);
        block.SetVector(MainTexStId, new Vector4(1f, 1f, 0f, -scrollAmount));
        target.SetPropertyBlock(block);
    }

    private static float Glow(Color color)
    {
        return color.maxColorComponent * color.a;
    }

    private void BuildMesh(Vector3 top, Vector3 bottom, Vector3 viewer)
    {
        vertices.Clear();
        uvs.Clear();
        colors.Clear();
        triangles.Clear();

        // Bright and Blur sit at zero under this object, so the mesh is built in this object's space.
        top = transform.InverseTransformPoint(top);
        bottom = transform.InverseTransformPoint(bottom);
        viewer = transform.InverseTransformPoint(viewer);

        Vector3 down = bottom - top;
        float length = down.magnitude;
        Vector3 across = Vector3.Cross(down, viewer - top).normalized;
        float repeatsPerMetre = textureRepeats / length;

        List<Vector3> path = AddRibbon(top, bottom, across, width.x, width.y, segments, length * zigzag, 0f, repeatsPerMetre, false);
        for (int i = 0; i < branches; i++)
        {
            int joint = Random.Range(segments / 4, segments * 3 / 4);
            float jointAlong = joint / (float)segments;
            float side = Random.value < 0.5f ? -1f : 1f;
            float branchLength = length * Random.Range(0.2f, 0.35f);
            Vector3 end = path[joint] + (down / length + across * side * Random.Range(0.5f, 0.9f)).normalized * branchLength;
            float branchWidth = Mathf.Lerp(width.x, width.y, jointAlong) * 0.6f;
            AddRibbon(path[joint], end, across, branchWidth, branchWidth * 0.4f, Mathf.Max(2, segments / 2), branchLength * zigzag * 1.5f,
                jointAlong * textureRepeats, repeatsPerMetre, true);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    // A zigzag strip from start to end. Returns its joints. fadeOut = the far end fades away (branches).
    private List<Vector3> AddRibbon(Vector3 start, Vector3 end, Vector3 across, float startWidth, float endWidth, int steps, float wander,
        float startV, float repeatsPerMetre, bool fadeOut)
    {
        List<Vector3> path = new List<Vector3>(steps + 1);
        float ribbonLength = Vector3.Distance(start, end);
        float offset = 0f;
        for (int i = 0; i <= steps; i++)
        {
            float along = i / (float)steps;
            if (i > 0) offset = Mathf.Clamp(offset + Random.Range(-wander, wander), -wander, wander);
            Vector3 joint = Vector3.Lerp(start, end, along) + across * offset;
            Vector3 half = across * (Mathf.Lerp(startWidth, endWidth, along) * 0.5f);
            float v = startV + along * ribbonLength * repeatsPerMetre;
            Color color = new Color(1f, 1f, 1f, fadeOut ? 1f - along : 1f);

            path.Add(joint);
            vertices.Add(joint - half);
            vertices.Add(joint + half);
            uvs.Add(new Vector2(0f, v));
            uvs.Add(new Vector2(1f, v));
            colors.Add(color);
            colors.Add(color);
            if (i == 0) continue;

            int corner = vertices.Count - 4;
            triangles.Add(corner);
            triangles.Add(corner + 1);
            triangles.Add(corner + 2);
            triangles.Add(corner + 1);
            triangles.Add(corner + 3);
            triangles.Add(corner + 2);
        }
        return path;
    }
}
