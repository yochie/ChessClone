using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class PieceTypeData : ScriptableObject
{
    [SerializeField]
    private List<ScriptableObject> pieceTypes;

    public IPieceType GetPieceTypeForByID(PieceTypeID typeID)
    {
        return this.pieceTypes.Cast<IPieceType>().Single(pieceType => pieceType.ForPieceTypeID == typeID);
    }
}
