using FEBibliothek.Modell;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class Elastizitätsmodell3DVisualisieren
{
    // Veränderung des Kippwinkels, wenn hoch/runter Taste gedrückt wird
    //private const double CameraDPhi = 0.1;
    // Veränderung des Drehwinkels, wenn links/rechts Taste gedrückt wird
    //private const double CameraDTheta = 0.1;
    // Veränderung des Abstands, wenn BildHoch/BildRunter Taste gedrückt wird
    private const double CameraDr = 10;

    // Horizontalverschiebung li/re
    private const double CameraDx = 10;

    // Vertikalverschiebung hoch/runter
    private const double CameraDy = 5;

    private readonly Darstellung3D darstellung3D;

    // 3D Modellgruppe
    private readonly Model3DGroup model3DGroup = new();

    // Anfangsposition der Kamera
    private double cameraPhi = 0.13; // 7,45 Grad
    private double cameraR = 60.0;
    private double cameraTheta = 1.65; // 94,5 Grad
    private double cameraX;
    private double cameraY;
    private ModelVisual3D modelVisual;
    private PerspectiveCamera theCamera;

    public Elastizitätsmodell3DVisualisieren(FeModell feModell)
    {
        darstellung3D = new Darstellung3D(feModell);
        InitializeComponent();
    }

    // Erstellung einer 3D-Szene
    // Viewport ist definiert als Viewport3D im XAML-Code, der alles darstellt 
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

        // Erzeugung des Modells
        darstellung3D.UnverformteGeometrie(model3DGroup, true);

        darstellung3D.Randbedingungen(model3DGroup);

        darstellung3D.Knotenlasten(model3DGroup);

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        modelVisual = new ModelVisual3D { Content = model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(modelVisual);
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

    // Veränderung der Kameraposition mit Tasten hoch/runter, links/rechts, BildUHoch/BildRunter
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up: // Vertikalverschiebung
                //cameraPhi -= CameraDPhi;
                //if (cameraPhi > Math.PI / 2.0) cameraPhi = Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                cameraY -= CameraDy;
                break;
            case Key.Down:
                //cameraPhi += CameraDPhi;
                //if (cameraPhi < -Math.PI / 2.0) cameraPhi = -Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                cameraY += CameraDy;
                break;

            case Key.Left: // Horizontalverschiebung
                //cameraTheta -= CameraDTheta;
                //ScrTheta.Value = cameraTheta;
                cameraX += CameraDx;
                break;
            case Key.Right:
                //cameraTheta += CameraDTheta;
                //ScrTheta.Value = cameraTheta;
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

    // An- und Abschalten der einzelnen Modelldarstellungen (GeometryModel3Ds)
    private void ShowKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in darstellung3D.koordinaten) model3DGroup.Children.Add(koordinaten);
    }

    private void RemoveKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in darstellung3D.koordinaten) model3DGroup.Children.Remove(koordinaten);
    }

    private void ShowOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in darstellung3D.oberflächen) model3DGroup.Children.Add(oberflächen);
    }

    private void RemoveOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in darstellung3D.oberflächen) model3DGroup.Children.Remove(oberflächen);
    }

    private void ShowDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in darstellung3D.kanten) model3DGroup.Children.Add(kanten);
    }

    private void RemoveDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in darstellung3D.kanten) model3DGroup.Children.Remove(kanten);
    }

    private void ShowRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in darstellung3D.randbedingungenFest)
            model3DGroup.Children.Add(randbedingungenFest);
    }

    private void RemoveRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in darstellung3D.randbedingungenFest)
            model3DGroup.Children.Remove(randbedingungenFest);
    }

    private void ShowRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in darstellung3D.randbedingungenVor)
            model3DGroup.Children.Add(randbedingungenVor);
    }

    private void RemoveRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in darstellung3D.randbedingungenVor)
            model3DGroup.Children.Remove(randbedingungenVor);
    }

    private void ShowKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in darstellung3D.knotenLasten) model3DGroup.Children.Add(knotenlasten);
    }

    private void RemoveKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in darstellung3D.knotenLasten) model3DGroup.Children.Remove(knotenlasten);
    }
}