using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    [Header("Prefab & Layout")]
    public GameObject tilePrefab;
    public int tileCount = 20;
    public float radius = 8f;
    public Vector3 tileOffset = Vector3.zero;

    [Header("Generated tiles (Runtime)")]
    public List<Transform> tiles = new List<Transform>();

    [Header("Type Rules")]
    public int startReward = 200;   // Start 奖励
    public int taxAmount = 100;     // Tax 扣钱
    public int propertyPrice = 200; // 地产价格
    public int propertyRent = 50;   // 地产租金
    public int taxEveryN = 5;       // 每 N 格一个 Tax（例如 5：5/10/15）

    [Header("Colors")]
    public Color startColor = new Color(0.2f, 0.9f, 0.2f, 1f);   // 绿色
    public Color taxColor = new Color(0.95f, 0.25f, 0.25f, 1f);  // 红色
    public Color propertyColor = new Color(0.2f, 0.6f, 1f, 1f);  // 蓝色
    public Color emptyColor = new Color(0.85f, 0.85f, 0.85f, 1f);// 灰色

    private void Start()
    {
        // 运行时：先清旧，再生成
        ClearGeneratedTiles();
        GenerateTiles();
    }

#if UNITY_EDITOR
    // 编辑器里右键可手动生成/清理（不需要写工具脚本）
    [ContextMenu("Generate (Clear + Create)")]
    private void EditorGenerate()
    {
        ClearGeneratedTiles();
        GenerateTiles();
    }

    [ContextMenu("Clear Generated Tiles")]
    private void EditorClear()
    {
        ClearGeneratedTiles();
    }
#endif

    private void GenerateTiles()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("TileManager: tilePrefab is not assigned!");
            return;
        }

        tiles.Clear();

        for (int i = 0; i < tileCount; i++)
        {
            float angle = i * Mathf.PI * 2f / tileCount;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + tileOffset;

            GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            tileObj.name = $"Tile_{i:00}";

            // 确保有 Tile 组件
            Tile tile = tileObj.GetComponent<Tile>();
            if (tile == null)
            {
                Debug.LogError("TileManager: tilePrefab is missing Tile component!");
                Destroy(tileObj);
                continue;
            }

            // 这里如果你还保留 tileIndex 字段，也可以顺手设置
            // tile.tileIndex = i;  // 如果你的 Tile.cs 里还有 tileIndex 就取消注释

            // 类型分配规则
            if (i == 0)
            {
                tile.tileType = TileType.Start;
                tile.value = startReward;
            }
            else if (taxEveryN > 0 && i % taxEveryN == 0)
            {
                tile.tileType = TileType.Tax;
                tile.value = taxAmount;
            }
            else
            {
                tile.tileType = TileType.Property;
                tile.price = propertyPrice;
                tile.rent = propertyRent;
            }

            // 颜色设置
            ApplyColor(tile);

            tiles.Add(tileObj.transform);
        }
    }

    private void ApplyColor(Tile tile)
    {
        Renderer r = tile.GetComponent<Renderer>();
        if (r == null) return;

        // 注意：访问 material 会实例化材质（对小项目没问题）
        switch (tile.tileType)
        {
            case TileType.Start:
                r.material.color = startColor;
                break;
            case TileType.Tax:
                r.material.color = taxColor;
                break;
            case TileType.Property:
                r.material.color = propertyColor;
                break;
            default:
                r.material.color = emptyColor;
                break;
        }
    }

    private void ClearGeneratedTiles()
    {
        tiles.Clear();

        // 把 TileManager 下面的所有子物体删掉（这些就是旧棋盘格）
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

#if UNITY_EDITOR
            // 编辑模式用 DestroyImmediate
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }
}