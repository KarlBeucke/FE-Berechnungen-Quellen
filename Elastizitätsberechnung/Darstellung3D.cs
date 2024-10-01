using FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung;

public class Darstellung3D
{
    public readonly List<GeometryModel3D> Kanten = [];
    public readonly List<GeometryModel3D> KnotenLasten = [];

    public readonly List<GeometryModel3D> Koordinaten = [];
    private readonly double maxX, minY, maxY, minZ, maxZ;
    public readonly double MinX;
    public readonly FeModell Modell;
    public readonly List<GeometryModel3D> Oberflächen = [];

    // Erzeugung eines Dictionary, um Dreieckspunkte effizient zu finden
    private readonly Dictionary<Point3D, int> punktDictionary = new();
    public readonly List<GeometryModel3D> RandbedingungenFest = [];
    public readonly List<GeometryModel3D> RandbedingungenVor = [];
    public readonly List<GeometryModel3D> SpannungenXx = [];
    public readonly List<GeometryModel3D> SpannungenXy = [];
    public readonly List<GeometryModel3D> SpannungenYy = [];
    public readonly List<GeometryModel3D> SpannungenYz = [];
    public readonly List<GeometryModel3D> SpannungenZx = [];
    public readonly List<GeometryModel3D> SpannungenZz = [];
    public readonly List<GeometryModel3D> Verformungen = [];
    private GeometryModel3D drahtModell;
    private GeometryModel3D knotenLastenModell;
    private GeometryModel3D oberflächenModell;
    private GeometryModel3D randbedingungenBoussinesqModell;
    private GeometryModel3D randbedingungenModell;
    private GeometryModel3D spannungenModell;
    public double ÜberhöhungVerformung = 1;
    private GeometryModel3D verformungsModell;

    public Darstellung3D(FeModell feModell)
    {
        Modell = feModell;

        var x = new List<double>();
        var y = new List<double>();
        var z = new List<double>();
        foreach (var item in Modell.Knoten)
        {
            x.Add(item.Value.Koordinaten[0]);
            maxX = x.Max();
            MinX = x.Min();
            y.Add(item.Value.Koordinaten[2]);
            maxY = y.Max();
            minY = y.Min();
            z.Add(item.Value.Koordinaten[1]);
            maxZ = z.Max();
            minZ = z.Min();
        }
    }

    public void Koordinatensystem(Model3DGroup modelGroup)
    {
        // Point3D als Ursprung der Rect3D, i.d.R. die hintere untere linke Ecke.
        // Die positive Y-Achse im 3D-Koordinatensystem zeigt nach oben (vorausgesetzt,
        // dass die-Eigenschaft der Kamera UpDirection positiv ist). 

        var meshX = new MeshGeometry3D();
        var koordinatenModell = XAchse(meshX);
        modelGroup.Children.Add(koordinatenModell);
        Koordinaten.Add(koordinatenModell);

        var meshY = new MeshGeometry3D();
        koordinatenModell = YAchse(meshY);
        modelGroup.Children.Add(koordinatenModell);
        Koordinaten.Add(koordinatenModell);

        var meshZ = new MeshGeometry3D();
        koordinatenModell = ZAchse(meshZ);
        modelGroup.Children.Add(koordinatenModell);
        Koordinaten.Add(koordinatenModell);
    }

    private GeometryModel3D XAchse(MeshGeometry3D mesh)
    {
        const double wichte = 0.1;
        const double vektorLänge = 1.0;
        const double achsüberstand = 0.8;
        // x-Achse
        var start = new Point3D(-achsüberstand, 0, 0);
        var end = new Point3D(vektorLänge, 0, 0);
        var punkte = QuaderPunkteX(start, end, wichte);
        ErzeugQuader(mesh, punkte);

        const double pfeillänge = 0.4;
        const double breite = 2 * wichte;
        var pfeile = PfeilPunkteX(end, pfeillänge, breite);
        ErzeugPfeilspitze(mesh, pfeile);

        var material = new DiffuseMaterial(Brushes.Red);
        var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        return model;
    }

