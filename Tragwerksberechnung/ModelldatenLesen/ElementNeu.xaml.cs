﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell _modell;

    public ElementNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
        ElementId.Text = string.Empty;
        StartknotenId.Text = string.Empty;
        EndknotenId.Text = string.Empty;
        MaterialId.Text = string.Empty;
        QuerschnittId.Text = string.Empty;
    }

    private void FachwerkChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = true;
        Gelenk2.IsChecked = true;
        BalkenCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }

    private void BalkenChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false;
        Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }

    private void FederChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false;
        Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        BalkenCheck.IsChecked = false;
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        AbstraktElement element = null;
        if (ElementId.Text == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Element");
            return;
        }

        // vorhandenes Element wird komplett entfernt, da Elementdefinition
        // (Fachwerk, Biegebalken, BiegebalkenGelenk) geändert werden kann
        // neues Element wird angelegt und unter vorhandenem Key gespeichert
        if (_modell.Elemente.ContainsKey(ElementId.Text)) _modell.Elemente.Remove(ElementId.Text);
        var knotenIds = new string[2];
        knotenIds[0] = StartknotenId.Text;
        if (EndknotenId.Text.Length != 0) knotenIds[1] = EndknotenId.Text;
        _modell.Knoten.TryGetValue(knotenIds[0], out var startKnoten);
        if (startKnoten == null)
        {
            _ = MessageBox.Show("Startknoten ist nicht definiert", "neues Element");
            return;
        }
        _modell.Knoten.TryGetValue(knotenIds[1], out var endKnoten);
        if (endKnoten == null)
        {
            _ = MessageBox.Show("Endknoten ist nicht definiert", "neues Element");
            return;
        }

        try
        {
            if (FachwerkCheck.IsChecked != null && (bool)FachwerkCheck.IsChecked)
            {
                startKnoten.AnzahlKnotenfreiheitsgrade = 2;
                element = new Fachwerk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                // check, ob ein Knoten des Fachwerkstabs ein eingespannter lagerknoten ist
                if (Gelenkstab(startKnoten.Id) || (Gelenkstab(endKnoten.Id)))
                    _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' kann nicht eingespannt werden");
            }
            else if (BalkenCheck.IsChecked != null && (bool)BalkenCheck.IsChecked)
            {
                if (_modell.Querschnitt.TryGetValue(QuerschnittId.Text, out var querschnitt))
                {
                    if (querschnitt.QuerschnittsWerte.Length < 2 && Trägheitsmoment.Text.Length == 0)
                    {
                        _ = MessageBox.Show("Trägheitsmoment ist nicht definiert", "neues Element");
                        return;
                    }
                }

                switch (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked)
                {
                    // falls Biegebalken angewählt ist und der Biegebalken an Knoten mit <3 Freiheitsgraden angeschlossen ist,
                    // wird Biegebalken automatisch als BiegebalkenGelenk bzw. Fachwerk eingefügt
                    case false when Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked:
                        startKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 2);
                        break;
                    case true when Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked:
                        if (Gelenkstab(startKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' kann nicht eingespannt werden");
                        endKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 1);
                        break;
                    case false when Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked:
                        if (Gelenkstab(endKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' kann nicht eingespannt werden");
                        startKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        endKnoten.AnzahlKnotenfreiheitsgrade = 3;
                        element = new Biegebalken(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                        break;
                    case true when Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked:
                        if (Gelenkstab(startKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' kann nicht eingespannt werden");
                        if (Gelenkstab(endKnoten.Id))
                            _ = MessageBox.Show("\nGelenkstab '" + ElementId.Text + "' kann nicht eingespannt werden");
                        element = new Fachwerk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
                        break;
                }
            }
            else if (FederCheck.IsChecked != null && (bool)FederCheck.IsChecked)
            {
                element = new FederElement(knotenIds, MaterialId.Text, _modell);
            }
            else
            {
                _ = MessageBox.Show("Elementtyp muss definiert sein", "neues Element");
                return;
            }
        }
        catch (ModellAusnahme elementNeu)
        {
            _ = MessageBox.Show(elementNeu.Message);
        }


        if (element != null)
        {
            try
            {
                element.ElementId = ElementId.Text;
                if (EModul.Text != string.Empty) element.E = double.Parse(EModul.Text);
                if (Masse.Text != string.Empty) element.M = double.Parse(Masse.Text);
                if (Fläche.Text != string.Empty) element.A = double.Parse(Fläche.Text);
                if (Trägheitsmoment.Text != string.Empty) element.I = double.Parse(Trägheitsmoment.Text);
                _modell.Elemente.Add(ElementId.Text, element);
            }
            catch (FormatException)
            {
                _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Element");
            }
        }

        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual.ElementKeys?.Close();
        Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        StartFenster.TragwerkVisual.ElementKeys?.Close();
        StartFenster.TragwerkVisual.IsElement = false;
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.Keys.Contains(ElementId.Text)) return;
        _modell.Elemente.Remove(ElementId.Text);
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual.ElementKeys?.Close();

        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
        StartFenster.Berechnet = false;
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.ContainsKey(ElementId.Text))
        {
            StartknotenId.Text = "";
            EndknotenId.Text = "";
            MaterialId.Text = "";
            QuerschnittId.Text = "";
            return;
        }

        // vorhandene element definitionen
        _modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null");
        ElementId.Text = "";

        ElementId.Text = vorhandenesElement.ElementId;
        switch (vorhandenesElement)
        {
            case Fachwerk:
                FachwerkCheck.IsChecked = true;
                Gelenk1.IsChecked = true;
                Gelenk2.IsChecked = true;
                BalkenCheck.IsChecked = false;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case Biegebalken:
                FachwerkCheck.IsChecked = false;
                BalkenCheck.IsChecked = true;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case BiegebalkenGelenk:
                {
                    BalkenCheck.IsChecked = true;
                    switch (vorhandenesElement.Typ)
                    {
                        case 1:
                            Gelenk1.IsChecked = true;
                            break;
                        case 2:
                            Gelenk2.IsChecked = true;
                            break;
                    }

                    FachwerkCheck.IsChecked = false;
                    EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                    break;
                }
            case FederElement:
                FederCheck.IsChecked = true;
                break;
        }

        switch (vorhandenesElement)
        {
            case Fachwerk:
                FachwerkCheck.IsChecked = true;
                BalkenCheck.IsChecked = false;
                break;
            case Biegebalken:
                FachwerkCheck.IsChecked = false;
                BalkenCheck.IsChecked = true;
                break;
        }

        StartknotenId.Text = vorhandenesElement.KnotenIds[0];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
        EModul.Text = vorhandenesElement.E == 0
            ? string.Empty
            : vorhandenesElement.E.ToString("E2", CultureInfo.CurrentCulture);
        Masse.Text = vorhandenesElement.M == 0
            ? string.Empty
            : vorhandenesElement.M.ToString("E2", CultureInfo.CurrentCulture);
        QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
        Fläche.Text = vorhandenesElement.A == 0
            ? string.Empty
            : vorhandenesElement.A.ToString("E2", CultureInfo.CurrentCulture);
        Trägheitsmoment.Text = vorhandenesElement.I == 0
            ? string.Empty
            : vorhandenesElement.I.ToString("E2", CultureInfo.CurrentCulture);
    }

    private bool Gelenkstab(string knotenId)
    {
        return _modell.Randbedingungen.Any(lager
            => lager.Value.KnotenId == knotenId && lager.Value.Typ == 7);
    }
}