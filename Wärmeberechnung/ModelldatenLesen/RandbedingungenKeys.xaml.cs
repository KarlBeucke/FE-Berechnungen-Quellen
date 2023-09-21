using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;
using System.Linq;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class RandbedingungenKeys
{
    public RandbedingungenKeys(FeModell modell)
    {
        InitializeComponent();
        this.Left = 2 * this.Width;
        this.Top = this.Height;

        var randbedingungen = new List<AbstraktRandbedingung>();
        var temperaturen = modell.Randbedingungen.
            Select(item => item.Value).ToList();
        randbedingungen.AddRange(temperaturen);
        var zeitabhängigeTemperaturen = modell.ZeitabhängigeRandbedingung.
            Select(item => item.Value).ToList();
        randbedingungen.AddRange(zeitabhängigeTemperaturen);
        RandbedingungKey.ItemsSource = randbedingungen;
    }
}