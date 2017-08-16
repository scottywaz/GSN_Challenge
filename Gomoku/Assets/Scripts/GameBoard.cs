using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PieceType
{
	none,
	player,
	ai
}

public class GameBoard : MonoBehaviour
{
	#region Unity Defined
	public GameObject menuScreen;
	public Text endGameText;
	public GameObject aiScreen;
	public GameObject grid;
	public GameObject square;
	#endregion

	[HideInInspector] public TileSquare[][] squares;
	[HideInInspector] public int dimension;
	[HideInInspector] public List<TileSquare> pieceLocations = new List<TileSquare>();
	[HideInInspector] public PieceType nextPlayer;
	[HideInInspector] public PieceType prevPlayer;

	public bool isPlayerX;
	public const int NUM_ROWS_COLS = 15;
	public const int NUM_TO_WIN = 5;
	private WaitForSeconds waitForSeconds = new WaitForSeconds(.5f);

	/// <summary>
	/// Creating the original game board. Setting up the squares and saving them.
	/// </summary>
	/// <param name="turnSelected">int value to determine if player is first or second</param>
	public void InitBoard(int turnSelected)
	{
		menuScreen.SetActive(false);
		isPlayerX = turnSelected == 1;
		dimension = NUM_ROWS_COLS;

		if (squares == null)
		{
			// Setup the inital board squares
			squares = new TileSquare[dimension][];
			for (int i = 0; i < dimension; i++)
			{
				squares[i] = new TileSquare[dimension];
				for (int j = 0; j < dimension; j++)
				{
					TileSquare tileSquare = GameObject.Instantiate(square).GetComponent<TileSquare>();
					tileSquare.coords = new Coords(i, j);
					tileSquare.isPlayerX = isPlayerX;
					tileSquare.CurrentPiece = PieceType.none;

					squares[i][j] = tileSquare;
					tileSquare.transform.SetParent(grid.transform);
					tileSquare.buttonComponent.onClick.AddListener(() => PlayerClickedSquare(tileSquare.gameObject));
				}
			}
		}
		else
		{
			for (int i = 0; i < dimension; i++)
			{
				for (int j = 0; j < dimension; j++)
				{
					TileSquare current = squares[i][j];
					current.isPlayerX = isPlayerX;
					current.CurrentPiece = PieceType.none;
				}
			}
		}

		// This starts the game
		if(!isPlayerX)
		{
			StartCoroutine(AITurn());
		}
		else
		{
			nextPlayer = PieceType.player;
		}
	}
	
	/// <summary>
	/// When any square is clicked, we check if the square is free and it is the player's turn
	/// And then we place a move and tell the AI to take a turn.
	/// </summary>
	/// <param name="square"></param>
	public void PlayerClickedSquare(GameObject square)
	{
		TileSquare tileSquare = square.GetComponent<TileSquare>();
		if (nextPlayer == PieceType.player)
		{
			if (tileSquare.CurrentPiece == PieceType.none)
			{
				PlaceMove(tileSquare, PieceType.player);
			}
		}
	}

	IEnumerator AITurn()
	{
		aiScreen.SetActive(true);
		yield return waitForSeconds;

		Coords cords = AI.NextAIMove(this);
		PlaceMove(squares[cords.row][cords.col], PieceType.ai);
		aiScreen.SetActive(false);
	}

	private void PlaceMove(TileSquare square, PieceType piece)
	{
		square.CurrentPiece = piece;
		
		if (CheckForWinner(square))
		{
			GameOver(piece == PieceType.player);
		}
		else
		{
			prevPlayer = piece;
			pieceLocations.Add(square);
			if (piece == PieceType.player)
			{
				nextPlayer = PieceType.ai;
				StartCoroutine(AITurn());
			}
			else
			{
				nextPlayer = PieceType.player;
			}
		}
	}

	public void Restart()
	{
		menuScreen.SetActive(true);
		pieceLocations = new List<TileSquare>();
	}

	/// <summary>
	/// Use the last move to determine if there was a winner beacuse of it
	/// </summary>
	/// <param name="lastMove">the last move made</param>
	/// <returns></returns>
	private bool CheckForWinner(TileSquare lastMove)
	{
		TileSquare[] row = squares[lastMove.coords.row];
		List<TileSquare> col = new List<TileSquare>();
		List<TileSquare> rDiag = new List<TileSquare>();
		List<TileSquare> lDiag = new List<TileSquare>();

		int lowerNum = Mathf.Min(lastMove.coords.row, lastMove.coords.col);
		Coords diagR = new Coords(lastMove.coords.row - lowerNum, lastMove.coords.col - lowerNum);

		lowerNum = Mathf.Min(lastMove.coords.row, dimension - lastMove.coords.col - 1);
		Coords diagL = new Coords(lastMove.coords.row - lowerNum, lastMove.coords.col + lowerNum);

		int numInARow = 0;
		for (int i = 0; i < row.Length; i++)
		{
			col.Add(squares[i][lastMove.coords.col]);
			if (diagR.row + i < row.Length && diagR.col + i < row.Length) rDiag.Add(squares[diagR.row + i][diagR.col + i]);
			if (diagL.row + i < row.Length && diagL.col - i >= 0) lDiag.Add(squares[diagL.row + i][diagL.col - i]);

			if (lastMove.CurrentPiece != row[i].CurrentPiece)
			{
				numInARow = 0;
				continue;
			}

			numInARow++;
			if (numInARow == NUM_TO_WIN)
				return true;
		}

		int numInACol = 0;
		for (int j = 0; j < col.Count; j++)
		{
			if (lastMove.CurrentPiece != col[j].CurrentPiece)
			{
				numInACol = 0;
				continue;
			}

			numInACol++;
			if (numInACol == NUM_TO_WIN)
				return true;
		}

		int numInRDiag = 0;
		if (rDiag.Count >= NUM_TO_WIN)
		{
			for (int k = 0; k < rDiag.Count; k++)
			{
				if (lastMove.CurrentPiece != rDiag[k].CurrentPiece)
				{
					numInRDiag = 0;
					continue;
				}

				numInRDiag++;
				if (numInRDiag == NUM_TO_WIN)
					return true;
			}
		}

		int numInLDiag = 0;
		if (lDiag.Count >= NUM_TO_WIN)
		{
			for (int m = 0; m < lDiag.Count; m++)
			{
				if(lastMove.CurrentPiece != lDiag[m].CurrentPiece)
				{
					numInLDiag = 0;
					continue;
				}

				numInLDiag++;
				if (numInLDiag == NUM_TO_WIN)
					return true;
			}
		}

		return false;
	}

	private void GameOver(bool playerWon)
	{
		endGameText.text = playerWon ? "Congraulations! You won. Play Again?" : "You Lost! Try Again?";
		endGameText.gameObject.SetActive(true);
		Restart();
	}
}