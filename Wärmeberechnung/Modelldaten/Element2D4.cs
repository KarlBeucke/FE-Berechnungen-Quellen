namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element2D4 : AbstraktLinear2D4
{
    private FeModell Modell { get; set; }
    private readonly double[,] _elementMatrix = new double[4, 4];
    private readonly double[] _elementTemperatures = new double[4]; // at element nodes
    private Material _material;
    private double[] SpezifischeWärmeMatrix { get; }


    public Element2D4(string[] eNodes, string materialId, FeModell feModell)
    {
        // The null-coalescing operator ?? returns the value of its left-hand operand
        // if it isn't null; otherwise, it evaluates the right-hand operand and returns
        // its result. The ?? operator doesn't evaluate its right-hand operand if the
        // left-hand operand evaluates to non-null.
        Modell = feModell;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 4;
        KnotenIds = eNodes;
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (!Modell.Knoten.TryGetValue(KnotenIds[i], out var node))
                throw new ModellAusnahme("\nElement2D4: Knoten " + KnotenIds[i] + "nicht im Modell gefunden");

            Knoten[i] = node;
        }
        SpezifischeWärmeMatrix = new double[4];
        ElementMaterialId = materialId;
    }

    public Element2D4(string id, string[] eNodes, string materialId, FeModell feModell)
    {

        Modell = feModell;
        ElementId = id;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 4;
        KnotenIds = eNodes;
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (!Modell.Knoten.TryGetValue(KnotenIds[i], out var node))
                throw new ModellAusnahme("\nElement2D4: Knoten " + KnotenIds[i] + "nicht im Modell gefunden");

            Knoten[i] = node;
        }

        ElementMaterialId = materialId;
    }

    public override double[,] BerechneElementMatrix()
    {
        double[] gaussCoord = [-1 / Math.Sqrt(3), 1 / Math.Sqrt(3)];
        if (!Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial))
            throw new ModellAusnahme("\nElement2D4: Elementmaterial " + ElementMaterialId + "nicht im Modell gefunden");

        _material = (Material)abstractMaterial;
        ElementMaterial = _material;

        MatrizenAlgebra.Clear(_elementMatrix);
        var c = new[,] { { _material.MaterialWerte[0], 0 }, { 0, _material.MaterialWerte[1] } };
        foreach (var coor1 in gaussCoord)
        {
            foreach (var coor2 in gaussCoord)
            {
                BerechneGeometrie(coor1, coor2);
                Sx = BerechneSx(coor1, coor2);
                // Ke = Sx*c*SxT*determinant
                var temp = MatrizenAlgebra.Mult(Sx, c);
                MatrizenAlgebra.MultAddMatrixTransposed(_elementMatrix, Determinant, temp, Sx);
            }
        }

        return _elementMatrix;
    }

    // Compute diagonal Specific Heat Matrix
    public override double[] BerechneDiagonalMatrix()
    {
        //BerechneGeometrie();
        // Me = dichte * leitfähigkeit * 0.5*determinante/4 (area/4)
        SpezifischeWärmeMatrix[0] = _material.MaterialWerte[3] * Determinant / 8;
        SpezifischeWärmeMatrix[1] = SpezifischeWärmeMatrix[0];
        SpezifischeWärmeMatrix[2] = SpezifischeWärmeMatrix[0];
        SpezifischeWärmeMatrix[3] = SpezifischeWärmeMatrix[0];
        return SpezifischeWärmeMatrix;
    }

    // Compute the heat state at the (z0,z1) of the element
    public override double[] BerechneZustandsvektor()
    {
        var elementWärmeStatus = new double[2]; // in element
        return elementWärmeStatus;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        _ = new double[2]; // in element
        BerechneGeometrie(z0, z1);
        Sx = BerechneSx(z0, z1);
        for (var i = 0; i < KnotenProElement; i++)
            _elementTemperatures[i] = Knoten[i].Knotenfreiheitsgrade[0];
        var conductivity = _material.MaterialWerte[0];
        var midpointHeatState = MatrizenAlgebra.MultTransposed(-conductivity, Sx, _elementTemperatures);
        return midpointHeatState;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[KnotenProElement * ElementFreiheitsgrade];
        var counter = 0;
        for (var i = 0; i < KnotenProElement; i++)
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
    }

    public override Point BerechneSchwerpunkt()
    {
        var p = new Point[4];
        p[0] = new Point(Knoten[0].Koordinaten[0], Knoten[0].Koordinaten[1]);
        p[1] = new Point(Knoten[1].Koordinaten[0], Knoten[1].Koordinaten[1]);
        p[2] = new Point(Knoten[2].Koordinaten[0], Knoten[2].Koordinaten[1]);
        p[3] = new Point(Knoten[3].Koordinaten[0], Knoten[3].Koordinaten[1]);
        var cg = FEBibliothek.Werkzeuge.FeGeometrie.PolygonSchwerpunkt(p);
        return cg;
    }
}