    private GeometryModel3D YAchse(MeshGeometry3D mesh)
    {
        const double wichte = 0.1;
        const double vektorLänge = 1.0;
        const double achsüberstand = 0.8;
        // y-Achse
        var start = new Point3D(0, -achsüberstand, 0);
        var end = new Point3D(0, vektorLänge, 0);
        var punkte = QuaderPunkteY(start, end, wichte);
        ErzeugQuader(mesh, punkte);

        const double pfeillänge = 0.4;
        const double breite = 2 * wichte;
        var pfeile = PfeilPunkteY(end, pfeillänge, breite);
        ErzeugPfeilspitze(mesh, pfeile);

        var material = new DiffuseMaterial(Brushes.Green);
        var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        return model;
    }

    private GeometryModel3D ZAchse(MeshGeometry3D mesh)
    {
        const double wichte = 0.1;
        const double vektorLänge = 1.0;
        const double achsüberstand = 0.8;
        // z-Achse
        var start = new Point3D(0, 0, -achsüberstand);
        var end = new Point3D(0, 0, vektorLänge);
        var punkte = QuaderPunkteZ(start, end, wichte);
        ErzeugQuader(mesh, punkte);

        const double pfeillänge = 0.4;
        const double breite = 2 * wichte;
        var pfeile = PfeilPunkteZ(end, pfeillänge, breite);
        ErzeugPfeilspitze(mesh, pfeile);

        var material = new DiffuseMaterial(Brushes.Blue);
        var model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        return model;
    }

    public void UnverformteGeometrie(Model3DGroup modelGroup, bool volumen)
    {
        var mesh = new MeshGeometry3D();
        foreach (var item in Modell.Elemente)
        {
            var punkte = new Point3DCollection();
            punktDictionary.Clear();
            punkte.Clear();
            foreach (var knotenId in item.Value.KnotenIds)
                if (Modell.Knoten.TryGetValue(knotenId, out var knoten))
                    punkte.Add(new Point3D(knoten.Koordinaten[0], -knoten.Koordinaten[2], knoten.Koordinaten[1]));

            ErzeugQuader(mesh, punkte);

            if (volumen)
            {
                // Erzeugung des Oberflächenmodells
                // Darstellung des Materials des Oberflächenmodells in LightGreen
                var surfaceMaterial = new DiffuseMaterial(Brushes.LightGreen);
                oberflächenModell = new GeometryModel3D(mesh, surfaceMaterial) { BackMaterial = surfaceMaterial };
                // Sichtbarkeit der Oberfläche von beiden Seiten
                // Hinzufügen des Modells zur Modellgruppe
                modelGroup.Children.Add(oberflächenModell);

                Oberflächen.Add(oberflächenModell);
            }

            // Erzeugung des Drahtmodells, thickness (Wichte der kanten) = 0.02
            const double kantenwichte = 0.02;
            var wireframe = mesh.ToWireframe(kantenwichte);
            var wireframeMaterial = new DiffuseMaterial(Brushes.Black);
            drahtModell = new GeometryModel3D(wireframe, wireframeMaterial);
            modelGroup.Children.Add(drahtModell);

            Kanten.Add(drahtModell);
        }
    }

    public void VerformteGeometrie(Model3DGroup modelGroup)
    {
        var mesh = new MeshGeometry3D();
        foreach (var item in Modell.Elemente)
        {
            var punkte = new Point3DCollection();
            punktDictionary.Clear();
            punkte.Clear();
            foreach (var knotenId in item.Value.KnotenIds)
                if (Modell.Knoten.TryGetValue(knotenId, out var knoten))
                    punkte.Add(new Point3D(
                        knoten.Koordinaten[0] + knoten.Knotenfreiheitsgrade[0] * ÜberhöhungVerformung,
                        -knoten.Koordinaten[2] - knoten.Knotenfreiheitsgrade[2] * ÜberhöhungVerformung,
                        knoten.Koordinaten[1] + knoten.Knotenfreiheitsgrade[1] * ÜberhöhungVerformung));

            ErzeugQuader(mesh, punkte);

            // Erzeugung des Drahtmodells, thickness (Wichte der kanten) = 0.02
            const double kantenwichte = 0.02;
            var verformung = mesh.ToWireframe(kantenwichte);
            var verformungMaterial = new DiffuseMaterial(Brushes.Red);
            verformungsModell = new GeometryModel3D(verformung, verformungMaterial);
            modelGroup.Children.Add(verformungsModell);

            Verformungen.Add(verformungsModell);
        }
    }

