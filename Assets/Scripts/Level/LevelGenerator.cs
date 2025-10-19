using System.Globalization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.Cinemachine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Grid")]
    public int width = 120;
    public int height = 80;

    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap overlayTilemap;

    [Header("Tiles")]
    public TileBase[] floorVariants; // random floor variants
    public TileBase[] wallVariants4Bit; // length 16 mapping by 4-bit mask
    public TileBase doorTile;

    [Header("Rooms")]
    public List<RoomTemplate> handcraftedTemplates;
    public int maxRooms = 24;
    public int minRoomSize = 5;
    public int maxRoomSize = 12;
    public int placementAttempts = 500;

    [Header("Seed")]
    public int seed = 12345;

    // internal
    private int[,] grid; // 0 empty, 1 floor, 2 wall, 3 door
    private List<Room> rooms;
    private System.Random rng;

    [Header("Player")]
    public GameObject playerPrefab;
    private GameObject playerInstance;

    [Header("Player")]
    public CinemachineCamera vcam;

    [ContextMenu("Regenerate Level")]
    public void RegenerateInEditor()
    {
        GenerateLevel(seed);
    }

    void Start()
    {
        GenerateLevel(seed);
    }

    public void GenerateLevel(int seed)
    {
        this.seed = seed;
        rng = new System.Random(seed);
        grid = new int[width, height];
        rooms = new List<Room>();
        ClearTilemaps();

        // 1) Place handcrafted rooms first
        PlaceHandcraftedRooms();

        // 2) Place procedural rooms
        PlaceProceduralRooms();

        // 3) Connect rooms (MST + loops)
        var edges = BuildRoomGraphAndConnect();

        // 4) Carve corridors for each selected edge
        foreach (var e in edges)
        {
            CarveCorridor(rooms[e.a].center, rooms[e.b].center);
        }

        // 5) Build walls around floors
        BuildWalls();

        // 6) Autotile walls (4-bit example)
        ApplyAutotileWalls();

        // Pick a random room to start
        UnityEngine.Random.InitState(seed);
        Room startRoom = rooms[UnityEngine.Random.Range(0, rooms.Count)];
        SpawnPlayer(startRoom);

        // Vector3 worldPos = grid.CellToWorld((Vector3Int)startRoom.center);
        // worldPos += new Vector3(0.5f, 0.5f, 0f); // center on tile

        // 7) Paint floor/walls into Tilemaps
        PaintTilemaps();

        if (vcam != null)
        {
            vcam.Follow = playerInstance.transform;
        }

        // 8) Place doors / spawn points (simple example)
        PlaceDoors();

        // 9) Validate connectivity (BFS)
        if (!ValidateConnectivity())
        {
            Debug.LogWarning("Generated level not fully connected for seed: " + seed);
        }
    }

    void ClearTilemaps()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        overlayTilemap.ClearAllTiles();
    }

    #region Room Placement
    void PlaceHandcraftedRooms()
    {
        // naive placement: try to place each template at random top-left pos within bounds
        foreach (var tpl in handcraftedTemplates)
        {
            bool placed = false;
            for (int t = 0; t < placementAttempts; t++)
            {
                int x = rng.Next(1, width - tpl.width - 1);
                int y = rng.Next(1, height - tpl.height - 1);
                var rect = new RectInt(x, y, tpl.width, tpl.height);
                if (!IsOverlapping(rect))
                {
                    PlaceTemplate(tpl, rect.xMin, rect.yMin);
                    rooms.Add(new Room(rect, true));
                    placed = true;
                    break;
                }
            }
            if (!placed)
            {
                Debug.Log("Could not place handcrafted template: " + tpl.name);
            }
        }
    }

    void PlaceProceduralRooms()
    {
        int tries = 0;
        while (rooms.Count < maxRooms && tries < placementAttempts)
        {
            tries++;
            int rw = rng.Next(minRoomSize, maxRoomSize + 1);
            int rh = rng.Next(minRoomSize, maxRoomSize + 1);
            int rx = rng.Next(1, width - rw - 1);
            int ry = rng.Next(1, height - rh - 1);
            var rect = new RectInt(rx, ry, rw, rh);
            if (!IsOverlapping(rect))
            {
                CarveRoom(rect);
                rooms.Add(new Room(rect, false));
            }
        }
    }

    bool IsOverlapping(RectInt r)
    {
        // check a 1-tile buffer around to prevent back-to-back rooms
        int x0 = Mathf.Max(0, r.xMin - 1);
        int x1 = Mathf.Min(width - 1, r.xMax + 1);
        int y0 = Mathf.Max(0, r.yMin - 1);
        int y1 = Mathf.Min(height - 1, r.yMax + 1);
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                if (grid[x, y] != 0)
                    return true;
        return false;
    }

    void PlaceTemplate(RoomTemplate tpl, int originX, int originY)
    {
        for (int tx = 0; tx < tpl.width; tx++)
        {
            for (int ty = 0; ty < tpl.height; ty++)
            {
                var tile = tpl.GetTile(tx, ty);
                if (tile != null)
                {
                    int gx = originX + tx;
                    int gy = originY + ty;
                    if (InBounds(gx, gy))
                        grid[gx, gy] = 1;
                }
            }
        }
    }

    void CarveRoom(RectInt r)
    {
        for (int x = r.xMin; x < r.xMax; x++)
            for (int y = r.yMin; y < r.yMax; y++)
                grid[x, y] = 1;
    }
    #endregion

    #region Graph + MST
    struct Edge
    {
        public int a,
            b;
        public int dist;
    }

    List<Edge> BuildRoomGraphAndConnect()
    {
        int n = rooms.Count;
        var edges = new List<Edge>();
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                int dx = rooms[i].center.x - rooms[j].center.x;
                int dy = rooms[i].center.y - rooms[j].center.y;
                int d = dx * dx + dy * dy; // squared distance is fine
                edges.Add(
                    new Edge
                    {
                        a = i,
                        b = j,
                        dist = d
                    }
                );
            }
        // Kruskal
        edges.Sort((e1, e2) => e1.dist.CompareTo(e2.dist));
        var uf = new UnionFind(n);
        var result = new List<Edge>();
        foreach (var e in edges)
        {
            if (uf.Find(e.a) != uf.Find(e.b))
            {
                uf.Union(e.a, e.b);
                result.Add(e);
            }
        }
        // add some random extra edges to create loops
        int extra = Mathf.Max(1, n / 4);
        var remaining = edges.Except(result).ToList();
        for (int i = 0; i < extra && remaining.Count > 0; i++)
        {
            int idx = rng.Next(remaining.Count);
            result.Add(remaining[idx]);
            remaining.RemoveAt(idx);
        }
        return result;
    }

    class UnionFind
    {
        int[] parent;

        public UnionFind(int n)
        {
            parent = new int[n];
            for (int i = 0; i < n; i++)
                parent[i] = i;
        }

        public int Find(int a)
        {
            if (parent[a] == a)
                return a;
            parent[a] = Find(parent[a]);
            return parent[a];
        }

        public void Union(int a, int b)
        {
            parent[Find(a)] = Find(b);
        }
    }
    #endregion

    #region Corridors, walls, autotile, paint
    void CarveCorridor(Vector2Int a, Vector2Int b)
    {
        int x = a.x;
        int y = a.y;
        // choose random order to vary shape
        if (rng.NextDouble() < 0.5)
        {
            // horizontal then vertical
            while (x != b.x)
            {
                grid[x, y] = 1;
                x += Math.Sign(b.x - x);
            }
            while (y != b.y)
            {
                grid[x, y] = 1;
                y += Math.Sign(b.y - y);
            }
        }
        else
        {
            while (y != b.y)
            {
                grid[x, y] = 1;
                y += Math.Sign(b.y - y);
            }
            while (x != b.x)
            {
                grid[x, y] = 1;
                x += Math.Sign(b.x - x);
            }
        }
    }

    void BuildWalls()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (grid[x, y] == 0)
                {
                    // check if adjacent to floor -> becomes wall
                    if (IsNeighborFloor(x, y))
                        grid[x, y] = 2;
                }
            }
        }
    }

    bool IsNeighborFloor(int x, int y)
    {
        return (
            GetGrid(x + 1, y) == 1
            || GetGrid(x - 1, y) == 1
            || GetGrid(x, y + 1) == 1
            || GetGrid(x, y - 1) == 1
        );
    }

    void ApplyAutotileWalls()
    {
        // 4-bit mapping (Up=1,Right=2,Down=4,Left=8)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == 2)
                {
                    int mask = 0;
                    if (GetGrid(x, y + 1) == 2 || GetGrid(x, y + 1) == 1)
                        mask |= 1; // up
                    if (GetGrid(x + 1, y) == 2 || GetGrid(x + 1, y) == 1)
                        mask |= 2; // right
                    if (GetGrid(x, y - 1) == 2 || GetGrid(x, y - 1) == 1)
                        mask |= 4; // down
                    if (GetGrid(x - 1, y) == 2 || GetGrid(x - 1, y) == 1)
                        mask |= 8; // left
                    // store mask in overlay (or just paint when painting)
                    // We'll paint later using wallVariants4Bit[mask]
                }
            }
    }

    public void SpawnPlayer(Room startRoom)
    {
        if (playerPrefab != null)
        {
            Vector3 worldPos = floorTilemap.CellToWorld((Vector3Int)startRoom.center);
            worldPos += new Vector3(0.5f, 0.5f, 0f);
            playerInstance = Instantiate(playerPrefab, worldPos, Quaternion.identity);

            if (vcam != null)
            {
                vcam.Follow = playerInstance.transform;
            }
        }
    }

    void PaintTilemaps()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int p = new Vector3Int(x, y, 0);
                if (grid[x, y] == 1)
                {
                    // choose a floor variant based on RNG or position
                    var tile = floorVariants[
                        (Mathf.Abs(x * 92821 + y * 689287) % floorVariants.Length)
                    ];
                    floorTilemap.SetTile(p, tile);
                }
                else if (grid[x, y] == 2)
                {
                    // compute mask again to pick variant
                    int mask = 0;
                    if (GetGrid(x, y + 1) == 2 || GetGrid(x, y + 1) == 1)
                        mask |= 1;
                    if (GetGrid(x + 1, y) == 2 || GetGrid(x + 1, y) == 1)
                        mask |= 2;
                    if (GetGrid(x, y - 1) == 2 || GetGrid(x, y - 1) == 1)
                        mask |= 4;
                    if (GetGrid(x - 1, y) == 2 || GetGrid(x - 1, y) == 1)
                        mask |= 8;
                    var tile = wallVariants4Bit[mask];
                    wallTilemap.SetTile(p, tile);
                }
                else if (grid[x, y] == 3)
                {
                    wallTilemap.SetTile(p, doorTile);
                }
            }
        }
    }

    int GetGrid(int x, int y)
    {
        if (!InBounds(x, y))
            return 0;
        return grid[x, y];
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    #endregion

    void PlaceDoors()
    {
        // Simple: where corridor adjacent to room wall, put a door
        for (int i = 0; i < rooms.Count; i++)
        {
            var r = rooms[i];
            // sample rim cells and if adjacent cell is corridor/open floor, mark there's a door
            for (int x = r.bounds.xMin; x < r.bounds.xMax; x++)
            {
                for (int y = r.bounds.yMin; y < r.bounds.yMax; y++)
                {
                    // check perimeter
                    if (
                        x == r.bounds.xMin
                        || x == r.bounds.xMax - 1
                        || y == r.bounds.yMin
                        || y == r.bounds.yMax - 1
                    )
                    {
                        // if this is wall (currently may be 1 floor because we placed template floor), check neighbors
                        if (IsCorridorNextTo(x, y))
                        {
                            // place door on this tile or replace adjacent wall tile with a door
                            grid[x, y] = 3;
                        }
                    }
                }
            }
        }
    }

    bool IsCorridorNextTo(int x, int y)
    {
        // corridor is floor cell that is not part of the room? Simple rule:
        return (
            GetGrid(x + 1, y) == 1
            || GetGrid(x - 1, y) == 1
            || GetGrid(x, y + 1) == 1
            || GetGrid(x, y - 1) == 1
        );
    }

    bool ValidateConnectivity()
    {
        // BFS from first room center across floor tiles (1 or door 3)
        var start = rooms[0].center;
        var q = new Queue<Vector2Int>();
        var seen = new bool[width, height];
        q.Enqueue(start);
        seen[start.x, start.y] = true;
        int seenRooms = 0;
        while (q.Count > 0)
        {
            var v = q.Dequeue();
            // if within a room center, mark
            foreach (var r in rooms)
            {
                if (r.bounds.Contains(v))
                {
                    r.reached = true;
                }
            }
            Vector2Int[] dirs =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };
            foreach (var d in dirs)
            {
                int nx = v.x + d.x,
                    ny = v.y + d.y;
                if (InBounds(nx, ny) && !seen[nx, ny] && (grid[nx, ny] == 1 || grid[nx, ny] == 3))
                {
                    seen[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        foreach (var r in rooms)
            if (r.reached)
                seenRooms++;
        return seenRooms == rooms.Count;
    }

    // simple Room descriptor
    public class Room
    {
        public RectInt bounds;
        public Vector2Int center;
        public bool handcrafted;
        public bool reached;
        public bool isStartRoom;

        public Room(RectInt r, bool handcrafted)
        {
            bounds = r;
            center = new Vector2Int(r.xMin + r.width / 2, r.yMin + r.height / 2);
            this.handcrafted = handcrafted;
        }
    }
}
