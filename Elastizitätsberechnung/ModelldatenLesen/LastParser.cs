using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public class LastParser
    {
        private FEModell modell;
        private string[] substrings;
        private readonly char[] delimiters = { '\t' };

        private string loadId;
        private string nodeId;
        private KnotenLast knotenLast;
        private LinienLast linienLast;

        public static double[] NodeLoad { get; set; }

        public void ParseLasten(string[] lines, FEModell feModel)
        {

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i] != "Knotenlasten") continue;
                modell = feModel;
                FeParser.EingabeGefunden += "\nKnotenlasten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    loadId = substrings[0];
                    nodeId = substrings[1];
                    NodeLoad = new double[3];
                    NodeLoad[0] = double.Parse(substrings[2]);
                    NodeLoad[1] = double.Parse(substrings[3]);

                    switch (substrings.Length)
                    {
                        case 4:
                            knotenLast = new KnotenLast(nodeId, NodeLoad[0], NodeLoad[1]);
                            break;
                        case 5:
                            {
                                NodeLoad[2] = double.Parse(substrings[4]);
                                //var p = 4 * NodeLoad[2];
                                knotenLast = new KnotenLast(nodeId, NodeLoad[0], NodeLoad[1], NodeLoad[2]);
                                break;
                            }
                        default:
                            throw new ParseAusnahme((i + 1) + ": Knotenlasten erfordert 4 oder 5 Eingabeparameter");
                    }

                    knotenLast.LastId = loadId;
                    modell.Lasten.Add(loadId, knotenLast);
                    i++;
                } while (lines[i + 1].Length != 0);
                break;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                modell = feModel;
                if (lines[i] != "Linienlasten") continue;
                FeParser.EingabeGefunden += "\nLinienlasten";
                do
                {
                    substrings = lines[i + 1].Split(delimiters);
                    if (substrings.Length == 7)
                    {
                        loadId = substrings[0];
                        var startNodeId = substrings[1];
                        NodeLoad = new double[4];
                        NodeLoad[0] = double.Parse(substrings[2]);
                        NodeLoad[1] = double.Parse(substrings[3]);
                        var endNodeId = substrings[4];
                        NodeLoad[2] = double.Parse(substrings[5]);
                        NodeLoad[3] = double.Parse(substrings[6]);

                        linienLast = new LinienLast(startNodeId, NodeLoad[0], NodeLoad[1], endNodeId, NodeLoad[2], NodeLoad[3]);
                        modell.LinienLasten.Add(loadId, linienLast);
                        i++;
                    }
                    else
                    {
                        throw new ParseAusnahme((i + 1) + ": Linienlasten erfordert 7 Eingabeparameter");
                    }
                } while (lines[i + 1].Length != 0);
                break;
            }
        }
    }
}