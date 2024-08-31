using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UIElements;

public class GameBoard : MonoBehaviour
{
    [SerializeField] int _size = 9;
    [SerializeField] float _spacing = 0.5f;

    [SerializeField] Tile[] _tiles;
    [SerializeField] Wall[] _walls;

    Dictionary<Vector2Int, bool> _wallPlacedBetweenTiles = new Dictionary<Vector2Int, bool>();
    Dictionary<Vector2Int, bool> _wallPlacedAtCoordinates = new Dictionary<Vector2Int, bool>();
    public int Size => _size;
    public float Spacing => _spacing;

    public Wall[] GetPlayerWalls(int playerID)
    {
        Wall[] walls = new Wall[_size];
        for (int i = 0; i < _size; i++)
            walls[i] = _walls[i + playerID * _size];
        return walls;
    }

    private void Awake()
    {
        for (int i = 0; i < _tiles.Length; i++)
        {
            _tiles[i].SetCoordinates(new Vector2Int(i % _size, i / _size));
        }
    }
    public bool IsCoordInBounds(Vector2Int coordinates)
    {
        return coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x < _size && coordinates.y < _size;
    }
    public bool IsCoordOccupiedByPawn(Vector2Int coordinates)
    {
        foreach (var player in GameManager.Instance.EnumerateOtherPlayers())
        {
            if (player.Coordinates == coordinates)
                return true;
        }
        return false;
    }
    public Tile GetTileAtCoord(Vector2Int coordinates)
    {
        if (IsCoordInBounds(coordinates))
            return _tiles[coordinates.x + coordinates.y * _size];
        else
            return null;
    }
    public void InstallWall(Wall wall, Vector2Int wallCoordinates)
    {
        wall.Installed = true;
        // Wall coord is at the right top of the til with same coordinate
        if (wall.IsHorizontal)
        {
            PlaceWallBetweenTiles(wallCoordinates, wallCoordinates + Vector2Int.up);
            PlaceWallBetweenTiles(wallCoordinates + Vector2Int.right, wallCoordinates + Vector2Int.right + Vector2Int.up);
        }
        else
        {
            PlaceWallBetweenTiles(wallCoordinates, wallCoordinates + Vector2Int.right);
            PlaceWallBetweenTiles(wallCoordinates + Vector2Int.up, wallCoordinates + Vector2Int.up + Vector2Int.right);
        }
        _wallPlacedAtCoordinates[wallCoordinates] = true;
    }
    void PlaceWallBetweenTiles(Vector2Int tileCoordA, Vector2Int tileCoordB)
    {
        _wallPlacedBetweenTiles[new Vector2Int(tileCoordA.x + tileCoordA.y * _size, tileCoordB.x + tileCoordB.y * _size)] = true;
        _wallPlacedBetweenTiles[new Vector2Int(tileCoordB.x + tileCoordB.y * _size, tileCoordA.x + tileCoordA.y * _size)] = true;
    }
    public Vector3 GetClosestWallPosition(Vector3 position)
    {
        var wallCoordinates = GetWallCoord(position);
        return WallCoordToPosition(wallCoordinates);
    }
    Vector3 WallCoordToPosition(Vector2Int wallCoordinates)
    {
        Vector3 cellSize = new Vector3(_tiles[0].transform.lossyScale.x + _spacing, 0, _tiles[0].transform.lossyScale.z + _spacing);
        Vector3 position = new Vector3((wallCoordinates.x - Mathf.RoundToInt(_size / 2f)) * cellSize.x,
                                    0, (wallCoordinates.y - Mathf.RoundToInt(_size / 2f)) * cellSize.z);
        if (_size % 2 == 1)
            position += cellSize / 2f;
        return position;
    }
    public Vector2Int GetWallCoord(Vector3 position)
    {
        Vector3 cellSize = new Vector3(_tiles[0].transform.lossyScale.x + _spacing, 0, _tiles[0].transform.lossyScale.z + _spacing);
        if (_size % 2 == 1)
            position -= cellSize / 2f;
        return new Vector2Int(Mathf.RoundToInt(position.x / cellSize.x) + Mathf.RoundToInt(_size / 2f),
                              Mathf.RoundToInt(position.z / cellSize.z) + Mathf.RoundToInt(_size / 2f));
    }
    public bool IsWallPlacedBetweenTiles(Vector2Int tileCoordA, Vector2Int tileCoordB)
    {
        var coordPair = new Vector2Int(tileCoordA.x + tileCoordA.y * _size, tileCoordB.x + tileCoordB.y * _size);
        if (_wallPlacedBetweenTiles.ContainsKey(coordPair))
            return _wallPlacedBetweenTiles[coordPair];
        return false;
    }
    public bool IsWallInstallable(Wall wall)
    {
        var wallCoordinates = GetWallCoord(wall.transform.position);
        if (wallCoordinates.x >= 0 && wallCoordinates.y >= 0 && wallCoordinates.x < _size - 1 && wallCoordinates.y < _size - 1)
        {
            if (_wallPlacedAtCoordinates.ContainsKey(wallCoordinates) && _wallPlacedAtCoordinates[wallCoordinates])
                return false;
            if (wall.IsHorizontal)
            {
                if (IsWallPlacedBetweenTiles(wallCoordinates, wallCoordinates + Vector2Int.up) ||
                    IsWallPlacedBetweenTiles(wallCoordinates + Vector2Int.right, wallCoordinates + Vector2Int.right + Vector2Int.up))
                    return false;
            }
            else
            {
                if (IsWallPlacedBetweenTiles(wallCoordinates, wallCoordinates + Vector2Int.right) ||
                    IsWallPlacedBetweenTiles(wallCoordinates + Vector2Int.up, wallCoordinates + Vector2Int.up + Vector2Int.right))
                    return false;
            }
            // TODO : Check player validity
            return true;
        }
        return false;
    }
    public Vector3 GetTilePosition(int x, int z)
    {
        return _tiles[x + z * _size].transform.position;
    }
#if UNITY_EDITOR
    public void Editor_SetTile(Tile tile, int x, int z)
    {
        _tiles[x + z * _size] = tile;
        EditorUtility.SetDirty(this);
    }
    public void Editor_ResetTiles()
    {
        _tiles = new Tile[_size * _size];
        EditorUtility.SetDirty(this);
    }
    public void Editor_ResetWalls(List<Wall> walls)
    {
        _walls = new Wall[walls.Count];
        for (int i = 0; i < walls.Count; i++)
        {
            _walls[i] = walls[i];
        }
        EditorUtility.SetDirty(this);
    }
#endif
}

