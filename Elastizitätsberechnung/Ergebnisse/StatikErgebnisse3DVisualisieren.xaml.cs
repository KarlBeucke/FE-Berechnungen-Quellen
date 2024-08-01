using FEBibliothek.Modell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;

public partial class StatikErgebnisse3DVisualisieren
{
    // Veränderung des Abstands, wenn +/- Taste gedrückt wird
    private const double CameraDr = 1;

    // Horizontalverschiebung li/re
    private const double CameraDx = 1;

    // Vertikalverschiebung hoch/runter
    private const double CameraDy = 1;
    private readonly Darstellung3D darstellung3D;

    private readonly Model3DGroup model3DGroup = new();

    // Anfangsposition der Kamera
    private double cameraPhi = 0.13; // 7,45 Grad
    private double cameraR = 60.0;
    private double cameraTheta = 1.65; // 94,5 Grad
    private double cameraX;
    private double cameraY;
    private string maxKey_xx, minKey_xx, maxKey_yy, minKey_yy, maxKey_zz, minKey_zz;
    private string maxKey_xy, minKey_xy, maxKey_yz, minKey_yz, maxKey_zx, minKey_zx;
    private double maxSigma_xx, minSigma_xx, maxSigma_yy, minSigma_yy, maxSigma_zz, minSigma_zz;
    private double maxSigma_xy, minSigma_xy, maxSigma_yz, minSigma_yz, maxSigma_zx, minSigma_zx;
    private ModelVisual3D modelVisual;
    private Dictionary<string, ElementSpannung> sigma;

    private PerspectiveCamera theCamera;

    public StatikErgebnisse3DVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        darstellung3D = new Darstellung3D(feModell);
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Festlegung der Anfangsposition der Kamera
        theCamera = new PerspectiveCamera { FieldOfView = 60 };
        Viewport.Camera = theCamera;
        PositionierKamera();

        // Festlegung der Beleuchtung
        FestlegungBeleuchtung();

        // Koordinatensystem
        darstellung3D.Koordinatensystem(model3DGroup);

        // unverformte Geometrie als Drahtmodell und optional als Oberflächenmodell
        darstellung3D.UnverformteGeometrie(model3DGroup, false);

        // Elementspannungen
        sigma = new Dictionary<string, ElementSpannung>();
        foreach (var item in darstellung3D.modell.Elemente)
            sigma.Add(item.Key, new ElementSpannung(item.Value.BerechneZustandsvektor()));
        // Spannungsauswahl
        var richtung = new List<string>
            { "sigma_xx", "sigma_yy", "sigma_xy", "sigma_zz", "sigma_yz", "sigma_zx", "keine" };
        //Spannungsauswahl.SelectedValue = "sigma_xx";
        Spannungsauswahl.ItemsSource = richtung;

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void DropDownSpannungsauswahlClosed(object sender, EventArgs e)
    {
        var spannung = (string)Spannungsauswahl.SelectedItem;
        switch (spannung)
        {
            case "sigma_xx":
                ShowSpannungen_xx();
                break;
            case "sigma_yy":
                ShowSpannungen_yy();
                break;
            case "sigma_xy":
                ShowSpannungen_xy();
                break;
            case "sigma_zz":
                ShowSpannungen_zz();
                break;
            case "sigma_yz":
                ShowSpannungen_yz();
                break;
            case "sigma_zx":
                ShowSpannungen_zx();
                break;
            case "keine":
                RemoveSpannungen();
                break;
        }
    }

    private void PositionierKamera()
    {
        // z-Blickrichtung, y-up, x-seitlich, _cameraR=Abstand
        // ermittle die Kameraposition in kartesischen Koordinaten
        // y=Abstand*sin(Kippwinkel) (hoch, runter)
        // hypotenuse = Abstand*cos(Kippwinkel)
        // x= hypotenuse * cos(Drehwinkel) (links, rechts)
        // z= hypotenuse * sin(Drehwinkel)
        var y = cameraR * Math.Sin(cameraPhi);
        var hyp = cameraR * Math.Cos(cameraPhi);
        var x = hyp * Math.Cos(cameraTheta);
        var z = hyp * Math.Sin(cameraTheta);
        theCamera.Position = new Point3D(x + cameraX, y + cameraY, z);
        double offset = 0;

        // Blick in Richtung Koordinatenursprung (0; 0; 0), zentriert
        // falls Koordinatenursprung links oben, versetz Darstellung um offset
        if (darstellung3D.minX >= 0) offset = 10;
        theCamera.LookDirection = new Vector3D(-(x - offset), -(y + offset), -z);

        // Setzen der Up Richtung
        theCamera.UpDirection = new Vector3D(0, 1, 0);

        //_ = MessageBox.Show("Camera.Position: (" + x + ", " + y + ", " + z + ")", "3D Wireframe");
    }

