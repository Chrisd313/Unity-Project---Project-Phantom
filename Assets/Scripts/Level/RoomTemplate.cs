using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Dungeon/RoomTemplate")]
public class RoomTemplate : ScriptableObject
{
    public int width;
    public int height;

    [Tooltip("Row-major: index = y * width + x. Leave null entries as empty (null).")]
    public TileBase[] tiles; // length should be width * height
    public Vector2Int[] doorAnchors; // local coords relative to template origin (0..width-1,0..height-1)

    public TileBase GetTile(int x, int y)
    {
        int idx = y * width + x;
        if (idx < 0 || idx >= tiles.Length)
            return null;
        return tiles[idx];
    }
}
