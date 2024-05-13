using Grpc.Core;
using ConnectFour.Shared;
using System.Diagnostics;
using Google.Protobuf.WellKnownTypes;

namespace ConnectFour.Server.Services;
public class GameService : ConnectFourGameService.ConnectFourGameServiceBase
{
    private Game currentGame;
    private int connectedPlayers = 0;
    private bool isGameReady = false;

    public GameService()
    {
        this.currentGame = new Game
        {
            Player1 = null,
            Player2 = null,
            CurrentTurn = 1,
            IsGameOver = false,
            Winner = null
        };
        for (int i = 0; i < 6; i++)
        {
            var row = new Row();
            for (int j = 0; j < 7; j++)
            {
                row.Pieces.Add(PieceType.Empty);
            }
            this.currentGame.Board.Add(row);
        }
    }

    public override Task<MoveResponse> MakeMove(MoveRequest request, ServerCallContext context)
    {
        MoveResponse response = new MoveResponse();

        // Check if the game is ready
        if (!isGameReady)
        {
            response.IsValidMove = false;
            response.Game = currentGame;
            return Task.FromResult(response);
        }

        // Check if the game is already over
        if (currentGame.IsGameOver)
        {
            response.IsValidMove = false;
            response.Game = currentGame;
            return Task.FromResult(response);
        }

         // Verify if the player is the current turn
        if (request.PlayerId != currentGame.CurrentTurn)
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

        // Validate the move position
        int column = request.Column;
        if (column < 0 || column > currentGame.Board[0].Pieces.Count)
        {
            response.IsValidMove = false;
            response.Game = currentGame;
            return Task.FromResult(response);
        }

        Console.WriteLine($"Player {currentPlayer.PlayerId} is attempting to place a piece in column {column}");

        // Process the move and update the game state
        bool moveResult = ProcessMove(column, currentPlayer);
        response.IsValidMove = moveResult;
        response.Game = currentGame;

        currentGame.CurrentTurn = (currentGame.CurrentTurn == 1) ? 2 : 1;

        return Task.FromResult(response);
    }

    private bool ProcessMove(int column, Player currentPlayer)
    {
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

        Console.WriteLine($"Horizontal count: {horizontalCount}");

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
        
        Console.WriteLine($"Vertical count: {verticalCount}");

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

        if (isDraw)
        {
            return true;
        }

        return false;
    }


    public override Task<Game> GetGameStatus(Empty request, ServerCallContext context)
    {
        return Task.FromResult(currentGame);
    }

    public override Task<Empty> ConnectPlayer(Player request, ServerCallContext context)
    {
        if (currentGame.IsGameOver)
        {
            ResetGame();
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
        }

        if (connectedPlayers == 2)
        {
            isGameReady = true;
        }

        // Define o ID do jogador com base na ordem de conexão
        request.PlayerId = connectedPlayers;

        return Task.FromResult(new Empty());
    }

 

    private void ResetGame()
    {
        connectedPlayers = 0;
        isGameReady = false;

        currentGame = new Game
        {
            Player1 = null,
            Player2 = null,
            CurrentTurn = 1,
            IsGameOver = false,
            Winner = null
        };
        for (int i = 0; i < 6; i++)
        {
            var row = new Row();
            for (int j = 0; j < 7; j++)
            {
                row.Pieces.Add(PieceType.Empty);
            }
            currentGame.Board.Add(row);
        }
    }

}
