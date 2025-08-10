using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Element3D8Neu
    {
        private readonly FeModell _modell;
        private ElementKeys _elementKeys;
        private MaterialKeys _materialKeys;
        private string _aktuelleId;
        public Element3D8Neu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            _aktuelleId = "";
            Show();
        }

        public Element3D8Neu(FeModell modell, AbstraktLinear3D8 element)
        {
            InitializeComponent();
            _modell = modell;
            ElementId.Text = element.ElementId;
            _aktuelleId = element.ElementId;
            Knoten1Id.Text = element.KnotenIds[0];
            Knoten2Id.Text = element.KnotenIds[1];
            Knoten3Id.Text = element.KnotenIds[2];
            Knoten4Id.Text = element.KnotenIds[3];
            Knoten5Id.Text = element.KnotenIds[4];
            Knoten6Id.Text = element.KnotenIds[5];
            Knoten7Id.Text = element.KnotenIds[6];
            Knoten8Id.Text = element.KnotenIds[7];
            MaterialId.Text = element.ElementMaterialId;
            Show();
        }

        private void ElementIdGotFocus(object sender, RoutedEventArgs e)
        {
            _aktuelleId = ElementId.Text;
            _elementKeys = new ElementKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _elementKeys.Show();
            ElementId.Focus();
        }
        private void ElementIdLostFocus(object sender, RoutedEventArgs e)
        {
            _elementKeys?.Close();
            if (!_modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement)) return;

            // Elementtyp aus vorhandener Elementdefinition
            ElementId.Text = vorhandenesElement.ElementId;
            Knoten1Id.Text = vorhandenesElement.KnotenIds[0];
            Knoten2Id.Text = vorhandenesElement.KnotenIds[1];
            Knoten3Id.Text = vorhandenesElement.KnotenIds[2];
            Knoten4Id.Text = vorhandenesElement.KnotenIds[3];
            Knoten5Id.Text = vorhandenesElement.KnotenIds[4];
            Knoten6Id.Text = vorhandenesElement.KnotenIds[5];
            Knoten7Id.Text = vorhandenesElement.KnotenIds[6];
            Knoten8Id.Text = vorhandenesElement.KnotenIds[7];
            MaterialId.Text = vorhandenesElement.ElementMaterialId;
        }

        private void MaterialIdGotFocus(object sender, RoutedEventArgs e)
        {
            _materialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _materialKeys.Show();
            MaterialId.Focus();
        }
        private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (_materialKeys.Id != null) MaterialId.Text = _materialKeys.Id;
            _materialKeys.Close();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementId = ElementId.Text;
            if (elementId == "")
            {
                _ = MessageBox.Show("Element Id muss definiert sein", "neues Element2D3");
                return;
            }

            // vorhandenes Element3D8
            if (_modell.Elemente.TryGetValue(elementId, out var vorhandenesElement))
            {
                if (ElementId.Text.Length > 0)
                    vorhandenesElement.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
                try
                {
                    if (Knoten1Id.Text.Length > 0) vorhandenesElement.KnotenIds[0] = Knoten1Id.Text;
                    if (Knoten2Id.Text.Length > 0) vorhandenesElement.KnotenIds[1] = Knoten2Id.Text;
                    if (Knoten3Id.Text.Length > 0) vorhandenesElement.KnotenIds[2] = Knoten3Id.Text;
                    if (Knoten4Id.Text.Length > 0) vorhandenesElement.KnotenIds[3] = Knoten4Id.Text;
                    if (Knoten5Id.Text.Length > 0) vorhandenesElement.KnotenIds[4] = Knoten5Id.Text;
                    if (Knoten6Id.Text.Length > 0) vorhandenesElement.KnotenIds[5] = Knoten6Id.Text;
                    if (Knoten7Id.Text.Length > 0) vorhandenesElement.KnotenIds[6] = Knoten7Id.Text;
                    if (Knoten8Id.Text.Length > 0) vorhandenesElement.KnotenIds[7] = Knoten8Id.Text;

                    if (MaterialId.Text.Length > 0) vorhandenesElement.ElementMaterialId = MaterialId.Text;
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neues Element2D3");
                    return;
                }
            }

            // neues Element3D8
            else
            {
                var knotenIds = new string[8];
                var materialId = "";
                try
                {
                    if (Knoten1Id.Text.Length > 0) knotenIds[0] = Knoten1Id.Text;
                    if (Knoten2Id.Text.Length > 0) knotenIds[1] = Knoten2Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[2] = Knoten3Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[3] = Knoten4Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[4] = Knoten5Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[5] = Knoten6Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[6] = Knoten7Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[7] = Knoten8Id.Text;

                    if (MaterialId.Text.Length > 0) materialId = MaterialId.Text;

                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Punktlast");
                    return;
                }
                var element = new Element3D8(knotenIds, materialId, _modell)
                {
                    ElementId = elementId,
                };
                _modell.Elemente.Add(elementId, element);
            }
            if (_aktuelleId != ElementId.Text) _modell.Elemente.Remove(_aktuelleId);

            StartFenster.ElastizitätVisual3D.Close();
            _elementKeys?.Close();
            Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
            _modell.Berechnet = false;
        }
        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (!_modell.Elemente.Remove(ElementId.Text, out _)) return;
            Close();
            StartFenster.ElastizitätVisual3D.Close();

            StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
            StartFenster.ElastizitätVisual3D.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            _elementKeys?.Close();
            Close();
        }
    }
}
