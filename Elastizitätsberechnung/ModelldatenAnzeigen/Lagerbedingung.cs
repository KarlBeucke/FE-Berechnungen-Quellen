namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

internal class Lagerbedingung(string lagerId, string nodeId, string[] vordefiniert)
{
    public string LagerId { get; } = lagerId;
    public string NodeId { get; } = nodeId;
    public string[] Vordefiniert { get; } = vordefiniert;
    
}
internal class LagerbedingungFläche(string lagerId, string nodeId, string face, string[] vordefiniert)
{
    public string LagerId { get; } = lagerId;
    public string NodeId { get; } = nodeId;
    public string Face { get; } = face;
    public string[] Vordefiniert { get; } = vordefiniert;

}
internal class LagerbedingungBoussinesq(string lagerId, string nodeId, string[] vordefiniert, string material, string face)
{
    public string LagerId { get; } = lagerId;
    public string NodeId { get; } = nodeId;
    public string[] Vordefiniert { get; } = vordefiniert;
    public string Material { get; } = material;
    public string Face { get; } = face;
}