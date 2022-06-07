using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Windows;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class NeuesLager
    {
        private readonly FEModell modell;

        public NeuesLager(FEModell modell)
        {
            InitializeComponent();
            this.modell = modell;
            InitialKnotenId.Text = string.Empty;
            AnzahlKnoten.Text = string.Empty;
            VorX.Text = string.Empty;
            VorY.Text = string.Empty;
            VorZ.Text = string.Empty;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var prescribed = new double[3];
            var faces = new List<string>();


            if (FlächenId.Text.Length == 0)
            {
                var lagerId = LagerId.Text;
                var knotenId = KnotenId.Text;
                int conditions = 0;
                if (XFest.IsChecked != null && (bool)XFest.IsChecked) conditions += 1;
                if (YFest.IsChecked != null && (bool)YFest.IsChecked) conditions += 2;
                if (ZFest.IsChecked != null && (bool)ZFest.IsChecked) conditions += 4;

                var randbedingung = new Lager(knotenId, "0", conditions, prescribed, modell);
                modell.Randbedingungen.Add(lagerId, randbedingung);
            }
            else
            {
                var supportInitial = LagerId.Text;
                var face = FlächenId.Text;
                faces.Add(face);
                var nodeInitial = InitialKnotenId.Text;
                int nNodes = short.Parse(AnzahlKnoten.Text);

                int conditions = 0;
                if (XFest.IsChecked != null && (bool)XFest.IsChecked) conditions += 1;
                if (YFest.IsChecked != null && (bool)YFest.IsChecked) conditions += 2;
                if (ZFest.IsChecked != null && (bool)ZFest.IsChecked) conditions += 4;

                if (VorX.Text.Length > 0) prescribed[0] = double.Parse(VorX.Text);
                if (VorY.Text.Length > 0) prescribed[1] = double.Parse(VorY.Text);
                if (VorZ.Text.Length > 0) prescribed[2] = double.Parse(VorZ.Text);

                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (modell.Randbedingungen.TryGetValue(supportName, out _))
                            throw new ParseAusnahme("Randbedingung \"" + supportName + "\" bereits vorhanden.");
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
                                throw new ParseAusnahme("falsche FlächenId = " + face.Substring(0, 1) +
                                                        ", muss sein:\n" + " X, Y or Z");
                        }

                        var lager = new Lager(nodeName, face, conditions, prescribed, modell);
                        modell.Randbedingungen.Add(supportName, lager);
                    }
                }
            }
            Close();
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}