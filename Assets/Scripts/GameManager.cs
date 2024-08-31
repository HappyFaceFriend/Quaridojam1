
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Scenegleton<GameManager>
{
    public const int PlayerCount = 2;

    [SerializeField] GameBoard _gameBoard;
    [SerializeField] Pawn[] _pawns = new Pawn[PlayerCount];
    
    Player[] _players;

    Player _currentTurnPlayer = null;
    //public Player CurrentTurnPlayer => _currentTurnPlayer;

    public IEnumerable<Player> EnumerateOtherPlayers()
    {
        for(int i=0; i<_players.Length; i++)
        {
            if (_players[i] == _currentTurnPlayer)
                continue;
            yield return _players[i];
        }
    }
    private new void Awake()
    {
        Debug.Assert(_gameBoard);
        Debug.Assert(PlayerCount == 2); // Needs to change when player count changes
        Debug.Assert(_pawns.Length == PlayerCount);
        foreach (var pawn in _pawns)
            Debug.Assert(pawn);

        base.Awake();
        _players = new Player[PlayerCount];

        _players[0] = new Player("1P", _gameBoard, _pawns[0], new Vector2Int(Mathf.RoundToInt(_gameBoard.Size / 2f), 0));
        _players[1] = new Player("2P", _gameBoard, _pawns[1], new Vector2Int(Mathf.RoundToInt(_gameBoard.Size / 2f), _gameBoard.Size - 1));
    }
    private void Start()
    {
        StartGame();
    }
    void StartGame()
    {
        StartCoroutine(GameLoopCoroutine());
    }
    IEnumerator GameLoopCoroutine()
    {
        int currentPlayerIndex = 0;
        bool _gameOver = false;
        while(!_gameOver)
        {
            _currentTurnPlayer = _players[currentPlayerIndex];

            yield return PlayerTurnCoroutine(_currentTurnPlayer);

            currentPlayerIndex = (currentPlayerIndex + 1) % _players.Length;
        }
        yield return null;
    }

    IEnumerator PlayerTurnCoroutine(Player player)
    {
        yield return PlayerInput.Instance.WaitForPlayerInputCoroutine(player);
        var action = PlayerInput.Instance.LatestAction;
        if (action.ActionType == PlayerAction.Type.MovePawn)
        {
            player.MoveTo(action.Coordinates);
        }
        else if (action.ActionType == PlayerAction.Type.InstallWall)
        {
            _gameBoard.InstallWall(action.SelectedWall, action.Coordinates);
        }
    }

}