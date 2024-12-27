using System.Linq;

namespace FEBibliothek.Werkzeuge
{
    public static class FeGeometrie
    {
        private static double Polygonfläche(Point[] k)
        {
            double fläche = 0;
            var p = new Point[k.Length + 1];
            for (var i = 0; i < k.Length; i++) p[i] = k[i];
            p[k.Length] = p[0];
            for (var i = 0; i < p.Length - 1; i++)
            {
                fläche += p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y;
            }
            return 0.5 * fläche;
        }

        public static Point PolygonSchwerpunkt(Point[] k)
        {
            double xs = 0, ys = 0;
            var fläche = Polygonfläche(k);
            var p = new Point[k.Length + 1];
            for (var i = 0; i < k.Length; i++) p[i] = k[i];
            p[k.Length] = p[0];
            for (var i = 0; i < p.Length - 1; i++)
            {
                xs += (p[i].X + p[i + 1].X) * (p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y);
                ys += (p[i].Y + p[i + 1].Y) * (p[i].X * p[i + 1].Y - p[i + 1].X * p[i].Y);
            }
            var cg = new Point(xs / 6 / fläche, ys / 6 / fläche);
            return cg;
        }
        private static Vector RotateVector(Vector vec, double angle)  // clockwise in degree
        {
            var winkel = angle * Math.PI / 180;
            var rotated = new Vector(vec.X * Math.Cos(winkel) - vec.Y * Math.Sin(winkel),
                vec.X * Math.Sin(winkel) + vec.Y * Math.Cos(winkel));
            return rotated;
        }

        private static List<Knoten> _innenKnoten = [];
        public static List<Knoten> ConvexHull(List<Knoten> knoten, bool eng, double formFaktor)
        // ***** The convex hull or convex envelope or convex closure of a shape is the smallest convex set that contains it. 
        // ***** For a bounded subset of the plane, the convex hull may be visualized as the shape enclosed by a rubber band stretched around the subset. 
        //
        // The current implementation assumes the first element to be in the upper left corner with a 
        // successive sequence of Nodes in clockwise direction.
        // Input: A list containing node definitions of a Finite Element Model, 
        //        the FormFactor for neighbouring elements, i.e. the maximum length to find neighbours, e.g. 1.2
        //        _eng = true  > Koordinatensystem links-unten, y nach oben,  Ingenieurkoordinaten
        //        _eng = false > Koordinatensystem links-oben,  y nach unten, Bildschirmkoordinaten
        //
        // Output: A list of successive FE nodes as basis for the hull geometry
        //         node geometries can be added to a PointCollection
        //         as base for a Polygon which is closed by definition
        //        "Geometry.innerNodes" available as a list of nodes
        // ***** 
        {
            var factor = 1;
            if (eng) factor = -1;
            double startWinkel = factor * 100;
            Knoten found = null;
            var hullKnotenList = new List<Knoten>();
            var next = new Point(knoten[0].Koordinaten[0], knoten[0].Koordinaten[1]);
            var start = next;
            hullKnotenList.Add(knoten[0]);
            var basisVektor = new Vector(1, 0);
            basisVektor = RotateVector(basisVektor, startWinkel);

            _innenKnoten = knoten.ToList();
            _innenKnoten.Remove(knoten[0]);
            foreach (var unused in knoten)
            {
                Point end;
                Vector vec;
                foreach (var rest in _innenKnoten)
                {
                    end = new Point(rest.Koordinaten[0], rest.Koordinaten[1]);
                    vec = (Vector)end - (Vector)start;
                    if (vec.Length < formFaktor)
                    {
                        startWinkel = Vector.AngleBetween(basisVektor, vec);
                        next = end; found = rest;
                        break;
                    }
                    _innenKnoten.Remove(found);
                }
                foreach (var rest in _innenKnoten)
                {
                    end = new Point(rest.Koordinaten[0], rest.Koordinaten[1]);
                    vec = (Vector)end - (Vector)start;
                    if (vec.Length < formFaktor)
                    {
                        var winkel = -Math.Abs(Vector.AngleBetween(basisVektor, vec));
                        if (!(winkel < startWinkel)) continue;
                        next = end; found = rest; startWinkel = winkel;
                    }
                }
                _innenKnoten.Remove(found);
                hullKnotenList.Add(found);
                basisVektor = RotateVector((Vector)next - (Vector)start, factor * 100);
                start = next;
                if (found != null && (hullKnotenList.Count > 2) &&
                    (Math.Sqrt(Math.Pow(knoten[0].Koordinaten[0] - found.Koordinaten[0], 2) +
                               Math.Pow((knoten[0].Koordinaten[1] - found.Koordinaten[1]), 2))) <= 1)
                { break; }
            }
            return hullKnotenList;
        }
    }
}
