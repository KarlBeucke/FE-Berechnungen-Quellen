using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public class Stabendkräfte
{
    public string ElementId { get; set; }
    public double Na { get; set; }
    public double Qa { get; set; }
    public double Ma { get; set; }
    public double Nb { get; set; }
    public double Qb { get; set; }
    public double Mb { get; set; }

    public Stabendkräfte(string elementId, IReadOnlyList<double> endkräfte)
    {
        ElementId = elementId;
        switch (endkräfte.Count)
        {
            case 2:
                Na = endkräfte[0]; Nb = endkräfte[1];
                break;
            case 6:
                Na = endkräfte[0]; Qa = endkräfte[1]; Ma = endkräfte[2];
                Nb = endkräfte[3]; Qb = endkräfte[4]; Mb = endkräfte[5];
                break;
        }
    }
}