using FE_Berechnungen.Dateieingabe;
using FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;
using FE_Berechnungen.Tragwerksberechnung.Ergebnisse;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FE_Berechnungen.Wärmeberechnung.Ergebnisse;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

namespace FE_Berechnungen;

public partial class StartFenster
{
    private FeModell _wärmeModell;
    private FeModell _tragwerkModell;
    private FeModell _elastizitätsModell;
    private Berechnung _modellBerechnung;
    public static TragwerkmodellVisualisieren TragwerkVisual { get; set; }
    public static ErgebnisseVisualisieren StatikErgebnisse { get; private set; }
    public static WärmemodellVisualisieren WärmeVisual { get; set; }
    public static StationäreErgebnisseVisualisieren StationäreErgebnisse { get; private set; }
    public static ElastizitätsmodellVisualisieren ElastizitätVisual { get; set; }
    public static Elastizitätsmodell3DVisualisieren ElastizitätVisual3D { get; set; }
    private static StatikErgebnisseVisualisieren ElastizitätsErgebnisse { get; set; }

    private OpenFileDialog _dateiDialog;
    private string _dateiPfad;
    private string[] _dateiZeilen;
    private FeParser _parse;

    public StartFenster()
    {
        InitializeComponent();
    }

    // Wärmeberechnung
    private void WärmedatenEinlesen(object sender, RoutedEventArgs e)
    {
        Language = XmlLanguage.GetLanguage("de-DE");

        var initial = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\input\\Wärmeberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\input\\Wärmeberechnung",
                         "Wärmeberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Wärmeberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _wärmeModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var wärmeElemente = new Wärmeberechnung.ModelldatenLesen.ElementParser();
            wärmeElemente.ParseWärmeElements(_dateiZeilen, _wärmeModell);

            var wärmeMaterial = new Wärmeberechnung.ModelldatenLesen.MaterialParser();
            wärmeMaterial.ParseMaterials(_dateiZeilen, _wärmeModell);

            var wärmeLasten = new Wärmeberechnung.ModelldatenLesen.LastParser();
            wärmeLasten.ParseLasten(_dateiZeilen, _wärmeModell);

            var wärmeRandbedingungen = new Wärmeberechnung.ModelldatenLesen.RandbedingungParser();
            wärmeRandbedingungen.ParseRandbedingungen(_dateiZeilen, _wärmeModell);

            var wärmeTransient = new Wärmeberechnung.ModelldatenLesen.TransientParser();
            wärmeTransient.ParseZeitintegration(_dateiZeilen, _wärmeModell);

            _wärmeModell.ZeitintegrationDaten = wärmeTransient.ZeitintegrationDaten;

            WärmeVisual = new WärmemodellVisualisieren(_wärmeModell);
            WärmeVisual.Show();
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void WärmedatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var wärmeDatenEdit = new ModelldatenEditieren();
            wärmeDatenEdit.Show();
        }
        else
        {
            var wärmeDatenEdit = new ModelldatenEditieren(_dateiPfad);
            wärmeDatenEdit.Show();
        }
    }

    private void WärmedatenSichern(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        var wärmedatei = new NeuerDateiname();
        wärmedatei.ShowDialog();

        var name = wärmedatei.DateiName;

        if (_wärmeModell == null)
        {
            _ = MessageBox.Show("Modell ist noch nicht definiert", "Wärmeberechnung");
            return;
        }

        var zeilen = new List<string>
        {
            "ModellName",
            _wärmeModell.ModellId,
            "\nRaumdimension"
        };
        const int numberNodalDof = 1;
        zeilen.Add(_wärmeModell.Raumdimension + "\t" + numberNodalDof + "\n");

        // Knoten
        zeilen.Add("Knoten");
        if (_wärmeModell.Raumdimension == 2)
            zeilen.AddRange(_wärmeModell.Knoten.Select(knoten => knoten.Key
                                                                 + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                 knoten.Value.Koordinaten[1]));
        else
            zeilen.AddRange(_wärmeModell.Knoten.Select(knoten => knoten.Key
                                                                 + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                 knoten.Value.Koordinaten[1] + "\t" +
                                                                 knoten.Value.Koordinaten[2]));

        // Elemente
        var alleElement2D2 = new List<Element2D2>();
        var alleElement2D3 = new List<Element2D3>();
        var alleElement2D4 = new List<Element2D4>();
        var alleElement3D8 = new List<Element3D8>();
        foreach (var item in _wärmeModell.Elemente)
            switch (item.Value)
            {
                case Element2D2 element2D2:
                    alleElement2D2.Add(element2D2);
                    break;
                case Element2D3 element2D3:
                    alleElement2D3.Add(element2D3);
                    break;
                case Element2D4 element2D4:
                    alleElement2D4.Add(element2D4);
                    break;
                case Element3D8 element3D8:
                    alleElement3D8.Add(element3D8);
                    break;
            }

        if (alleElement2D2.Count != 0)
        {
            zeilen.Add("\n" + "Elemente2D2Knoten");
            zeilen.AddRange(alleElement2D2.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.ElementMaterialId));
        }

        if (alleElement2D3.Count != 0)
        {
            zeilen.Add("\n" + "Elemente2D3Knoten");
            zeilen.AddRange(alleElement2D3.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.ElementMaterialId));
        }

        if (alleElement2D4.Count != 0)
        {
            zeilen.Add("\n" + "Elemente2D4Knoten");
            zeilen.AddRange(alleElement2D4.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.KnotenIds[3] + "\t" + item.ElementMaterialId));
        }

        if (alleElement3D8.Count != 0)
        {
            zeilen.Add("\n" + "Elemente3D8Knoten");
            zeilen.AddRange(alleElement3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                          item.KnotenIds[3] + "\t"
                                                          + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" +
                                                          item.KnotenIds[6] + "\t"
                                                          + item.KnotenIds[7] + "\t" + item.ElementMaterialId));
        }

        // Materialien
        zeilen.Add("\n" + "Material");
        foreach (var item in _wärmeModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++) sb.Append("\t" + item.Value.MaterialWerte[i]);
            zeilen.Add(sb.ToString());
        }

        // Lasten
        foreach (var item in _wärmeModell.Lasten)
        {
            sb.Clear();
            sb.Append("\n" + "KnotenLasten");
            sb.Append(item.Value.LastId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
            zeilen.Add(sb.ToString());
        }

        foreach (var item in _wärmeModell.LinienLasten)
        {
            sb.Clear();
            sb.Append("\n" + "LinienLasten");
            sb.Append(item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.EndKnotenId + "\t"
                      + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]);
            zeilen.Add(sb.ToString());
        }

        var alleElementlasten3 = new List<ElementLast3>();
        var alleElementlasten4 = new List<ElementLast4>();
        foreach (var item in _wärmeModell.ElementLasten)
            switch (item.Value)
            {
                case ElementLast3 elementlast3:
                    alleElementlasten3.Add(elementlast3);
                    break;
                case ElementLast4 elementlast4:
                    alleElementlasten4.Add(elementlast4);
                    break;
            }

        if (alleElementlasten3.Count != 0)
        {
            zeilen.Add("\n" + "ElementLast3");
            zeilen.AddRange(alleElementlasten3.Select(item => item.LastId + "\t" + item.ElementId + "\t"
                                                              + item.Lastwerte[0] + "\t" + item.Lastwerte[1] + "\t" +
                                                              item.Lastwerte[2]));
        }

        if (alleElementlasten4.Count != 0)
        {
            zeilen.Add("\n" + "ElementLast4");
            zeilen.AddRange(alleElementlasten4.Select(item => item.LastId + "\t" + item.ElementId + "\t"
                                                              + item.Lastwerte[0] + "\t" + item.Lastwerte[1]
                                                              + "\t" + item.Lastwerte[2] + "\t" + item.Lastwerte[3]));
        }

        // Randbedingungen
        zeilen.Add("\n" + "Randbedingungen");
        foreach (var item in _wärmeModell.Randbedingungen)
        {
            sb.Clear();
            sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + item.Value.Vordefiniert[0]);
            zeilen.Add(sb.ToString());
        }

        // Eigenlösungen
        if (_wärmeModell.Eigenzustand != null)
        {
            zeilen.Add("\n" + "Eigenlösungen");
            zeilen.Add(_wärmeModell.Eigenzustand.Id + "\t" + _wärmeModell.Eigenzustand.AnzahlZustände);
        }

        // Parameter
        if (_wärmeModell.Zeitintegration != null)
        {
            zeilen.Add("\n" + "Zeitintegration");
            zeilen.Add(_wärmeModell.Zeitintegration.Id + "\t" + _wärmeModell.Zeitintegration.Tmax + "\t" +
                       _wärmeModell.Zeitintegration.Dt
                       + "\t" + _wärmeModell.Zeitintegration.Parameter1);
        }

        // zeitabhängige Anfangsbedingungen
        if (_wärmeModell.Zeitintegration.VonStationär || _wärmeModell.Zeitintegration.Anfangsbedingungen.Count != 0)
            zeilen.Add("\n" + "Anfangstemperaturen");
        if (_wärmeModell.Zeitintegration.VonStationär) zeilen.Add("stationäre Loesung");

        zeilen.AddRange(from Knotenwerte knotenwerte in _wärmeModell.Zeitintegration.Anfangsbedingungen
                        select knotenwerte.KnotenId + "\t" + knotenwerte.Werte[0]);

        // zeitabhängige Randbedingungen
        if (_wärmeModell.ZeitabhängigeRandbedingung.Count != 0) zeilen.Add("\n" + "Zeitabhängige Randtemperaturen");
        foreach (var item in _wärmeModell.ZeitabhängigeRandbedingung)
        {
            sb.Clear();
            sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId);
            switch (item.Value.VariationsTyp)
            {
                case 0:
                    sb.Append("\tdatei");
                    break;
                case 1:
                    sb.Append("\tkonstant" + item.Value.KonstanteTemperatur);
                    break;
                case 2:
                    sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" +
                              item.Value.PhasenWinkel);
                    break;
                case 3:
                    {
                        sb.Append("\tlinear");
                        var anzahlIntervalle = item.Value.Intervall.Length;
                        for (var i = 0; i < anzahlIntervalle; i += 2)
                            sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);

                        break;
                    }
            }

            zeilen.Add(sb.ToString());
        }

        // zeitabhängige Knotentemperaturen
        if (_wärmeModell.ZeitabhängigeKnotenLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Knotenlast");
        foreach (var item in _wärmeModell.ZeitabhängigeKnotenLasten)
        {
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId);
            switch (item.Value.VariationsTyp)
            {
                case 0:
                    sb.Append("\tdatei");
                    break;
                case 2:
                    sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" +
                              item.Value.PhasenWinkel);
                    break;
                case 3:
                    {
                        sb.Append("\tlinear");
                        var anzahlIntervalle = item.Value.Intervall.Length;
                        for (var i = 0; i < anzahlIntervalle; i += 2)
                            sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);

                        break;
                    }
            }

            zeilen.Add(sb.ToString());
        }

        // zeitabhängige Elementtemperaturen
        if (_wärmeModell.ZeitabhängigeElementLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Elementtemperaturen");
        foreach (var item in _wärmeModell.ZeitabhängigeElementLasten)
        {
            sb.Clear();
            sb.Append(item.Key + "\t" + item.Value.ElementId);

            if (item.Value.VariationsTyp == 1)
            {
                sb.Append("\tkonstant");
                foreach (var wert in item.Value.P) sb.Append("\t" + wert);
            }

            zeilen.Add(sb.ToString());
        }

        // Dateiende
        zeilen.Add("\nend");

        // alle Zeilen in Datei schreiben
        var dateiName = "\\" + name + ".inp";
        _dateiPfad = _dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(_dateiPfad, zeilen);
    }

    private void WärmedatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            var wärme = new WärmedatenAnzeigen(_wärmeModell);
            wärme.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }

    private void WärmedatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            WärmeVisual = new WärmemodellVisualisieren(_wärmeModell);
            WärmeVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }

    private void WärmedatenBerechnen(object sender, EventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                _modellBerechnung = new Berechnung(_wärmeModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _wärmeModell.Berechnet = true;
                _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Wärmeberechnung");
            }
            else
            {
                _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void WärmeberechnungErgebnisseAnzeigen(object sender, EventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                if (!_wärmeModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_wärmeModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _wärmeModell.Berechnet = true;
                }

                var ergebnisse = new StationäreErgebnisseAnzeigen(_wärmeModell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void WärmeberechnungErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_wärmeModell != null)
            {
                if (!_wärmeModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_wärmeModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _wärmeModell.Berechnet = true;
                }

                StationäreErgebnisse = new StationäreErgebnisseVisualisieren(_wärmeModell);
                StationäreErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void InstationäreDaten(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            var wärme = new InstationäreDatenAnzeigen(_wärmeModell);
            wärme.Show();
            _wärmeModell.ZeitintegrationBerechnet = false;
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void WärmeAnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            //_modellBerechnung ??= new Berechnung(_wärmeModell);
            var anregung = new Wärmeberechnung.ModelldatenLesen.ZeitAnregungVisualisieren(_wärmeModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void EigenlösungWärmeBerechnen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung = new Berechnung(_wärmeModell);
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                _wärmeModell.Berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte != null) return;
            _modellBerechnung.Eigenzustände();
            _wärmeModell.EigenBerechnet = true;
            _ = MessageBox.Show("Eigenlösung erfolgreich ermittelt", "Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void EigenlösungWärmeAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung ??= new Berechnung(_wärmeModell);
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                _wärmeModell.Berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
            var eigen = new Wärmeberechnung.Ergebnisse.EigenlösungAnzeigen(_wärmeModell); //Eigenlösung.Eigenlösung(modell));
            eigen.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void EigenlösungWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell != null)
        {
            _modellBerechnung ??= new Berechnung(_wärmeModell);
            if (!_wärmeModell.ZeitintegrationBerechnet)
            {
                _modellBerechnung.BerechneSystemMatrix();
                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            }

            // default = 2 Eigenzustände, falls nicht anders spezifiziert
            _wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (_wärmeModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
            var visual = new Wärmeberechnung.Ergebnisse.EigenlösungVisualisieren(_wärmeModell);
            visual.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }

    private void InstationäreBerechnung(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationDaten && _wärmeModell != null)
        {
            if (!_wärmeModell.Berechnet)
            {
                _modellBerechnung = new Berechnung(_wärmeModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _wärmeModell.Berechnet = true;
            }

            _modellBerechnung.ZeitintegrationErsterOrdnung();
            _wärmeModell.ZeitintegrationBerechnet = true;
            _ = MessageBox.Show("Zeitintegration erfolgreich durchgeführt", "instationäre Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Wärmeberechnung");
            const double tmax = 0;
            const double dt = 0;
            const double alfa = 0;
            if (_wärmeModell != null)
            {
                _wärmeModell.Zeitintegration = new Wärmeberechnung.Modelldaten.Zeitintegration(tmax, dt, alfa) { VonStationär = false };
                _wärmeModell.ZeitintegrationDaten = true;
                var wärme = new InstationäreDatenAnzeigen(_wärmeModell);
                wärme.Show();
            }

            _wärmeModell.ZeitintegrationBerechnet = false;
        }
    }

    private void InstationäreErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        //WärmeVisual?.Close();
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var ergebnisse = new InstationäreErgebnisseAnzeigen(_wärmeModell, WärmeVisual);
            ergebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    private void InstationäreModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var modellzuständeVisualisieren = new InstationäreModellzuständeVisualisieren(_wärmeModell);
            modellzuständeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    private void KnotenzeitverläufeWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_wärmeModell.ZeitintegrationBerechnet && _wärmeModell != null)
        {
            var knotenzeitverläufeVisualisieren =
                new Wärmeberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(_wärmeModell);
            knotenzeitverläufeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    // Tragwerksberechnung
    private void TragwerksdatenEinlesen(object sender, RoutedEventArgs e)
    {
        var initial = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\input\\Tragwerksberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\input\\Tragwerksberechnung",
                         "Tragwerksberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Tragwerksberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _tragwerkModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var tragwerksMaterial = new Tragwerksberechnung.ModelldatenLesen.MaterialParser();
            tragwerksMaterial.ParseMaterials(_dateiZeilen, _tragwerkModell);

            var tragwerksElemente = new Tragwerksberechnung.ModelldatenLesen.ElementParser();
            tragwerksElemente.ParseElements(_dateiZeilen, _tragwerkModell);

            var tragwerksLasten = new Tragwerksberechnung.ModelldatenLesen.LastParser();
            tragwerksLasten.ParseLasten(_dateiZeilen, _tragwerkModell);

            var tragwerksRandbedingungen = new Tragwerksberechnung.ModelldatenLesen.RandbedingungParser();
            tragwerksRandbedingungen.ParseRandbedingungen(_dateiZeilen, _tragwerkModell);

            var tragwerksTransient = new Tragwerksberechnung.ModelldatenLesen.TransientParser();
            tragwerksTransient.ParseZeitintegration(_dateiZeilen, _tragwerkModell);

            _tragwerkModell.ZeitintegrationDaten = tragwerksTransient.ZeitintegrationDaten;
            _tragwerkModell.Berechnet = false;
            _tragwerkModell.ZeitintegrationBerechnet = false;

            TragwerkVisual = new TragwerkmodellVisualisieren(_tragwerkModell);
            TragwerkVisual.Show();
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void TragwerksdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var tragwerksdaten = new ModelldatenEditieren();
            tragwerksdaten.Show();
        }
        else
        {
            var tragwerksdaten = new ModelldatenEditieren(_dateiPfad);
            tragwerksdaten.Show();
        }
    }

    private void TragwerksdatenSichern(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        var tragwerksdatei = new NeuerDateiname();
        tragwerksdatei.ShowDialog();

        var name = tragwerksdatei.DateiName;

        var zeilen = new List<string>
        {
            "ModellName",
            _tragwerkModell.ModellId,
            "\nRaumdimension",
            _tragwerkModell.Raumdimension + "\t" + _tragwerkModell.AnzahlKnotenfreiheitsgrade,
            // Knoten
            "\nKnoten"
        };

        switch (_tragwerkModell.Raumdimension)
        {
            case 1:
                zeilen.AddRange(_tragwerkModell.Knoten.Select(knoten => knoten.Key
                                                                        + "\t" + knoten.Value.Koordinaten[0]));
                break;
            case 2:
                zeilen.AddRange(_tragwerkModell.Knoten.Select(knoten => knoten.Key
                                                                        + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                        knoten.Value.Koordinaten[1]));
                break;
            case 3:
                zeilen.AddRange(_tragwerkModell.Knoten.Select(knoten => knoten.Key
                                                                        + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                        knoten.Value.Koordinaten[1] + "\t" +
                                                                        knoten.Value.Koordinaten[2]));
                break;
            default:
                _ = MessageBox.Show("falsche Raumdimension, muss 1, 2 oder 3 sein", "Structural Analysis");
                return;
        }


        // Elemente
        var alleFachwerkelemente = new List<Fachwerk>();
        var alleBiegebalken = new List<Biegebalken>();
        var alleBiegebalkenGelenk = new List<BiegebalkenGelenk>();
        var alleFederElemente = new List<FederElement>();
        foreach (var item in _tragwerkModell.Elemente)
            switch (item.Value)
            {
                case Fachwerk fachwerk:
                    alleFachwerkelemente.Add(fachwerk);
                    break;
                case Biegebalken biegebalken:
                    alleBiegebalken.Add(biegebalken);
                    break;
                case BiegebalkenGelenk biegebalkenGelenk:
                    alleBiegebalkenGelenk.Add(biegebalkenGelenk);
                    break;
                case FederElement federElement:
                    alleFederElemente.Add(federElement);
                    break;
            }

        var alleQuerschnitte = _tragwerkModell.Querschnitt.Select(item => item.Value).ToList();

        if (alleFachwerkelemente.Count != 0)
        {
            zeilen.Add("\nFachwerk");
            zeilen.AddRange(alleFachwerkelemente.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                                + item.KnotenIds[1] + "\t" + item.ElementQuerschnittId +
                                                                "\t" + item.ElementMaterialId));
        }

        if (alleBiegebalken.Count != 0)
        {
            zeilen.Add("\nBiegebalken");
            zeilen.AddRange(alleBiegebalken.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.ElementQuerschnittId +
                                                           "\t" + item.ElementMaterialId));
        }

        if (alleBiegebalkenGelenk.Count != 0)
        {
            zeilen.Add("\nBiegebalkenGelenk");
            zeilen.AddRange(alleBiegebalkenGelenk.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                                 + item.KnotenIds[1] + "\t" +
                                                                 item.ElementQuerschnittId + "\t" +
                                                                 item.ElementMaterialId + "\t" + item.Typ));
        }

        if (alleFederElemente.Count != 0)
        {
            zeilen.Add("\nFederelement");
            zeilen.AddRange(alleFederElemente.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                             + item.ElementMaterialId));
        }

        if (alleQuerschnitte.Count != 0)
        {
            zeilen.Add("\nQuerschnitt");
            zeilen.AddRange(alleQuerschnitte.Select(item => item.QuerschnittId + "\t"
                                                                               + item.QuerschnittsWerte[0] + "\t" +
                                                                               item.QuerschnittsWerte[1]));
        }

        // Materialien
        zeilen.Add("\nMaterial");
        foreach (var item in _tragwerkModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++) sb.Append("\t" + item.Value.MaterialWerte[i]);
            zeilen.Add(sb.ToString());
        }

        // Lasten
        foreach (var item in _tragwerkModell.Lasten)
        {
            zeilen.Add("\nKnotenlast");
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
            zeilen.Add(sb.ToString());
        }

        foreach (var item in _tragwerkModell.PunktLasten)
        {
            var punktlast = (PunktLast)item.Value;
            sb.Clear();
            zeilen.Add("\nPunktlast");
            zeilen.Add(punktlast.LastId + "\t" + punktlast.ElementId
                       + "\t" + punktlast.Lastwerte[0] + "\t" + punktlast.Lastwerte[1] + "\t" + punktlast.Offset);
        }

        foreach (var item in _tragwerkModell.ElementLasten)
        {
            sb.Clear();
            zeilen.Add("\nLinienlast");
            zeilen.Add(item.Value.LastId + "\t" + item.Value.ElementId
                       + "\t" + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]
                       + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]
                       + "\t" + item.Value.InElementKoordinatenSystem);
        }

        // Randbedingungen
        var fest = string.Empty;
        zeilen.Add("\nLager");
        foreach (var item in _tragwerkModell.Randbedingungen)
        {
            if (item.Value.Typ == 1) fest = "x";
            else if (item.Value.Typ == 2) fest = "y";
            else if (item.Value.Typ == 3) fest = "xy";
            else if (item.Value.Typ == 7) fest = "xyr";
            zeilen.Add(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + fest);
        }

        // Dateiende
        zeilen.Add("\nend");

        // alle Zeilen in Datei schreiben
        var dateiName = "\\" + name + ".inp";
        _dateiPfad = _dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(_dateiPfad, zeilen);
    }

    private void TragwerksdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell != null)
        {
            var tragwerk = new TragwerkdatenAnzeigen(_tragwerkModell);
            tragwerk.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksmodelldaten müssen erst definiert werden", "statische Tragwerksanalyse");
        }
    }

    private void TragwerksdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell != null)
        {
            TragwerkVisual = new TragwerkmodellVisualisieren(_tragwerkModell);
            TragwerkVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksmodelldaten müssen erst definiert werden", "statische Tragwerksanalyse");
        }
    }

    private void TragwerksdatenBerechnen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                //if (Berechnet) return;
                _modellBerechnung = new Berechnung(_tragwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _tragwerkModell.Berechnet = true;
                _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "statische Tragwerksberechnung");
            }
            else
            {
                _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void StatikErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                if (!_tragwerkModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_tragwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _tragwerkModell.Berechnet = true;
                }

                var ergebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseAnzeigen(_tragwerkModell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void StatikErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                if (!_tragwerkModell.Berechnet)
                {
                    _modellBerechnung = new Berechnung(_tragwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _modellBerechnung.BerechneSystemVektor();
                    _modellBerechnung.LöseGleichungen();
                    _tragwerkModell.Berechnet = true;
                }

                StatikErgebnisse = new ErgebnisseVisualisieren(_tragwerkModell);
                StatikErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void EigenlösungTragwerkBerechnen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                if (!_tragwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_tragwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _tragwerkModell.Berechnet = true;
                }

                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                _tragwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_tragwerkModell.Eigenzustand.Eigenwerte != null) return;
                _modellBerechnung.Eigenzustände();
                _tragwerkModell.EigenBerechnet = true;
                _ = MessageBox.Show("Eigenfrequenzen erfolgreich ermittelt", "Tragwerksberechnung");
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void EigenlösungTragwerkAnzeigen(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                if (!_tragwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_tragwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _tragwerkModell.Berechnet = true;
                }

                // default = 2 Eigenstates, falls nicht anders spezifiziert
                _tragwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_tragwerkModell.Eigenzustand.Eigenwerte == null) _modellBerechnung.Eigenzustände();
                var eigen = new Tragwerksberechnung.Ergebnisse.EigenlösungAnzeigen(_tragwerkModell);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void EigenlösungTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell != null)
            {
                if (!_tragwerkModell.Berechnet)
                {
                    _modellBerechnung ??= new Berechnung(_tragwerkModell);
                    _modellBerechnung.BerechneSystemMatrix();
                    _tragwerkModell.Berechnet = true;
                }

                // default = 2 Eigenstates, falls nicht anders spezifiziert
                _tragwerkModell.Eigenzustand ??= new Eigenzustände("default", 2);
                if (_tragwerkModell.Eigenzustand.Eigenwerte != null) _modellBerechnung.Eigenzustände();
                var visual = new Tragwerksberechnung.Ergebnisse.EigenlösungVisualisieren(_tragwerkModell);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void DynamischeDaten(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell == null)
            {
                _ = MessageBox.Show("Tragwerkmodell noch nicht spezifiziert", "Tragwerksberechnung");
                return;
            }

            if (_tragwerkModell.Zeitintegration == null)
            {
                _tragwerkModell.Zeitintegration = new Tragwerksberechnung.Modelldaten.Zeitintegration(0, 0, 0, 0)
                { VonStationär = false };
                _tragwerkModell.ZeitintegrationDaten = true;
            }

            var tragwerk = new DynamikDatenAnzeigen(_tragwerkModell);
            tragwerk.Show();
            _tragwerkModell.ZeitintegrationBerechnet = false;
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void AnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell != null)
        {
            //_modellBerechnung ??= new Berechnung(_tragwerkModell);
            var anregung = new Tragwerksberechnung.ModelldatenLesen.ZeitAnregungVisualisieren(_tragwerkModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Tragwerkberechnung sind noch nicht spezifiziert", "Tragwerkberechnung");
        }
    }

    private void DynamischeBerechnung(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_tragwerkModell.ZeitintegrationDaten && _tragwerkModell != null)
            {
                _modellBerechnung = new Berechnung(_tragwerkModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _tragwerkModell.Berechnet = true;

                _modellBerechnung.ZeitintegrationZweiterOrdnung();
                _tragwerkModell.ZeitintegrationBerechnet = true;
            }
            else
            {
                _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void DynamischeErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell is { ZeitintegrationBerechnet: false })
            _ = new DynamischeErgebnisseAnzeigen(_tragwerkModell);
        else
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
    }

    private void DynamischeModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell is { ZeitintegrationBerechnet: false })
        {
            var dynamikErgebnisse = new DynamischeModellzuständeVisualisieren(_tragwerkModell);
            dynamikErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }

    private void KnotenzeitverläufeTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_tragwerkModell is { ZeitintegrationBerechnet: true })
        {
            var knotenzeitverläufe =
                new Tragwerksberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(_tragwerkModell);
            knotenzeitverläufe.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }

    // Elastizitätsberechnung
    private void ElastizitätsdatenEinlesen(object sender, RoutedEventArgs e)
    {
        var initial = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\FE Berechnungen";
        _dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = initial
        };

        if (Directory.Exists(_dateiDialog.InitialDirectory))
        {
            _dateiDialog.InitialDirectory += "\\input\\Elastizitätsberechnung";
            _dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für FE Berechnungen \"" + initial + "\" nicht gefunden," +
                                " Eingabedatei am Speicherort von \\FE Berechnungen\\input\\Elastizitätsberechnung",
                         "Elastizitätsberechnung");
            _dateiDialog.ShowDialog();
        }

        _dateiPfad = _dateiDialog.FileName;

        if (_dateiPfad.Length == 0)
        {
            _ = MessageBox.Show("Eingabedatei ist leer", "Elastizitätsberechnung");
            return;
        }

        _dateiZeilen = File.ReadAllLines(_dateiPfad, Encoding.UTF8);

        try
        {
            _parse = new FeParser();
            _parse.ParseModell(_dateiZeilen);
            _elastizitätsModell = _parse.FeModell;
            _parse.ParseNodes(_dateiZeilen);

            var parseElastizität = new ElastizitätsParser();
            parseElastizität.ParseElastizität(_dateiZeilen, _elastizitätsModell);

            _elastizitätsModell.Berechnet = false;
        }
        catch (ParseAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
        switch (_elastizitätsModell.Raumdimension)
        {
            case 2:
                {
                    ElastizitätVisual = new ElastizitätsmodellVisualisieren(_elastizitätsModell);
                    ElastizitätVisual.Show();
                    break;
                }
            case 3:
                {
                    ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_elastizitätsModell);
                    ElastizitätVisual3D.Show();
                    break;
                }
        }
    }

    private void ElastizitätsdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (_dateiPfad == null)
        {
            var elastizitätsdaten = new ModelldatenEditieren();
            elastizitätsdaten.Show();
        }
        else
        {
            var elastizitätsdaten = new ModelldatenEditieren(_dateiPfad);
            elastizitätsdaten.Show();
        }
    }

    private void ElastizitätsdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (_elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
            return;
        }

        var tragwerk = new ElastizitätsdatenAnzeigen(_elastizitätsModell);
        tragwerk.Show();
    }

    private void ElastizitätsdatenSichern(object sender, RoutedEventArgs e)
    {
        var elastizitätsdatei = new NeuerDateiname();
        elastizitätsdatei.ShowDialog();

        var name = elastizitätsdatei.DateiName;

        var zeilen = new List<string>
        {
            "ModellName",
            _elastizitätsModell.ModellId,
            "\nRaumdimension"
        };
        const int knotenfreiheitsgrade = 3;
        zeilen.Add(_elastizitätsModell.Raumdimension + "\t" + knotenfreiheitsgrade);

        // Knoten
        zeilen.Add("\nKnoten");
        if (_elastizitätsModell.Raumdimension == 2)
            zeilen.AddRange(_elastizitätsModell.Knoten.Select(knoten => knoten.Key
                                                                        + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                        knoten.Value.Koordinaten[1]));
        else
            zeilen.AddRange(_elastizitätsModell.Knoten.Select(knoten => knoten.Key
                                                                        + "\t" + knoten.Value.Koordinaten[0] + "\t" +
                                                                        knoten.Value.Koordinaten[1] + "\t" +
                                                                        knoten.Value.Koordinaten[2]));

        // Elemente
        var alleElemente2D3 = new List<Elastizitätsberechnung.Modelldaten.Element2D3>();
        var alleElemente3D8 = new List<Elastizitätsberechnung.Modelldaten.Element3D8>();
        foreach (var item in _elastizitätsModell.Elemente)
            switch (item.Value)
            {
                case Elastizitätsberechnung.Modelldaten.Element2D3 element2D3:
                    alleElemente2D3.Add(element2D3);
                    break;
                case Elastizitätsberechnung.Modelldaten.Element3D8 element3D8:
                    alleElemente3D8.Add(element3D8);
                    break;
            }

        var alleQuerschnitte = _elastizitätsModell.Querschnitt.Select(item => item.Value).ToList();

        if (alleElemente2D3.Count != 0)
        {
            zeilen.Add("\nElement2D3");
            zeilen.AddRange(alleElemente2D3.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t"
                                                           + item.ElementQuerschnittId + "\t" +
                                                           item.ElementMaterialId));
        }

        if (alleElemente3D8.Count != 0)
        {
            zeilen.Add("\nElement3D8");
            zeilen.AddRange(alleElemente3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" +
                                                           item.KnotenIds[3] + "\t"
                                                           + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" +
                                                           item.KnotenIds[6] + "\t"
                                                           + item.KnotenIds[7] + "\t" + item.ElementMaterialId));
        }

        if (alleQuerschnitte.Count != 0)
        {
            zeilen.Add("\nQuerschnitt");
            zeilen.AddRange(alleQuerschnitte.Select(item => item.QuerschnittId + "\t"
                                                                               + item.QuerschnittsWerte[0]));
        }

        // Materialien
        zeilen.Add("\n" + "Material");
        var sb = new StringBuilder();
        foreach (var item in _elastizitätsModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++) sb.Append("\t" + item.Value.MaterialWerte[i]);
            zeilen.Add(sb.ToString());
        }

        // Lasten
        if (_elastizitätsModell.Lasten.Count > 0) zeilen.Add("\nKnotenlasten");
        foreach (var item in _elastizitätsModell.Lasten)
        {
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++) sb.Append("\t" + item.Value.Lastwerte[i]);
            zeilen.Add(sb.ToString());
        }

        if (_elastizitätsModell.LinienLasten.Count > 0) zeilen.Add("\nLinienlasten");
        zeilen.AddRange(_elastizitätsModell.LinienLasten.Select(item
            => item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.Lastwerte[0] + "\t" +
               item.Value.Lastwerte[1]
               + "\t" + item.Value.EndKnotenId + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]));

        // Randbedingungen
        var fest = string.Empty;
        zeilen.Add("\nRandbedingungen");
        foreach (var item in _elastizitätsModell.Randbedingungen)
        {
            sb.Clear();
            switch (_elastizitätsModell.Raumdimension)
            {
                case 2 when item.Value.Typ == 1:
                    fest = "x";
                    break;
                case 2 when item.Value.Typ == 2:
                    fest = "y";
                    break;
                case 2 when item.Value.Typ == 3:
                    fest = "xy";
                    break;
                case 2:
                    {
                        if (item.Value.Typ == 7) fest = "xyr";
                        break;
                    }
                case 3 when item.Value.Typ == 1:
                    fest = "x";
                    break;
                case 3 when item.Value.Typ == 2:
                    fest = "y";
                    break;
                case 3 when item.Value.Typ == 3:
                    fest = "xy";
                    break;
                case 3 when item.Value.Typ == 4:
                    fest = "z";
                    break;
                case 3 when item.Value.Typ == 5:
                    fest = "xz";
                    break;
                case 3 when item.Value.Typ == 6:
                    fest = "yz";
                    break;
                case 3:
                    {
                        if (item.Value.Typ == 7) fest = "xyz";
                        break;
                    }
            }

            sb.Append(item.Key + "\t" + item.Value.KnotenId + "\t" + fest);
            foreach (var wert in item.Value.Vordefiniert) sb.Append("\t" + wert);
            zeilen.Add(sb.ToString());
        }

        // Dateiende
        zeilen.Add("\nend");

        // alle Zeilen in Datei schreiben
        var dateiName = "\\" + name + ".inp";
        _dateiPfad = _dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(_dateiPfad, zeilen);
    }

    private void ElastizitätsdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (_elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
            return;
        }

        switch (_elastizitätsModell.Raumdimension)
        {
            case 2:
                {
                    ElastizitätVisual = new ElastizitätsmodellVisualisieren(_elastizitätsModell);
                    ElastizitätVisual.Show();
                    break;
                }
            case 3:
                {
                    ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_elastizitätsModell);
                    ElastizitätVisual3D.Show();
                    break;
                }
        }
    }

    private void ElastizitätsdatenBerechnen(object sender, RoutedEventArgs e)
    {
        if (_elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert",
                "Elastizitätsberechnung");
            return;
        }

        try
        {
            _modellBerechnung = new Berechnung(_elastizitätsModell);
            _modellBerechnung.BerechneSystemMatrix();
            _modellBerechnung.BerechneSystemVektor();
            _modellBerechnung.LöseGleichungen();
            _elastizitätsModell.Berechnet = true;

            _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Elastizitätsberechnung");
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void ElastizitätsberechnungErgebnisse(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_elastizitätsModell.Berechnet)
            {
                if (_elastizitätsModell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert",
                        "Elastizitätsberechnung");
                    return;
                }

                _modellBerechnung = new Berechnung(_elastizitätsModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _elastizitätsModell.Berechnet = true;
            }

            var ergebnisse = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisseAnzeigen(_elastizitätsModell);
            ergebnisse.Show();
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }
    }

    private void ElastizitätsErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        try
        {
            if (!_elastizitätsModell.Berechnet)
            {
                if (_elastizitätsModell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert",
                        "Elastizitätsberechnung");
                    return;
                }

                _modellBerechnung = new Berechnung(_elastizitätsModell);
                _modellBerechnung.BerechneSystemMatrix();
                _modellBerechnung.BerechneSystemVektor();
                _modellBerechnung.LöseGleichungen();
                _elastizitätsModell.Berechnet = true;
            }
        }
        catch (BerechnungAusnahme e2)
        {
            _ = MessageBox.Show(e2.Message);
        }

        switch (_elastizitätsModell!.Raumdimension)
        {
            case 2:
                {
                    ElastizitätsErgebnisse = new StatikErgebnisseVisualisieren(_elastizitätsModell);
                    ElastizitätsErgebnisse.Show();
                    break;
                }
            case 3:
                {
                    var elastizitätsErgebnisse3D = new StatikErgebnisse3DVisualisieren(_elastizitätsModell);
                    elastizitätsErgebnisse3D.Show();
                    break;
                }
            default:
                _ = MessageBox.Show(sb.ToString(), "falsche Raumdimension, muss 2 oder 3 sein");
                break;
        }
    }

}