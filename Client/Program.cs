using System.Threading.Tasks;
using Grpc.Net.Client;
using ConnectFour.Shared;

var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = 
    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
using var channel = GrpcChannel.ForAddress("http://localhost:5034",
    new GrpcChannelOptions { HttpHandler = handler });
var client = new Game.GameClient(channel);

var joinGameResponse = await client.JoinGameAsync(new JoinGameRequest { PlayerName = "Player 1" });

Console.WriteLine($"Player {joinGameResponse.Player.Name} joined the game");