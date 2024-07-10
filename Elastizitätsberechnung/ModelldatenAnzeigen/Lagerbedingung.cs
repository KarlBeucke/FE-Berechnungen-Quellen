namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

internal class Lagerbedingung
{
    public Lagerbedingung(string lagerId, string nodeId, string[] vordefiniert)
    {
        LagerId = lagerId;
        NodeId = nodeId;
        Vordefiniert = vordefiniert;
    }

    public string LagerId { get; }
    public string NodeId { get; }
    public string[] Vordefiniert { get; }
}