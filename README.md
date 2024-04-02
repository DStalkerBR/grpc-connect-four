# Connect Four Game using gRPC

This project is a Connect Four game implementation using gRPC in .NET 7 for the Distributed Systems course at UESC (State University of Santa Cruz).

## Introduction

In this project, we aim to develop a Connect Four game using gRPC as the communication protocol. The game will be implemented in .NET 7 and will allow players to connect to a server and play against each other.

The game will be played by two players, each taking turns to drop a piece in one of the columns. The first player to connect four pieces in a row, column, or diagonal wins the game.

The server will be responsible for managing the game state, validating moves, and notifying players of the game's progress. The client will be responsible for displaying the game board, accepting player input, and sending moves to the server.

The game will be implemented using the following technologies:

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

To make a move, enter the column number where you want to drop your piece (1-7).

The game board is displayed as follows:

```
    1 2 3 4 5 6 7
1 | - - - - - - - |
2 | - - - - - - - |
3 | - - - - - - - |
4 | - - - - - - - |
5 | - - - - - - - |
6 | - - - - - - - |
```