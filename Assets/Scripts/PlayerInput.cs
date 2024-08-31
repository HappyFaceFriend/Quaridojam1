
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerAction
{
    public enum Type { MovePawn, InstallWall }
    public Type ActionType { get; private set; }
    public Vector2Int Coordinates { get; private set; }
    public Wall SelectedWall { get; private set; }
    public PlayerAction(Type actionType, Vector2Int position, Wall selectedWall = null)
    {
        ActionType = actionType;
        Coordinates = position;
        SelectedWall = selectedWall;
    }
}
public interface ISelectable
{
    void SetSelectable(bool selectable);
}
public class PlayerInput : Scenegleton<PlayerInput>
{
    [SerializeField] GameBoard _gameBoard;
    enum State { WaitingInput, PawnSelected, WallSelected, InputDone }
    State _currentState;

    Player _currentPlayer;
    List<ISelectable> _currentSelectables = new List<ISelectable>();
    PlayerAction _latestAction;
    Wall _currentSelectedWall;
    public PlayerAction LatestAction => _latestAction;

    public IEnumerator WaitForPlayerInputCoroutine(Player currentPlayer)
    {
        _currentPlayer = currentPlayer;
        _currentState = State.WaitingInput;
        yield return new WaitUntil(()=>_currentState == State.InputDone);
    }
    private void Update()
    {
        if (_currentState == State.PawnSelected || _currentState == State.WallSelected) 
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                CancelSelection();
        }
        if (_currentState == State.WallSelected)
        {
            var mousePoint = GetMousePositionAtPlane(0);
            _currentSelectedWall.transform.position = _gameBoard.GetClosestWallPosition(mousePoint);

            if (Input.mouseScrollDelta.y > 0)
            {
                _currentSelectedWall.Rotate();
            }

            bool isInstallable = _gameBoard.IsWallInstallable(_currentSelectedWall);
            if (isInstallable)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var wallCoordinates = _gameBoard.GetWallCoord(_currentSelectedWall.transform.position);
                    _latestAction = new PlayerAction(PlayerAction.Type.InstallWall, wallCoordinates, _currentSelectedWall);
                    _currentState = State.InputDone;
                    _currentSelectedWall = null;
                    ClearSelectables();
                }
                // TODO : Change current wall to green
            }
            else
            {
                // TODO : Change current wall to red
            }
        }
    }
    void CancelSelection()
    {
        _currentState = State.WaitingInput;
        if (_currentSelectedWall)
        {
            _currentSelectedWall.ReturnToOriginalPosition();
            _currentSelectedWall = null;
        }
        ClearSelectables();
    }
    bool IsTileBlocked(Vector2Int fromCoordinates, Vector2Int toCoordinates, bool allowPawns)
    {
        if (!_gameBoard.IsCoordInBounds(toCoordinates))
            return true;
        if (_gameBoard.IsWallPlacedBetweenTiles(fromCoordinates, toCoordinates))
            return true;
        if (!allowPawns && _gameBoard.IsCoordOccupiedByPawn(toCoordinates))
            return true;
        return false;
    }
    public void TrySelectPawn(Pawn pawn)
    {
        if (_currentState == State.WaitingInput && _currentPlayer.Pawn == pawn)
        {
            _currentState = State.PawnSelected;

            ClearSelectables();
            foreach (var direction in EnumerateFourDirections())
            {
                var destinationCoordinates = _currentPlayer.Coordinates + direction;
                if (IsTileBlocked(_currentPlayer.Coordinates, destinationCoordinates, true))
                    continue;
                if (_gameBoard.IsCoordOccupiedByPawn(destinationCoordinates))
                {
                    var jumpedDestinationCoordinates = destinationCoordinates + direction;
                    if (!_gameBoard.IsCoordInBounds(jumpedDestinationCoordinates) || _gameBoard.IsCoordOccupiedByPawn(jumpedDestinationCoordinates))
                        continue;
                    if (_gameBoard.IsWallPlacedBetweenTiles(_currentPlayer.Coordinates + direction, jumpedDestinationCoordinates))
                    {
                        foreach(var diagnal in EnumeratePerpendicularDirections(direction))
                        {
                            print(diagnal);
                            var diagnalDestination = _currentPlayer.Coordinates + diagnal;
                            if (IsTileBlocked(_currentPlayer.Coordinates + direction, diagnalDestination, false))
                                continue;
                            var diagnalTile = _gameBoard.GetTileAtCoord(diagnalDestination);
                            _currentSelectables.Add(diagnalTile);
                            diagnalTile.SetSelectable(true);
                        }
                    }
                    else
                    {
                        var jumpedTile = _gameBoard.GetTileAtCoord(jumpedDestinationCoordinates);
                        _currentSelectables.Add(jumpedTile);
                        jumpedTile.SetSelectable(true);
                    }
                }
                else
                {
                    var tile = _gameBoard.GetTileAtCoord(destinationCoordinates);
                    _currentSelectables.Add(tile);
                    tile.SetSelectable(true);
                }
            }
        }

    }
    public void TrySelectWall(Wall wall)
    {
        if (_currentState == State.WaitingInput && _currentPlayer.OwnsWall(wall))
        {
            _currentState = State.WallSelected;
            _currentSelectedWall = wall;
        }
    }
    void ClearSelectables()
    {
        foreach (var tile in _currentSelectables)
            tile.SetSelectable(false);
        _currentSelectables.Clear();
    }
    public void SelectTile(Tile tile)
    {
        if (_currentState == State.PawnSelected)
        {
            _latestAction = new PlayerAction(PlayerAction.Type.MovePawn, tile.Coordinates);
            _currentState = State.InputDone;
            ClearSelectables();
        }
    }

    static IEnumerable<Vector2Int> EnumerateFourDirections()
    {
        yield return new Vector2Int(-1, 0);
        yield return new Vector2Int(0, 1);
        yield return new Vector2Int(1, 0);
        yield return new Vector2Int(0, -1);
    }
    static IEnumerable<Vector2Int> EnumeratePerpendicularDirections(Vector2Int direction)
    {
        if (direction.x == 0)
        {
            yield return new Vector2Int(-1, direction.y);
            yield return new Vector2Int(1, direction.y); 
        }
        else
        {
            yield return new Vector2Int(direction.x, -1);
            yield return new Vector2Int(direction.x, 1); 
        }
    }

    static Vector3 GetMousePositionAtPlane(float y = 0)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float t = -ray.origin.y / ray.direction.y;
        return ray.origin + t * ray.direction;
    }
}