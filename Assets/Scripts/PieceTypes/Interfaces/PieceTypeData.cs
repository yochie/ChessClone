using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class PieceTypeData : ScriptableObject
{
    [SerializeField]
    private List<ScriptableObject> pieceTypes;

    [SerializeField]
    private List<BoardPiece> boardPiecePrefabs;

    public IPieceType GetPieceTypeByID(PieceTypeID typeID)
    {
        return this.pieceTypes.Cast<IPieceType>().Single(pieceType => pieceType.ForPieceTypeID == typeID);
    }

    public BoardPiece GetBoardPiecePrefab(PieceTypeID typeID, PlayerColor color)
    {
        return this.boardPiecePrefabs.Single(prefab => prefab.GetPieceTypeID() == typeID && prefab.GetOwnerColor() == color);
    }
}