    public void Randbedingungen(Model3DGroup modelGroup)
    {
        const double d = 0.1;
        var randbedingungenFestMaterial = new DiffuseMaterial(Brushes.Red);
        var randbedingungenVorMaterial = new DiffuseMaterial(Brushes.LightPink);

        foreach (var item in ElastizitätsParser.ParseElastizitätsRandbedingungen.Faces)
        {
            var punkte = new Point3DCollection();
            punktDictionary.Clear();
            punkte.Clear();
            var mesh = new MeshGeometry3D();

            switch (item)
            {
                case "X0": //links
                    punkte.Add(new Point3D(MinX, -minY, minZ)); //0
                    punkte.Add(new Point3D(MinX, -maxY, minZ)); //1
                    punkte.Add(new Point3D(MinX, -maxY, maxZ)); //2
                    punkte.Add(new Point3D(MinX, -minY, maxZ)); //3
                    punkte.Add(new Point3D(MinX - d, -minY, minZ)); //4
                    punkte.Add(new Point3D(MinX - d, -maxY, minZ)); //5
                    punkte.Add(new Point3D(MinX - d, -maxY, maxZ)); //6
                    punkte.Add(new Point3D(MinX - d, -minY, maxZ)); //7

                    ErzeugQuader(mesh, punkte);

                    randbedingungenModell = new GeometryModel3D(mesh, randbedingungenFestMaterial)
                    { BackMaterial = randbedingungenFestMaterial };
                    modelGroup.Children.Add(randbedingungenModell);

                    RandbedingungenFest.Add(randbedingungenModell);
                    break;

                case "Y0": // hinten
                    punkte.Add(new Point3D(MinX, -minY, minZ)); //0
                    punkte.Add(new Point3D(MinX, -maxY, minZ)); //1
                    punkte.Add(new Point3D(maxX, -maxY, minZ)); //2
                    punkte.Add(new Point3D(maxX, -minY, minZ)); //3
                    punkte.Add(new Point3D(MinX, -minY, minZ - d)); //4
                    punkte.Add(new Point3D(MinX, -maxY, minZ - d)); //5
                    punkte.Add(new Point3D(maxX, -maxY, minZ - d)); //6
                    punkte.Add(new Point3D(maxX, -minY, minZ - d)); //7

                    ErzeugQuader(mesh, punkte);

                    randbedingungenModell = new GeometryModel3D(mesh, randbedingungenFestMaterial)
                    { BackMaterial = randbedingungenFestMaterial };
                    modelGroup.Children.Add(randbedingungenModell);

                    RandbedingungenFest.Add(randbedingungenModell);
                    break;
            }
        }

        foreach (var item2 in ElastizitätsParser.ParseElastizitätsRandbedingungen.Faces)
        {
            var punkte = new Point3DCollection();
            punktDictionary.Clear();
            punkte.Clear();
            var mesh = new MeshGeometry3D();

            switch (item2)
            {
                case "XMax": // rechts
                    punkte.Add(new Point3D(maxX, -minY, minZ)); //0
                    punkte.Add(new Point3D(maxX, -maxY, minZ)); //1
                    punkte.Add(new Point3D(maxX, -maxY, maxZ)); //2
                    punkte.Add(new Point3D(maxX, -minY, maxZ)); //3
                    punkte.Add(new Point3D(maxX + d, -minY, minZ)); //4
                    punkte.Add(new Point3D(maxX + d, -maxY, minZ)); //5
                    punkte.Add(new Point3D(maxX + d, -maxY, maxZ)); //6
                    punkte.Add(new Point3D(maxX + d, -minY, maxZ)); //7

                    ErzeugQuader(mesh, punkte);

                    randbedingungenBoussinesqModell = new GeometryModel3D(mesh, randbedingungenVorMaterial)
                    { BackMaterial = randbedingungenVorMaterial };
                    modelGroup.Children.Add(randbedingungenBoussinesqModell);
                    RandbedingungenVor.Add(randbedingungenBoussinesqModell);
                    break;

                case "YMax": // unten
                    punkte.Add(new Point3D(MinX, -maxY, minZ)); //0
                    punkte.Add(new Point3D(MinX, -maxY, maxZ)); //1
                    punkte.Add(new Point3D(maxX, -maxY, maxZ)); //2
                    punkte.Add(new Point3D(maxX, -maxY, minZ)); //3
                    punkte.Add(new Point3D(MinX, -maxY - d, minZ)); //4
                    punkte.Add(new Point3D(MinX, -maxY - d, maxZ)); //5
                    punkte.Add(new Point3D(maxX, -maxY - d, maxZ)); //6
                    punkte.Add(new Point3D(maxX, -maxY - d, minZ)); //7

                    ErzeugQuader(mesh, punkte);

                    randbedingungenBoussinesqModell = new GeometryModel3D(mesh, randbedingungenVorMaterial)
                    { BackMaterial = randbedingungenVorMaterial };
                    modelGroup.Children.Add(randbedingungenBoussinesqModell);
                    RandbedingungenVor.Add(randbedingungenBoussinesqModell);
                    break;

                case "ZMax": // vorn
                    punkte.Add(new Point3D(MinX, -minY, maxZ)); //0
                    punkte.Add(new Point3D(MinX, -maxY, maxZ)); //1
                    punkte.Add(new Point3D(maxX, -maxY, maxZ)); //2
                    punkte.Add(new Point3D(maxX, -minY, maxZ)); //3  
                    punkte.Add(new Point3D(MinX, -minY, maxZ + d)); //4
                    punkte.Add(new Point3D(MinX, -maxY, maxZ + d)); //5
                    punkte.Add(new Point3D(maxX, -maxY, maxZ + d)); //6
                    punkte.Add(new Point3D(maxX, -minY, maxZ + d)); //7

                    ErzeugQuader(mesh, punkte);

                    randbedingungenBoussinesqModell = new GeometryModel3D(mesh, randbedingungenVorMaterial)
                    { BackMaterial = randbedingungenVorMaterial };
                    modelGroup.Children.Add(randbedingungenBoussinesqModell);
                    RandbedingungenVor.Add(randbedingungenBoussinesqModell);
                    break;
            }
        }
    }