    private void FestlegungBeleuchtung()
    {
        var ambientLight = new AmbientLight(Colors.Gray);
        var directionalLight =
            new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
        model3DGroup.Children.Add(ambientLight);
        model3DGroup.Children.Add(directionalLight);
    }

    private void ShowKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in darstellung3D.koordinaten) model3DGroup.Children.Add(koordinaten);
    }

    private void RemoveKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in darstellung3D.koordinaten) model3DGroup.Children.Remove(koordinaten);
    }

    private void ShowDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in darstellung3D.kanten) model3DGroup.Children.Add(kanten);
    }

    private void RemoveDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in darstellung3D.kanten) model3DGroup.Children.Remove(kanten);
    }

    private void ShowVerformungen(object sender, RoutedEventArgs e)
    {
        if (darstellung3D.verformungen.Count == 0)
            // verformte Geometrie als Drahtmodell
            darstellung3D.VerformteGeometrie(model3DGroup);
        else
            foreach (var verformungen in darstellung3D.verformungen)
                model3DGroup.Children.Add(verformungen);
    }

    private void RemoveVerformungen(object sender, RoutedEventArgs e)
    {
        foreach (var verformungen in darstellung3D.verformungen) model3DGroup.Children.Remove(verformungen);
    }

    private void ShowSpannungen_xx()
    {
        //var maxSigma_xx = sigma.Max(elementSpannung => elementSpannung.Value.Spannungen[0]);
        //var minSigma_xx = sigma.Min(elementSpannung => elementSpannung.Value.Spannungen[0]);
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[0] > maxSigma_xx)
            {
                maxSigma_xx = item.Value.Spannungen[0];
                maxKey_xx = item.Key;
            }

            if (!(item.Value.Spannungen[0] < minSigma_xx)) continue;
            minSigma_xx = item.Value.Spannungen[0];
            minKey_xx = item.Key;
        }

        MaxMin.Text = "maxSigma_xx = " + maxSigma_xx.ToString("0.###E+00") + " in Element " + maxKey_xx
                      + ",  minSigma_xx = " + minSigma_xx.ToString("0.###E+00") + " in Element " + minKey_xx;

        if (darstellung3D.spannungen_xx.Count == 0)
        {
            var maxWert = maxSigma_xx;
            if (Math.Abs(minSigma_xx) > maxWert) maxWert = Math.Abs(minSigma_xx);
            darstellung3D.ElementSpannungen_xx(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_xx) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void ShowSpannungen_yy()
    {
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[1] > maxSigma_yy)
            {
                maxSigma_yy = item.Value.Spannungen[1];
                maxKey_yy = item.Key;
            }

            if (!(item.Value.Spannungen[1] < minSigma_yy)) continue;
            minSigma_yy = item.Value.Spannungen[1];
            minKey_yy = item.Key;
        }

        MaxMin.Text = "maxSigma_yy = " + maxSigma_yy.ToString("0.###E+00") + " in Element " + maxKey_yy
                      + ",  minSigma_yy = " + minSigma_yy.ToString("0.###E+00") + " in Element " + minKey_yy;

        if (darstellung3D.spannungen_yy.Count == 0)
        {
            var maxWert = maxSigma_yy;
            if (Math.Abs(minSigma_yy) > maxWert) maxWert = Math.Abs(minSigma_yy);
            darstellung3D.ElementSpannungen_yy(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_yy) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void ShowSpannungen_xy()
    {
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[2] > maxSigma_xy)
            {
                maxSigma_xy = item.Value.Spannungen[2];
                maxKey_xy = item.Key;
            }

            if (!(item.Value.Spannungen[2] < minSigma_xy)) continue;
            minSigma_xy = item.Value.Spannungen[2];
            minKey_xy = item.Key;
        }

        MaxMin.Text = "maxSigma_xy = " + maxSigma_xy.ToString("0.###E+00") + " in Element " + maxKey_xy
                      + ",  minSigma_xy = " + minSigma_xy.ToString("0.###E+00") + " in Element " + minKey_xy;

        if (darstellung3D.spannungen_xy.Count == 0)
        {
            var maxWert = maxSigma_xy;
            if (Math.Abs(minSigma_xy) > maxWert) maxWert = Math.Abs(minSigma_xy);
            darstellung3D.ElementSpannungen_xy(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_xy) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void ShowSpannungen_zz()
    {
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[3] > maxSigma_zz)
            {
                maxSigma_zz = item.Value.Spannungen[3];
                maxKey_zz = item.Key;
            }

            if (!(item.Value.Spannungen[3] < minSigma_zz)) continue;
            minSigma_zz = item.Value.Spannungen[3];
            minKey_zz = item.Key;
        }

        MaxMin.Text = "maxSigma_zz = " + maxSigma_zz.ToString("0.###E+00") + " in Element " + maxKey_zz
                      + ",  minSigma_zz = " + minSigma_zz.ToString("0.###E+00") + " in Element " + minKey_zz;

        if (darstellung3D.spannungen_zz.Count == 0)
        {
            var maxWert = maxSigma_zz;
            if (Math.Abs(minSigma_zz) > maxWert) maxWert = Math.Abs(minSigma_zz);
            darstellung3D.ElementSpannungen_zz(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_zz) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void ShowSpannungen_yz()
    {
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[4] > maxSigma_yz)
            {
                maxSigma_yz = item.Value.Spannungen[4];
                maxKey_yz = item.Key;
            }

            if (!(item.Value.Spannungen[4] < minSigma_yz)) continue;
            minSigma_yz = item.Value.Spannungen[4];
            minKey_yz = item.Key;
        }

        MaxMin.Text = "maxSigma_yz = " + maxSigma_yz.ToString("0.###E+00") + " in Element " + maxKey_yz
                      + ",  minSigma_yz = " + minSigma_yz.ToString("0.###E+00") + " in Element " + minKey_yz;

        if (darstellung3D.spannungen_yz.Count == 0)
        {
            var maxWert = maxSigma_yz;
            if (Math.Abs(minSigma_yz) > maxWert) maxWert = Math.Abs(minSigma_yz);
            darstellung3D.ElementSpannungen_yz(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_yz) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void ShowSpannungen_zx()
    {
        RemoveSpannungen();
        foreach (var item in sigma)
        {
            if (item.Value.Spannungen[5] > maxSigma_zx)
            {
                maxSigma_zx = item.Value.Spannungen[5];
                maxKey_zx = item.Key;
            }

            if (!(item.Value.Spannungen[5] < minSigma_zx)) continue;
            minSigma_zx = item.Value.Spannungen[5];
            minKey_zx = item.Key;
        }

        MaxMin.Text = "maxSigma_zx = " + maxSigma_zx.ToString("0.###E+00") + " in Element " + maxKey_zx
                      + ",  minSigma_zx = " + minSigma_zx.ToString("0.###E+00") + " in Element " + minKey_zx;

        if (darstellung3D.spannungen_zx.Count == 0)
        {
            var maxWert = maxSigma_zx;
            if (Math.Abs(minSigma_yz) > maxWert) maxWert = Math.Abs(minSigma_zx);
            darstellung3D.ElementSpannungen_zx(model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in darstellung3D.spannungen_zx) model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
    }

    private void RemoveSpannungen()
    {
        foreach (var sigmaModell in darstellung3D.spannungen_xx) model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in darstellung3D.spannungen_yy) model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in darstellung3D.spannungen_xy) model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in darstellung3D.spannungen_zz) model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in darstellung3D.spannungen_yz) model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in darstellung3D.spannungen_zx) model3DGroup.Children.Remove(sigmaModell);
    }

    // Veränderung der Kameraposition mit Scrollbars
    private void ScrThetaScroll(object sender, ScrollEventArgs e)
    {
        cameraTheta = ScrTheta.Value;
        PositionierKamera();
    }

    private void ScrPhiScroll(object sender, ScrollEventArgs e)
    {
        cameraPhi = ScrPhi.Value;
        PositionierKamera();
    }

    // Veränderung der Kameraposition mit Tasten hoch/runter, links/rechts, PgUp/PgDn
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up: // Vertikalverschiebung
                cameraY -= CameraDy;

                break;
            case Key.Down:
                cameraY += CameraDy;
                break;

            case Key.Left: // Horizontalverschiebung
                cameraX += CameraDx;
                break;
            case Key.Right:
                cameraX -= CameraDx;
                break;

            case Key.Add: //  + Ziffernblock
            case Key.OemPlus: //  + alfanumerisch
                cameraR -= CameraDr;
                if (cameraR < CameraDr) cameraR = CameraDr;
                break;
            case Key.PageUp:
                cameraR -= CameraDr;
                if (cameraR < CameraDr) cameraR = CameraDr;
                break;

            case Key.Subtract: //  - Ziffernblock
            case Key.OemMinus: //  - alfanumerisch
                cameraR += CameraDr;
                break;
            case Key.PageDown:
                cameraR += CameraDr;
                if (cameraR < CameraDr) cameraR = CameraDr;
                break;
        }

        // Neufestlegung der Kameraposition
        PositionierKamera();
    }

    // Überhöhungsfaktor für die Darstellung der Verformungen
    private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
    {
        darstellung3D.überhöhungVerformung = double.Parse(Überhöhung.Text);
        foreach (var verformungen in darstellung3D.verformungen) model3DGroup.Children.Remove(verformungen);
        darstellung3D.VerformteGeometrie(model3DGroup);
    }

    private class ElementSpannung
    {
        public ElementSpannung(double[] spannungen)
        {
            Spannungen = spannungen;
        }

        public double[] Spannungen { get; }
    }
}