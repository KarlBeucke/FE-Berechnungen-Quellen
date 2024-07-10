using System;
using System.Collections.Generic;
using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class RandbedingungenParser : FeParser
{
    public readonly List<string> faces = new();
    private Lager lager;
    private FeModell modell;
    private string nodeId;
    private string[] substrings;
    private string supportId;

    public void ParseRandbedingungen(string[] lines, FeModell feModell)
    {
        modell = feModell;
        ParseRandbedingungenKnoten(lines);
        ParseRandbedingungenFläche(lines);
        ParseRandbedingungBoussinesq(lines);
    }

    private void ParseRandbedingungenKnoten(IReadOnlyList<string> lines)
    {
        char[] delimiters = { '\t' };

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Randbedingungen") continue;
            EingabeGefunden += "\nRandbedingungen";
            double[] prescribed = { 0, 0, 0 };
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                if (substrings.Length == 5 || substrings.Length == 6)
                {
                    supportId = substrings[0];
                    nodeId = substrings[1];
                    var conditions = 0;
                    var type = substrings[2];
                    for (var k = 0; k < type.Length; k++)
                    {
                        var subType = type.Substring(k, 1);
                        switch (subType)
                        {
                            case "x":
                                conditions += Lager.XFixed;
                                break;
                            case "y":
                                conditions += Lager.YFixed;
                                break;
                            case "z":
                                conditions += Lager.ZFixed;
                                break;
                        }
                    }

                    if (substrings.Length > 3) prescribed[0] = double.Parse(substrings[3]);
                    if (substrings.Length > 4) prescribed[1] = double.Parse(substrings[4]);
                    if (substrings.Length > 5) prescribed[2] = double.Parse(substrings[5]);
                    lager = new Lager(nodeId, "0", conditions, prescribed, modell);
                    modell.Randbedingungen.Add(supportId, lager);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme(i + 1 + ":\nRandbedingungen erfordert 5 oder 6 Eingabeparameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseRandbedingungenFläche(IReadOnlyList<string> lines)
    {
        char[] delimiters = { '\t' };
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "RandbedingungFläche") continue;
            EingabeGefunden += "\nRandbedingungFläche";
            var prescribed = new double[3];
            do
            {
                substrings = lines[i + 1].Split(delimiters);
                var supportInitial = substrings[0];
                var face = substrings[1];
                faces.Add(face);
                var nodeInitial = substrings[2];
                int nNodes = short.Parse(substrings[3]);
                var type = substrings[4];
                var conditions = 0;
                for (var count = 0; count < type.Length; count++)
                {
                    var subType = type.Substring(count, 1).ToLower();
                    switch (subType)
                    {
                        case "x":
                            conditions += Lager.XFixed;
                            break;
                        case "y":
                            conditions += Lager.YFixed;
                            break;
                        case "z":
                            conditions += Lager.ZFixed;
                            break;
                        default:
                            throw new ParseAusnahme("\nLagerbedingung für x, y und/oder z muss definiert werden");
                    }
                }

                var j = 0;
                for (var k = 5; k < substrings.Length; k++)
                {
                    prescribed[j] = double.Parse(substrings[k]);
                    j++;
                }

                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (modell.Randbedingungen.TryGetValue(supportName, out _))
                            throw new ParseAusnahme($"\nRandbedingung \"{supportName}\" bereits vorhanden.");
                        string nodeName;
                        const string faceNode = "00";
                        switch (face.Substring(0, 1))
                        {
                            case "X":
                                nodeName = nodeInitial + faceNode + id1 + id2;
                                break;
                            case "Y":
                                nodeName = nodeInitial + id1 + faceNode + id2;
                                break;
                            case "Z":
                                nodeName = nodeInitial + id1 + id2 + faceNode;
                                break;
                            default:
                                throw new ParseAusnahme(
                                    $"\nfalsche FlächenId = {face.Substring(0, 1)}, muss sein:\n X, Y or Z");
                        }

                        lager = new Lager(nodeName, face, conditions, prescribed, modell);
                        modell.Randbedingungen.Add(supportName, lager);
                    }
                }

                i++;
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseRandbedingungBoussinesq(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "RandbedingungBoussinesq") continue;
            var gModulus = MaterialParser.GModul;
            var poisson = MaterialParser.Poisson;
            if (LastParser.NodeLoad == null)
                throw new ParseAusnahme("\nKnotenlast für Boussinesq Randbedingung nicht definiert");
            var p = 4.0 * LastParser.NodeLoad[2];
            char[] delimiters = { '\t' };

            // 1. Zeile: Feld mit Offsets
            // 2. Zeile: supportInitial, face, nodeInitial, type
            EingabeGefunden += "\nRandbedingungBoussinesq";
            substrings = lines[i + 1].Split(delimiters);
            var offset = new double[substrings.Length];
            for (var k = 0; k < substrings.Length; k++)
                offset[k] = double.Parse(substrings[k]);

            var prescribed = new double[3];
            i += 2;
            do
            {
                var conditions = 0;
                string subType;
                substrings = lines[i].Split(delimiters);

                var supportInitial = substrings[0];
                var face = substrings[1];
                faces.Add(face);
                var nodeInitial = substrings[2];
                //int nNodes = short.Parse(substrings[3]);
                var nNodes = offset.Length;
                face = $"{face.Substring(0, 1)}0{nNodes - 1}";
                var type = substrings[3];
                for (var count = 0; count < type.Length; count++)
                {
                    subType = type.Substring(count, 1).ToLower();
                    switch (subType)
                    {
                        case "x":
                            conditions += Lager.XFixed;
                            break;
                        case "y":
                            conditions += Lager.YFixed;
                            break;
                        case "z":
                            conditions += Lager.ZFixed;
                            break;
                        default:
                            throw new ParseAusnahme("\n5. Parameter muss x und/der y und/oder z sein");
                    }
                }

                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (modell.Randbedingungen.TryGetValue(supportName, out _))
                            throw new ParseAusnahme($"\nRandbedingung \"{supportName}\" bereits vorhanden.");
                        string nodeName;
                        var faceNode = $"0{offset.Length - 1}";
                        switch (face.Substring(0, 1))
                        {
                            case "X":
                                nodeName = nodeInitial + faceNode + id1 + id2;
                                break;
                            case "Y":
                                nodeName = nodeInitial + id1 + faceNode + id2;
                                break;
                            case "Z":
                                nodeName = nodeInitial + id1 + id2 + faceNode;
                                break;
                            default:
                                throw new ParseAusnahme(
                                    $"\nfalsche Flächen Id = {face.Substring(0, 1)}, muss sein:\n X, Y or Z");
                        }

                        for (var count = 0; count < type.Length; count++)
                        {
                            subType = type.Substring(count, 1).ToLower();
                            double x, y, z, r, a, factor;
                            switch (subType)
                            {
                                case "x":
                                    x = offset[nNodes - 1];
                                    y = offset[m];
                                    z = offset[k];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[0] = x / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                    factor;
                                    break;
                                case "y":
                                    x = offset[m];
                                    y = offset[nNodes - 1];
                                    z = offset[k];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[1] = y / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                    factor;
                                    break;
                                case "z":
                                    x = offset[m];
                                    y = offset[k];
                                    z = offset[nNodes - 1];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[2] = (z * z / (a * a) + 2 * (1 - poisson)) * factor;
                                    break;
                                default:
                                    throw new ParseAusnahme(
                                        "\nfalsche Anzahl Parameter in RandbedingungBoussinesq, muss sein:\n"
                                        + "4 für lagerInitial, flaeche, knotenInitial, Art\n");
                            }
                        }

                        lager = new Lager(nodeName, face, conditions, prescribed, modell);
                        modell.Randbedingungen.Add(supportName, lager);
                    }
                }

                i++;
            } while (lines[i].Length != 0);
        }
    }
}