    public void Knotenlasten(Model3DGroup modelGroup)
    {
        var mesh = new MeshGeometry3D();
        double lastWert = 0;
        var lastAngriff = new Point3D();

        foreach (var last in Modell.Lasten)
        {
            var lastRichtung = new Vector3D(0, 0, 0);
            const double lastSkalierung = 10.0;
            var knotenId = last.Value.KnotenId;
            if (Modell.Knoten.TryGetValue(knotenId, out var knoten))
            {
                lastAngriff.X = knoten.Koordinaten[0];
                lastAngriff.Y = -knoten.Koordinaten[2];
                lastAngriff.Z = knoten.Koordinaten[1];
            }

            if (Math.Abs(last.Value.Lastwerte[0]) > 0)
            {
                lastRichtung.X = 1;
                lastWert = lastSkalierung * last.Value.Lastwerte[0];
            }
            else if (Math.Abs(last.Value.Lastwerte[2]) > 0)
            {
                lastRichtung.Y = -1;
                lastWert = lastSkalierung * last.Value.Lastwerte[2];
            }
            else if (Math.Abs(last.Value.Lastwerte[1]) > 0)
            {
                lastRichtung.Z = 1;
                lastWert = lastSkalierung * last.Value.Lastwerte[1];
            }

            knotenLastenModell = LastVektor(mesh, lastAngriff, lastRichtung, lastWert);
            modelGroup.Children.Add(knotenLastenModell);
            KnotenLasten.Add(knotenLastenModell);
        }
    }

