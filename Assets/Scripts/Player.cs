
using UnityEngine;

public class Player
{
    string _name;
    Vector2Int _coordinates;
    Pawn _pawn;
    GameBoard _gameBoard;
    public string Name => _name;
    public Vector2Int Coordinates => _coordinates;
    public Pawn Pawn => _pawn;
    public Player(string name, GameBoard gameBoard, Pawn pawn, Vector2Int startingCoordinates)
    {
        _name = name;
        _gameBoard = gameBoard;
        _pawn = pawn;
        _coordinates = startingCoordinates;
        MoveTo(startingCoordinates);
    }
    
    public void MoveTo(Vector2Int coordinates)
    {
        _coordinates = coordinates;
        _pawn.transform.position = _gameBoard.GetTilePosition(_coordinates.x, _coordinates.y);
    }
}