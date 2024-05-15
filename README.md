# Connect Four Game using gRPC

This project is a Connect Four game implementation using gRPC in .NET 7 for the Distributed Systems course at UESC (State University of Santa Cruz).

## Introduction

In this project, we have developed a Connect Four game using gRPC as the communication protocol. The game is implemented in .NET 7 and allows players to connect to a server and play against each other.

The game is played by two players, each taking turns to drop a piece in one of the columns. The first player to connect four pieces in a row, column, or diagonal wins the game.

The server manages the game state, validates moves, and notifies players of the game's progress. The client displays the game board, accepts player input, and sends moves to the server.

The game was implemented using the following technologies:

- .NET 7
- gRPC
- C#

## Requirements

To run the Connect Four game, you need to have the following tools installed on your machine:

- .NET 7

You can download the .NET 7 SDK from the following link:

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)


## Installation

To install and run the Connect Four game, follow these steps:

1. Clone the repository:

   ```shell
   git clone https://github.com/DStalkerBR/grpc-connect-four.git
   
   cd grpc-connect-four
    ```
2. Run the server:

   ```shell
   dotnet run --project ConnectFour.Server
   ```
3. Run the client:

   ```shell
    dotnet run --project ConnectFour.Client
    ```
4. Play the game!

## Instructions

The game is played by two players, each taking turns to drop a piece in one of the columns. The first player to connect four pieces in a row, column, or diagonal wins the game.

### Controls
- Use the **left and right arrows**  keys to select the column where you want to drop your piece.
- Press the **Enter** key to confirm your move.
- Press the **ESC** key to exit the game.
- Press **Space** to restart the game.

The game board is displayed as follows:

```
    1   2   3   4   5   6   7 
  ┌───┬───┬───┬───┬───┬───┬───┐
  │ · │ · │ · │ · │ · │ · │ · │
  ├───┼───┼───┼───┼───┼───┼───┤
  │ · │ · │ · │ · │ · │ · │ · │
  ├───┼───┼───┼───┼───┼───┼───┤
  │ · │ · │ · │ · │ · │ · │ · │
  ├───┼───┼───┼───┼───┼───┼───┤
  │ · │ · │ · │ · │ · │ · │ · │
  ├───┼───┼───┼───┼───┼───┼───┤
  │ · │ · │ · │ · │ · │ · │ · │
  ├───┼───┼───┼───┼───┼───┼───┤
  │ · │ · │ · │ · │ · │ · │ · │
  └───┴───┴───┴───┴───┴───┴───┘
```

The game board displays the columns from 1 to 7, and the rows from 1 to 6. The empty cells are represented by a dot (·), and the player pieces are represented by the following characters:
- Player 1: X
- Player 2: O

<small>It's possible to change the size of the board by changing the `BoardSize` in appsettings.json.</small>

## Limitations
- The game currently supports only two players per match.
- No automatic reconnection support for clients. If a player disconnects, the game ends, and the server must be restarted to begin a new match.
- No support for multiple matches. The server can only handle one match at a time.
- No use of streaming in protobuf messages, which could potentially improve application performance, but not essential for this project.


