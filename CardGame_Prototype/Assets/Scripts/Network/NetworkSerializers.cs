using Mirror;

public static class NetworkSerializers
{
    
    public static void WriteCardNullable(this NetworkWriter writer, Card? card)
    {
        writer.WriteBool(card.HasValue);
        if (card.HasValue)
        {
            writer.Write<Card>(card.Value);
        }
    }

    public static Card? ReadCardNullable(this NetworkReader reader)
    {
        var hasReadValue = reader.ReadBool();
        if (hasReadValue)
        {
            return reader.Read<Card>();
        }
        return null;
    }
}