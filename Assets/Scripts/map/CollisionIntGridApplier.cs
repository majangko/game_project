using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class CollisionIntGridApplier : MonoBehaviour
{
    [Header("Source (LDtk IntGrid tilemap)")]
    public Tilemap intGridMap;               // LDtk가 만든 Collision(IntGrid) 타일맵

    [Header("Targets (generated)")]
    public Tilemap solidMap;                 // value==solidValue -> 여기에 타일을 찍어 콜라이더 생성
    public Tilemap platformMap;              // value==platformValue -> 여기에 타일을 찍어 발판 생성

    [Header("Values")]
    public int solidValue = 1;               // 벽/바닥
    public int platformValue = 2;            // 발판(One-way)

    [Header("Tiles used for baking (ColliderType=Grid)")]
    public TileBase solidTile;               // 비워두면 런타임 타일 자동 생성
    public TileBase platformTile;

    [ContextMenu("Bake From IntGrid")]
    public void Bake()
    {
        if (!intGridMap)
        {
            Debug.LogError("IntGrid tilemap is not assigned.", this);
            return;
        }

        AutoAssignTargetsIfMissing();

        // 타일 준비(없으면 코드로 생성)
        if (!solidTile)    solidTile    = CreateRuntimeTile("SolidTile");
        if (!platformTile) platformTile = CreateRuntimeTile("PlatformTile");

        // 타겟 초기화
        solidMap.ClearAllTiles();
        platformMap.ClearAllTiles();

        // IntGrid 순회
        var bounds = intGridMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            var p = new Vector3Int(x, y, 0);
            var t = intGridMap.GetTile(p);
            if (!t) continue;

            int v = ParseIntGridValue(t.name);
            if (v == solidValue)
                solidMap.SetTile(p, solidTile);
            else if (v == platformValue)
                platformMap.SetTile(p, platformTile);
        }

        SetupSolidColliders();
        SetupPlatformColliders();

        Debug.Log("Collision bake complete.", this);
    }

    // ===== Helpers =====
    int ParseIntGridValue(string tileName)
    {
        // "1","2","3" 또는 "Value_1" 같은 이름을 숫자로 파싱
        if (int.TryParse(tileName, out int v)) return v;

        for (int i = tileName.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(tileName[i]))
            {
                if (i < tileName.Length - 1 && int.TryParse(tileName[(i + 1)..], out v))
                    return v;
                break;
            }
        }
        return -1;
    }

    TileBase CreateRuntimeTile(string name)
    {
        var t = ScriptableObject.CreateInstance<Tile>();
        t.name = name;
        ((Tile)t).colliderType = Tile.ColliderType.Grid; // 셀 전체 충돌
        return t;
    }

    void AutoAssignTargetsIfMissing()
    {
        if (!solidMap)
        {
            var go = FindOrCreateChild("SolidMap");
            solidMap = EnsureTilemap(go);
        }
        if (!platformMap)
        {
            var go = FindOrCreateChild("PlatformMap");
            platformMap = EnsureTilemap(go);
        }
    }

    GameObject FindOrCreateChild(string name)
    {
        var t = transform.Find(name);
        if (t) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go;
    }

    Tilemap EnsureTilemap(GameObject go)
    {
        // 보통 상위에 Grid가 이미 있음. 없으면 추가
        if (!GetComponentInParent<Grid>())
            gameObject.AddComponent<Grid>();

        var tm = go.GetComponent<Tilemap>();
        if (!tm) tm = go.AddComponent<Tilemap>();
        var r = go.GetComponent<TilemapRenderer>();
        if (!r) r = go.AddComponent<TilemapRenderer>();
        r.sortingLayerName = "Default";
        r.sortingOrder = 0;
        return tm;
    }

    void SetupSolidColliders()
    {
        var rb = solidMap.GetComponent<Rigidbody2D>();
        if (!rb) rb = solidMap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        var col = solidMap.GetComponent<TilemapCollider2D>();
        if (!col) col = solidMap.gameObject.AddComponent<TilemapCollider2D>();
#if UNITY_2022_2_OR_NEWER
        col.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
        col.usedByComposite = true;
#endif

        var comp = solidMap.GetComponent<CompositeCollider2D>();
        if (!comp) comp = solidMap.gameObject.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;
    }

    void SetupPlatformColliders()
    {
        var rb = platformMap.GetComponent<Rigidbody2D>();
        if (!rb) rb = platformMap.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        var col = platformMap.GetComponent<TilemapCollider2D>();
        if (!col) col = platformMap.gameObject.AddComponent<TilemapCollider2D>();
#if UNITY_2022_2_OR_NEWER
        col.compositeOperation = Collider2D.CompositeOperation.None; // 개별 셀 유지
#else
        col.usedByComposite = false;
#endif
        col.usedByEffector = true;

        var eff = platformMap.GetComponent<PlatformEffector2D>();
        if (!eff) eff = platformMap.gameObject.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 180f;
    }
}
