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
using System.Windows.Controls;
using System.Windows.Markup;

namespace FE_Berechnungen
{
    public partial class StartFenster
    {
        private FeParser parse;
        private FeModell modell;
        private Berechnung modellBerechnung;
        private OpenFileDialog dateiDialog;
        private string dateiPfad;
        public static Tragwerksberechnung.ModelldatenAnzeigen.TragwerkmodellVisualisieren tragwerksModell;
        public static Tragwerksberechnung.Ergebnisse.StatikErgebnisseVisualisieren statikErgebnisse;
        public static Wärmeberechnung.ModelldatenAnzeigen.WärmemodellVisualisieren wärmeModell;
        public static Wärmeberechnung.Ergebnisse.StationäreErgebnisseVisualisieren stationäreErgebnisse;

        private string[] dateiZeilen;
        private bool wärmeDaten, tragwerksDaten, zeitintegrationDaten;
        public static bool berechnet, zeitintegrationBerechnet;

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
            modell = parse.FeModell;
            parse.ParseNodes(dateiZeilen);

            var wärmeElemente = new Wärmeberechnung.ModelldatenLesen.ElementParser();
            wärmeElemente.ParseWärmeElements(dateiZeilen, modell);

            var wärmeMaterial = new Wärmeberechnung.ModelldatenLesen.MaterialParser();
            wärmeMaterial.ParseMaterials(dateiZeilen, modell);

            var wärmeLasten = new Wärmeberechnung.ModelldatenLesen.LastParser();
            wärmeLasten.ParseLasten(dateiZeilen, modell);

            var wärmeRandbedingungen = new Wärmeberechnung.ModelldatenLesen.RandbedingungParser();
            wärmeRandbedingungen.ParseRandbedingungen(dateiZeilen, modell);

            var wärmeTransient = new Wärmeberechnung.ModelldatenLesen.TransientParser();
            wärmeTransient.ParseZeitintegration(dateiZeilen, modell);

            zeitintegrationDaten = wärmeTransient.zeitintegrationDaten;
            wärmeDaten = true;
            berechnet = false;
            zeitintegrationBerechnet = false;

