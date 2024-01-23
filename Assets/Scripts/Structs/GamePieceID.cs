public readonly struct GamePieceID
{
    public readonly PlayerColor color;
    public readonly PieceTypeID typeID;
    public readonly int index;

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
