using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ConnectFour.Shared;
using Google.Protobuf.WellKnownTypes;

class Program
{
    static async Task Main(string[] args)
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using var channel = GrpcChannel.ForAddress("http://localhost:5034", new GrpcChannelOptions { HttpHandler = handler });

        var client = new ConnectFourGameService.ConnectFourGameServiceClient(channel);

        Random random = new Random();

        var player1 = new Player { Name = "Player 1", PlayerId = 1 };
        var player2 = new Player { Name = "Player 2", PlayerId = 2 };
        var gameStatus = await client.GetGameStatusAsync(new Empty());

        // server still does not logic to determine who is the first player
        bool player1Turn = true;
        while (true)
        {
            var currentPlayer = player1Turn ? player1 : player2;
            Console.WriteLine("Movimento do jogador " + currentPlayer.Name);
            int column = random.Next(1, 8);
            Console.WriteLine("Jogador está tentando colocar uma peça na coluna " + column);
            var moveRequest = new MoveRequest
            {
                Player = currentPlayer,
                Column = column
            };

            var moveResponse = await client.MakeMoveAsync(moveRequest);

            if (moveResponse.IsValidMove)
            {
                Console.WriteLine("Movimento válido. Estado do jogo atualizado:");
                DisplayGame(moveResponse.Game);
            }
            else if (moveResponse.Game.IsGameOver)
            {
                Console.WriteLine("O jogo acabou!");
                Console.WriteLine("Vencedor: " + moveResponse.Game.Winner.Name);
                break;
            }
            else
            {
                Console.WriteLine("Movimento inválido.");
            }

            player1Turn = !player1Turn;
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