            sb.Append(FeParser.EingabeGefunden + "\n\nWärmemodelldaten erfolgreich eingelesen");
            _ = MessageBox.Show(sb.ToString(), "Wärmeberechnung");
            sb.Clear();
        }
        private void WärmedatenEditieren(object sender, RoutedEventArgs e)
        {
            if (dateiPfad == null)
            {
                var wärmedaten = new Dateieingabe.ModelldatenEditieren();
                wärmedaten.Show();
            }
            else
            {
                var wärmedaten = new Dateieingabe.ModelldatenEditieren(dateiPfad);
                wärmedaten.Show();
            }
        }
        private void WärmedatenSichern(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var wärmedatei = new Dateieingabe.NeuerDateiname();
            wärmedatei.ShowDialog();

            var name = wärmedatei.dateiName;

            if (modell == null)
            {
                _ = MessageBox.Show("Modell ist noch nicht definiert", "Wärmeberechnung");
                return;
            }
            var zeilen = new List<string>
            {
                "ModellName",
                modell.ModellId,
                "\nRaumdimension"
            };
            var numberNodalDof = 1;
            zeilen.Add(modell.Raumdimension + "\t" + numberNodalDof + "\n");

            // Knoten
            zeilen.Add("Knoten");
            if (modell.Raumdimension == 2)
            {
                zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                                + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
            }
            else
            {
                zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                                + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1] + "\t" + knoten.Value.Koordinaten[2]));
            }

            // Elemente
            var alleElement2D2 = new List<Element2D2>();
            var alleElement2D3 = new List<Element2D3>();
            var alleElement2D4 = new List<Element2D4>();
            var alleElement3D8 = new List<Element3D8>();
            foreach (var item in modell.Elemente)
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
            foreach (var item in modell.Material)
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
            foreach (var item in modell.Lasten)
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
            foreach (var item in modell.LinienLasten)
            {
                sb.Clear();
                sb.Append("\n" + "LinienLasten");
                sb.Append(item.Value.LastId + "\t" + item.Value.StartKnotenId + "\t" + item.Value.EndKnotenId + "\t"
                          + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]);
                zeilen.Add(sb.ToString());
            }

            var alleElementlasten3 = new List<ElementLast3>();
            var alleElementlasten4 = new List<ElementLast4>();
            foreach (var item in modell.ElementLasten)
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
            foreach (var item in modell.Randbedingungen)
            {
                sb.Clear();
                sb.Append(item.Value.RandbedingungId + "\t" + item.Value.KnotenId + "\t" + item.Value.Vordefiniert[0]);
                zeilen.Add(sb.ToString());
            }

            // Eigenlösungen
            if (modell.Eigenzustand != null)
            {
                zeilen.Add("\n" + "Eigenlösungen");
                zeilen.Add(modell.Eigenzustand.Id + "\t" + modell.Eigenzustand.AnzahlZustände);
            }

            // Parameter
            if (modell.Zeitintegration != null)
            {
                zeilen.Add("\n" + "Zeitintegration");
                zeilen.Add(modell.Zeitintegration.Id + "\t" + modell.Zeitintegration.Tmax + "\t" + modell.Zeitintegration.Dt
                                + "\t" + modell.Zeitintegration.Parameter1);
            }

            // zeitabhängige Anfangsbedingungen
            if (modell.Zeitintegration.VonStationär || modell.Zeitintegration.Anfangsbedingungen.Count != 0)
                zeilen.Add("\n" + "Anfangstemperaturen");
            if (modell.Zeitintegration.VonStationär)
            {
                zeilen.Add("stationäre Loesung");
            }

            foreach (var item in modell.Zeitintegration.Anfangsbedingungen)
            {
                var knotenwerte = (Knotenwerte)item;
                zeilen.Add(knotenwerte.KnotenId + "\t" + knotenwerte.Werte[0]);
            }

            // zeitabhängige Randbedingungen
            if (modell.ZeitabhängigeRandbedingung.Count != 0) zeilen.Add("\n" + "Zeitabhängige Randtemperaturen");
            foreach (var item in modell.ZeitabhängigeRandbedingung)
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
            if (modell.ZeitabhängigeKnotenLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Knotenlast");
            foreach (var item in modell.ZeitabhängigeKnotenLasten)
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
            if (modell.ZeitabhängigeElementLasten.Count != 0) zeilen.Add("\n" + "Zeitabhängige Elementtemperaturen");
            foreach (var item in modell.ZeitabhängigeElementLasten)
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
            var wärme = new Wärmeberechnung.ModelldatenAnzeigen.WärmedatenAnzeigen(modell);
            wärme.Show();
        }
        private void WärmedatenVisualisieren(object sender, RoutedEventArgs e)
        {
            wärmeModell = new Wärmeberechnung.ModelldatenAnzeigen.WärmemodellVisualisieren(modell);
            wärmeModell.Show();
        }
        private void WärmedatenBerechnen(object sender, EventArgs e)
        {
            if (wärmeDaten)
            {
                modellBerechnung = new Berechnung(modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
                _ = MessageBox.Show("Systemgleichungen erfolgreich gelöst", "Wärmeberechnung");
            }
            else
            {
                _ = MessageBox.Show("Modelldaten müssen zuerst eingelesen werden", "Wärmeberechnung");
            }
        }
        private void WärmeberechnungErgebnisseAnzeigen(object sender, EventArgs e)
        {
            if (wärmeDaten)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    modellBerechnung.BerechneSystemVektor();
                    modellBerechnung.LöseGleichungen();
                    berechnet = true;
                }
                var ergebnisse = new Wärmeberechnung.Ergebnisse.StationäreErgebnisseAnzeigen(modell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        private void WärmeberechnungErgebnisseVisualisieren(object sender, RoutedEventArgs e)
        {
            if (wärmeDaten)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    modellBerechnung.BerechneSystemVektor();
                    modellBerechnung.LöseGleichungen();
                    berechnet = true;
                }
                stationäreErgebnisse = new Wärmeberechnung.Ergebnisse.StationäreErgebnisseVisualisieren(modell);
                stationäreErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        private void InstationäreDaten(object sender, RoutedEventArgs e)
        {
            if (modell == null)
            {
                _ = MessageBox.Show("Modelldaten für Wärmeberechnung sind noch nicht spezifiziert", "Wärmeberechnung");
            }
            else
            {
                var wärme = new Wärmeberechnung.ModelldatenAnzeigen.InstationäreDatenAnzeigen(modell);
                wärme.Show();
                zeitintegrationBerechnet = false;
            }
        }
        private void EigenlösungWärmeBerechnen(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    berechnet = true;
                }
                // default = 2 Eigenstates, falls nicht anders spezifiziert
                if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                modellBerechnung.Eigenzustände();
                _ = MessageBox.Show("Eigenlösung erfolgreich ermittelt", "Wärmeberechnung");
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        private void EigenlösungWärmeAnzeigen(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                }

                // default = 2 Eigenstates, falls nicht anders spezifiziert
                if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                modellBerechnung.Eigenzustände();

                var eigen = new Wärmeberechnung.Ergebnisse.EigenlösungAnzeigen(modell); //Eigenlösung.Eigenlösung(modell));
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Wärmeberechnung");
            }
        }
        private void EigenlösungWärmeVisualisieren(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!zeitintegrationBerechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    // default = 2 Eigenstates, falls nicht anders spezifiziert
                    if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                }
                modellBerechnung.Eigenzustände();
                var visual = new Wärmeberechnung.Ergebnisse.EigenlösungVisualisieren(modell);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        private void InstationäreBerechnung(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationDaten)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
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
                double tmax = 0;
                double dt = 0;
                double alfa = 0;
                modell.Zeitintegration = new Wärmeberechnung.Modelldaten.Zeitintegration(tmax, dt, alfa) { VonStationär = false };
                zeitintegrationDaten = true;
                var wärme = new Wärmeberechnung.ModelldatenAnzeigen.InstationäreDatenAnzeigen(modell);
                wärme.Show();
                zeitintegrationBerechnet = false;
            }

        }
        private void InstationäreErgebnisseAnzeigen(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationBerechnet)
            {
                var ergebnisse = new Wärmeberechnung.Ergebnisse.InstationäreErgebnisseAnzeigen(modell);
                ergebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
            }
        }
        private void InstationäreModellzuständeVisualisieren(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationBerechnet)
            {
                var modellzuständeVisualisieren = new Wärmeberechnung.Ergebnisse.InstationäreModellzuständeVisualisieren(modell);
                modellzuständeVisualisieren.Show();
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
            }
        }
        private void KnotenzeitverläufeWärmeVisualisieren(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationBerechnet)
            {
                var knotenzeitverläufeVisualisieren = 
                    new Wärmeberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(modell);
                knotenzeitverläufeVisualisieren.Show();
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "Wärmeberechnung");
            }
        }

        //********************************************************************
        // Tragwerksberechnung
        private void TragwerksdatenEinlesen(object sender, EventArgs e)
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
            modell = parse.FeModell;
            parse.ParseNodes(dateiZeilen);

            var tragwerksElemente = new Tragwerksberechnung.ModelldatenLesen.ElementParser();
            tragwerksElemente.ParseElements(dateiZeilen, modell);

            var tragwerksMaterial = new Tragwerksberechnung.ModelldatenLesen.MaterialParser();
            tragwerksMaterial.ParseMaterials(dateiZeilen, modell);

            var tragwerksLasten = new Tragwerksberechnung.ModelldatenLesen.LastParser();
            tragwerksLasten.ParseLasten(dateiZeilen, modell);

            var tragwerksRandbedingungen = new Tragwerksberechnung.ModelldatenLesen.RandbedingungParser();
            tragwerksRandbedingungen.ParseRandbedingungen(dateiZeilen, modell);

            var tragwerksTransient = new Tragwerksberechnung.ModelldatenLesen.TransientParser();
            tragwerksTransient.ParseZeitintegration(dateiZeilen, modell);

            zeitintegrationDaten = tragwerksTransient.zeitintegrationDaten;
            tragwerksDaten = true;
            berechnet = false;
            zeitintegrationBerechnet = false;

            sb.Append(FeParser.EingabeGefunden + "\n\nTragwerksdaten erfolgreich eingelesen");
            _ = MessageBox.Show(sb.ToString(), "Tragwerksberechnung");
            sb.Clear();
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
                modell.ModellId,
                "\nRaumdimension",
                modell.Raumdimension + "\t" + modell.AnzahlKnotenfreiheitsgrade,
                // Knoten
                "\nKnoten"
            };

            switch (modell.Raumdimension)
            {
                case 1:
                    zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                                                                   + "\t" + knoten.Value.Koordinaten[0]));
                    break;
                case 2:
                    zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                                                                   + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
                    break;
                case 3:
                    zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
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
            foreach (var item in modell.Elemente)
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

            foreach (var item in modell.Querschnitt)
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
            foreach (var item in modell.Material)
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
            foreach (var item in modell.Lasten)
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
            foreach (var item in modell.PunktLasten)
            {
                var punktlast = (PunktLast)item.Value;
                sb.Clear();
                zeilen.Add("\nPunktlast");
                zeilen.Add(punktlast.LastId + "\t" + punktlast.ElementId
                           + "\t" + punktlast.Lastwerte[0] + "\t" + punktlast.Lastwerte[1] + "\t" + punktlast.Offset);
            }
            foreach (var item in modell.ElementLasten)
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
            foreach (var item in modell.Randbedingungen)
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
        private void TragwerksdatenAnzeigen(object sender, EventArgs e)
        {
            var tragwerk = new Tragwerksberechnung.ModelldatenAnzeigen.TragwerkdatenAnzeigen(modell);
            tragwerk.Show();
        }
        public void TragwerksdatenVisualisieren(object sender, RoutedEventArgs e)
        {
            tragwerksModell = new Tragwerksberechnung.ModelldatenAnzeigen.TragwerkmodellVisualisieren(modell);
            tragwerksModell.Show();
        }
        private void TragwerksdatenBerechnen(object sender, EventArgs e)
        {
            if (tragwerksDaten)
            {
                modellBerechnung = new Berechnung(modell);
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
        private void StatikErgebnisseAnzeigen(object sender, EventArgs e)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            var ergebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseAnzeigen(modell);
            ergebnisse.Show();
        }
        private void StatikErgebnisseVisualisieren(object sender, RoutedEventArgs e)
        {
            if (!berechnet)
            {
                modellBerechnung = new Berechnung(modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            statikErgebnisse = new Tragwerksberechnung.Ergebnisse.StatikErgebnisseVisualisieren(modell);
            statikErgebnisse.Show();
        }
        private void EigenlösungTragwerkBerechnen(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                }
                // default = 2 Eigenstates, falls nicht anders spezifiziert
                if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                modellBerechnung.Eigenzustände();
                _ = MessageBox.Show("Eigenfrequenzen erfolgreich ermittelt", "Tragwerksberechnung");
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        private void EigenlösungTragwerkAnzeigen(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    // default = 2 Eigenstates, falls nicht anders spezifiziert
                    if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                }
                modellBerechnung.Eigenzustände();
                var eigen = new Tragwerksberechnung.Ergebnisse.EigenlösungAnzeigen(modell);
                eigen.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        private void EigenlösungTragwerkVisualisieren(object sender, RoutedEventArgs e)
        {
            if (modell != null)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
                    modellBerechnung.BerechneSystemMatrix();
                    // default = 2 Eigenstates, falls nicht anders spezifiziert
                    if (modell.Eigenzustand == null) { modell.Eigenzustand = new Eigenzustände("default", 2); }
                }
                modellBerechnung.Eigenzustände();
                var visual = new Tragwerksberechnung.Ergebnisse.EigenlösungVisualisieren(modell);
                visual.Show();
            }
            else
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        private void DynamischeDaten(object sender, EventArgs e)
        {
            if (zeitintegrationDaten)
            {
                var tragwerk = new Tragwerksberechnung.ModelldatenAnzeigen.DynamikDatenAnzeigen(modell);
                tragwerk.Show();
            }
            else
            {
                _ = MessageBox.Show("Daten für Zeitintegration sind noch nicht spezifiziert", "Tragwerksberechnung");
            }
        }
        private void DynamischeBerechnung(object sender, EventArgs e)
        {
            if (zeitintegrationDaten)
            {
                if (!berechnet)
                {
                    modellBerechnung = new Berechnung(modell);
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
            if (zeitintegrationBerechnet)
            {
                _ = new Tragwerksberechnung.Ergebnisse.DynamischeErgebnisseAnzeigen(modell);
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
            }
        }
        private void DynamischeModellzuständeVisualisieren(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationBerechnet)
            {
                var dynamikErgebnisse = new Tragwerksberechnung.Ergebnisse.DynamischeModellzuständeVisualisieren(modell);
                dynamikErgebnisse.Show();
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
            }
        }
        private void KnotenzeitverläufeTragwerkVisualisieren(object sender, RoutedEventArgs e)
        {
            if (zeitintegrationBerechnet)
            {
                var knotenzeitverläufe = new Tragwerksberechnung.Ergebnisse.KnotenzeitverläufeVisualisieren(modell);
                knotenzeitverläufe.Show();
            }
            else
            {
                _ = MessageBox.Show("Zeitintegration noch nicht ausgeführt!!", "dynamische Tragwerksberechnung");
            }
        }

        //********************************************************************
        // Elastizitätsberechnung
        private void ElastizitätsdatenEinlesen(object sender, EventArgs e)
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
            modell = parse.FeModell;
            parse.ParseNodes(dateiZeilen);

            var parseElastizität = new Elastizitätsberechnung.ModelldatenLesen.ElastizitätsParser();
            parseElastizität.ParseElastizität(dateiZeilen, modell);

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
        private void ElastizitätsdatenAnzeigen(object sender, EventArgs e)
        {
            if (modell == null)
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }
            var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.ElastizitätsdatenAnzeigen(modell);
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
                modell.ModellId,
                "\nRaumdimension"
            };
            int knotenfreiheitsgrade = 3;
            zeilen.Add(modell.Raumdimension + "\t" + knotenfreiheitsgrade);

            // Knoten
            zeilen.Add("\nKnoten");
            if (modell.Raumdimension == 2)
            {
                zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                                   + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1]));
            }
            else
            {
                zeilen.AddRange(modell.Knoten.Select(knoten => knoten.Key
                + "\t" + knoten.Value.Koordinaten[0] + "\t" + knoten.Value.Koordinaten[1] + "\t" + knoten.Value.Koordinaten[2]));
            }

            // Elemente
            var alleElemente2D3 = new List<Elastizitätsberechnung.Modelldaten.Element2D3>();
            var alleElemente3D8 = new List<Elastizitätsberechnung.Modelldaten.Element3D8>();
            var alleQuerschnitte = new List<Querschnitt>();
            foreach (var item in modell.Elemente)
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
            foreach (var item in modell.Querschnitt)
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
            foreach (var item in modell.Material)
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
            if (modell.Lasten.Count > 0) zeilen.Add("\nKnotenlasten");
            foreach (var item in modell.Lasten)
            {
                sb.Clear();
                sb.Append(item.Value.LastId + "\t" + item.Value.KnotenId + "\t" + item.Value.Lastwerte[0]);
                for (var i = 1; i < item.Value.Lastwerte.Length; i++)
                {
                    sb.Append("\t" + item.Value.Lastwerte[i]);
                }
                zeilen.Add(sb.ToString());
            }

            if (modell.LinienLasten.Count > 0) zeilen.Add("\nLinienlasten");
            foreach (var item in modell.LinienLasten)
            {
                zeilen.Add(item.Value.LastId + "\t" + item.Value.StartKnotenId
                           + "\t" + item.Value.Lastwerte[0] + "\t" + item.Value.Lastwerte[1]
                           + "\t" + item.Value.EndKnotenId
                           + "\t" + item.Value.Lastwerte[2] + "\t" + item.Value.Lastwerte[3]);
            }

            // Randbedingungen
            var fest = string.Empty;
            zeilen.Add("\nRandbedingungen");
            foreach (var item in modell.Randbedingungen)
            {
                sb.Clear();
                if (modell.Raumdimension == 2)
                {
                    if (item.Value.Typ == 1) fest = "x";
                    else if (item.Value.Typ == 2) fest = "y";
                    else if (item.Value.Typ == 3) fest = "xy";
                    else if (item.Value.Typ == 7) fest = "xyr";
                }
                else if (modell.Raumdimension == 3)
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
            if (modell == null)
            {
                _ = MessageBox.Show("Modelldaten sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }
            switch (modell.Raumdimension)
            {
                case 2:
                    {
                        var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.ElastizitätsmodellVisualisieren(modell);
                        tragwerk.Show();
                        break;
                    }
                case 3:
                    {
                        var tragwerk = new Elastizitätsberechnung.ModelldatenAnzeigen.Elastizitätsmodell3DVisualisieren(modell);
                        tragwerk.Show();
                        break;
                    }
            }
        }

        private void ElastizitätsdatenBerechnen(object sender, EventArgs e)
        {
            if (modell == null)
            {
                _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
                return;
            }
            try
            {
                modellBerechnung = new Berechnung(modell);
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
        private void ElastizitätsberechnungErgebnisse(object sender, EventArgs e)
        {
            if (!berechnet)
            {
                if (modell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
                    return;
                }
                modellBerechnung = new Berechnung(modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }
            var ergebnisse = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisseAnzeigen(modell);
            ergebnisse.Show();
        }
        private void ElastizitätsErgebnisseVisualisieren(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            if (!berechnet)
            {
                if (modell == null)
                {
                    _ = MessageBox.Show("Modelldaten für Elastizitätsberechnung sind noch nicht spezifiziert", "Elastizitätsberechnung");
                    return;
                }
                modellBerechnung = new Berechnung(modell);
                modellBerechnung.BerechneSystemMatrix();
                modellBerechnung.BerechneSystemVektor();
                modellBerechnung.LöseGleichungen();
                berechnet = true;
            }

            if (modell.Raumdimension == 2)
            {
                var tragwerk = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisseVisualisieren(modell);
                tragwerk.Show();
            }
            else if (modell.Raumdimension == 3)
            {
                var tragwerk = new Elastizitätsberechnung.Ergebnisse.StatikErgebnisse3DVisualisieren(modell);
                tragwerk.Show();
            }
            else _ = MessageBox.Show(sb.ToString(), "falsche Raumdimension, muss 2 oder 3 sein");
        }
    }
}