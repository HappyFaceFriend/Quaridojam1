using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GameBoard))]
public class GameBoardLayout : MonoBehaviour
{
    [SerializeField] Tile _tilePrefab;
    [SerializeField] Wall _wallPrefab;
    [SerializeField] Transform _floor;

    [SerializeField, HideInInspector] List<Tile> _tiles = new List<Tile>();
    [SerializeField, HideInInspector] List<Wall> _walls = new List<Wall>();


#if UNITY_EDITOR
    public void Editor_Initialize()
    {
        var gameBoard = GetComponent<GameBoard>();

        // Cleanup
        foreach (var tile in _tiles)
            DestroyImmediate(tile.gameObject);
        _tiles.Clear();
        foreach (var wall in _walls)
            DestroyImmediate(wall.gameObject);
        _walls.Clear();
        gameBoard.Editor_ResetTiles();
        gameBoard.Editor_ResetWalls();

        // Spawn tiles
        var tileSize = _tilePrefab.transform.localScale;

        _floor.localScale = new Vector3(gameBoard.Size * (tileSize.x + gameBoard.Spacing) + gameBoard.Spacing,
                                        _floor.localScale.y,
                                        gameBoard.Size * (tileSize.z + gameBoard.Spacing) + gameBoard.Spacing);

        for (int x = 0; x < gameBoard.Size; x++) 
        {
            for(int z = 0; z < gameBoard.Size; z++)
            {
                var newTile = PrefabUtility.InstantiatePrefab(_tilePrefab, transform) as Tile;
                var offset = new Vector3(-_floor.localScale.x, 0, -_floor.localScale.z) / 2f;
                offset += new Vector3(x * (tileSize.x + gameBoard.Spacing) + tileSize.x / 2 + gameBoard.Spacing,
                                        0,
                                        z * (tileSize.z + gameBoard.Spacing) + tileSize.z / 2 + gameBoard.Spacing);
                newTile.transform.localPosition = new Vector3(0, newTile.transform.localPosition.y, 0) + offset;
                _tiles.Add(newTile);
                gameBoard.Editor_SetTile(newTile, x, z);
            }
        }

        // Spawn walls
        int playerID = 0;
        foreach (var direction in EnumeratePlayerDirections())
        {
            List<Wall> playerWalls = new List<Wall>();
            for (int i = 0; i < gameBoard.Size + 1; i++)
            {
                var newWall = PrefabUtility.InstantiatePrefab(_wallPrefab, transform) as Wall;
                var offset = new Vector3(direction.y, 0, direction.y) * gameBoard.Size / 2f * (tileSize.x + gameBoard.Spacing);
                offset += new Vector3(direction.x, 0, direction.x) * gameBoard.Size / 2f * (tileSize.z + gameBoard.Spacing);

                offset += new Vector3(direction.x, 0, direction.y);
                offset += new Vector3(i * (tileSize.x + gameBoard.Spacing) * -direction.y,
                                      0,
                                      i * (tileSize.z + gameBoard.Spacing) * -direction.x); 
                newWall.transform.localPosition = new Vector3(0, newWall.transform.localPosition.y, 0) + offset;
                newWall.transform.localEulerAngles = new Vector3(0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 0);
                _walls.Add(newWall);
                playerWalls.Add(newWall);
            }
            gameBoard.Editor_SetWalls(playerWalls, playerID);
            playerID++;
        }
        EditorUtility.SetDirty(this);

    }
#endif
    static IEnumerable<Vector2Int> EnumeratePlayerDirections()
    {
#pragma warning disable CS0162
        yield return new Vector2Int(0, -1);
        yield return new Vector2Int(0, 1);
        if (GameManager.PlayerCount >= 3)
            yield return new Vector2Int(-1, 0);
        if (GameManager.PlayerCount == 4)
            yield return new Vector2Int(1, 0);
#pragma warning restore CS0162
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(GameBoardLayout))]
public class GameBoardLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var board = (GameBoardLayout)target;
        if (!Application.isPlaying)
        {
            if (GUILayout.Button("Create Board"))
            {
                board.Editor_Initialize();
            }
        }
    }
}
#endif