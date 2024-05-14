using System.Text;
using ConnectFour.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;

namespace ConnectFour.Client.Services;

public class ConnectFourGame
{
    private readonly ConnectFourGameService.ConnectFourGameServiceClient _client;
    private readonly GrpcChannel _channel; 
    private Game _gameState;
    private Player _currentPlayer;    

    public ConnectFourGame()
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _channel = GrpcChannel.ForAddress("http://localhost:5034", new GrpcChannelOptions { HttpHandler = handler });
        _client = new ConnectFourGameService.ConnectFourGameServiceClient(_channel);
        _gameState = new Game();
        _currentPlayer = new Player();
    }
    public async Task StartGameAsync()
    {
        await ConnectToGameAsync();
        await PlayGameAsync();
    }

    private async Task ConnectToGameAsync()
    {
        var nameList = new List<string> { "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace", "Heidi", "Ivan", "Judy" };
        var playerName = nameList[new Random().Next(0, nameList.Count)];

        Console.WriteLine($"Conectando o jogador {playerName}...");
        _currentPlayer = await _client.ConnectPlayerAsync(new Player { Name = playerName });

        Console.WriteLine("Aguardando a conexão do segundo jogador...");
        while (true)
        {
            _gameState = await _client.GetGameStatusAsync(new Empty());
            if (_gameState.Player1 != null && _gameState.Player2 != null)
                break;
            await Task.Delay(500);
        }

        DisplayGame(_gameState);
    }

    private async Task PlayGameAsync()
    {
        Random random = new Random();

        while (!_gameState.IsGameOver)
        {
            var turnResponse = await _client.GetTurnAsync(new Empty());
            Console.WriteLine($"Turno do jogador: {turnResponse.PlayerId}");
            Console.WriteLine($"Você é o jogador: {_currentPlayer.PlayerId}");

            if (turnResponse.PlayerId == _currentPlayer.PlayerId)
            {
                await MakeMoveAsync();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Aguarde o outro jogador...");
                DisplayGame(_gameState);
            }
            await Task.Delay(500);
        }

        if (_gameState.Winner != null)
        {
            Console.Clear();
            DisplayGame(_gameState);
            Console.WriteLine($"O jogador {_gameState.Winner.Name} venceu!");
        }
        else
        {
            Console.WriteLine("Empate!");
        }
    }

    private async Task MakeMoveAsync()
    {
        Console.Clear();
        Console.WriteLine("Sua vez de jogar!");
        Console.WriteLine("Digite a coluna (1 a 7): ");
        _gameState = await _client.GetGameStatusAsync(new Empty());
        DisplayGame(_gameState);
        if (_gameState.IsGameOver)
            return;
        int column = int.Parse(Console.ReadLine());         

        var moveRequest = new MoveRequest { PlayerId = _currentPlayer.PlayerId, Column = column };
        var moveResponse = await _client.MakeMoveAsync(moveRequest);
        _gameState = moveResponse.Game;

        if (!moveResponse.IsValidMove)
        {
            Console.WriteLine("Movimento inválido. Tente novamente.");
        }
    }

    private void DisplayGame(Game game)
    {
        int boardWidth = game.Board[0].Pieces.Count;

        Console.Write("   ");
        for (int col = 0; col < boardWidth; col++)
        {
            Console.Write($" {col + 1}  ");
        }
        Console.WriteLine();

        Console.WriteLine("  ┌───" + "┬───".Repeat(boardWidth - 1) + "┐");
        for (int row = 0; row < game.Board.Count; row++)
        {
            Console.Write($"  │");
            foreach (var piece in game.Board[row].Pieces)
            {
                string symbol = GetPieceSymbol(piece);
                if (piece == PieceType.Player1)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (piece == PieceType.Player2)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($" {symbol} ");            
                Console.ResetColor();
                Console.Write("│");
            }
            Console.WriteLine();
            if (row < game.Board.Count - 1)
            {
                Console.WriteLine("  ├───" + "┼───".Repeat(boardWidth - 1) + "┤");
            }
        }
        Console.WriteLine("  └───" + "┴───".Repeat(boardWidth - 1) + "┘");
    }



    private string GetPieceSymbol(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Empty:
                return "·";
            case PieceType.Player1:
                return "X";
            case PieceType.Player2:
                return "O";
            default:
                return "?";
        }
    }
}

public static class StringExtensions
{
    public static string Repeat(this string value, int count)
    {
        return new StringBuilder().Insert(0, value, count).ToString();
    }
}