    private GeometryModel3D LastVektor(MeshGeometry3D mesh, Point3D lastAngriff,
        Vector3D lastRichtung, double lastWert)
    {
        const double wichte = 0.1;
        const double pfeilLänge = 0.4;
        var start = lastAngriff;
        var end = new Point3D(0, 0, 0);
        var model = new GeometryModel3D();

        // Horizontallast in x
        if (Math.Abs(lastRichtung.X) > 0)
        {
            start.X -= pfeilLänge;
            end.X = start.X - lastRichtung.X * lastWert;
            var punkte = QuaderPunkteX(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var gross = 2 * wichte;
            // Pfeilspitze
            var weiter = (Vector3D)lastAngriff - lastRichtung * pfeilLänge;
            var cross = Vector3D.CrossProduct(new Vector3D(0, 0, 1), weiter);
            cross.Normalize();

            var lastPfeil = new Point3DCollection
            {
                lastAngriff, // spitze
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, 0.0, gross)), // vorn-links
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, 0.0, -gross)), // hinten-links
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, 0.0, -gross)), // hinten-rechts
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, 0.0, gross)) // vorn-rechts
            };
            ErzeugPfeilspitze(mesh, lastPfeil);
            var material = new DiffuseMaterial(Brushes.Red);
            model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        // Vertikallast in y
        if (Math.Abs(lastRichtung.Y) > 0)
        {
            start.Y += pfeilLänge;
            end.Y = -lastRichtung.Y * lastWert;
            var punkte = QuaderPunkteY(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            const double gross = 2 * wichte;
            // Pfeilspitze
            var weiter = (Vector3D)lastAngriff - lastRichtung * pfeilLänge;
            var cross = Vector3D.CrossProduct(new Vector3D(0, 0, 1), weiter);
            cross.Normalize();

            var lastPfeil = new Point3DCollection
            {
                lastAngriff, // spitze
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, 0.0, gross)), // vorn-links
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, 0.0, -gross)), // hinten-links
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, 0.0, -gross)), // hinten-rechts
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, 0.0, gross)) // vorn-rechts
            };
            ErzeugPfeilspitze(mesh, lastPfeil);
            var material = new DiffuseMaterial(Brushes.Red);
            model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        // Horizontallast in z (Tiefenrichtung)
        if (!(Math.Abs(lastRichtung.Z) > 0)) return model;
        {
            start.Z -= pfeilLänge;
            end.Z = start.Z - lastRichtung.Z * lastWert;
            var punkte = QuaderPunkteZ(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            const double gross = 2 * wichte;
            // Pfeilspitze
            var weiter = (Vector3D)lastAngriff - lastRichtung * pfeilLänge;
            var cross = Vector3D.CrossProduct(new Vector3D(0, -1, 0), weiter);
            cross.Normalize();

            var lastPfeil = new Point3DCollection
            {
                lastAngriff, // spitze
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, -gross, 0)), // vorn-links
                (Point3D)(weiter - cross * gross + new Vector3D(0.0, gross, 0)), // hinten-links
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, gross, 0)), // hinten-rechts
                (Point3D)(weiter + cross * gross + new Vector3D(0.0, -gross, 0)) // vorn-rechts
            };
            ErzeugPfeilspitze(mesh, lastPfeil);
            var material = new DiffuseMaterial(Brushes.Red);
            model = new GeometryModel3D(mesh, material) { BackMaterial = material };
        }

        return model;
    }

    public void ElementSpannungen_xx(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var normalXRichtung = new Vector3D(1, 0, 0);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var normalXWert = elementSpannungen.Spannungen[0] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + normalXRichtung * normalXWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - normalXRichtung * normalXWert / 2);

            // Normalspannungen sigma-xx
            var punkte = QuaderPunkteX(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = normalXWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenXx.Add(spannungenModell);
        }
    }

    public void ElementSpannungen_yy(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var normalYRichtung = new Vector3D(0, 1, 0);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var normalYWert = elementSpannungen.Spannungen[1] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + normalYRichtung * normalYWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - normalYRichtung * normalYWert / 2);

            // Normalspannungen sigma-yy
            var punkte = QuaderPunkteY(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = normalYWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenYy.Add(spannungenModell);
        }
    }

    public void ElementSpannungen_xy(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var schubXRichtung = new Vector3D(1, 0, 0);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var schubXWert = elementSpannungen.Spannungen[2] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + schubXRichtung * schubXWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - schubXRichtung * schubXWert / 2);

            // Schubspannungen tau-xy
            var punkte = QuaderPunkteZ(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = schubXWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenXy.Add(spannungenModell);
        }
    }

    public void ElementSpannungen_zz(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var normalZRichtung = new Vector3D(0, 0, 1);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var normalZWert = elementSpannungen.Spannungen[3] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + normalZRichtung * normalZWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - normalZRichtung * normalZWert / 2);

            // Normalspannungen sigma-yy
            var punkte = QuaderPunkteZ(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = normalZWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenZz.Add(spannungenModell);
        }
    }

    public void ElementSpannungen_yz(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var schubYRichtung = new Vector3D(0, 1, 0);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var schubYWert = elementSpannungen.Spannungen[4] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + schubYRichtung * schubYWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - schubYRichtung * schubYWert / 2);

            // Schubspannungen tau-yz
            var punkte = QuaderPunkteZ(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = schubYWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenYz.Add(spannungenModell);
        }
    }

    public void ElementSpannungen_zx(Model3DGroup modelGroup, double maxSpannung)
    {
        const double wichte = 0.04;
        const double vektorLänge = 1.0;
        var mesh = new MeshGeometry3D();
        var skalierung = vektorLänge / maxSpannung;
        var schubZRichtung = new Vector3D(0, 0, 1);

        foreach (var item in Modell.Elemente)
        {
            var element = (Abstrakt3D)item.Value;
            var elementSpannungen = new ElementSpannung(item.Value.BerechneZustandsvektor());
            var schubZWert = elementSpannungen.Spannungen[5] * skalierung;
            var schwerpunkt = element.BerechneSchwerpunkt3D();
            schwerpunkt.Y = -schwerpunkt.Y;

            var start = (Point3D)((Vector3D)schwerpunkt + schubZRichtung * schubZWert / 2);
            var end = (Point3D)((Vector3D)schwerpunkt - schubZRichtung * schubZWert / 2);

            // Schubspannungen tau-zx
            var punkte = QuaderPunkteZ(start, end, wichte);
            ErzeugQuader(mesh, punkte);

            var material = schubZWert < 0 ? new DiffuseMaterial(Brushes.Red) : new DiffuseMaterial(Brushes.Blue);
            spannungenModell = new GeometryModel3D(mesh, material) { BackMaterial = material };
            modelGroup.Children.Add(spannungenModell);
            SpannungenZx.Add(spannungenModell);
        }
    }

    private static Point3DCollection QuaderPunkteX(Point3D start, Point3D end, double wichte)
    {
        var punkte = new Point3DCollection
        {
            new(start.X, start.Y - wichte, start.Z + wichte),
            new(start.X, start.Y + wichte, start.Z + wichte),
            new(start.X, start.Y + wichte, start.Z - wichte),
            new(start.X, start.Y - wichte, start.Z - wichte),
            new(end.X, end.Y - wichte, end.Z + wichte),
            new(end.X, end.Y + wichte, end.Z + wichte),
            new(end.X, end.Y + wichte, end.Z - wichte),
            new(end.X, end.Y - wichte, end.Z - wichte)
        };
        return punkte;
    }

    private static Point3DCollection QuaderPunkteY(Point3D start, Point3D end, double wichte)
    {
        var punkte = new Point3DCollection
        {
            new(start.X - wichte, start.Y, start.Z + wichte),
            new(start.X - wichte, start.Y, start.Z - wichte),
            new(start.X + wichte, start.Y, start.Z - wichte),
            new(start.X + wichte, start.Y, start.Z + wichte),
            new(end.X - wichte, end.Y, end.Z + wichte),
            new(end.X - wichte, end.Y, end.Z - wichte),
            new(end.X + wichte, end.Y, end.Z - wichte),
            new(end.X + wichte, end.Y, end.Z + wichte)
        };
        return punkte;
    }

    private static Point3DCollection QuaderPunkteZ(Point3D start, Point3D end, double wichte)
    {
        var punkte = new Point3DCollection
        {
            new(start.X - wichte, start.Y + wichte, start.Z),
            new(start.X - wichte, start.Y - wichte, start.Z),
            new(start.X + wichte, start.Y - wichte, start.Z),
            new(start.X + wichte, start.Y + wichte, start.Z),
            new(end.X - wichte, end.Y + wichte, end.Z),
            new(end.X - wichte, end.Y - wichte, end.Z),
            new(end.X + wichte, end.Y - wichte, end.Z),
            new(end.X + wichte, end.Y + wichte, end.Z)
        };
        return punkte;
    }

    private void ErzeugQuader(MeshGeometry3D mesh, Point3DCollection punkte)
    {
        //oben
        AddDreieck(mesh, punkte[0], punkte[1], punkte[2]);
        AddDreieck(mesh, punkte[0], punkte[2], punkte[3]);

        //unten
        AddDreieck(mesh, punkte[4], punkte[6], punkte[5]);
        AddDreieck(mesh, punkte[4], punkte[7], punkte[6]);

        //vorn
        AddDreieck(mesh, punkte[0], punkte[1], punkte[5]);
        AddDreieck(mesh, punkte[0], punkte[5], punkte[4]);

        //hinten
        AddDreieck(mesh, punkte[3], punkte[6], punkte[2]);
        AddDreieck(mesh, punkte[3], punkte[7], punkte[6]);


        //links
        AddDreieck(mesh, punkte[0], punkte[7], punkte[3]);
        AddDreieck(mesh, punkte[0], punkte[4], punkte[7]);

        //rechts
        AddDreieck(mesh, punkte[1], punkte[2], punkte[6]);
        AddDreieck(mesh, punkte[1], punkte[6], punkte[5]);
    }

    private static Point3DCollection PfeilPunkteX(Point3D end, double länge, double breite)
    {
        var pfeile = new Point3DCollection
        {
            new(end.X + länge, end.Y, end.Z),
            new(end.X, end.Y - breite, end.Z + breite),
            new(end.X, end.Y - breite, end.Z - breite),
            new(end.X, end.Y + breite, end.Z - breite),
            new(end.X, end.Y + breite, end.Z + breite)
        };
        return pfeile;
    }

    private static Point3DCollection PfeilPunkteY(Point3D end, double länge, double breite)
    {
        var pfeile = new Point3DCollection
        {
            new(end.X, end.Y + länge, end.Z),
            new(end.X - breite, end.Y, end.Z + breite),
            new(end.X - breite, end.Y, end.Z - breite),
            new(end.X + breite, end.Y, end.Z - breite),
            new(end.X + breite, end.Y, end.Z + breite)
        };
        return pfeile;
    }

    private static Point3DCollection PfeilPunkteZ(Point3D end, double länge, double breite)
    {
        var pfeile = new Point3DCollection
        {
            new(end.X, end.Y, end.Z + länge),
            new(end.X - breite, end.Y - breite, end.Z),
            new(end.X - breite, end.Y - breite, end.Z),
            new(end.X + breite, end.Y + breite, end.Z),
            new(end.X + breite, end.Y + breite, end.Z)
        };
        return pfeile;
    }

    private void ErzeugPfeilspitze(MeshGeometry3D mesh, Point3DCollection pfeile)
    {
        AddDreieck(mesh, pfeile[0], pfeile[1], pfeile[4]); // hinten
        AddDreieck(mesh, pfeile[0], pfeile[1], pfeile[2]); // unten
        AddDreieck(mesh, pfeile[0], pfeile[2], pfeile[3]); // vorn
        AddDreieck(mesh, pfeile[0], pfeile[3], pfeile[4]); // oben

        AddDreieck(mesh, pfeile[1], pfeile[2], pfeile[3]); // Deckel
        AddDreieck(mesh, pfeile[1], pfeile[3], pfeile[4]); //
    }

    // Hinzufügen eines Dreiecks zum mesh, Wiederbenutzung der Dreieckspunkte, die schon definiert sind
    private void AddDreieck(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
    {
        // lies Indizes der Dreieckspunkte im Netz
        var index1 = AddPunkt(mesh.Positions, point1);
        var index2 = AddPunkt(mesh.Positions, point2);
        var index3 = AddPunkt(mesh.Positions, point3);

        // Erzeugung des Dreiecks
        mesh.TriangleIndices.Add(index1);
        mesh.TriangleIndices.Add(index2);
        mesh.TriangleIndices.Add(index3);
    }

    // Falls ein Punkt schon definiert ist, lies den Index, andernfalls erzeuge den Punkt und lies den neuen Index
    private int AddPunkt(Point3DCollection points, Point3D point)
    {
        // falls der Punkt im Dictionary existiert, lies gespeicherten Index
        if (punktDictionary.TryGetValue(point, out var punkt))
            return punkt;

        // falls der Punkt nicht gefunden wurde, erzeuge ihn neu
        points.Add(point);
        punktDictionary.Add(point, points.Count - 1);
        return points.Count - 1;
    }
}

internal class ElementSpannung(double[] spannungen)
{
    public double[] Spannungen { get; } = spannungen;
}