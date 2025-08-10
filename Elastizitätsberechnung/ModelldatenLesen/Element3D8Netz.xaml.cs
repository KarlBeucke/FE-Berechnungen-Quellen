using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Element3D8Netz
    {
        private readonly FeModell _modell;
        private const int NodesPerElement = 8;

        public Element3D8Netz(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementPre = "";
            var elementKnoten = "";
            var anzahl = 0;
            var material = "";
            try
            {
                if (Elementpräfix.Text.Length > 0) elementPre = Elementpräfix.Text;
                if (Knotenpräfix.Text.Length > 0) elementKnoten = Knotenpräfix.Text;
                if (Anzahl.Text.Length > 0) anzahl = short.Parse(Anzahl.Text);
                if (Material.Text.Length > 0) material = Material.Text;
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format in der Eingabe", "Element3D8Netz");
                return;
            }
            //_modell.Elemente.Clear();

            for (var n = 0; n < anzahl; n++)
            {
                var idX = n.ToString().PadLeft(2, '0');
                var idXp = (n + 1).ToString().PadLeft(2, '0');
                for (var m = 0; m < anzahl; m++)
                {
                    var idY = m.ToString().PadLeft(2, '0');
                    var idYp = (m + 1).ToString().PadLeft(2, '0');
                    for (var k = 0; k < anzahl; k++)
                    {
                        var idZ = k.ToString().PadLeft(2, '0');
                        var idZp = (k + 1).ToString().PadLeft(2, '0');
                        var eNode = new string[NodesPerElement];
                        var elementName = elementPre + idX + idY + idZ;
                        //
                        //
                        if (_modell.Elemente.TryGetValue(elementName, out var element))
                            throw new ParseAusnahme($"\nElement \"{elementName}\" bereits vorhanden.");
                        eNode[0] = elementKnoten + idX + idY + idZ;
                        eNode[1] = elementKnoten + idXp + idY + idZ;
                        eNode[2] = elementKnoten + idXp + idYp + idZ;
                        eNode[3] = elementKnoten + idX + idYp + idZ;
                        eNode[4] = elementKnoten + idX + idY + idZp;
                        eNode[5] = elementKnoten + idXp + idY + idZp;
                        eNode[6] = elementKnoten + idXp + idYp + idZp;
                        eNode[7] = elementKnoten + idX + idYp + idZp;
                        element = new Element3D8(eNode, material, _modell) { ElementId = elementName };
                        _modell.Elemente.Add(elementName, element);
                    }
                }
            }

            StartFenster.ElastizitätVisual3D.Close();
            Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
