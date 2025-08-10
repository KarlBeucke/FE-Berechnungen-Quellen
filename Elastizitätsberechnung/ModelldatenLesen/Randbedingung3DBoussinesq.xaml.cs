using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Collections.ObjectModel;
using Lager = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Lager;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Randbedingung3DBoussinesq
    {
        private readonly FeModell _modell;
        private const int AnzahlFreiheitsgrade = 3;
        private string _material;
        private readonly LagerKeys _lagerKeys;
        private readonly Dictionary<string, Lager> _lagerDictionary;
        private ObservableCollection<LagerbedingungBoussinesq> _randbedingungenListe;

        public Randbedingung3DBoussinesq(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            _randbedingungenListe = [];
            RandbedingungenGrid.Items.Clear();
            RandbedingungInitialId.Text = string.Empty;
            KnotenInitial.Text = string.Empty;
            Fläche.Text = string.Empty;
            Last.Text = string.Empty;
            Lasty.Text = string.Empty;
            Lastz.Text = string.Empty;
            Material.Text = string.Empty;
            XFest.IsChecked = false;
            YFest.IsChecked = false;
            ZFest.IsChecked = false;
            Inkremente.Text = string.Empty;
            Show();
            _lagerKeys = new LagerKeys(_modell) { Owner = this };
            _lagerKeys.Show();
            _lagerDictionary = new Dictionary<string, Lager>();
        }

        private void BtnTabelleneintrag(object sender, RoutedEventArgs e)
        {
            var prescribed = new double[3];
            char[] delimiters = [';'];
            var supportInitial = "";
            var knotenInitial = "";
            var face = "";
            var inkremente = "";
            var type = "xyz";
            double gModulus=0, poisson=0, p=0;
            try
            {
                if (RandbedingungInitialId.Text.Length > 0) supportInitial = RandbedingungInitialId.Text;
                if (KnotenInitial.Text.Length > 0) knotenInitial = KnotenInitial.Text;
                if (Fläche.Text.Length > 0) face = Fläche.Text;
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format: Randbedingung Initial ID, Knoten Initial Id oder Fläche", "Randbedingung3DBoussinesq");
            }

            try
            {
                if (Material.Text.Length > 0) _material = Material.Text;
                
                if (_modell.Material.TryGetValue(_material, out var vorhandenesMaterial))
                {
                    poisson = vorhandenesMaterial.MaterialWerte[0];
                    gModulus = vorhandenesMaterial.MaterialWerte[1];
                }
                else
                {
                    var neuesMaterial = new MaterialNeu(_modell);
                    gModulus = double.Parse(neuesMaterial.Schub.Text);
                    poisson = double.Parse(neuesMaterial.Poisson.Text);
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format: Material", "Randbedingung3DBoussinesq");
            }

            try
            {
                var knotenlast = "";
                if (Last.Text.Length > 0) knotenlast = Last.Text;
                p = _modell.Lasten.TryGetValue(knotenlast, out var vorhandeneLast) ? vorhandeneLast.Lastwerte[2] : double.Parse(Lastz.Text);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format: Knotenlast", "Randbedingung3DBoussinesq");
            }

            try
            {
                if (Inkremente.Text.Length > 0) inkremente = Inkremente.Text;
                var abständeText = inkremente.Split(delimiters);
                var abstände = new double[abständeText.Length];

                if (Inkremente.Text.Length > 0) abstände = new double[abständeText.Length];
                for (var i = 0; i < abständeText.Length; i++)
                    abstände[i] = double.Parse(abständeText[i]);

                var nKnoten = abstände.Length;
                if (Fläche.Text.Length > 0) face = Fläche.Text;
                for (var m = 0; m < nKnoten; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nKnoten; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName=supportInitial + face + id1 + id2;
                        if (_modell.Randbedingungen.TryGetValue(supportName, out _))
                        {
                            _ = MessageBox.Show($"\nRandbedingung Boussinesq \"{supportName}\" bereits vorhanden.", "neue Randbedingung3DFlächen");
                            return;
                        }
                        var faceNode = $"0{abstände.Length - 1}";
                        var knotenName=face[..1] switch
                        {
                            "X" => knotenInitial + faceNode + id1 + id2,
                            "Y" => knotenInitial + id1 + faceNode + id2,
                            "Z" => knotenInitial + id1 + id2 + faceNode,
                            _ => throw new ParseAusnahme(
                                $"\nfalsche Flächen Id = {face[..1]}, muss sein:\n X, Y or Z")
                        };

                        for (var count = 0; count < type.Length; count++)
                        {
                            var subType = type.Substring(count, 1).ToLower();
                            double x, y, z, r, a, factor;
                            switch (subType)
                            {
                                case "x":
                                    x = abstände[nKnoten - 1];
                                    y = abstände[m];
                                    z = abstände[k];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[0] = x / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                    factor;
                                    break;
                                case "y":
                                    x = abstände[m];
                                    y = abstände[nKnoten - 1];
                                    z = abstände[k];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[1] = y / r * (r * z / (a * a) - (1 - 2 * poisson) * r / (a + z)) *
                                                    factor;
                                    break;
                                case "z":
                                    x = abstände[m];
                                    y = abstände[k];
                                    z = abstände[nKnoten - 1];
                                    r = Math.Sqrt(x * x + y * y);
                                    a = Math.Sqrt(z * z + r * r);
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[2] = (z * z / (a * a) + 2 * (1 - poisson)) * factor;
                                    break;
                                default:
                                    throw new ParseAusnahme(
                                        "\nfalsche Anzahl Parameter in RandbedingungBoussinesq, muss sein:\n"
                                        + "4 für lagerInitial, fläche, knotenInitial, Art\n");
                            }
                        }
                        var conditions = 0;
                        if (XFest.IsChecked != null && (bool)XFest.IsChecked)
                        {
                            conditions += 1;
                            type = "x";
                        }

                        if (YFest.IsChecked != null && (bool)YFest.IsChecked)
                        {
                            conditions += 2;
                            type += "y";
                        }

                        if (ZFest.IsChecked != null && (bool)ZFest.IsChecked)
                        {
                            conditions += 4;
                            type += "z";
                        }
                        var neuesLager = new Lager(knotenName, face, conditions, prescribed, AnzahlFreiheitsgrade)
                        {
                            RandbedingungId = supportName
                        };
                        _lagerDictionary.Add(supportName, neuesLager);
                    }
                }
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges Format: Inkremente", "Randbedingung3DBoussinesq");
            }
            
            _randbedingungenListe = Lagerbedingungen(_lagerDictionary);
            RandbedingungenGrid.Items.Clear();
            RandbedingungenGrid.ItemsSource = _randbedingungenListe;
        }
        private ObservableCollection<LagerbedingungBoussinesq> Lagerbedingungen(Dictionary<string, Lager> lagerDictionary)
        {
            foreach (var item in lagerDictionary)
            {
                var nodeId = item.Value.KnotenId;
                var supportName = item.Value.RandbedingungId;
                var material = _material;
                var fläche = item.Value.Face;
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
                        throw new ModellAusnahme("\nLagerbedingung für Lager " + supportName + " falsch definiert");
                }

                var lagerBedingung = new LagerbedingungBoussinesq(item.Key, nodeId, vordefiniert, material, fläche);
                _randbedingungenListe.Add(lagerBedingung);

            }
            return _randbedingungenListe;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _lagerDictionary.Where(item 
                         => !_modell.Randbedingungen.TryAdd(item.Key, item.Value)))
            {
                // vorhandene Randbedingung
                _ = MessageBox.Show("Randbedingung " + item.Key + " nicht hinzugefügt, da schon vorhanden", "neues Knotennetz");
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
}