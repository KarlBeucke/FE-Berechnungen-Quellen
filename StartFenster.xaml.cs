using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace FE_Berechnungen;

public partial class StartFenster
{
    private FeParser parse;
    public static Berechnung modellBerechnung;
    private OpenFileDialog dateiDialog;
    private string dateiPfad;
    private FeModell tragwerksModell;
    public static Tragwerksberechnung.ModelldatenAnzeigen.TragwerkmodellVisualisieren tragwerkVisual;
    public static Tragwerksberechnung.Ergebnisse.StatikErgebnisseVisualisieren statikErgebnisse;
    private FeModell wärmeModell;
    public static Wärmeberechnung.ModelldatenAnzeigen.WärmemodellVisualisieren wärmeVisual;
    public static Wärmeberechnung.Ergebnisse.StationäreErgebnisseVisualisieren stationäreErgebnisse;
    private FeModell elastizitätsModell;

    private string[] dateiZeilen;
    public static bool zeitintegrationDaten;
    public static bool berechnet, eigenBerechnet, zeitintegrationBerechnet;

    public StartFenster()
    {
        InitializeComponent();
    }
    //********************************************************************
    // Wärmeberechnung
    private void WärmedatenEinlesen(object sender, RoutedEventArgs e)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        var sb = new StringBuilder();
        dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        };

        if (Directory.Exists(dateiDialog.InitialDirectory + "\\FE-Berechnungen-App\\input"))
        {
            dateiDialog.InitialDirectory += "\\FE-Berechnungen-App\\input\\Wärmeberechnung";
            dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für Eingabedatei " + dateiDialog.InitialDirectory +
                                " \\FE-Berechnungen-App\\input nicht gefunden", "Wärmeberechnung");
            dateiDialog.ShowDialog();
        }
        dateiPfad = dateiDialog.FileName;

        try
        {
            if (dateiPfad.Length == 0)
            {
                _ = MessageBox.Show("Eingabedatei ist leer", "Wärmeberechnung");
                return;
            }
            dateiZeilen = File.ReadAllLines(dateiPfad, Encoding.UTF8);
        }
        catch (ParseAusnahme)
        {
            throw new ParseAusnahme("Abbruch: Fehler beim Lesen aus Eingabedatei ");
        }

        parse = new FeParser();
        parse.ParseModell(dateiZeilen);
        wärmeModell = parse.FeModell;
        parse.ParseNodes(dateiZeilen);

        var wärmeElemente = new Wärmeberechnung.ModelldatenLesen.ElementParser();
        wärmeElemente.ParseWärmeElements(dateiZeilen, wärmeModell);

        var wärmeMaterial = new Wärmeberechnung.ModelldatenLesen.MaterialParser();
        wärmeMaterial.ParseMaterials(dateiZeilen, wärmeModell);

        var wärmeLasten = new Wärmeberechnung.ModelldatenLesen.LastParser();
        wärmeLasten.ParseLasten(dateiZeilen, wärmeModell);

        var wärmeRandbedingungen = new Wärmeberechnung.ModelldatenLesen.RandbedingungParser();
        wärmeRandbedingungen.ParseRandbedingungen(dateiZeilen, wärmeModell);

        var wärmeTransient = new Wärmeberechnung.ModelldatenLesen.TransientParser();
        wärmeTransient.ParseZeitintegration(dateiZeilen, wärmeModell);

        zeitintegrationDaten = wärmeTransient.zeitintegrationDaten;
        berechnet = false;
        zeitintegrationBerechnet = false;

        sb.Append(FeParser.EingabeGefunden + "\n\nWärmemodelldaten erfolgreich eingelesen");
        _ = MessageBox.Show(sb.ToString(), "Wärmeberechnung");
        sb.Clear();

        wärmeVisual = new Wärmeberechnung.ModelldatenAnzeigen.WärmemodellVisualisieren(wärmeModell);
        wärmeVisual.Show();
    }
    private void WärmedatenEditieren(object sender, RoutedEventArgs e)
    {
        if (dateiPfad == null)
        {
            var wärmeDatenEdit = new Dateieingabe.ModelldatenEditieren();
            wärmeDatenEdit.Show();
        }
        else
        {
            var wärmeDatenEdit = new Dateieingabe.ModelldatenEditieren(dateiPfad);
            wärmeDatenEdit.Show();
        }
    }
    private void WärmedatenSichern(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        var wärmedatei = new Dateieingabe.NeuerDateiname();
        wärmedatei.ShowDialog();

        var name = wärmedatei.dateiName;

        if (wärmeModell == null)
        {
            _ = MessageBox.Show("Modell ist noch nicht definiert", "Wärmeberechnung");
            return;
        }
        var zeilen = new List<string>
        {
            "ModellName",
            wärmeModell.ModellId,
            "\nRaumdimension"
        };
        const int numberNodalDof = 1;
        zeilen.Add(wärmeModell.Raumdimension + "\t" + numberNodalDof + "\n");

        // Knoten
        zeilen.Add("Knoten");
        if (wärmeModell.Raumdimension == 2)
        {
            zeilen.AddRange(wärmeModell.Knoten.Select(knoten => knoten.Key
                                                           + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
        }
        else
        {
            zeilen.AddRange(wärmeModell.Knoten.Select(knoten => knoten.Key
                                                           + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1] + "\t" + knoten.Value.Koordinaten[2]));
        }

        // Elemente
        var alleElement2D2 = new List<Element2D2>();
        var alleElement2D3 = new List<Element2D3>();
        var alleElement2D4 = new List<Element2D4>();
        var alleElement3D8 = new List<Element3D8>();
        foreach (var item in wärmeModell.Elemente)
        {
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
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" + item.ElementMaterialId));
        }
        if (alleElement2D4.Count != 0)
        {
            zeilen.Add("\n" + "Elemente2D4Knoten");
            zeilen.AddRange(alleElement2D4.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" + item.KnotenIds[3] + "\t" + item.ElementMaterialId));
        }
        if (alleElement3D8.Count != 0)
        {
            zeilen.Add("\n" + "Elemente3D8Knoten");
            zeilen.AddRange(alleElement3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                          + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" + item.KnotenIds[3] + "\t"
                                                          + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" + item.KnotenIds[6] + "\t"
                                                          + item.KnotenIds[7] + "\t" + item.ElementMaterialId));
        }

        // Materialien
        zeilen.Add("\n" + "Material");
        foreach (var item in wärmeModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++)
            {
                sb.Append("\t" + item.Value.MaterialWerte[i]);
            }
            zeilen.Add(sb.ToString());
        }

        // Lasten
        foreach (var item in wärmeModell.Lasten)
        {
            sb.Clear();
            sb.Append("\n" + "KnotenLasten");
            sb.Append(item.Value.LastId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++)
            {
                sb.Append("\t" + item.Value.Lastwerte[i]);
            }
            zeilen.Add(sb.ToString());
        }
        foreach (var item in wärmeModell.LinienLasten)
        {
            sb.Clear();
            sb.Append("\n" + "LinienLasten");
            sb.Append(item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.EndKnotenId + "\t"
                      + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]);
            zeilen.Add(sb.ToString());
        }

        var alleElementlasten3 = new List<ElementLast3>();
        var alleElementlasten4 = new List<ElementLast4>();
        foreach (var item in wärmeModell.ElementLasten)
        {
            switch (item.Value)
            {
                case ElementLast3 elementlast3:
                    alleElementlasten3.Add(elementlast3);
                    break;
                case ElementLast4 elementlast4:
                    alleElementlasten4.Add(elementlast4);
                    break;
            }
        }
        if (alleElementlasten3.Count != 0)
        {
            zeilen.Add("\n" + "ElementLast3");
            zeilen.AddRange(alleElementlasten3.Select(item => item.LastId + "\t" + item.ElementId + "\t"
                                                              + item.Lastwerte[0] + "\t" + item.Lastwerte[1] + "\t" + item.Lastwerte[2]));
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
        foreach (var item in wärmeModell.Randbedingungen)
        {
            sb.Clear();
            sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + item.Value.Vordefiniert[0]);
            zeilen.Add(sb.ToString());
        }

        // Eigenlösungen
        if (wärmeModell.Eigenzustand != null)
        {
            zeilen.Add("\n" + "Eigenlösungen");
            zeilen.Add(wärmeModell.Eigenzustand.Id + "\t" + wärmeModell.Eigenzustand.AnzahlZustände);
        }

        // Parameter
        if (wärmeModell.Zeitintegration != null)
        {
            zeilen.Add("\n" + "Zeitintegration");
            zeilen.Add(wärmeModell.Zeitintegration.Id + "\t" + wärmeModell.Zeitintegration.Tmax + "\t" + wärmeModell.Zeitintegration.Dt
                       + "\t" + wärmeModell.Zeitintegration.Parameter1);
        }

        // zeitabhängige Anfangsbedingungen
        if (wärmeModell.Zeitintegration.VonStationär || wärmeModell.Zeitintegration.Anfangsbedingungen.Count != 0)
            zeilen.Add("\n" + "Anfangstemperaturen");
        if (wärmeModell.Zeitintegration.VonStationär)
        {
            zeilen.Add("stationäre Loesung");
        }

        foreach (var item in wärmeModell.Zeitintegration.Anfangsbedingungen)
        {
            var knotenwerte = (Knotenwerte)item;
            zeilen.Add(knotenwerte.KnotenId + "\t" + knotenwerte.Werte[0]);
        }

        // zeitabhängige Randbedingungen
        if (wärmeModell.ZeitabhängigeRandbedingung.Count != 0) zeilen.Add("\n" + "Zeitabhängige Randtemperaturen");
        foreach (var item in wärmeModell.ZeitabhängigeRandbedingung)
        {
            sb.Clear();
            sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId);
            if (item.Value.VariationsTyp == 0)
            {
                sb.Append("\tdatei");
            }
            if (item.Value.VariationsTyp == 1)
            {
                sb.Append("\tkonstant" + item.Value.KonstanteTemperatur);
            }
            else if (item.Value.VariationsTyp == 2)
            {
                sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" + item.Value.PhasenWinkel);
            }
            else if (item.Value.VariationsTyp == 3)
            {
                sb.Append("\tlinear");
                var anzahlIntervalle = item.Value.Intervall.Length;
                for (var i = 0; i < anzahlIntervalle; i += 2)
                {
                    sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);
                }
            }
            zeilen.Add(sb.ToString());
        }

        // zeitabhängige Knotentemperaturen
        if (wärmeModell.ZeitabhängigeKnotenLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Knotenlast");
        foreach (var item in wärmeModell.ZeitabhängigeKnotenLasten)
        {
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId);
            if (item.Value.VariationsTyp == 0)
            {
                sb.Append("\tdatei");
            }
            else if (item.Value.VariationsTyp == 2)
            {
                sb.Append("\tharmonisch\t" + item.Value.Amplitude + "\t" + item.Value.Frequenz + "\t" + item.Value.PhasenWinkel);
            }
            else if (item.Value.VariationsTyp == 3)
            {
                sb.Append("\tlinear");
                var anzahlIntervalle = item.Value.Intervall.Length;
                for (var i = 0; i < anzahlIntervalle; i += 2)
                {
                    sb.Append("\t" + item.Value.Intervall[i] + ";" + item.Value.Intervall[i + 1]);
                }
            }
            zeilen.Add(sb.ToString());
        }

        // zeitabhängige Elementtemperaturen
        if (wärmeModell.ZeitabhängigeElementLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Elementtemperaturen");
        foreach (var item in wärmeModell.ZeitabhängigeElementLasten)
        {
            sb.Clear();
            sb.Append(item.Key + "\t" + item.Value.ElementId);

            if (item.Value.VariationsTyp == 1)
            {
                sb.Append("\tkonstant");
                for (var i = 0; i < item.Value.P.Length; i++)
                {
                    sb.Append("\t" + item.Value.P[i]);
                }
            }

            zeilen.Add(sb.ToString());
        }

        // Dateiende
        zeilen.Add("\nend");

        // alle Zeilen in Datei schreiben
        var dateiName = "\\" + name + ".inp";
        dateiPfad = dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(dateiPfad, zeilen);
    }
    private void WärmedatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            var wärme = new Wärmeberechnung.ModelldatenAnzeigen.WärmedatenAnzeigen(wärmeModell);
            wärme.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }
    private void WärmedatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            wärmeVisual = new Wärmeberechnung.ModelldatenAnzeigen.WärmemodellVisualisieren(wärmeModell);
            wärmeVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }
    private void WärmedatenBerechnen(object sender, EventArgs e)
    {
        if (wärmeModell != null)
        {
            modellBerechnung = new Berechnung(wärmeModell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            berechnet = true;
            _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("WärmeModelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
        }
    }
    private void WärmeberechnungErgebnisseAnzeigen(object sender, EventArgs e)
    {
        if (wärmeModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(wärmeModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            var ergebnisse = new Wärmeberechnung.Ergebnisse.StationäreErgebnisseAnzeigen(wärmeModell);
            ergebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void WärmeberechnungErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(wärmeModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            stationäreErgebnisse = new Wärmeberechnung.Ergebnisse.StationäreErgebnisseVisualisieren(wärmeModell);
            stationäreErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void InstationäreDaten(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            var wärme = new Wärmeberechnung.ModelldatenAnzeigen.InstationäreDatenAnzeigen(wärmeModell);
            wärme.Show();
            zeitintegrationBerechnet = false;
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void WärmeAnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            modellBerechnung ??= new Berechnung(wärmeModell);
            var anregung = new Wärmeberechnung.ModelldatenAnzeigen.AnregungVisualisieren(wärmeModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeBerechnen(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            modellBerechnung = new Berechnung(wärmeModell);
            if (!berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                berechnet = true;
            }
            // default = 2 Eigenstates, falls nicht anders spezifiziert
            wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (wärmeModell.Eigenzustand.Eigenwerte != null) return;
            modellBerechnung.Eigenzustände();
            eigenBerechnet = true;
            _ = MessageBox.Show("Eigenlösung erfolgreich ermittelt", "Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeAnzeigen(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            modellBerechnung ??= new Berechnung(wärmeModell);
            if (!berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (wärmeModell.Eigenzustand.Eigenwerte == null) modellBerechnung.Eigenzustände();
            var eigen = new Wärmeberechnung.Ergebnisse.EigenlösungAnzeigen(wärmeModell); //Eigenlösung.Eigenlösung(modell));
            eigen.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void EigenlösungWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (wärmeModell != null)
        {
            modellBerechnung ??= new Berechnung(wärmeModell);
            if (!zeitintegrationBerechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                // default = 2 Eigenzustände, falls nicht anders spezifiziert
                wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            }
            // default = 2 Eigenzustände, falls nicht anders spezifiziert
            wärmeModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (wärmeModell.Eigenzustand.Eigenwerte == null) modellBerechnung.Eigenzustände();
            var visual = new Wärmeberechnung.Ergebnisse.EigenlösungVisualisieren(wärmeModell);
            visual.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
        }
    }
    private void InstationäreBerechnung(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationDaten && wärmeModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(wärmeModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            modellBerechnung.ZeitintegrationErsterOrdnung();
            zeitintegrationBerechnet = true;
            _ = MessageBox.Show("Zeitintegration erfolgreich durchgeführt", "instationäre Wärmeberechnung");
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Wärmeberechnung");
            const double tmax = 0;
            const double dt = 0;
            const double alfa = 0;
            if (wärmeModell != null)
            {
                wärmeModell.Zeitintegration = new Wärmeberechnung.Modelldaten.Zeitintegration(tmax, dt, alfa) { VonStationär = false };
                zeitintegrationDaten = true;
                var wärme = new Wärmeberechnung.ModelldatenAnzeigen.InstationäreDatenAnzeigen(wärmeModell);
                wärme.Show();
            }
            zeitintegrationBerechnet = false;
        }
    }
    private void InstationäreErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && wärmeModell != null)
        {
            var ergebnisse = new Wärmeberechnung.Ergebnisse.InstationäreErgebnisseAnzeigen(wärmeModell);
            ergebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }
    private void InstationäreModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && wärmeModell != null)
        {
            var modellzuständeVisualisieren = new Wärmeberechnung.Ergebnisse.InstationäreModellzuständeVisualisieren(wärmeModell);
            modellzuständeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }
    private void KnotenzeitverläufeWärmeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && wärmeModell != null)
        {
            var knotenzeitverläufeVisualisieren =
                new Wärmeberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(wärmeModell);
            knotenzeitverläufeVisualisieren.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
        }
    }

    //********************************************************************
    // Tragwerksberechnung
    private void TragwerksdatenEinlesen(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            //InitialDirectory = Directory.GetCurrentDirectory()
        };

        if (Directory.Exists(dateiDialog.InitialDirectory + "\\FE-Berechnungen-App\\input"))
        {
            dateiDialog.InitialDirectory += "\\FE-Berechnungen-App\\input\\Tragwerksberechnung";
            dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für Eingabedatei " + dateiDialog.InitialDirectory +
                                " \\FE-Berechnungen-App\\input\\Tragwerksberechnung nicht gefunden", "Tragwerksberechnung");
            dateiDialog.ShowDialog();
        }
        dateiPfad = dateiDialog.FileName;

        try
        {
            if (dateiPfad.Length == 0)
            {
                _ = MessageBox.Show("Eingabedatei ist leer", "Tragwerksberechnung");
                return;
            }
            dateiZeilen = File.ReadAllLines(dateiPfad, Encoding.UTF8);
        }
        catch (ParseAusnahme)
        {
            throw new ParseAusnahme("Abbruch: Fehler beim Lesen aus Eingabedatei ");
        }

        parse = new FeParser();
        parse.ParseModell(dateiZeilen);
        tragwerksModell = parse.FeModell;
        parse.ParseNodes(dateiZeilen);

        var tragwerksElemente = new Tragwerksberechnung.ModelldatenLesen.ElementParser();
        tragwerksElemente.ParseElements(dateiZeilen, tragwerksModell);

        var tragwerksMaterial = new Tragwerksberechnung.ModelldatenLesen.MaterialParser();
        tragwerksMaterial.ParseMaterials(dateiZeilen, tragwerksModell);

        var tragwerksLasten = new Tragwerksberechnung.ModelldatenLesen.LastParser();
        tragwerksLasten.ParseLasten(dateiZeilen, tragwerksModell);

        var tragwerksRandbedingungen = new Tragwerksberechnung.ModelldatenLesen.RandbedingungParser();
        tragwerksRandbedingungen.ParseRandbedingungen(dateiZeilen, tragwerksModell);

        var tragwerksTransient = new Tragwerksberechnung.ModelldatenLesen.TransientParser();
        tragwerksTransient.ParseZeitintegration(dateiZeilen, tragwerksModell);

        zeitintegrationDaten = tragwerksTransient.zeitintegrationDaten;
        berechnet = false;
        zeitintegrationBerechnet = false;

        sb.Append(FeParser.EingabeGefunden + "\n\nTragwerksdaten erfolgreich eingelesen");
        _ = MessageBox.Show(sb.ToString(), "Tragwerksberechnung");
        sb.Clear();

        tragwerkVisual = new Tragwerksberechnung.ModelldatenAnzeigen.TragwerkmodellVisualisieren(tragwerksModell);
        tragwerkVisual.Show();
    }
    private void TragwerksdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (dateiPfad == null)
        {
            var tragwerksdaten = new Dateieingabe.ModelldatenEditieren();
            tragwerksdaten.Show();
        }
        else
        {
            var tragwerksdaten = new Dateieingabe.ModelldatenEditieren(dateiPfad);
            tragwerksdaten.Show();
        }
    }
    private void TragwerksdatenSichern(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        var tragwerksdatei = new Dateieingabe.NeuerDateiname();
        tragwerksdatei.ShowDialog();

        var name = tragwerksdatei.dateiName;

        var zeilen = new List<string>
        {
            "ModellName",
            tragwerksModell.ModellId,
            "\nRaumdimension",
            tragwerksModell.Raumdimension + "\t" + tragwerksModell.AnzahlKnotenfreiheitsgrade,
            // Knoten
            "\nKnoten"
        };

        switch (tragwerksModell.Raumdimension)
        {
            case 1:
                zeilen.AddRange(tragwerksModell.Knoten.Select(knoten => knoten.Key
                                                               + "\t" + knoten.Value.Koordinaten[0]));
                break;
            case 2:
                zeilen.AddRange(tragwerksModell.Knoten.Select(knoten => knoten.Key
                                                               + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
                break;
            case 3:
                zeilen.AddRange(tragwerksModell.Knoten.Select(knoten => knoten.Key
                                                               + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1] + "\t" + knoten.Value.Koordinaten[2]));
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
        var alleQuerschnitte = new List<Querschnitt>();
        foreach (var item in tragwerksModell.Elemente)
        {
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
        }

        foreach (var item in tragwerksModell.Querschnitt)
        {
            alleQuerschnitte.Add(item.Value);
        }

        if (alleFachwerkelemente.Count != 0)
        {
            zeilen.Add("\nFachwerk");
            zeilen.AddRange(alleFachwerkelemente.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                                + item.KnotenIds[1] + "\t" + item.ElementQuerschnittId + "\t" + item.ElementMaterialId));
        }
        if (alleBiegebalken.Count != 0)
        {
            zeilen.Add("\nBiegebalken");
            zeilen.AddRange(alleBiegebalken.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.ElementQuerschnittId + "\t" + item.ElementMaterialId));
        }
        if (alleBiegebalkenGelenk.Count != 0)
        {
            zeilen.Add("\nBiegebalkenGelenk");
            zeilen.AddRange(alleBiegebalkenGelenk.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                                 + item.KnotenIds[1] + "\t" + item.ElementQuerschnittId + "\t" + item.ElementMaterialId + "\t" + item.Typ));
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
                                                                               + item.QuerschnittsWerte[0] + "\t" + item.QuerschnittsWerte[1]));
        }

        // Materialien
        zeilen.Add("\nMaterial");
        foreach (var item in tragwerksModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++)
            {
                sb.Append("\t" + item.Value.MaterialWerte[i]);
            }
            zeilen.Add(sb.ToString());
        }

        // Lasten
        foreach (var item in tragwerksModell.Lasten)
        {
            zeilen.Add("\nKnotenlast");
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++)
            {
                sb.Append("\t" + item.Value.Lastwerte[i]);
            }
            zeilen.Add(sb.ToString());
        }
        foreach (var item in tragwerksModell.PunktLasten)
        {
            var punktlast = (PunktLast)item.Value;
            sb.Clear();
            zeilen.Add("\nPunktlast");
            zeilen.Add(punktlast.LastId + "\t" + punktlast.ElementId
                       + "\t" + punktlast.Lastwerte[0] + "\t" + punktlast.Lastwerte[1] + "\t" + punktlast.Offset);
        }
        foreach (var item in tragwerksModell.ElementLasten)
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
        foreach (var item in tragwerksModell.Randbedingungen)
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
        dateiPfad = dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(dateiPfad, zeilen);
    }
    private void TragwerksdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            var tragwerk = new Tragwerksberechnung.ModelldatenAnzeigen.TragwerkdatenAnzeigen(tragwerksModell);
            tragwerk.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksmodelldaten müssen erst definiert werden", "statische Tragwerksanalyse");

        }
    }
    private void TragwerksdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            tragwerkVisual = new Tragwerksberechnung.ModelldatenAnzeigen.TragwerkmodellVisualisieren(tragwerksModell);
            tragwerkVisual.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksmodelldaten müssen erst definiert werden", "statische Tragwerksanalyse");

        }
    }
    private void TragwerksdatenBerechnen(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            modellBerechnung = new Berechnung(tragwerksModell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            berechnet = true;
            _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "statische Tragwerksberechnung");
        }
        else
        {
            _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
        }

    }
    private void StatikErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(tragwerksModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            var ergebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseAnzeigen(tragwerksModell);
            ergebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
        }
    }
    private void StatikErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(tragwerksModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            statikErgebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseVisualisieren(tragwerksModell);
            statikErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Tragwerksdaten müssen zuerst eingelesen werden", "statische Tragwerksberechnung");
        }
    }
    private void EigenlösungTragwerkBerechnen(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            modellBerechnung ??= new Berechnung(tragwerksModell);
            if (!berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                berechnet = true;
            }
            // default = 2 Eigenzustände, falls nicht anders spezifiziert
            tragwerksModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (tragwerksModell.Eigenzustand.Eigenwerte != null) return;
            modellBerechnung.Eigenzustände();
            eigenBerechnet = true;
            _ = MessageBox.Show("Eigenfrequenzen erfolgreich ermittelt", "Tragwerksberechnung");
        }
        else
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void EigenlösungTragwerkAnzeigen(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            modellBerechnung ??= new Berechnung(tragwerksModell);
            if (!berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                berechnet = true;
            }
            // default = 2 Eigenstates, falls nicht anders spezifiziert
            tragwerksModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (tragwerksModell.Eigenzustand.Eigenwerte == null) modellBerechnung.Eigenzustände();
            var eigen = new Tragwerksberechnung.Ergebnisse.EigenlösungAnzeigen(tragwerksModell);
            eigen.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void EigenlösungTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        if (tragwerksModell != null)
        {
            modellBerechnung ??= new Berechnung(tragwerksModell);
            if (!berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                berechnet = true;
            }

            // default = 2 Eigenstates, falls nicht anders spezifiziert
            tragwerksModell.Eigenzustand ??= new Eigenzustände("default", 2);
            if (tragwerksModell.Eigenzustand.Eigenwerte != null) modellBerechnung.Eigenzustände();
            var visual = new Tragwerksberechnung.Ergebnisse.EigenlösungVisualisieren(tragwerksModell);
            visual.Show();
        }
        else
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void DynamischeDaten(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationDaten && tragwerksModell != null)
        {
            var tragwerk = new Tragwerksberechnung.ModelldatenAnzeigen.DynamikDatenAnzeigen(tragwerksModell);
            tragwerk.Show();
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void AnregungVisualisieren(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationDaten && tragwerksModell != null)
        {
            modellBerechnung ??= new Berechnung(tragwerksModell);
            var anregung = new Tragwerksberechnung.ModelldatenAnzeigen.AnregungVisualisieren(tragwerksModell);
            anregung.Show();
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void DynamischeBerechnung(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationDaten && tragwerksModell != null)
        {
            if (!berechnet)
            {
                modellBerechnung ??= new Berechnung(tragwerksModell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            modellBerechnung.ZeitintegrationZweiterOrdnung();
            zeitintegrationBerechnet = true;
        }
        else
        {
            _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
        }
    }
    private void DynamischeErgebnisseAnzeigen(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && tragwerksModell != null)
        {
            _ = new Tragwerksberechnung.Ergebnisse.DynamischeErgebnisseAnzeigen(tragwerksModell);
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }
    private void DynamischeModellzuständeVisualisieren(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && tragwerksModell != null)
        {
            var dynamikErgebnisse = new Tragwerksberechnung.Ergebnisse.DynamischeModellzuständeVisualisieren(tragwerksModell);
            dynamikErgebnisse.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }
    private void KnotenzeitverläufeTragwerkVisualisieren(object sender, RoutedEventArgs e)
    {
        if (zeitintegrationBerechnet && tragwerksModell != null)
        {
            var knotenzeitverläufe = new Tragwerksberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(tragwerksModell);
            knotenzeitverläufe.Show();
        }
        else
        {
            _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
        }
    }

    //********************************************************************
    // Elastizitätsberechnung
    private void ElastizitätsdatenEinlesen(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        dateiDialog = new OpenFileDialog
        {
            Filter = "inp files (*.inp)|*.inp|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
        };

        if (Directory.Exists(dateiDialog.InitialDirectory + "\\FE-Berechnungen-App\\input"))
        {
            dateiDialog.InitialDirectory += "\\FE-Berechnungen-App\\input\\Elastizitätsberechnung";
            dateiDialog.ShowDialog();
        }
        else
        {
            _ = MessageBox.Show("Directory für Eingabedatei " + dateiDialog.InitialDirectory +
                                " \\FE-Berechnungen-App\\input\\Elastizitätsberechnung nicht gefunden", "Elastizitätsberechnung");
            dateiDialog.ShowDialog();
        }
        dateiPfad = dateiDialog.FileName;

        try
        {
            if (dateiPfad.Length == 0)
            {
                _ = MessageBox.Show("Eingabedatei ist leer", "Elastizitätsberechnung");
                return;
            }
            dateiZeilen = File.ReadAllLines(dateiPfad, Encoding.UTF8);
        }
        catch (ParseAusnahme)
        {
            throw new ParseAusnahme("Abbruch: Fehler beim Lesen aus Eingabedatei ");
        }

        parse = new FeParser();
        parse.ParseModell(dateiZeilen);
        elastizitätsModell = parse.FeModell;
        parse.ParseNodes(dateiZeilen);

        var parseElastizität = new Elastizitätsberechnung.ModelldatenLesen.ElastizitätsParser();
        parseElastizität.ParseElastizität(dateiZeilen, elastizitätsModell);

        berechnet = false;

        sb.Clear();
        sb.Append(FeParser.EingabeGefunden + "\n\nModelldaten für Elastizitätsberechnung erfolgreich eingelesen");
        _ = MessageBox.Show(sb.ToString(), "Elastizitätsberechnung");
        sb.Clear();
    }
    private void ElastizitätsdatenEditieren(object sender, RoutedEventArgs e)
    {
        if (dateiPfad == null)
        {
            var elastizitätsdaten = new Dateieingabe.ModelldatenEditieren();
            elastizitätsdaten.Show();
        }
        else
        {
            var elastizitätsdaten = new Dateieingabe.ModelldatenEditieren(dateiPfad);
            elastizitätsdaten.Show();
        }
    }
    private void ElastizitätsdatenAnzeigen(object sender, RoutedEventArgs e)
    {
        if (elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
            return;
        }
        var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.ElastizitätsdatenAnzeigen(elastizitätsModell);
        tragwerk.Show();
    }
    private void ElastizitätsdatenSichern(object sender, RoutedEventArgs e)
    {
        var elastizitätsdatei = new Dateieingabe.NeuerDateiname();
        elastizitätsdatei.ShowDialog();

        var name = elastizitätsdatei.dateiName;

        var zeilen = new List<string>
        {
            "ModellName",
            elastizitätsModell.ModellId,
            "\nRaumdimension"
        };
        int knotenfreiheitsgrade = 3;
        zeilen.Add(elastizitätsModell.Raumdimension + "\t" + knotenfreiheitsgrade);

        // Knoten
        zeilen.Add("\nKnoten");
        if (elastizitätsModell.Raumdimension == 2)
        {
            zeilen.AddRange(elastizitätsModell.Knoten.Select(knoten => knoten.Key
                                                                       + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
        }
        else
        {
            zeilen.AddRange(elastizitätsModell.Knoten.Select(knoten => knoten.Key
                                                                       + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1] + "\t" + knoten.Value.Koordinaten[2]));
        }

        // Elemente
        var alleElemente2D3 = new List<Elastizitätsberechnung.Modelldaten.Element2D3>();
        var alleElemente3D8 = new List<Elastizitätsberechnung.Modelldaten.Element3D8>();
        var alleQuerschnitte = new List<Querschnitt>();
        foreach (var item in elastizitätsModell.Elemente)
        {
            switch (item.Value)
            {
                case Elastizitätsberechnung.Modelldaten.Element2D3 element2D3:
                    alleElemente2D3.Add(element2D3);
                    break;
                case Elastizitätsberechnung.Modelldaten.Element3D8 element3D8:
                    alleElemente3D8.Add(element3D8);
                    break;
            }
        }
        foreach (var item in elastizitätsModell.Querschnitt)
        {
            alleQuerschnitte.Add(item.Value);
        }

        if (alleElemente2D3.Count != 0)
        {
            zeilen.Add("\nElement2D3");
            zeilen.AddRange(alleElemente2D3.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t"
                                                           + item.ElementQuerschnittId + "\t" + item.ElementMaterialId));
        }
        if (alleElemente3D8.Count != 0)
        {
            zeilen.Add("\nElement3D8");
            zeilen.AddRange(alleElemente3D8.Select(item => item.ElementId + "\t" + item.KnotenIds[0] + "\t"
                                                           + item.KnotenIds[1] + "\t" + item.KnotenIds[2] + "\t" + item.KnotenIds[3] + "\t"
                                                           + item.KnotenIds[4] + "\t" + item.KnotenIds[5] + "\t" + item.KnotenIds[6] + "\t"
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
        foreach (var item in elastizitätsModell.Material)
        {
            sb.Clear();
            sb.Append(item.Value.MaterialId + "\t" + item.Value.MaterialWerte[0]);
            for (var i = 1; i < item.Value.MaterialWerte.Length; i++)
            {
                sb.Append("\t" + item.Value.MaterialWerte[i]);
            }
            zeilen.Add(sb.ToString());
        }

        // Lasten
        if (elastizitätsModell.Lasten.Count > 0) zeilen.Add("\nKnotenlasten");
        foreach (var item in elastizitätsModell.Lasten)
        {
            sb.Clear();
            sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
            for (var i = 1; i < item.Value.Lastwerte.Length; i++)
            {
                sb.Append("\t" + item.Value.Lastwerte[i]);
            }
            zeilen.Add(sb.ToString());
        }

        if (elastizitätsModell.LinienLasten.Count > 0) zeilen.Add("\nLinienlasten");
        foreach (var item in elastizitätsModell.LinienLasten)
        {
            zeilen.Add(item.Value.LastId + "\t" + item.Value.StartKnotenId
                       + "\t" + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]
                       + "\t" + item.Value.EndKnotenId
                       + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]);
        }

        // Randbedingungen
        var fest = string.Empty;
        zeilen.Add("\nRandbedingungen");
        foreach (var item in elastizitätsModell.Randbedingungen)
        {
            sb.Clear();
            if (elastizitätsModell.Raumdimension == 2)
            {
                if (item.Value.Typ == 1) fest = "x";
                else if (item.Value.Typ == 2) fest = "y";
                else if (item.Value.Typ == 3) fest = "xy";
                else if (item.Value.Typ == 7) fest = "xyr";
            }
            else if (elastizitätsModell.Raumdimension == 3)
            {
                if (item.Value.Typ == 1) fest = "x";
                else if (item.Value.Typ == 2) fest = "y";
                else if (item.Value.Typ == 3) fest = "xy";
                else if (item.Value.Typ == 4) fest = "z";
                else if (item.Value.Typ == 5) fest = "xz";
                else if (item.Value.Typ == 6) fest = "yz";
                else if (item.Value.Typ == 7) fest = "xyz";
            }

            sb.Append(item.Key + "\t" + item.Value.KnotenId + "\t" + fest);
            foreach (var wert in item.Value.Vordefiniert) { sb.Append("\t" + wert); }
            zeilen.Add(sb.ToString());
        }

        // Dateiende
        zeilen.Add("\nend");

        // alle Zeilen in Datei schreiben
        var dateiName = "\\" + name + ".inp";
        dateiPfad = dateiDialog.InitialDirectory + dateiName;
        File.WriteAllLines(dateiPfad, zeilen);
    }
    private void ElastizitätsdatenVisualisieren(object sender, RoutedEventArgs e)
    {
        if (elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
            return;
        }
        switch (elastizitätsModell.Raumdimension)
        {
            case 2:
                {
                    var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.ElastizitätsmodellVisualisieren(elastizitätsModell);
                    tragwerk.Show();
                    break;
                }
            case 3:
                {
                    var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.Elastizitätsmodell3DVisualisieren(elastizitätsModell);
                    tragwerk.Show();
                    break;
                }
        }
    }

    private void ElastizitätsdatenBerechnen(object sender, RoutedEventArgs e)
    {
        if (elastizitätsModell == null)
        {
            _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
            return;
        }
        try
        {
            modellBerechnung = new Berechnung(elastizitätsModell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            berechnet = true;

            _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Elastizitätsberechnung");
        }

        catch (BerechnungAusnahme)
        {
            throw new BerechnungAusnahme("Abbruch: Fehler bei Lösung der Systemgleichungen");
        }
    }
    private void ElastizitätsberechnungErgebnisse(object sender, RoutedEventArgs e)
    {
        if (!berechnet)
        {
            if (elastizitätsModell == null)
            {
                _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }
            modellBerechnung = new Berechnung(elastizitätsModell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            berechnet = true;
        }
        var ergebnisse = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisseAnzeigen(elastizitätsModell);
        ergebnisse.Show();
    }
    private void ElastizitätsErgebnisseVisualisieren(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        if (!berechnet)
        {
            if (elastizitätsModell == null)
            {
                _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }
            modellBerechnung = new Berechnung(elastizitätsModell);
            modellBerechnung.BerechneSystemMatrix();
            modellBerechnung.BerechneSystemVektor();
            modellBerechnung.LöseGleichungen();
            berechnet = true;
        }

        if (elastizitätsModell.Raumdimension == 2)
        {
            var tragwerk = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisseVisualisieren(elastizitätsModell);
            tragwerk.Show();
        }
        else if (elastizitätsModell.Raumdimension == 3)
        {
            var tragwerk = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisse3DVisualisieren(elastizitätsModell);
            tragwerk.Show();
        }
        else _ = MessageBox.Show(sb.ToString(), "falsche Raumdimension, muss 2 oder 3 sein");
    }
}