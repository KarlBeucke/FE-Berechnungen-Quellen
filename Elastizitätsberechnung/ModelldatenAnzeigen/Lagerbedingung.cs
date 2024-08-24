namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

internal class Lagerbedingung(string lagerId, string nodeId, string[] vordefiniert)
{
    public string LagerId { get; } = lagerId;
    public string NodeId { get; } = nodeId;
    public string[] Vordefiniert { get; } = vordefiniert;
}