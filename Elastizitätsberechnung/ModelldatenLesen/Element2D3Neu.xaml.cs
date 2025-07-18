using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Globalization;
using Element2D3 = FE_Berechnungen.Elastizitätsberechnung.Modelldaten.Element2D3;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class Element2D3Neu
    {
        private readonly FeModell _modell;
        private ElementKeys _elementKeys;
        private MaterialKeys _materialKeys;
        private QuerschnittKeys _querschnittKeys;
        private string _aktuelleId;

        public Element2D3Neu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            _aktuelleId = "";
            Show();
        }

        public Element2D3Neu(FeModell modell, AbstraktLinear2D3 element)
        {
            InitializeComponent();
            _modell = modell;
            ElementId.Text = element.ElementId;
            _aktuelleId = element.ElementId;
            Knoten1Id.Text = element.KnotenIds[0];
            Knoten2Id.Text = element.KnotenIds[1];
            Knoten3Id.Text = element.KnotenIds[2];
            MaterialId.Text = element.ElementMaterialId;
            EModul.Text = element.E.ToString("E2", CultureInfo.CurrentCulture);
            Poisson.Text = element.Nue.ToString("G2", CultureInfo.CurrentCulture);
            QuerschnittId.Text = element.ElementQuerschnittId;
            Dicke.Text = element.Dicke.ToString("E2", CultureInfo.CurrentCulture);
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
            MaterialId.Text = vorhandenesElement.ElementMaterialId;
            EModul.Text = vorhandenesElement.E == 0
                ? string.Empty
                : vorhandenesElement.E.ToString("E2", CultureInfo.CurrentCulture);
            Poisson.Text = vorhandenesElement.Nue == 0
                ? string.Empty
                : vorhandenesElement.Nue.ToString("G2", CultureInfo.CurrentCulture);
            QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
            Dicke.Text = vorhandenesElement.Dicke == 0
                ? string.Empty
                : vorhandenesElement.Dicke.ToString("G2", CultureInfo.CurrentCulture);
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

        private void QuerschnittIdGotFocus(object sender, RoutedEventArgs e)
        {
            _querschnittKeys = new QuerschnittKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _querschnittKeys.Show();
            QuerschnittId.Focus();
        }
        private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (_querschnittKeys.Id != null) QuerschnittId.Text = _querschnittKeys.Id;
            _querschnittKeys.Close();
            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var elementId = ElementId.Text;
            if (elementId == "")
            {
                _ = MessageBox.Show("Element Id muss definiert sein", "neues Element2D3");
                return;
            }

            // vorhandenes Element2D3
            if (_modell.Elemente.TryGetValue(elementId, out var vorhandenesElement))
            {
                if (ElementId.Text.Length > 0)
                    vorhandenesElement.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
                try
                {
                    if (Knoten1Id.Text.Length > 0) vorhandenesElement.KnotenIds[0] = Knoten1Id.Text;
                    if (Knoten2Id.Text.Length > 0) vorhandenesElement.KnotenIds[1] = Knoten2Id.Text;
                    if (Knoten3Id.Text.Length > 0) vorhandenesElement.KnotenIds[2] = Knoten3Id.Text;

                    if (MaterialId.Text.Length > 0) vorhandenesElement.ElementMaterialId = MaterialId.Text;
                    if (EModul.Text.Length > 0) vorhandenesElement.E = double.Parse(EModul.Text);
                    if (Poisson.Text.Length > 0) vorhandenesElement.Nue = double.Parse(Poisson.Text);

                    if (QuerschnittId.Text.Length > 0) vorhandenesElement.ElementQuerschnittId = QuerschnittId.Text;
                    if (Dicke.Text.Length > 0) vorhandenesElement.Dicke = double.Parse(Dicke.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neues Element2D3");
                    return;
                }
            }

            // neues Element2D3
            else
            {
                var knotenIds = new string[3];
                double eModul = 0, poisson = 0, dicke = 0;
                string materialId = "", querschnittId = "";
                try
                {
                    if (Knoten1Id.Text.Length > 0) knotenIds[0] = Knoten1Id.Text;
                    if (Knoten2Id.Text.Length > 0) knotenIds[1] = Knoten2Id.Text;
                    if (Knoten3Id.Text.Length > 0) knotenIds[2] = Knoten3Id.Text;

                    if (MaterialId.Text.Length > 0) materialId = MaterialId.Text;
                    if (EModul.Text.Length > 0) eModul = double.Parse(EModul.Text);
                    if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);

                    if (QuerschnittId.Text.Length != 0) querschnittId = QuerschnittId.Text;
                    if (Dicke.Text.Length > 0) dicke = double.Parse(Dicke.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges Format in der Eingabe", "neue Punktlast");
                    return;
                }
                var element = new Element2D3(knotenIds, querschnittId, materialId, _modell)
                {
                    ElementId = elementId,
                    E = eModul,
                    Nue = poisson,
                    Dicke = dicke

                };
                _modell.Elemente.Add(elementId, element);
            }
            if (_aktuelleId != ElementId.Text) _modell.Elemente.Remove(_aktuelleId);

            StartFenster.ElastizitätVisual.Close();
            _elementKeys?.Close();
            Close();

            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            _elementKeys?.Close();
            StartFenster.ElastizitätVisual.IsElement = false;
            Close();
        }

        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (!_modell.Elemente.Remove(ElementId.Text, out _)) return;
            Close();
            StartFenster.ElastizitätVisual.Close();

            StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
            StartFenster.ElastizitätVisual.Show();
            _modell.Berechnet = false;
        }
    }
}
