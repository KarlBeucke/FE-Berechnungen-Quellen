using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell _modell;
    private readonly ElementKeys _elementKeys;

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
        _elementKeys = new ElementKeys(modell) { Owner = this };
        _elementKeys.Show();
    }

    private void FachwerkChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = true; Gelenk2.IsChecked = true;
        BalkenCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }
    private void BalkenChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false; Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }
    private void FederChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false; Gelenk2.IsChecked = false;
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
        if (_modell.Elemente.ContainsKey(ElementId.Text))
        {
            _modell.Elemente.Remove(ElementId.Text);
        }
        var knotenIds = new string[2];
        knotenIds[0] = StartknotenId.Text;
        if (EndknotenId.Text.Length != 0) knotenIds[1] = EndknotenId.Text;

        if (FachwerkCheck.IsChecked != null && (bool)FachwerkCheck.IsChecked)
            element = new Fachwerk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
        else if (BalkenCheck.IsChecked != null && (bool)BalkenCheck.IsChecked)
        {
            if ((Gelenk1.IsChecked != null && !(bool)Gelenk1.IsChecked) &&
                (Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked))
                element = new Biegebalken(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell);
            if (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked)
                element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 1);
            else if (Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked)
                element = new BiegebalkenGelenk(knotenIds, MaterialId.Text, QuerschnittId.Text, _modell, 2);
        }
        else if (FederCheck.IsChecked != null && (bool)FederCheck.IsChecked)
            element = new FederElement(knotenIds, MaterialId.Text, _modell);
        else
        {
            _ = MessageBox.Show("Elementtyp muss definiert sein", "neues Element");
            return;
        }
        if (element != null)
        {
            element.ElementId = ElementId.Text;
            if (EModul.Text != string.Empty) element.E = double.Parse(EModul.Text);
            if (Masse.Text != string.Empty) element.M = double.Parse(Masse.Text);
            if (Fläche.Text != string.Empty) element.A = double.Parse(Fläche.Text);
            if (Trägheitsmoment.Text != string.Empty) element.I = double.Parse(Trägheitsmoment.Text);
            _modell.Elemente.Add(ElementId.Text, element);
        }
        Close();
        StartFenster.TragwerkVisual.Close();
        _elementKeys?.Close();
        StartFenster.TragwerkVisual = new ModelldatenAnzeigen.TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _elementKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.Keys.Contains(ElementId.Text)) return;
        _modell.Elemente.Remove(ElementId.Text);
        Close();
        StartFenster.TragwerkVisual.Close();
        StartFenster.TragwerkVisual = new TragwerkmodellVisualisieren(StartFenster.TragwerksModell);
        StartFenster.TragwerkVisual.Show();
        _elementKeys?.Close();
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
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null"); ElementId.Text = "";

        ElementId.Text = vorhandenesElement.ElementId;
        switch (vorhandenesElement)
        {
            case Fachwerk:
                FachwerkCheck.IsChecked = true;
                Gelenk1.IsChecked = true; Gelenk2.IsChecked = true;
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
                FachwerkCheck.IsChecked = true; BalkenCheck.IsChecked = false;
                break;
            case Biegebalken:
                FachwerkCheck.IsChecked = false; BalkenCheck.IsChecked = true;
                break;
        }
        StartknotenId.Text = vorhandenesElement.KnotenIds[0];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
        EModul.Text = vorhandenesElement.E == 0 ? string.Empty : vorhandenesElement.E.ToString("E2", CultureInfo.CurrentCulture);
        Masse.Text = vorhandenesElement.M == 0 ? string.Empty : vorhandenesElement.M.ToString("E2", CultureInfo.CurrentCulture);
        QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
        Fläche.Text = vorhandenesElement.A == 0 ? string.Empty : vorhandenesElement.A.ToString("E2", CultureInfo.CurrentCulture);
        Trägheitsmoment.Text = vorhandenesElement.I == 0 ? string.Empty : vorhandenesElement.I.ToString("E2", CultureInfo.CurrentCulture);
    }
}