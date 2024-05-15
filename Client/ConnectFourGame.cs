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
    private bool _autoPlay;

    public ConnectFourGame()
    {
        _autoPlay = false;
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
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
        TurnResponse turnResponse;

        while (!_gameState.IsGameOver)
        {
            turnResponse = await _client.GetTurnAsync(new Empty());

            if (turnResponse.PlayerId == _currentPlayer.PlayerId)
            {
                await MakeMoveAsync();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("\x1b[3J");
                Console.WriteLine("Aguarde o outro jogador...");

                DisplayGame(_gameState);
            }
            await Task.Delay(100);
        }

        if (_gameState.Winner != null)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            DisplayGame(_gameState);
            if (_gameState.Winner.PlayerId == _currentPlayer.PlayerId)
                Console.WriteLine("Parabéns! Você venceu!");
            else
                Console.WriteLine($"O jogador {_gameState.Winner.Name} venceu!");
        }
        else
        {
            Console.WriteLine("Empate!");
        }
    }

    private async Task MakeMoveAsync()
    {
        int selectedColumn = 0;
        int boardWidth = _gameState.Board[0].Pieces.Count;

        ConsoleKey key;

        // Clear the input buffer
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }

        // If the game is in auto play mode, select a random column
        if (_autoPlay)
        {
            Console.Clear();
            Console.WriteLine("\x1b[3J");
            Console.WriteLine("Realizando jogada automática...");
            _gameState = await _client.GetGameStatusAsync(new Empty());   

            selectedColumn = new Random().Next(0, boardWidth);
            DisplayGame(_gameState, selectedColumn);
            
            if (_gameState.IsGameOver)
                    return;
            await Task.Delay(500);
        }
        else
        {
            // Wait for the player to make a move
            do
            {
                Console.Clear();
                Console.WriteLine("\x1b[3J");
                Console.WriteLine("Sua vez de jogar!");
                _gameState = await _client.GetGameStatusAsync(new Empty());
                DisplayGame(_gameState, selectedColumn);
                if (_gameState.IsGameOver)
                    return;

                key = Console.ReadKey(true).Key;

                // Verify if the player wants to exit the game
                if (key == ConsoleKey.Escape)
                {
                    // Use ConnectPlayer to send a player with an invalid ID to the server                    
                    await _client.ConnectPlayerAsync(new Player { PlayerId = -1, Name = "Invalid" });
                    Environment.Exit(0);
                }

                // Verify if the player wants to play automatically
                if (key == ConsoleKey.Spacebar)
                {
                    _autoPlay = true;
                }

                // Move the cursor to the left or right
                if (key == ConsoleKey.LeftArrow && selectedColumn > 0)
                {
                    selectedColumn--;
                }
                else if (key == ConsoleKey.RightArrow && selectedColumn < boardWidth - 1)
                {
                    selectedColumn++;
                }
            } while (key != ConsoleKey.Enter && key != ConsoleKey.Spacebar);
        }


        var moveRequest = new MoveRequest { PlayerId = _currentPlayer.PlayerId, Column = selectedColumn + 1 };
        var moveResponse = await _client.MakeMoveAsync(moveRequest);
        _gameState = moveResponse.Game;

        if (!moveResponse.IsValidMove)
        {
            Console.WriteLine("Movimento inválido. Tente novamente.");
            await Task.Delay(1000);
        }
    }

    /// <summary>
    /// Displays the Connect Four game board with the current state of the game.
    /// </summary>
    /// <param name="game">The Connect Four game object.</param>
    /// <param name="selectedColumn">The index of the selected column (optional).</param>
    private static void DisplayGame(Game game, int selectedColumn = -1)
    {
        int boardWidth = game.Board[0].Pieces.Count;

        Console.Write("   ");
        for (int col = 0; col < boardWidth; col++)
        {
            if (selectedColumn != -1)
                Console.Write($" {(col == selectedColumn ? "V" : " ")}  ");
            else
                Console.Write($" {col + 1}  ");
        }
        Console.WriteLine();

        Console.WriteLine("  ┌───" + "┬───".Repeat(boardWidth - 1) + "┐");
        for (int row = 0; row < game.Board.Count; row++)
        {
            Console.Write("  │");
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


    /// <summary>
    /// Gets the symbol representation of a given piece type.
    /// </summary>
    /// <param name="pieceType">The type of the piece.</param>
    /// <returns>The symbol representation of the piece type.</returns>
    private static string GetPieceSymbol(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Empty => "·",
            PieceType.Player1 => "X",
            PieceType.Player2 => "O",
            _ => "?",
        };
    }
}

public static class StringExtensions
{
    public static string Repeat(this string value, int count)
    {
        return new StringBuilder().Insert(0, value, count).ToString();
    }
}