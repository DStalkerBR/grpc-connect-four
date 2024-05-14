using ConnectFour.Client.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var game = new ConnectFourGame();
        await game.StartGameAsync();
    }
}
