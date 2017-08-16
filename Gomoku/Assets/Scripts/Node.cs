using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public struct Coords
{
	public Coords(int r, int c)
	{
		row = r;
		col = c;
	}
	public int row, col;
}

public class Node
{
	public string[][] squares;
	public int dimension;
	public string nextPlayer;
	public string prevPlayer;
	public List<Coords> piecePositions;
	public List<Node> children;
	public Coords lastMove;

	private string playerPiece;

	/// <summary>
	/// Always creating for AI, so prevPlayer is the player's piece and 
	/// nextPlayer is the ai's piece
	/// </summary>
	/// <param name="gameboard">The board to copy from</param>
	public void CopyFromGameBoard(GameBoard gameboard)
	{
		dimension = gameboard.dimension;
		squares = new string[dimension][];

		if(gameboard.prevPlayer == PieceType.player)
		{
			prevPlayer = gameboard.isPlayerX ? "x" : "o";
			playerPiece = prevPlayer;
		}
		else
		{
			prevPlayer = gameboard.isPlayerX ? "o" : "x";
			playerPiece = prevPlayer == "o" ? "x" : "o";
		}
		nextPlayer = prevPlayer == "x" ? "o" : "x";

		for (int row = 0; row < dimension; row++)
		{
			squares[row] = new string[dimension];
			for(int col = 0; col < dimension; col++)
			{
				string space = "_";
				if(gameboard.squares[row][col].xPiece.activeSelf)
				{
					space = "x";
				}
				else if(gameboard.squares[row][col].oPiece.activeSelf)
				{
					space = "o";
				}

				squares[row][col] = space;
			}
		}

		piecePositions = new List<Coords>();
		foreach(TileSquare square in gameboard.pieceLocations)
		{
			piecePositions.Add(square.coords);
		}
	}

	public void CopyFromNode(Node node)
	{
		dimension = node.dimension;
		squares = new string[dimension][];
		for(int i = 0; i < dimension; i++)
		{
			squares[i] = node.squares[i].Clone() as string[];
		}

		piecePositions = new List<Coords>(node.piecePositions);
		playerPiece = node.playerPiece;
		prevPlayer = node.prevPlayer;
		nextPlayer = node.nextPlayer;
	}

	/// <summary>
	/// Places a move on the node and updates correct values
	/// </summary>
	/// <param name="cords">coordinates to place the piece</param>
	/// <param name="piece">piece type</param>
	public void PlaceMove(Coords cords, string piece)
	{
		piecePositions.Add(cords);
		lastMove = cords;
		prevPlayer = piece;
		squares[cords.row][cords.col] = piece;
		nextPlayer = piece == "x" ? "o" : "x";
	}

	public float Evaluate(string piece = "x")
	{
		string opposite = piece == "x" ? "o" : "x";
		float result = 0;

		for (int i = 0; i < dimension; i++)
		{
			string rowString = string.Join("", squares[i]);
			string colString = "";
			string rDiagLString = "";
			string rDiagRString = "";
			string lDiagLString = "";
			string lDiagRString = "";

			// Find matches in the cols
			for (int j = 0; j < dimension; j++)
			{
				colString += squares[j][i];
				if(j + i < dimension) rDiagLString += squares[j + i][j];
				if (i != 0 && j + i < dimension) rDiagRString += squares[j][j + i];
				if(dimension - 1 - i >= 0) lDiagLString += squares[j][dimension - 1 - i];
				if(i != 0 && dimension - 1 - j >= 0 && j + i < dimension) lDiagRString += squares[j + i][dimension - 1 - j];
			}

			string winning = piece + piece + piece + piece + piece;
			// Check if winner and break free
			if(rowString.Contains(winning) || colString.Contains(winning) ||
				rDiagLString.Contains(winning) || rDiagRString.Contains(winning) ||
				lDiagLString.Contains(winning) || lDiagRString.Contains(winning))
			{
				return float.PositiveInfinity;
			}

			foreach(var entry in AI.patterns)
			{
				int score = entry.Key;
				List<string> patterns = entry.Value;
				int numFound = 0;

				foreach(string pattern in patterns)
				{
					string correctPattern = pattern.Replace("p", piece).Replace("t", opposite);
					numFound += Regex.Matches(Regex.Escape(rowString), correctPattern).Count;
					numFound += Regex.Matches(Regex.Escape(colString), correctPattern).Count;
					if(rDiagLString.Length >= 5) numFound += Regex.Matches(Regex.Escape(rDiagLString), correctPattern).Count;
					if (rDiagRString.Length >= 5) numFound += Regex.Matches(Regex.Escape(rDiagRString), correctPattern).Count;
					if (rDiagLString.Length >= 5) numFound += Regex.Matches(Regex.Escape(rDiagLString), correctPattern).Count;
					if (lDiagRString.Length >= 5) numFound += Regex.Matches(Regex.Escape(lDiagRString), correctPattern).Count;
				}

				result += Mathf.Pow(2, score) * numFound;
			}
		}
		
		return result;
	}

	/// <summary>
	/// We find all the empty squares that are near taken squares
	/// then we create a new node for each open square and take that square with the next 
	/// player's piece
	/// </summary>
	public void CreateChildren()
	{
		children = new List<Node>();
		List<Coords> emptySquares = FindEmptySquaresNearTakenOnes();
		foreach(Coords cord in emptySquares)
		{
			Node childNode = new Node();
			childNode.CopyFromNode(this);
			childNode.PlaceMove(cord, nextPlayer);
			children.Add(childNode);
		}
	}

	/// <summary>
	/// This function first grabs the list of player squares and searches near them for empty squares. Puts those in the front of the list
	/// And then it adds all the rest of the squares that haven't already been added.
	/// </summary>
	/// <returns>returns the list</returns>
	private List<Coords> FindEmptySquaresNearTakenOnes()
	{
		List<Coords> listOfEmpties = new List<Coords>();
		foreach(Coords cord in piecePositions)
		{
			if(cord.row - 1 >= 0)
			{
				if (squares[cord.row-1][cord.col] == "_") listOfEmpties.Add(new Coords(cord.row - 1, cord.col)); // north
				if (cord.col - 1 > 0 && squares[cord.row - 1][cord.col - 1] == "_") listOfEmpties.Add(new Coords(cord.row - 1, cord.col - 1)); // northwest
				if (cord.col + 1 < dimension && squares[cord.row - 1][cord.col + 1] == "_") listOfEmpties.Add(new Coords(cord.row - 1, cord.col + 1)); // northeast
			}

			if(cord.row + 1 < dimension)
			{
				if(squares[cord.row + 1][cord.col] == "_") listOfEmpties.Add(new Coords(cord.row + 1, cord.col)); // south
				if (cord.col - 1 > 0 && squares[cord.row + 1][cord.col-1] == "_") listOfEmpties.Add(new Coords(cord.row + 1, cord.col - 1)); // southwest
				if (cord.col + 1 < dimension && squares[cord.row + 1][cord.col+1] == "_") listOfEmpties.Add(new Coords(cord.row + 1, cord.col + 1)); // southeast
			}

			if (cord.col - 1 > 0 && squares[cord.row][cord.col - 1] == "_") listOfEmpties.Add(new Coords(cord.row, cord.col - 1));
			if (cord.col + 1 < dimension && squares[cord.row][cord.col + 1] == "_") listOfEmpties.Add(new Coords(cord.row, cord.col + 1));
		}

		return listOfEmpties;
	}
}
