using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung;

public static class NetzErweiterungen
{
    // gibt eine MeshGeometry3D zurück für das Drahtmodell des Netzes
    public static MeshGeometry3D ToWireframe(this MeshGeometry3D mesh, double thickness)
    {
        // erzeug ein Dictionary, um Dreiecke mit gleichen Kanten zu identifizieren,
        // damit diese nur einmal gezeichnet werden
        var alreadyDrawn = new Dictionary<int, int>();

        // erzeug eine Netz für das Drahtmodell
        var wireframe = new MeshGeometry3D();

        // Schleife über die Dreiecke des Netzes
        for (var triangle = 0; triangle < mesh.TriangleIndices.Count; triangle += 3)
        {
            // hol die Knotenindizes der Dreiecke
            var index1 = mesh.TriangleIndices[triangle];
            var index2 = mesh.TriangleIndices[triangle + 1];
            var index3 = mesh.TriangleIndices[triangle + 2];

            // erzeug die 3 Kanten eines Dreiecks
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index1, index2, thickness);
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index2, index3, thickness);
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index3, index1, thickness);
        }

        return wireframe;
    }

    // füg Dreieckskante zum Drahtmodell hinzu
    private static void AddTriangleSegment(MeshGeometry3D mesh,
        MeshGeometry3D wireframe, IDictionary<int, int> alreadyDrawn,
        int index1, int index2, double thickness)
    {
        // eine eindeutige ID für eine Kante mit 2 Punkten
        if (index1 > index2)
        {
            var temp = index1;
            index1 = index2;
            index2 = temp;
        }

        var segmentId = index1 * mesh.Positions.Count + index2;

        // ignorier die Kante, falls sie schon einem anderen Dreieck hinzugefügt wurde
        if (alreadyDrawn.ContainsKey(segmentId)) return;
        alreadyDrawn.Add(segmentId, segmentId);

        // sonst, erzeug die Kante
        AddSegment(wireframe, mesh.Positions[index1], mesh.Positions[index2], thickness);
    }

    // füg ein Dreieck dem Netz hinzu ohne Wiederverwendung von Punkten, 
    // damit Dreiecke nicht die gleiche Normale haben
    private static void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
    {
        // erzeug die Punkte
        var index1 = mesh.Positions.Count;
        mesh.Positions.Add(point1);
        mesh.Positions.Add(point2);
        mesh.Positions.Add(point3);

        // erzeug das Dreieck
        mesh.TriangleIndices.Add(index1++);
        mesh.TriangleIndices.Add(index1++);
        mesh.TriangleIndices.Add(index1);
    }

    // erzeug ein dünnes, rechteckiges Prisma zwischen den 2 Punkten
    // falls (extend is true), verlängere die Kante um die halbe Linienwichte,
    // damit Kanten mit 2 gleichen Endpunkten zusammenpassen
    // Falls ein up-Vektor fehlt, erzeug einen rechtwinkligen Vektor dafür
    private static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, double thickness, bool extend)
    {
        // finde einen Up-Vektor der nicht colinear mit der Kante ist
        // start mit einem Vektor parallel zur Y-Achse
        var up = new Vector3D(0, 1, 0);

        // falls eine Kante und ein Up-Vektor in etwa die gleiche Richtung zeigen
        // benutze eine Up-Vektor parallel zur X-Achse
        var segment = point2 - point1;
        segment.Normalize();
        if (Math.Abs(Vector3D.DotProduct(up, segment)) > 0.9)
            up = new Vector3D(1, 0, 0);

        // füg die Kante zum Netz hinzu
        AddSegment(mesh, point1, point2, up, thickness, extend);
    }

    private static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, double thickness)
    {
        AddSegment(mesh, point1, point2, thickness, false);
    }

    public static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, Vector3D up, double thickness)
    {
        AddSegment(mesh, point1, point2, up, thickness, false);
    }

    private static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, Vector3D up, double thickness,
        bool extend)
    {
        // der Kantenvektor
        var v = point2 - point1;

        if (extend)
        {
            // erhöhe die Kantenlänge an beiden Enden um Kantenwichte/2
            var n = ScaleVector(v, thickness / 2.0);
            point1 -= n;
            point2 += n;
        }

        // skalierter Kantenvektor
        var n1 = ScaleVector(up, thickness / 2.0);

        // ein zusätzlicher, rechtwinkliger Vektor
        var n2 = Vector3D.CrossProduct(v, n1);
        n2 = ScaleVector(n2, thickness / 2.0);

        // erzeug eine dünne Box
        // p1pm bedeutet point1 PLUS n1 MINUS n2
        var p1pp = point1 + n1 + n2;
        var p1mp = point1 - n1 + n2;
        var p1pm = point1 + n1 - n2;
        var p1mm = point1 - n1 - n2;
        var p2pp = point2 + n1 + n2;
        var p2mp = point2 - n1 + n2;
        var p2pm = point2 + n1 - n2;
        var p2mm = point2 - n1 - n2;

        // Seiten
        AddTriangle(mesh, p1pp, p1mp, p2mp);
        AddTriangle(mesh, p1pp, p2mp, p2pp);

        AddTriangle(mesh, p1pp, p2pp, p2pm);
        AddTriangle(mesh, p1pp, p2pm, p1pm);

        AddTriangle(mesh, p1pm, p2pm, p2mm);
        AddTriangle(mesh, p1pm, p2mm, p1mm);

        AddTriangle(mesh, p1mm, p2mm, p2mp);
        AddTriangle(mesh, p1mm, p2mp, p1mp);

        // Enden
        AddTriangle(mesh, p1pp, p1pm, p1mm);
        AddTriangle(mesh, p1pp, p1mm, p1mp);

        AddTriangle(mesh, p2pp, p2mp, p2mm);
        AddTriangle(mesh, p2pp, p2mm, p2pm);
    }

    // Vektorlänge
    private static Vector3D ScaleVector(Vector3D vector, double length)
    {
        var scale = length / vector.Length;
        return new Vector3D(
            vector.X * scale,
            vector.Y * scale,
            vector.Z * scale);
    }
}