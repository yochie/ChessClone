using Mirror;

public readonly struct GamePieceID
{
    public readonly PlayerColor color;
    public readonly PieceTypeID typeID;
    public readonly int index;

    //to ensure no overlap with base piece indexes, starting from 1000
    private static int nextIndexGenerated = 1000;

    //used for creating new IDs for promotedPieces
    [Server]
    public static GamePieceID CreateGamePieceIDWithUniqueIndex(PlayerColor ownerColor, PieceTypeID typeID)
    {
        int index = GamePieceID.GenerateNewUniqueIndex();
        return new GamePieceID(ownerColor, typeID, index);
    }

    [Server]
    private static int GenerateNewUniqueIndex()
    {
        return GamePieceID.nextIndexGenerated++;
    }

    public GamePieceID(PlayerColor ownerColor, PieceTypeID typeID, int index)
    {
        this.color = ownerColor;
        this.typeID = typeID;
        this.index = index;
    }

    public override string ToString()
    {
        return string.Format("{0} {1} ({2})", color, typeID, index);
    }
}
