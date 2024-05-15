using Grpc.Core;
using ConnectFour.Shared;
using Google.Protobuf.WellKnownTypes;

namespace ConnectFour.Server.Services;
public class GameService : ConnectFourGameService.ConnectFourGameServiceBase
{
    private Game currentGame;
    private int connectedPlayers = 0;
    private bool isGameReady = false;
    private readonly int rows;
    private readonly int columns;

    public GameService(int rows = 6, int columns = 7)
    {
        this.currentGame = new Game
        {
            Player1 = null,
            Player2 = null,
            CurrentTurn = new Random().Next(1, 3),
            IsGameOver = false,
            Winner = null
        };
        for (int i = 0; i < rows; i++)
        {
            var row = new Row();
            for (int j = 0; j < columns; j++)
            {
                row.Pieces.Add(PieceType.Empty);
            }
            this.currentGame.Board.Add(row);
        }
        this.rows = rows;
        this.columns = columns;
    }


    /// <summary>
    /// Retrieves the current status of the game.
    /// </summary>
    /// <param name="request">The request message.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>The current game status.</returns>
    public override Task<Game> GetGameStatus(Empty request, ServerCallContext context)
    {
        return Task.FromResult(currentGame);
    }

    /// <summary>
    /// Connects a player to the game and assigns a player ID.
    /// </summary>
    /// <param name="request">The player to connect.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>The connected player with assigned player ID.</returns>
    public override Task<Player> ConnectPlayer(Player request, ServerCallContext context)
    {
        if (currentGame.IsGameOver)
        {
            ResetGame();
        }

        // ResetGame if client sends a PlayerId of -1
        if (request.PlayerId == -1){
            ResetGame();
            return Task.FromResult(new Player());
        }

        if (connectedPlayers >= 2 || isGameReady)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Máximo de jogadores conectados atingido."));
        } 

        connectedPlayers++;

        if (connectedPlayers == 1)
        {
            currentGame.Player1 = request;
        }
        else if (connectedPlayers == 2)
        {
            currentGame.Player2 = request;
            isGameReady = true;
        }

        // Define o ID do jogador com base na ordem de conexão
        request.PlayerId = connectedPlayers;
        Player playerResponse = new Player
        {
            PlayerId = request.PlayerId,
            Name = request.Name
        };
        
