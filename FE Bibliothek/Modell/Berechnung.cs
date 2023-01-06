using FEBibliothek.Gleichungslöser;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Zeitlöser;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace FEBibliothek.Modell
{
    public class Berechnung
    {
        private FeModell modell;
        private Knoten knoten;
        private AbstraktElement element;
        private Gleichungen systemGleichungen;
        private ProfillöserStatus profilLöser;
        private int dimension;
        private bool zerlegt, setzDimension, profil, diagonalMatrix;

        public Berechnung(FeModell m)
        {
            modell = m;
            if (modell == null)
            {
                throw new BerechnungAusnahme("Modelleingabedaten noch nicht eingelesen");
            }
            // setz System Indizes
            var k = 0;
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                k = knoten.SetzSystemIndizes(k);
            }
            SetzReferenzen(m);
        }
        // Objekt Referenzen werden erst auf Basis der eindeutigen Identifikatoren ermittelt, d.h. unmittelbar vor Objekt Instantiierung
        // wenn eine Berechnung gestartet wird, müssen folglich ALLE Objektreferenzen auf Basis der eindeutigen Identifikatoren ermittelt werden
        private void SetzReferenzen(FeModell m)
        {
            modell = m;

            // Referenzen für Querschnittsverweise von 2D Elementen setzen
            foreach (var abstractElement in
                        from KeyValuePair<string, AbstraktElement> item in modell.Elemente
                        where item.Value != null
                        where item.Value is Abstrakt2D
                        let element = item.Value
                        select element)
            {
                var element2D = (Abstrakt2D)abstractElement;
                element2D.SetzQuerschnittReferenzen(modell);
            }
            // setzen aller notwendigen Elementreferenzen und der Systemindizes aller Elemente 
            foreach (var abstractElement in modell.Elemente.Select(item => item.Value))
            {
                abstractElement.SetzElementReferenzen(modell);
                abstractElement.SetzElementSystemIndizes();
            }

            foreach (var randbedingung in modell.Randbedingungen.Select(item => item.Value))
            {
                randbedingung.SetzRandbedingungenReferenzen(modell);
            }
            foreach (var elementLast in modell.ElementLasten.Select(item => item.Value))
            {
                elementLast.SetzElementlastReferenzen(modell);
            }
            foreach (var zeitabhängigeKnotenlast in modell.ZeitabhängigeKnotenLasten.Select(item => item.Value))
            {
                zeitabhängigeKnotenlast.SetzReferenzen(modell);
            }
            foreach (var zeitabhängigeElementLast in modell.ZeitabhängigeElementLasten.Select(item => item.Value))
            {
                zeitabhängigeElementLast.SetzElementlastReferenzen(modell);
            }
            foreach (var zeitabhängigeRandbedingung in modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                zeitabhängigeRandbedingung.SetzRandbedingungenReferenzen(modell);
            }
        }
        // bestimme Dimension der Systemmatrix *************************************************************************************
        private void BestimmeDimension()
        {
            dimension = 0;
            foreach (var item in modell.Knoten)
            {
                dimension += item.Value.AnzahlKnotenfreiheitsgrade;
            }
            systemGleichungen = new Gleichungen(dimension);
            setzDimension = true;
        }
        // berechne und löse die Matrix in Profilformat mit StatusVektor *****************************************************
        private void BestimmeProfil()
        {
            foreach (var item in modell.Elemente)
            {
                element = item.Value;
                systemGleichungen.SetzProfil(element.SystemIndizesElement);
            }
            systemGleichungen.AllokiereMatrix();
            profil = true;
        }
        public void BerechneSystemMatrix()
        {
            if (!setzDimension) BestimmeDimension();
            if (!profil) BestimmeProfil();
            // traversiere Elemente zur Bestimmung der Matrixkoeffizienten
            foreach (var item in modell.Elemente)
            {
                element = item.Value;
                var elementMatrix = element.BerechneElementMatrix();
                systemGleichungen.AddierMatrix(element.SystemIndizesElement, elementMatrix);
            }
            SetzStatusVektor();
        }
        private void SetzStatusVektor()
        {
            // für alle festen Randbedingungen
            foreach (var item in modell.Randbedingungen) StatusKnoten(item.Value);
        }
        private void StatusKnoten(AbstraktRandbedingung randbedingung)
        {
            var knotenId = randbedingung.KnotenId;

            if (modell.Knoten.TryGetValue(knotenId, out knoten))
            {
                systemGleichungen.SetzProfil(knoten.SystemIndizes);
                var vordefiniert = randbedingung.Vordefiniert;
                var festgehalten = randbedingung.Festgehalten;
                for (var i = 0; i < festgehalten.Length; i++)
                {
                    if (festgehalten[i])
                        systemGleichungen.SetzStatus(true, knoten.SystemIndizes[i], vordefiniert[i]);
                }
            }
            else
            {
                throw new BerechnungAusnahme("Endknoten " + knotenId + " ist nicht im Modell enthalten.");
            }
        }
        private void NeuberechnungSystemMatrix()
        {
            // traversiere die Element zur Bestimmung der Matrixkoeffizienten
            systemGleichungen.InitialisiereMatrix();
            foreach (var item in modell.Elemente)
            {
                element = item.Value;
                var indizes = element.SystemIndizesElement;
                var elementMatrix = element.BerechneElementMatrix();
                systemGleichungen.AddierMatrix(indizes, elementMatrix);
            }
        }
        public void BerechneSystemVektor()
        {
            int[] indizes;
            double[] lastVektor;

            // Knotenlasten
            foreach (var item in modell.Lasten)
            {
                var knotenLast = item.Value;
                var knotenId = item.Value.KnotenId;
                if (modell.Knoten.TryGetValue(knotenId, out var lastKnoten))
                {
                    indizes = lastKnoten.SystemIndizes;
                    lastVektor = knotenLast.BerechneLastVektor();
                    systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                {
                    throw new BerechnungAusnahme("Lastknoten " + knotenId + " ist nicht im Modell enthalten.");
                }
            }
            // Linienenlasten
            foreach (var item in modell.LinienLasten)
            {
                var linienLast = item.Value;
                var startKnotenId = item.Value.StartKnotenId;
                if (modell.Knoten.TryGetValue(startKnotenId, out knoten))
                {
                    linienLast.StartKnoten = knoten;
                }
                else
                {
                    throw new BerechnungAusnahme("Linienlastknoten " + startKnotenId + " ist nicht im Modell enthalten.");
                }
                var endKnotenId = item.Value.EndKnotenId;
                if (modell.Knoten.TryGetValue(endKnotenId, out knoten))
                {
                    linienLast.EndKnoten = knoten;
                }
                else
                {
                    throw new BerechnungAusnahme("Linienlastknoten " + endKnotenId + " ist nicht im Modell enthalten.");
                }
                var start = linienLast.StartKnoten.SystemIndizes.Length;
                var end = linienLast.EndKnoten.SystemIndizes.Length;
                indizes = new int[start + end];
                for (var i = 0; i < start; i++)
                    indizes[i] = linienLast.StartKnoten.SystemIndizes[i];
                for (var i = 0; i < end; i++)
                    indizes[start + i] = linienLast.EndKnoten.SystemIndizes[i];
                lastVektor = linienLast.BerechneLastVektor();
                systemGleichungen.AddVektor(indizes, lastVektor);
            }
            //Elementlasten
            foreach (var item in modell.ElementLasten)
            {
                var elementLast = item.Value;
                var elementId = item.Value.ElementId;
                if (modell.Elemente.TryGetValue(elementId, out element))
                {
                    indizes = element.SystemIndizesElement;
                    lastVektor = elementLast.BerechneLastVektor();
                    systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                {
                    throw new BerechnungAusnahme("Element " + elementId + " für Elementlast ist nicht im Modell enthalten.");
                }
            }
            foreach (var item in modell.PunktLasten)
            {
                var punktLast = item.Value;
                var elementId = item.Value.ElementId;
                if (modell.Elemente.TryGetValue(elementId, out element))
                {
                    punktLast.Element = element;
                    indizes = element.SystemIndizesElement;
                    lastVektor = punktLast.BerechneLastVektor();
                    systemGleichungen.AddVektor(indizes, lastVektor);
                }
                else
                {
                    throw new BerechnungAusnahme("Element " + elementId + " für Linienlasten ist nicht im Modell enthalten.");
                }
            }
        }
        public void LöseGleichungen()
        {
            if (!zerlegt)
            {
                profilLöser = new ProfillöserStatus(
                    systemGleichungen.Matrix, systemGleichungen.Vektor,
                    systemGleichungen.Primal, systemGleichungen.Dual,
                    systemGleichungen.Status, systemGleichungen.Profil);
                profilLöser.Dreieckszerlegung();
                zerlegt = true;
            }
            profilLöser.Lösung();
            // ... speichere System Unbekannte (primale Werte)
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                var index = knoten.SystemIndizes;
                knoten.Knotenfreiheitsgrade = new double[knoten.AnzahlKnotenfreiheitsgrade];
                for (var i = 0; i < knoten.Knotenfreiheitsgrade.Length; i++)
                    knoten.Knotenfreiheitsgrade[i] = systemGleichungen.Primal[index[i]];
            }
            // ... speichere duale Werte
            var reaktionen = systemGleichungen.Dual;
            foreach (var randbedingung in modell.Randbedingungen.Select(item => item.Value))
            {
                knoten = randbedingung.Knoten;
                var index = knoten.SystemIndizes;
                var reaktion = new double[knoten.AnzahlKnotenfreiheitsgrade];
                for (var i = 0; i < reaktion.Length; i++)
                    reaktion[i] = reaktionen[index[i]];
                knoten.Reaktionen = reaktion;
            }
        }

        // Eigenlösungen ***********************************************************************************************************
        public void Eigenzustände()
        {
            var anzahlZustände = modell.Eigenzustand.AnzahlZustände;
            var aMatrix = systemGleichungen.Matrix;
            if (!diagonalMatrix) BerechneDiagonalMatrix();
            var bDiag = systemGleichungen.DiagonalMatrix;

            // allgemeine B-Matrix wird erweitert auf die gleiche Struktur wie A
            var bMatrix = new double[dimension][];
            int zeile;
            for (zeile = 0; zeile < aMatrix.Length; zeile++)
            {
                bMatrix[zeile] = new double[aMatrix[zeile].Length];
                int spalte;
                for (spalte = 0; spalte < bMatrix[zeile].Length - 1; spalte++)
                    bMatrix[zeile][spalte] = 0;
                bMatrix[zeile][spalte] = bDiag[zeile];
            }

            if (!modell.ZeitIntegration)
            {
                SetzZeitabhängigenStatusVektor();
            }

            if (!zerlegt)
            {
                profilLöser = new ProfillöserStatus(
                    systemGleichungen.Matrix,
                    systemGleichungen.Status, systemGleichungen.Profil);
                profilLöser.Dreieckszerlegung();
                zerlegt = true;
            }

            var eigenLöser = new Eigenlöser(systemGleichungen.Matrix, bMatrix,
                systemGleichungen.Profil, systemGleichungen.Status,
                anzahlZustände);
            eigenLöser.LöseEigenzustände();

            // speichere Eigenwerte und -vektoren
            var eigenwerte = new double[anzahlZustände];
            var eigenvektoren = new double[anzahlZustände][];
            for (var i = 0; i < anzahlZustände; i++)
            {
                eigenwerte[i] = eigenLöser.HolEigenwert(i);
                eigenvektoren[i] = eigenLöser.HolEigenvektor(i);
            }
            modell.Eigenzustand.Eigenwerte = eigenwerte;
            modell.Eigenzustand.Eigenvektoren = eigenvektoren;
            modell.Eigen = true;
        }
        private void BerechneDiagonalMatrix()
        {
            // diagonale spezifische Wärme- bzw. Massenmatrix
            if (!setzDimension) BestimmeDimension();

            // traversier Elemente zur Ermittlung der Koeffizienten der Diagonalmatrix
            foreach (var item in modell.Elemente)
            {
                var abstraktesElement = item.Value;
                var index = abstraktesElement.SystemIndizesElement;
                var elementMatrix = abstraktesElement.BerechneDiagonalMatrix();
                systemGleichungen.AddDiagonalMatrix(index, elementMatrix);
            }

            // festgehaltene Freiheitsgrade liefern keine Beiträge zu Massenkräften
            foreach (var randbedingung in modell.Randbedingungen)
            {
                var systemIndizes = randbedingung.Value.Knoten.SystemIndizes;
                for (var i = 0; i < randbedingung.Value.Festgehalten.Length; i++)
                {
                    if (randbedingung.Value.Festgehalten[i]) systemGleichungen.DiagonalMatrix[systemIndizes[i]] = 0;
                }
            }
            diagonalMatrix = true;
        }

        // Zeitintegration 1er Ordnung ***********************************************************************************************
        public void ZeitintegrationErsterOrdnung()
        {
            // ... berechne spezifische Wärme Matrix ..............................
            if (!diagonalMatrix) BerechneDiagonalMatrix();
            _ = systemGleichungen.DiagonalMatrix;


            var dt = modell.Zeitintegration.Dt;
            if (dt == 0)
            {
                throw new BerechnungAusnahme("Abbruch: Zeitschrittintervall nicht definiert.");
            }
            var tmax = modell.Zeitintegration.Tmax;
            var alfa = modell.Zeitintegration.Parameter1;
            var nZeitschritte = (int)(tmax / dt) + 1;
            var anregungsFunktion = new double[nZeitschritte][];
            for (var k = 0; k < nZeitschritte; k++)
                anregungsFunktion[k] = new double[dimension];
            var temperatur = new double[nZeitschritte][];
            for (var i = 0; i < nZeitschritte; i++) temperatur[i] = new double[dimension];

            SetzAnfangsbedingungenErsterOrdnung(temperatur);
            SetzZeitabhängigenStatusVektor();

            // ... berechne zeitabhängige Anregungsfunktion und Randbedingungen
            BerechneAnregungsfunktionErsterOrdnung(dt, anregungsFunktion);
            BerechneRandbedingungenErsterOrdnung(dt, temperatur);

            // ... Systemmatrix muss neu berechnet werden, falls Dreieckszerlegung gespeichert
            if (zerlegt) { NeuberechnungSystemMatrix(); zerlegt = false; }

            var zeitintegration = new Zeitintegration1OrdnungStatus(
                systemGleichungen, anregungsFunktion, dt, alfa, temperatur);
            zeitintegration.Ausführung();

            // speichere Knotenzeitverläufe
            foreach (var item in modell.Knoten)
            {
                knoten = item.Value;
                var index = item.Value.SystemIndizes[0];
                knoten.KnotenVariable = new double[1][];
                knoten.KnotenVariable[0] = new double[nZeitschritte];
                knoten.KnotenAbleitungen = new double[1][];
                knoten.KnotenAbleitungen[0] = new double[nZeitschritte];

                // temperatur[nZeitschritte][index], KnotenVariable[index][nZeitschritte]
                for (var k = 0; k < nZeitschritte; k++)
                {
                    knoten.KnotenVariable[0][k] = temperatur[k][index];
                    knoten.KnotenAbleitungen[0][k] = zeitintegration.TemperaturGradient[k][index];
                }
            }
            modell.ZeitIntegration = true;
        }
        private void SetzAnfangsbedingungenErsterOrdnung(IList<double[]> temperatur)
        {
            // setz stationäre Lösung als Anfangsbedingungen
            if (modell.Zeitintegration.VonStationär) { temperatur[0] = systemGleichungen.Primal; }
            foreach (Knotenwerte anf in modell.Zeitintegration.Anfangsbedingungen)
            {
                if (anf.KnotenId == "alle")
                {
                    for (var i = 0; i < dimension; i++) temperatur[0][i] = anf.Werte[0];
                }
                else
                {
                    if (modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten))
                    {
                        temperatur[0][anfKnoten.SystemIndizes[0]] = anf.Werte[0];
                    }
                }
            }
        }
        private void SetzZeitabhängigenStatusVektor()
        {
            // für alle zeitabhängigen Randbedingungen
            if (modell == null) return;
            foreach (var randbedingung in
                modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                StatusKnoten(randbedingung);
            }
        }
        // zeitabhängige Knoten- und Elementlasten
        private void BerechneAnregungsfunktionErsterOrdnung(double dt, double[][] temperatur)
        {
            var last = new double[temperatur.Length];
            var nZeitschritte = last.Length;

            // finde zeitabhängige Knotenlasten
            foreach (var item in modell.ZeitabhängigeKnotenLasten)
            {
                if (modell.Knoten.TryGetValue(item.Value.KnotenId, out knoten))
                {
                    var lastIndex = knoten.SystemIndizes;

                    switch (item.Value.VariationsTyp)
                    {
                        case 0:
                            {
                                // Datei einlesen
                                const string inputDirectory = "\\FE-Berechnungen-App\\input\\Wärmeberechnung\\instationär\\Anregungsdateien";
                                const int spalte = -1;
                                AusDatei(inputDirectory, spalte, last);
                                break;
                            }
                        case 1:
                            {
                                // stückweise linear
                                var interval = item.Value.Intervall;
                                StückweiseLinear(dt, interval, last);
                                break;
                            }
                        case 2:
                            {
                                // periodisch
                                var amplitude = item.Value.Amplitude;
                                var frequenz = item.Value.Frequenz;
                                var phasenWinkel = item.Value.PhasenWinkel;
                                Periodisch(dt, amplitude, frequenz, phasenWinkel, last);
                                break;
                            }
                    }
                    for (var k = 0; k < nZeitschritte; k++)
                        temperatur[k][lastIndex[0]] = last[k];
                }
                else
                {
                    throw new BerechnungAusnahme("Knoten " + item.Value.KnotenId + " für zeitabhängige Knotenlast ist nicht im Modell enthalten.");
                }
            }

            // finde zeitabhängige Elementlasten
            foreach (var zeitabhängigeElementLast in modell.ZeitabhängigeElementLasten.Select(item => item.Value))
            {
                if (modell.Elemente.TryGetValue(zeitabhängigeElementLast.ElementId, out var abstraktElement))
                {
                    var index = abstraktElement.SystemIndizesElement;
                    var lastVektor = zeitabhängigeElementLast.BerechneLastVektor();
                    systemGleichungen.AddVektor(index, lastVektor);
                }
                for (var k = 0; k < nZeitschritte; k++)
                    temperatur[k] = systemGleichungen.Vektor;
            }
        }
        // zeitanhängige vordefinierte Randbedingungen
        private void BerechneRandbedingungenErsterOrdnung(double dt, double[][] temperatur)
        {
            var nZeitschritte = temperatur.Length;
            var vordefinierteTemperatur = new double[nZeitschritte];

            foreach (var item in modell.ZeitabhängigeRandbedingung)
            {
                if (modell.Knoten.TryGetValue(item.Value.KnotenId, out knoten))
                {
                    var lastIndex = knoten.SystemIndizes;

                    switch (item.Value.VariationsTyp)
                    {
                        case 0:
                            {
                                // Datei einlesen
                                const string inputDirectory = "\\FE-Berechnungen-App\\input\\Wärmeberechnung\\instationär\\Anregungsdateien";
                                const int spalte = 1;
                                AusDatei(inputDirectory, spalte, vordefinierteTemperatur);
                                break;
                            }
                        case 1:
                            {
                                // konstant
                                for (var k = 0; k < nZeitschritte; k++)
                                {
                                    vordefinierteTemperatur[k] = item.Value.KonstanteTemperatur;
                                }
                                break;
                            }
                        case 2:
                            {
                                // periodisch
                                var amplitude = item.Value.Amplitude;
                                var frequenz = item.Value.Frequenz;
                                var phasenWinkel = item.Value.PhasenWinkel;
                                Periodisch(dt, amplitude, frequenz, phasenWinkel, vordefinierteTemperatur);
                                break;
                            }
                        case 3:
                            {
                                // stückweise linear
                                var intervall = item.Value.Intervall;
                                StückweiseLinear(dt, intervall, vordefinierteTemperatur);
                                break;
                            }
                    }
                    StatusKnoten(item.Value);
                    for (var k = 0; k < nZeitschritte; k++)
                        temperatur[k][lastIndex[0]] = vordefinierteTemperatur[k];
                }
                else
                {
                    throw new BerechnungAusnahme("Knoten " + item.Value.KnotenId + " für zeitabhängige Randbedingung ist nicht im Modell enthalten.");
                }
            }
        }

        // 2nd order time integration ***********************************************************************************************
        public void ZeitintegrationZweiterOrdnung()
        {
            var dt = modell.Zeitintegration.Dt;
            if (dt == 0)
            {
                throw new BerechnungAusnahme("Zeitschrittintervall nicht definiert");
            }
            var tmax = modell.Zeitintegration.Tmax;
            var nZeitschritte = (int)(tmax / dt) + 1;
            var methode = modell.Zeitintegration.Methode;
            var parameter1 = modell.Zeitintegration.Parameter1;
            var parameter2 = modell.Zeitintegration.Parameter2;
            var anregung = new double[nZeitschritte + 1][];
            for (var i = 0; i < (nZeitschritte + 1); i++) anregung[i] = new double[dimension];
            // ... berechne diagonale Massenmatrix ..............................
            if (!diagonalMatrix) BerechneDiagonalMatrix();

            // ... berechne diagonale Dämpfungsmatrix ..............................
            var dämpfungsmatrix = BerechneDämpfungsMatrix();

            // ... berechne zeitabhängige Anregungsfunktion und Randbedingungen
            BerechneAnregungsfunktionZweiterOrdnung(dt, anregung);

            var verformung = new double[nZeitschritte][];
            for (var k = 0; k < nZeitschritte; k++) verformung[k] = new double[dimension];
            var geschwindigkeit = new double[2][];
            for (var k = 0; k < 2; k++) geschwindigkeit[k] = new double[dimension];

            SetzRandbedingungenZweiterOrdnung(verformung, geschwindigkeit);
            SetzDynamischenStatusVektor();

            if (zerlegt)
            {
                NeuberechnungSystemMatrix();
                zerlegt = false;
            }

            var zeitintegration = new Zeitintegration2OrdnungStatus(systemGleichungen, dämpfungsmatrix,
                dt, methode, parameter1, parameter2,
                verformung, geschwindigkeit, anregung);
            zeitintegration.Ausführen();

            // speichere Knotenzeitverläufe
            foreach (var item2 in modell.Knoten)
            {
                knoten = item2.Value;
                var index = knoten.SystemIndizes;
                var anzahlKnotenfreiheitsgrade = knoten.AnzahlKnotenfreiheitsgrade;

                knoten.KnotenVariable = new double[anzahlKnotenfreiheitsgrade][];
                for (var i = 0; i < anzahlKnotenfreiheitsgrade; i++) knoten.KnotenVariable[i] = new double[nZeitschritte];
                knoten.KnotenAbleitungen = new double[anzahlKnotenfreiheitsgrade][];
                for (var i = 0; i < anzahlKnotenfreiheitsgrade; i++) knoten.KnotenAbleitungen[i] = new double[nZeitschritte];

                // verformung[nZeitschritte][index], geschwindigkeit[2][index], KnotenVariable[index][nZeitschritte]
                for (var i = 0; i < knoten.AnzahlKnotenfreiheitsgrade; i++)
                {
                    if (systemGleichungen.Status[index[i]]) continue;
                    for (var k = 0; k < nZeitschritte; k++)
                    {
                        knoten.KnotenVariable[i][k] = zeitintegration.verformung[k][index[i]];
                        knoten.KnotenAbleitungen[i][k] = zeitintegration.beschleunigung[k][index[i]];
                    }
                }
            }
            modell.ZeitIntegration = true;
            _ = MessageBox.Show("Zeitverlaufberechnung 2. Ordnung erfolgreich durchgeführt", "Zeitintegration2Ordnung");
        }
        private double[] BerechneDämpfungsMatrix()
        {
            // ... falls "Dämpfung" modale Dämpfungsmaße beinhaltet,
            // ... kann die Dämpfungsmatrix ermittelt werden über die Summe aller berücksichtigten
            // ... Eigenzustände (s. Clough & Penzien S. 198, 13-37
            // ... M*(SUM(((2*(xi)n*(omega)n )/(M)n))*phi(n)*(phi)nT)*M
            // ... wobei M die Massenmatrix ist, (xi)n das modale Dämpfungsmaß,
            // ... omega(n) eigenfrequenz, (M)n modale Massen und phi die Eigenvektoren
            var dämpfungsMatrix = new double[dimension];
            if (modell.Eigenzustand.DämpfungsRaten.Count == 0)
            {
                _ = MessageBox.Show("ungedämpftes System", "BerechneDämpfungsMatrix");
                return dämpfungsMatrix;
            }
            // Eigenberechnung wird für Ermittlung der modalen Dämpfungsmaße benötigt
            if (modell.Eigen == false)
            {
                Eigenzustände();
                modell.Eigen = true;
            }
            // modale Dämpfungsmaße werden aus eingelesenen DämpfungsRaten ermittelt
            var modaleDämpfung = new double[modell.Eigenzustand.AnzahlZustände];
            for (var i = 0; i < modell.Eigenzustand.DämpfungsRaten.Count; i++)
            {
                modaleDämpfung[i] = ((ModaleWerte)modell.Eigenzustand.DämpfungsRaten[i]).Dämpfung;
            }
            // ist nur ein Dämpfungsmaß gegeben, werden ALLE Eigenzustände damit belegt
            for (var i = modell.Eigenzustand.DämpfungsRaten.Count; i < modell.Eigenzustand.AnzahlZustände; i++)
            {
                modaleDämpfung[i] = modaleDämpfung[0];
            }

            double faktor = 0;
            for (var n = 0; n < modell.Eigenzustand.AnzahlZustände; n++)
            {
                double phinPhinT = 0;
                double mn = 0;
                for (var i = 0; i < systemGleichungen.DiagonalMatrix.Length; i++)
                {
                    phinPhinT += modell.Eigenzustand.Eigenvektoren[n][i] * modell.Eigenzustand.Eigenvektoren[n][i];
                }

                for (var i = 0; i < systemGleichungen.DiagonalMatrix.Length; i++)
                {
                    mn += modell.Eigenzustand.Eigenvektoren[n][i] * systemGleichungen.DiagonalMatrix[i] * modell.Eigenzustand.Eigenvektoren[n][i];
                }

                faktor += 2 * modaleDämpfung[n] * Math.Sqrt(modell.Eigenzustand.Eigenwerte[n]) / 2 / Math.PI * phinPhinT / mn;
            }
            // diagonale Dämpfungsmatrix wird aus m*faktor*m ermittelt
            for (var i = 0; i < systemGleichungen.DiagonalMatrix.Length; i++)
            {
                dämpfungsMatrix[i] = systemGleichungen.DiagonalMatrix[i] * faktor * systemGleichungen.DiagonalMatrix[i];
            }
            return dämpfungsMatrix;
        }

        private void SetzRandbedingungenZweiterOrdnung(IReadOnlyList<double[]> displ, IReadOnlyList<double[]> veloc)
        {
            // finde vordefinierte Anfangsbedingungen
            foreach (Knotenwerte anf in modell.Zeitintegration.Anfangsbedingungen)
            {
                if (!modell.Knoten.TryGetValue(anf.KnotenId, out var anfKnoten)) continue;
                for (var i = 0; i < anf.Werte.Length / 2; i += 2)
                {
                    foreach (var knotenIndex in anfKnoten.SystemIndizes)
                    {
                        displ[i][knotenIndex] = anf.Werte[i];
                        veloc[i + 1][knotenIndex] = anf.Werte[i + 1];
                    }
                }
            }
        }
        private void SetzDynamischenStatusVektor()
        {
            // für alle zeitabhängigen Randbedingungen
            foreach (var randbedingung in
                modell.ZeitabhängigeRandbedingung.Select(item => item.Value))
            {
                StatusKnoten(randbedingung);
            }
        }

        // zeitabhängige Knoteneinwirkungen
        private void BerechneAnregungsfunktionZweiterOrdnung(double dt, IReadOnlyList<double[]> anregung)
        {
            // finde zweitabhängige Knoteneinwirkungen
            foreach (var item in modell.ZeitabhängigeKnotenLasten)
            {
                var force = new double[anregung.Count];
                switch (item.Value.VariationsTyp)
                {
                    case 0:
                        {
                            const string inputDirectory = "\\FE-Berechnungen-App\\input\\Tragwerksberechnung\\Dynamik\\Anregungsdateien";
                            const int col = -1; // ALLE Values in Datei
                                                // Ordinatenwerte im Zeitintervall dt aus Datei lesen
                            AusDatei(inputDirectory, col, force);
                            break;
                        }
                    case 1:
                        {
                            var intervall = item.Value.Intervall;
                            // lineare Interpolation der abschnittweise linearen Eingabedaten im Zeitintervall dt
                            StückweiseLinear(dt, intervall, force);
                            break;
                        }
                    case 2:
                        {
                            var amplitude = item.Value.Amplitude;
                            var frequenz = 2 * Math.PI * item.Value.Frequenz;
                            var phasenWinkel = Math.PI / 180 * item.Value.PhasenWinkel;
                            // periodische Anregung mit Ausgabe "force" im Zeitintervall dt
                            Periodisch(dt, amplitude, frequenz, phasenWinkel, force);
                            break;
                        }
                }

                if (item.Value.Bodenanregung)
                {
                    var knotenFreiheitsgrad = item.Value.KnotenFreiheitsgrad;

                    var masse = systemGleichungen.DiagonalMatrix;
                    foreach (var index in modell.Knoten.Select(item2 => 
                                 item2.Value.SystemIndizes).Where(index => 
                                 !systemGleichungen.Status[index[knotenFreiheitsgrad]]))
                    {
                        for (var k = 0; k < anregung.Count; k++)
                            anregung[k][index[knotenFreiheitsgrad]] = -masse[index[knotenFreiheitsgrad]] * force[k];
                    }
                }

                else
                {
                    if (!modell.Knoten.TryGetValue(item.Value.KnotenId, out knoten)) continue;
                    var index = knoten.SystemIndizes;
                    var knotenFreitsgrad = item.Value.KnotenFreiheitsgrad;

                    for (var k = 0; k < anregung.Count; k++)
                        for (var j = 0; j < anregung[0].Length; j++)
                            anregung[k][index[knotenFreitsgrad]] = force[k];
                }
            }
        }

        // zeitabhängige Eingabedaten
        private void AusDatei(string inputDirectory, int spalte, IList<double> last)
        {
            string[] zeilen, substrings;
            var delimiters = new[] { '\t' };

            var datei = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            datei.InitialDirectory += inputDirectory;

            if (datei.ShowDialog() != true)
                return;
            var pfad = datei.FileName;

            try
            {
                zeilen = File.ReadAllLines(pfad);
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Berechnung.AusDatei");
                return;
            }
            // Anregungsfunktion[timeSteps]
            // Datei enthält nur Anregungswerte im VORGEGEBENEM ZEITSCHRITT dt
            if (spalte < 0)
            {
                // lies alle Werte einer Datei
                var werte = new List<double>();
                foreach (var zeile in zeilen)
                {
                    substrings = zeile.Split(delimiters);
                    werte.AddRange(substrings.Select(double.Parse));
                }
                for (var i = 0; i < werte.Count; i++) { last[i] = werte[i]; }
            }
            else
            {
                // lies alle Werte einer bestimmten Spalte col [][0-n]
                var schritte = last.Count;
                if (schritte > zeilen.Length) schritte = zeilen.Length;
                for (var k = 0; k < schritte; k++)
                {
                    substrings = zeilen[k].Split(delimiters);
                    last[k] = double.Parse(substrings[spalte-1]);
                }
            }
        }
        public List<double> AusDatei(string inputDirectory)
        {
            var delimiters = new[] { '\t' };
            var werte = new List<double>();

            var datei = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            datei.InitialDirectory += inputDirectory;

            if (datei.ShowDialog() != true)
                return werte;
            var pfad = datei.FileName;

            try
            {
                var zeilen = File.ReadAllLines(pfad);
                foreach (var zeile in zeilen)
                {
                    var substrings = zeile.Split(delimiters);
                    werte.AddRange(substrings.Select(double.Parse));
                }
            }
            catch (IOException ex)
            {
                _ = MessageBox.Show(ex + " Anregungsfunktion konnte nicht aus Datei gelesen werden!!!", "Analysis FromFile");
                return werte;
            }
            return werte;
        }
        private static void StückweiseLinear(double dt, IReadOnlyList<double> intervall, IList<double> last)
        {
            int zähler = 0, nZeitschritte = last.Count;
            double endLast = 0;
            var startZeit = intervall[0];
            var startLast = intervall[1];
            last[zähler] = startLast;
            for (var j = 2; j < intervall.Count; j += 2)
            {
                var endZeit = intervall[j];
                endLast = intervall[j + 1];
                var schritteJeIntervall = (int)(Math.Round((endZeit - startZeit) / dt));
                var inkrement = (endLast - startLast) / schritteJeIntervall;
                for (var k = 1; k <= schritteJeIntervall; k++)
                {
                    zähler++;
                    if (zähler == nZeitschritte) return;
                    last[zähler] = last[zähler - 1] + inkrement;
                }
                startZeit = endZeit;
                startLast = endLast;
            }
            for (var k = zähler + 1; k < nZeitschritte; k++) last[k] = endLast;
        }
        private static void Periodisch(double dt, double amplitude, double frequenz, double phasenWinkel, double[] last)
        {
            var nZeitschritte = last.GetLength(0);
            double zeit = 0;
            for (var k = 0; k < nZeitschritte; k++)
            {
                last[k] = amplitude * Math.Sin(frequenz * zeit + phasenWinkel);
                zeit += dt;
            }
        }
    }
}