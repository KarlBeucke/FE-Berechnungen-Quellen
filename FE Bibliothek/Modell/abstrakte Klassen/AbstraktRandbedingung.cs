namespace FEBibliothek.Modell.abstrakte_Klassen
{
    public abstract class AbstraktRandbedingung
    {
        public string RandbedingungId { get; set; }
        public string KnotenId { get; set; }
        public Knoten Knoten { get; set; }
        public int Typ { get; set; }
        public double[] Vordefiniert { get; set; }
        public bool[] Festgehalten { get; set; }

        public void SetzRandbedingungenReferenzen(FEModell modell)
        {
            if (KnotenId != null)
            {
                if (modell.Knoten.TryGetValue(KnotenId, out Knoten node))
                {
                    Knoten = node;
                }

                if (node == null)
                {
                    throw new ModellAusnahme("Knoten mit ID = " + KnotenId + " ist nicht im Modell enthalten");
                }
            }
            else
            {
                throw new ModellAusnahme("Knotenidentifikator für Randbedingung " + RandbedingungId +
                                         " ist nicht definiert");
            }
        }
    }
}
