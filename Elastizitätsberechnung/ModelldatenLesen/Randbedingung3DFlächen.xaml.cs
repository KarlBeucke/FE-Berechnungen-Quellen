using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public partial class Randbedingung3DFlächen
{
    private readonly FeModell _modell;
    private readonly LagerKeys _lagerKeys;
    private readonly Dictionary<string, Lager> _lagerDictionary;
    private ObservableCollection<LagerbedingungFläche> _randbedingungenListe;

    public Randbedingung3DFlächen(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _randbedingungenListe = [];
        RandbedingungenGrid.Items.Clear();
        InitialKnotenId.Text = string.Empty;
        AnzahlKnoten.Text = string.Empty;
        Show();
        _lagerKeys = new LagerKeys(_modell) { Owner = this };
        _lagerKeys.Show();
        _lagerDictionary = new Dictionary<string, Lager>();
    }

    private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
    {
        var prescribed = new double[3];
        var supportInitial = "";
        var face = "";
        var knotenInitial="";
        var nKnoten = 0;

        try
        {
            if (LagerId.Text.Length > 0) supportInitial = LagerId.Text;
            if (FlächenId.Text.Length > 0) face = FlächenId.Text;
            if (InitialKnotenId.Text.Length > 0) knotenInitial = InitialKnotenId.Text;
            if (AnzahlKnoten.Text.Length > 0) nKnoten = short.Parse(AnzahlKnoten.Text);
        }
        catch (FormatException)
        {
            _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Randbedingung3DFläche");
            return;
        }

        var conditions = 0;
        if (XFest.IsChecked != null && (bool)XFest.IsChecked) conditions += 1;
        if (YFest.IsChecked != null && (bool)YFest.IsChecked) conditions += 2;
        if (ZFest.IsChecked != null && (bool)ZFest.IsChecked) conditions += 4;

        for (var m = 0; m < nKnoten; m++)
        {
            var id1 = m.ToString().PadLeft(2, '0');
            for (var k = 0; k < nKnoten; k++)
            {
                var id2 = k.ToString().PadLeft(2, '0');
                var supportName = supportInitial + face + id1 + id2;
                if (!_modell.Randbedingungen.TryGetValue(supportName, out _))
                {
                    _ = MessageBox.Show($"\nRandbedingung \"{supportName}\" bereits vorhanden.", "neue Randbedingung3DFlächen");
                    return;
                }
                const string faceNode = "00";
                var nodeName = face[..1] switch
                {
                    "X" => knotenInitial + faceNode + id1 + id2,
                    "Y" => knotenInitial + id1 + faceNode + id2,
                    "Z" => knotenInitial + id1 + id2 + faceNode,
                    _ => throw new ParseAusnahme(
                        $"\nfalsche FlächenId = {face[..1]}, muss sein:\n X, Y or Z")
                };

                var neuesLager = new Lager(nodeName,face, conditions, prescribed, 3)
                {
                    RandbedingungId = supportName
                };
                _lagerDictionary.Add(supportName, neuesLager);
            }
        }
        _randbedingungenListe = Lagerbedingungen(_lagerDictionary);
        RandbedingungenGrid.Items.Clear();
        RandbedingungenGrid.ItemsSource = _randbedingungenListe;
    }

    private ObservableCollection<LagerbedingungFläche> Lagerbedingungen(Dictionary<string, Lager> lagerDictionary)
    {
        foreach (var item in lagerDictionary)
        {
            var nodeId = item.Value.KnotenId;
            var supportName = item.Value.RandbedingungId;
            var face = item.Value.Face;
            string[] vordefiniert = ["frei", "frei", "frei"];

            switch (item.Value.Typ)
            {
                case 1:
                {
                    vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                    break;
                }
                case 2:
                {
                    vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                    if (_modell.Raumdimension == 2) vordefiniert[2] = string.Empty;
                    break;
                }
                case 3:
                {
                    vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                    vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                    break;
                }
                case 4:
                {
                    vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                    break;
                }
                case 5:
                {
                    vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                    vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                    break;
                }
                case 6:
                {
                    vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                    vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                    break;
                }
                case 7:
                {
                    vordefiniert[0] = item.Value.Vordefiniert[0].ToString("F4");
                    vordefiniert[1] = item.Value.Vordefiniert[1].ToString("F4");
                    vordefiniert[2] = item.Value.Vordefiniert[2].ToString("F4");
                    break;
                }
                default:
                    throw new ModellAusnahme("\nLagerbedingung für Lager " + supportName + " falscher Typ");
            }
            var lagerBedingung = new LagerbedingungFläche(item.Key, nodeId, face, vordefiniert);
            _randbedingungenListe.Add(lagerBedingung);
            
        }
        return _randbedingungenListe;
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _lagerDictionary.Where(item
                     => !_modell.Randbedingungen.TryAdd(item.Key,item.Value )))
        {
            _ = MessageBox.Show("Randbedingung " + item.Key + " nicht hinzugefügt, da schon vorhanden" +
                                "\nVorgang abgebrochen", "neues Knotennetz");
            break;
        }

        StartFenster.ElastizitätVisual3D.Close();
        Close();
        _lagerKeys.Close();

        StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
        StartFenster.ElastizitätVisual3D.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
        _lagerKeys.Close();
        _randbedingungenListe.Clear();
    }
}