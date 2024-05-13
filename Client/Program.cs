using Grpc.Net.Client;
using ConnectFour.Shared;
using Google.Protobuf.WellKnownTypes;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = GrpcChannel.ForAddress("http://localhost:5034", new GrpcChannelOptions { HttpHandler = handler });

        var client = new ConnectFourGameService.ConnectFourGameServiceClient(channel);

        var nomePlayer = Console.ReadLine();
        // Conecta o jogador
        var player1 = new Player { Name = nomePlayer };
        await client.ConnectPlayerAsync(player1);

        // Aguarda a conexão do segundo jogador
        var gameStatus = await client.GetGameStatusAsync(new Empty());
        while (gameStatus.Player1 == null || gameStatus.Player2 == null)
        {
            Console.WriteLine("Aguardando a conexão do segundo jogador...");
            gameStatus = await client.GetGameStatusAsync(new Empty());
            await Task.Delay(500);
        }
        DisplayGame(gameStatus);

        // Simula movimentos no jogo (substitua com sua lógica real de movimento)
        await MakeFakeMoves(client, gameStatus);

    }

    static async Task<bool> MakeFakeMoves(ConnectFourGameService.ConnectFourGameServiceClient client, Game game)
    {
        // Simula movimentos alternados entre os jogadores
        Random random = new Random();
        while (!game.IsGameOver)
        {
            int column = random.Next(1, 8); // Gera uma coluna aleatória (de 1 a 7)
            var moveRequest = new MoveRequest { PlayerId = game.CurrentTurn, Column = column };

            var moveResponse = await client.MakeMoveAsync(moveRequest);
            game = moveResponse.Game;
            DisplayGame(game);

            if (!moveResponse.IsValidMove)
            {
                Console.WriteLine("Movimento inválido. Tente novamente.");
            }

            await Task.Delay(100); // Aguarda 1 segundo entre os movimentos (para simulação)
        }

        if (game.Winner != null)
        {
            Console.WriteLine($"O jogador {game.Winner.Name} venceu!");
            return true;
        }
        else
        {
            Console.WriteLine("Empate!");
            return false;
        }
    }


    static void DisplayGame(Game game)
    {
        foreach (var row in game.Board)
        {
            foreach (var piece in row.Pieces)
            {
                Console.Write(GetPieceSymbol(piece) + " ");
            }
            Console.WriteLine();
        }
    }

    static string GetPieceSymbol(PieceType pieceType)
    {
        switch (pieceType)
        {
            case PieceType.Empty:
                return "-";
            case PieceType.Player1:
                return "X";
            case PieceType.Player2:
                return "O";
            default:
                return "?";
        }
    }
}
