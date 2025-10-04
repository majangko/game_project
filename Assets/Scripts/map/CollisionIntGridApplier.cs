using System.Linq;   // 문자열에서 숫자 추출용
using UnityEngine;
using UnityEngine.Tilemaps;

public class CollisionIntGridApplier : MonoBehaviour
{
    [Header("LDtk IntGrid Tilemap")]
    public Tilemap intGrid;   // LDtk IntGrid 레이어에서 연결

    [Header("Collision Maps")]
    public Tilemap solidMap;      // 벽/바닥
    public Tilemap platformMap;   // 플랫폼

    [Header("Collision Settings")]
    public TileBase solidTile;    // 벽, 바닥에 쓸 더미 타일
    public TileBase platformTile; // 플랫폼에 쓸 더미 타일

    /// <summary>
    /// IntGrid를 읽어서 충돌 타일로 변환하는 실행 함수
    /// </summary>
    [ContextMenu("Bake Collision From IntGrid")]
    public void Bake()
    {
        if (intGrid == null)
        {
            Debug.LogError("IntGrid Tilemap이 연결되지 않았습니다!");
            return;
        }

        // 기존 데이터 초기화
        solidMap.ClearAllTiles();
        platformMap.ClearAllTiles();

        BoundsInt bounds = intGrid.cellBounds;
        TileBase[] allTiles = intGrid.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                int index = x + y * bounds.size.x;
                TileBase tile = allTiles[index];
                if (tile == null) continue;

                // IntGrid에서 어떤 숫자인지 판별
                string tileName = tile.name;
                int value = ParseIntGridValue(tileName);

                Vector3Int pos = new Vector3Int(x + bounds.x, y + bounds.y, 0);

                switch (value)
                {
                    case 1: // 벽
                    case 2: // 바닥
                        solidMap.SetTile(pos, solidTile);
                        break;

                    case 3: // 플랫폼
                        platformMap.SetTile(pos, platformTile);
                        break;

                    case 4: // 시크릿 벽 (충돌 없음, 그림 전용)
                        // 아무것도 두지 않음
                        break;
                }
            }
        }

        Debug.Log("Collision Bake 완료!");
    }

    /// <summary>
    /// LDtk IntGrid 값에서 숫자만 추출
    /// "Collision 1", "Collision_2" → 1, 2
    /// </summary>
    private int ParseIntGridValue(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out int result))
            return result;

        return -1;
    }
}
