using Grpc.Core;
using ConnectFour.Shared;

namespace GrpcConnectFour.Server.Services;

public class GameService : Game.GameBase
{
    private readonly ILogger<GameService> _logger;
    private IList<Player> _players;
    private GameState _gameState;
    public GameService(ILogger<GameService> logger)
    {
        _logger = logger;
        _players = new List<Player>();
        _gameState = new GameState()
        {
            Board = { "......", "......", "......", "......", "......", "......", "......" },
            CurrentPlayer = new Player(),
            GameOver = false,
            Winner = ""
        };
    }

    public override Task<JoinGameResponse> JoinGame(JoinGameRequest request, ServerCallContext context)
    {
        if (this._players.Count <= 2)
        {
            var player = new Player()
            {
                Id = this._players.Count + 1,
                Name = request.PlayerName
            };
            this._players.Add(player);
            Console.WriteLine($"Player {player.Name} joined the game");
            return Task.FromResult(new JoinGameResponse
            {
                Player = player,
                GameState = _gameState
            });
        }
        else
        {
            return Task.FromResult(new JoinGameResponse
            {
                Player = null
            });
        }
    }

    public override Task<PlayTurnResponse> PlayTurn(PlayTurnRequest request, ServerCallContext context)
    {
        var player = this._players.FirstOrDefault(p => p.Id == request.PlayerId);
        if (player != null)
        {
            var row = request.Position.Row;
            var col = request.Position.Column;
            if (row >= 0 && row < 7 && col >= 0 && col < 6)
            {
                if (this._gameState.Board[col][row] == '.')
                {
                    this._gameState.Board[col] = this._gameState.Board[col].Remove(row, 1).Insert(row, player.Id.ToString());
                    this._gameState.CurrentPlayer = player;
                    this._gameState.GameOver = this._gameState.Board.Any(row => row.Contains("1111") || row.Contains("2222"));
                    if (this._gameState.GameOver)
                    {
                        this._gameState.Winner = player.Name;
                    }
                }
            }
        }
        return Task.FromResult(new PlayTurnResponse
        {
            GameState = _gameState
        });
    }

    public override Task<GameState> GetGameState(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
    {
        return Task.FromResult(_gameState);
    }



}