        return Task.FromResult(playerResponse);
    }

    public override Task<TurnResponse> GetTurn (Empty request, ServerCallContext context)
    {
        return Task.FromResult(new TurnResponse { PlayerId = currentGame.CurrentTurn });
    }
 

    /// <summary>
    /// Makes a move in the game.
    /// </summary>
    /// <param name="request">The move request.</param>
    /// <param name="context">The server call context.</param>
    /// <returns>The move response.</returns>
    public override Task<MoveResponse> MakeMove(MoveRequest request, ServerCallContext context)
    {
        MoveResponse response = new MoveResponse();

        // Verify if the game is ready and the player is allowed to make a move
        if (!isGameReady || currentGame.IsGameOver || request.PlayerId != currentGame.CurrentTurn)
        {
            response.IsValidMove = false;
            response.Game = currentGame;
            return Task.FromResult(response);
        }

        // Validate the player making the move
        Player currentPlayer = request.PlayerId == 1 ? currentGame.Player1 : currentGame.Player2;
        if (currentPlayer.PlayerId != currentGame.Player1.PlayerId && currentPlayer.PlayerId != currentGame.Player2.PlayerId)
        {
            response.IsValidMove = false;
            response.Game = currentGame;
            return Task.FromResult(response);
        }

        int column = request.Column;

        Console.WriteLine($"Player {currentPlayer.PlayerId} is attempting to place a piece in column {column}");

        // Process the move and update the game state
        bool moveResult = ProcessMove(column, currentPlayer);
        response.IsValidMove = moveResult;
        response.Game = currentGame;

        if (moveResult)
            currentGame.CurrentTurn = (currentGame.CurrentTurn == 1) ? 2 : 1;

        return Task.FromResult(response);
    }

    /// <summary>
    /// Processes a move in the game by placing a piece in the specified column for the current player.
    /// </summary>
    /// <param name="column">The column where the piece should be placed.</param>
    /// <param name="currentPlayer">The current player making the move.</param>
    /// <returns>True if the move was successfully processed, false otherwise.</returns>
    private bool ProcessMove(int column, Player currentPlayer)
    {

        // Check if the column is valid
        if (column < 0 || column > currentGame.Board[0].Pieces.Count)
        {
            return false;
        }        

        // Find the first empty row in the specified column
        int row = -1;
        for (int i = currentGame.Board.Count - 1; i >= 0; i--)
        {
            if (currentGame.Board[i].Pieces[column - 1] == PieceType.Empty)
            {
                row = i;
                break;
            }
        }

        // Check if the column is full
        if (row == -1)
        {
            return false;
        }

        // Determine the piece type based on the current player
        PieceType pieceType = currentPlayer.PlayerId == currentGame.Player1.PlayerId ? PieceType.Player1 : PieceType.Player2;

        // Place the piece in the board
        currentGame.Board[row].Pieces[column - 1] = pieceType;

        // Check if the game is over
        currentGame.IsGameOver = CheckGameOver(row, column, pieceType);

        return true;
    }

    /// <summary>
    /// Checks if the game is over by determining if there is a winning condition or a draw.
    /// </summary>
    /// <param name="row">The row index of the last played piece.</param>
    /// <param name="column">The column index of the last played piece.</param>
    /// <param name="pieceType">The type of the last played piece.</param>
    /// <returns>True if the game is over, false otherwise.</returns>
    private bool CheckGameOver(int row, int column, PieceType pieceType)
    {
        // Check for a horizontal win
        int horizontalCount = 1;
        for (int i = column - 2; i >= 0; i--)
        {
            if (currentGame.Board[row].Pieces[i] == pieceType)
            {
                horizontalCount++;
            }
            else
            {
                break;
            }
        }

        for (int i = column; i < currentGame.Board[row].Pieces.Count; i++)
        {
            if (currentGame.Board[row].Pieces[i] == pieceType)
            {
                horizontalCount++;
            }
            else
            {
                break;
            }
        }

        if (horizontalCount >= 4)
        {
            currentGame.Winner = pieceType == PieceType.Player1 ? currentGame.Player1 : currentGame.Player2;
            return true;
        }

        // Check for a vertical win

        int verticalCount = 1;
        for (int i = row - 1; i >= 0; i--)
        {
            if (currentGame.Board[i].Pieces[column - 1] == pieceType)
            {
                verticalCount++;
            }
            else
            {
                break;
            }
        }

        for (int i = row + 1; i < currentGame.Board.Count; i++)
        {
            if (currentGame.Board[i].Pieces[column - 1] == pieceType)
            {
                verticalCount++;
            }
            else
            {
                break;
            }
        }

        if (verticalCount >= 4)
        {
            currentGame.Winner = pieceType == PieceType.Player1 ? currentGame.Player1 : currentGame.Player2;
            return true;
        }
        
        // Check for a diagonal win (top-left to bottom-right)
        int diagonalCount1 = 1;
        for (int i = row - 1, j = column - 2; i >= 0 && j >= 0; i--, j--)
        {
            if (currentGame.Board[i].Pieces[j] == pieceType)
            {
                diagonalCount1++;
            }
            else
            {
                break;
            }
        }

        for (int i = row + 1, j = column; i < currentGame.Board.Count && j < currentGame.Board[row].Pieces.Count; i++, j++)
        {
            if (currentGame.Board[i].Pieces[j] == pieceType)
            {
                diagonalCount1++;
            }
            else
            {
                break;
            }
        }

        if (diagonalCount1 >= 4)
        {
            currentGame.Winner = pieceType == PieceType.Player1 ? currentGame.Player1 : currentGame.Player2;
            return true;
        }

        // Check for a diagonal win (bottom-left to top-right)

        int diagonalCount2 = 1;
        for (int i = row + 1, j = column - 2; i < currentGame.Board.Count && j >= 0; i++, j--)
        {
            if (currentGame.Board[i].Pieces[j] == pieceType)
            {
                diagonalCount2++;
            }
            else
            {
                break;
            }
        }

        for (int i = row - 1, j = column; i >= 0 && j < currentGame.Board[row].Pieces.Count; i--, j++)
        {
            if (currentGame.Board[i].Pieces[j] == pieceType)
            {
                diagonalCount2++;
            }
            else
            {
                break;
            }
        }

        if (diagonalCount2 >= 4)
        {
            currentGame.Winner = pieceType == PieceType.Player1 ? currentGame.Player1 : currentGame.Player2;
            return true;
        }

        // check for a draw
        bool isDraw = true;
        foreach (var boardRow in currentGame.Board)
        {
            foreach (var piece in boardRow.Pieces)
            {
                if (piece == PieceType.Empty)
                {
                    isDraw = false;
                    break;
                }
            }
        }

        return isDraw;
    }

    /// <summary>
    /// Resets the game by initializing the game state and clearing the game board.
    /// </summary>
    private void ResetGame()
    {
        connectedPlayers = 0;
        isGameReady = false;

        currentGame = new Game
        {
            Player1 = null,
            Player2 = null,
            CurrentTurn = new Random().Next(1, 3),
            IsGameOver = false,
            Winner = null
        };
        for (int i = 0; i < rows; i++)
        {
            var row = new Row();
            for (int j = 0; j < columns; j++)
            {
                row.Pieces.Add(PieceType.Empty);
            }
            currentGame.Board.Add(row);
        }
    }

}
