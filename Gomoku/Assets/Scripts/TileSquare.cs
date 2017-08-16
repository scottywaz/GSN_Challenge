using UnityEngine;
using UnityEngine.UI;

public class TileSquare : MonoBehaviour
{
	#region Unity Defined
	public Button buttonComponent;
	public GameObject xPiece, oPiece;
	#endregion

	[HideInInspector] public Coords coords;
	[HideInInspector] public bool isPlayerX;

	public PieceType CurrentPiece
	{
		get { return currentPiece; }
		set
		{
			currentPiece = value;
			xPiece.SetActive(false);
			oPiece.SetActive(false);

			if(currentPiece == PieceType.ai)
			{
				if (isPlayerX) oPiece.SetActive(true);
				else xPiece.SetActive(true);
			}
			else if(currentPiece == PieceType.player)
			{
				if (isPlayerX) xPiece.SetActive(true);
				else oPiece.SetActive(true);
			}
		}
	}
	private PieceType currentPiece;
}