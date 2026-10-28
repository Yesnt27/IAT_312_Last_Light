using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Draws a coloured outline around every mesh under this object, the outline-shader way: the mesh is
// drawn again, pushed out along its normals. Each mesh gets two copies: "Outline Mask" marks where the
// model is, and "Outline Fill" draws the pushed-out copy only outside that, so just the rim shows.
// Width is in screen pixels, so the outline stays the same thickness at any distance or zoom.
public class ModelOutline : MonoBehaviour
{
    private static readonly int ColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int WidthId = Shader.PropertyToID("_OutlineWidth");

    [Header("Materials")]
    [SerializeField] private Material maskMaterial;
    [SerializeField] private Material fillMaterial;

    [Header("Look")]
    [SerializeField] private float width = 3f;

    private readonly List<GameObject> copies = new List<GameObject>();
    private readonly List<Renderer> fills = new List<Renderer>();
    private MaterialPropertyBlock block;

    public void Show(Color color)
    {
        if (block == null) Build();

        block.SetColor(ColorId, color);
        block.SetFloat(WidthId, width);
        foreach (Renderer fill in fills) fill.SetPropertyBlock(block);
        foreach (GameObject copy in copies) copy.SetActive(true);
    }

    public void Hide()
    {
        foreach (GameObject copy in copies) copy.SetActive(false);
    }

    // The copies are made the first time the outline shows.
    private void Build()
    {
        block = new MaterialPropertyBlock();
        foreach (MeshFilter source in GetComponentsInChildren<MeshFilter>())
        {
            if (source.sharedMesh == null) continue;
            AddCopy(source, "Outline Mask", maskMaterial);
            fills.Add(AddCopy(source, "Outline Fill", fillMaterial));
        }
    }

    private Renderer AddCopy(MeshFilter source, string copyName, Material material)
    {
        GameObject copy = new GameObject(copyName);
        copy.layer = source.gameObject.layer;
        copy.transform.SetParent(source.transform, false);
        copy.AddComponent<MeshFilter>().sharedMesh = source.sharedMesh;

        Material[] materials = new Material[source.sharedMesh.subMeshCount];
        for (int i = 0; i < materials.Length; i++) materials[i] = material;

        MeshRenderer copyRenderer = copy.AddComponent<MeshRenderer>();
        copyRenderer.sharedMaterials = materials;
        copyRenderer.shadowCastingMode = ShadowCastingMode.Off;
        copyRenderer.receiveShadows = false;
        copies.Add(copy);
        return copyRenderer;
    }
}
