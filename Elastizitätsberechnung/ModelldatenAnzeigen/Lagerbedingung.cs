namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

internal class Lagerbedingung
{
    public string LagerId { get; }
    public string NodeId { get; }
    public string[] Vordefiniert { get; }

    public Lagerbedingung(string lagerId, string nodeId, string[] vordefiniert)
    {
        LagerId = lagerId;
        NodeId = nodeId;
        Vordefiniert = vordefiniert;
    }
}