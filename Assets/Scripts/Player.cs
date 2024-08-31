
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct WinCondition
{
    public enum Axis { X, Y }
    public Axis TargetAxis;
    public int Value;
}
public class Player
{
    string _name;
    Vector2Int _coordinates;
    WinCondition _winCondition;

    Pawn _pawn;
    GameBoard _gameBoard;
    Wall[] _walls; 
    public string Name => _name;
    public Vector2Int Coordinates => _coordinates;
    public Pawn Pawn => _pawn;
    public Player(string name, GameBoard gameBoard, Pawn pawn, Wall[] walls, Vector2Int startingCoordinates, WinCondition winCondition)
    {
        _name = name;
        _gameBoard = gameBoard;
        _pawn = pawn;
        _coordinates = startingCoordinates;
        _winCondition = winCondition;
        _walls = walls;
        MoveTo(startingCoordinates);
    }
    public bool IsAtWinPosition
    {
        get
        {
            if (_winCondition.TargetAxis == WinCondition.Axis.X)
            {
                return _coordinates.x == _winCondition.Value;
            }
            else
            {
                return _coordinates.y == _winCondition.Value;
            }
        }
    }
    public bool OwnsWall(Wall wall)
    {
        return _walls.Contains(wall);
    }
    public void MoveTo(Vector2Int coordinates)
    {
        _coordinates = coordinates;
        _pawn.transform.position = _gameBoard.GetTilePosition(_coordinates.x, _coordinates.y);
    }
}