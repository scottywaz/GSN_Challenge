using System.Collections.Generic;
using UnityEngine;

public static class AI
{
	public static int DEPTH = 1;
	// Did my best to come up with them all, didn't want to waste too much time with this.
	public static Dictionary<int, List<string>> patterns = new Dictionary<int, List<string>>()
	{
		{1, new List<string>{"_pp__", "__pp_","p_p__", "p__p_", "p___p", "_p_p_", "_p__p", "__p_p", "___pp", "pp___" } }, // all 2s
		{2, new List<string>{"tppp__", "tp_pp_", "tp_p_p", "tp__pp", "t_ppp_", "t_pp_p", "t_p_pp", "t__pppt", "t_ppp_t", "__pppt", "_pp_pt", "p_p_pt", "pp__pt", "_ppp_t", "p_pp_t", "pp_p_t"} }, // one end 3s
		{4, new List<string>{"_ppp_", "_p_pp_", "_pp_p_"} }, // open ended 3s
		{6, new List<string>{"_ppppt", "tpppp_" } }, // one end open 4s
		{10, new List<string>{"_pppp_"} }, // open ended 4s
	};

	private static Node root;
	private static Coords bestMove = new Coords(0,0);
	private static string aiPiece;
	private static string playerPiece;

	/// <summary>
	/// Will produce the next move for the AI.
	/// </summary>
	/// <param name="gameboard">the board in its current state</param>
	/// <returns></returns>
	public static Coords NextAIMove(GameBoard gameboard)
	{
		// Ai is going first so just return the middle
		if (gameboard.pieceLocations.Count == 0)
		{
			int loc = Mathf.FloorToInt(GameBoard.NUM_ROWS_COLS / 2);
			return new Coords(loc, loc);
		}
		else
		{
			root = new Node();
			root.CopyFromGameBoard(gameboard);
			aiPiece = gameboard.isPlayerX ? "o" : "x";
			playerPiece = aiPiece == "x" ? "o" : "x";
			MinimaxAB(root, DEPTH, float.NegativeInfinity, float.PositiveInfinity, true);
			return bestMove;
		}
	}

	private static float MinimaxAB(Node node, int depth, float alpha, float beta, bool maximizingAI)
	{
		if(depth == 0)
		{
			return node.Evaluate(aiPiece) - (node.Evaluate(playerPiece) + 2); // adding 2 to opponent so it chooses to block in ties
		}

		if (maximizingAI)
		{
			node.CreateChildren();
			foreach (Node child in node.children)
			{
				float temp = MinimaxAB(child, depth - 1, alpha, beta, false);
				if (alpha < temp)
				{
					alpha = temp;
					if (node == root)
					{
						bestMove = child.lastMove;
					}
				}

				if (beta <= alpha)
				{
					return alpha;
				}
			}

			return alpha;
		}
		else
		{
			node.CreateChildren();
			foreach (Node child in node.children)
			{
				float temp = MinimaxAB(child, depth - 1, alpha, beta, true);
				if (beta > temp)
				{
					beta = temp;
					if (node == root)
					{
						bestMove = child.lastMove;
					}
				}

				if (beta <= alpha)
				{
					return beta;
				}
			}

			return beta;
		}
	}
}
