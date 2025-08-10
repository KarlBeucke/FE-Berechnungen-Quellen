using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;
using System.Globalization;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen
{
    public partial class MaterialNeu : Window
    {
        private readonly FeModell _modell;
        private MaterialKeys _materialKeys;
        private QuerschnittKeys _querschnittKeys;
        public string AktuelleMaterialId;
        public string AktuelleQuerschnittId;

        public MaterialNeu(FeModell modell)
        {
            InitializeComponent();
            _modell = modell;
            AktuelleMaterialId = "";
            AktuelleQuerschnittId = "";
            Show();
        }

        public MaterialNeu(FeModell modell, AbstraktMaterial material)
        {
            InitializeComponent();
            _modell = modell;
            MaterialId.Text = material.MaterialId;
            AktuelleMaterialId = material.MaterialId;
            if (!material.Feder)
            {
                EModul.Text = material.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
                Poisson.Text = material.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            }
            else
            {
                EModul.Text = "";
                Poisson.Text = "";
            }

            Show();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            var materialId = MaterialId.Text;

            // vorhandenes Material
            if (_modell.Material.TryGetValue(materialId, out var vorhandenesMaterial))
            {
                try
                {
                    if (EModul.Text.Length > 0) vorhandenesMaterial.MaterialWerte[0] = double.Parse(EModul.Text);
                    if (Poisson.Text.Length > 0) vorhandenesMaterial.MaterialWerte[1] = double.Parse(Poisson.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                    return;
                }
            }
            // neues Material
            else
            {
                if (EModul.Text.Length > 0)
                {
                    double eModul = 0, poisson = 0;
                    try
                    {
                        eModul = double.Parse(EModul.Text);
                        if (Poisson.Text.Length > 0) poisson = double.Parse(Poisson.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neues Material");
                    }

                    var material = new Material(eModul, poisson)
                    {
                        MaterialId = materialId
                    };
                    _modell.Material.Add(materialId, material);
                }
            }
            if (AktuelleMaterialId != MaterialId.Text) _modell.Material.Remove(AktuelleMaterialId);

            var querschnittId = QuerschnittId.Text;
            // vorhandener Querschnitt
            if (_modell.Querschnitt.TryGetValue(querschnittId, out var vorhandenerQuerschnitt))
            {
                try
                {
                    if (Dicke.Text.Length > 0) vorhandenerQuerschnitt.QuerschnittsWerte[0] = double.Parse(Dicke.Text);
                }
                catch (FormatException)
                {
                    _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                    return;
                }
            }
            // neuer Querschnitt
            else
            {
                if (Dicke.Text.Length > 0)
                {
                    double dicke = 0;
                    try
                    {
                        dicke = double.Parse(Dicke.Text);
                    }
                    catch (FormatException)
                    {
                        _ = MessageBox.Show("ungültiges  Eingabeformat", "neuer Querschnitt");
                    }

                    var querschnitt = new Querschnitt(dicke)
                    {
                        QuerschnittId = querschnittId
                    };
                    _modell.Querschnitt.Add(querschnittId, querschnitt);
                }
            }
            if (AktuelleQuerschnittId != QuerschnittId.Text) _modell.Querschnitt.Remove(AktuelleQuerschnittId);

            Close();
            switch (_modell.Raumdimension)
            {
                case 2:
                    StartFenster.ElastizitätVisual.Close();
                    StartFenster.ElastizitätVisual = new ElastizitätsmodellVisualisieren(_modell);
                    StartFenster.ElastizitätVisual.Show();
                    break;
                case 3:
                    StartFenster.ElastizitätVisual3D.Close();
                    StartFenster.ElastizitätVisual3D = new Elastizitätsmodell3DVisualisieren(_modell);
                    StartFenster.ElastizitätVisual3D.Show();
                    break;
            }

            _modell.Berechnet = false;
        }

        private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MaterialIdGotFocus(object sender, RoutedEventArgs e)
        {
            _materialKeys = new MaterialKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _materialKeys.Show();
            MaterialId.Focus();
        }

        private void MaterialIdLostFocus(object sender, RoutedEventArgs e)
        {
            _materialKeys?.Close();
            if (!_modell.Material.TryGetValue(MaterialId.Text, out var vorhandenesMaterial)) return;

            // vorhandene Materialdefinition
            MaterialId.Text = vorhandenesMaterial.MaterialId;
            EModul.Text = vorhandenesMaterial.MaterialWerte[0].ToString("G3", CultureInfo.CurrentCulture);
            if (vorhandenesMaterial.MaterialWerte[1] > 0)
                Poisson.Text = vorhandenesMaterial.MaterialWerte[1].ToString("G3", CultureInfo.CurrentCulture);
            if (vorhandenesMaterial.MaterialWerte.Length > 2)
                Schub.Text = vorhandenesMaterial.MaterialWerte[2].ToString("G3", CultureInfo.CurrentCulture);
        }
        private bool MaterialReferenziert()
        {
            var id = MaterialId.Text;
            foreach (var element in _modell.Elemente.Where(element => element.Value.ElementMaterialId == id))
            {
                _ = MessageBox.Show(
                    "Material referenziert durch Element " + element.Value.ElementId + ", kann nicht gelöscht werden",
                    "neues Material");
                return true;
            }

            //if (_modell.Elemente.All(element => element.Value.ElementMaterialId != id)) return false;
            return false;
        }

        private void QuerschnittIdGotFocus(object sender, RoutedEventArgs e)
        {
            _querschnittKeys = new QuerschnittKeys(_modell) { Topmost = true, Owner = (Window)Parent };
            _querschnittKeys.Show();
            QuerschnittId.Focus();
        }
        private void QuerschnittIdLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_modell.Querschnitt.TryGetValue(QuerschnittId.Text, out var vorhandenerQuerschnitt)) return;

            // vorhandene Querschnittdefinition
            QuerschnittId.Text = vorhandenerQuerschnitt.QuerschnittId;
            Dicke.Text = vorhandenerQuerschnitt.QuerschnittsWerte[0].ToString("G3", CultureInfo.CurrentCulture);
        }

        private void BtnLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialReferenziert() || !_modell.Material.Remove(MaterialId.Text))
            {
                Close();
                return;
            }

            Close();
            _modell.Berechnet = false;
        }
    }
}