using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CanvasRenderer))]
public class UIProceduralRoundedRect : MaskableGraphic
{
    [Header("Shape")]
    [Range(0, 100)]
    public float cornerRadius = 20f;

    [Range(4, 32)]
    public int cornerResolution = 8;

    [Header("Texture")]
    public Sprite sprite;

    public override Texture mainTexture
    {
        get
        {
            if (sprite == null)
                return s_WhiteTexture;

            return sprite.texture;
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;
        Vector2 size = rect.size;

        float radius = Mathf.Min(
            cornerRadius,
            Mathf.Min(size.x, size.y) * 0.5f
        );

        if (radius <= 0)
        {
            AddQuad(vh, rect.min, rect.max, rect);
            return;
        }

        // Inner bounds
        Vector2 innerMin = rect.min + new Vector2(radius, radius);
        Vector2 innerMax = rect.max - new Vector2(radius, radius);

        // Center and side areas
        AddQuad(
            vh,
            new Vector2(rect.xMin, innerMin.y),
            new Vector2(rect.xMax, innerMax.y),
            rect
        );

        AddQuad(
            vh,
            new Vector2(innerMin.x, rect.yMin),
            new Vector2(innerMax.x, innerMin.y),
            rect
        );

        AddQuad(
            vh,
            new Vector2(innerMin.x, innerMax.y),
            new Vector2(innerMax.x, rect.yMax),
            rect
        );

        // Rounded corners
        AddCorner(
            vh,
            new Vector2(innerMax.x, innerMax.y),
            radius,
            0,
            rect
        );

        AddCorner(
            vh,
            new Vector2(innerMin.x, innerMax.y),
            radius,
            90,
            rect
        );

        AddCorner(
            vh,
            new Vector2(innerMin.x, innerMin.y),
            radius,
            180,
            rect
        );

        AddCorner(
            vh,
            new Vector2(innerMax.x, innerMin.y),
            radius,
            270,
            rect
        );
    }

    // =====================================================
    // QUAD
    // =====================================================

    private void AddQuad(
        VertexHelper vh,
        Vector2 min,
        Vector2 max,
        Rect fullRect
    )
    {
        int startIndex = vh.currentVertCount;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        Vector2[] positions =
        {
            new Vector2(min.x, min.y),
            new Vector2(min.x, max.y),
            new Vector2(max.x, max.y),
            new Vector2(max.x, min.y)
        };

        for (int i = 0; i < 4; i++)
        {
            vertex.position = positions[i];

            // Convert local position to UV coordinates
            vertex.uv0 = GetUV(
                positions[i],
                fullRect
            );

            vh.AddVert(vertex);
        }

        vh.AddTriangle(
            startIndex,
            startIndex + 1,
            startIndex + 2
        );

        vh.AddTriangle(
            startIndex,
            startIndex + 2,
            startIndex + 3
        );
    }

    // =====================================================
    // ROUNDED CORNERS
    // =====================================================

    private void AddCorner(
        VertexHelper vh,
        Vector2 center,
        float radius,
        float startAngle,
        Rect fullRect
    )
    {
        int centerIndex = vh.currentVertCount;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = center;
        vertex.uv0 = GetUV(center, fullRect);

        vh.AddVert(vertex);

        int previousIndex = -1;

        float angleStep =
            90f / cornerResolution;

        for (int i = 0; i <= cornerResolution; i++)
        {
            float angle =
                startAngle +
                (i * angleStep);

            float radians =
                angle * Mathf.Deg2Rad;

            Vector2 position =
                center +
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                ) * radius;

            vertex.position = position;

            vertex.uv0 =
                GetUV(position, fullRect);

            vh.AddVert(vertex);

            int currentIndex =
                vh.currentVertCount - 1;

            if (i > 0)
            {
                vh.AddTriangle(
                    centerIndex,
                    previousIndex,
                    currentIndex
                );
            }

            previousIndex = currentIndex;
        }
    }

    // =====================================================
    // UV CALCULATION
    // =====================================================

    private Vector2 GetUV(
        Vector2 position,
        Rect rect
    )
    {
        float u =
            Mathf.InverseLerp(
                rect.xMin,
                rect.xMax,
                position.x
            );

        float v =
            Mathf.InverseLerp(
                rect.yMin,
                rect.yMax,
                position.y
            );

        return new Vector2(u, v);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
        SetMaterialDirty();
    }
#endif
}