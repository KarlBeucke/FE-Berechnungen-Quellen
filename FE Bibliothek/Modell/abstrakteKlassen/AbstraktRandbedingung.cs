namespace FEBibliothek.Modell.abstrakteKlassen
{
    public abstract class AbstraktRandbedingung
    {
        public string RandbedingungId { get; set; }
        public string KnotenId { get; set; }
        public Knoten Knoten { get; set; }
        public int Typ { get; set; }
        public string Face { get; set; }
        public double[] Vordefiniert { get; set; }
        public bool[] Festgehalten { get; set; }

        public void SetzRandbedingungenReferenzen(FeModell modell)
        {
            if (KnotenId != null)
            {
                if (modell.Knoten.TryGetValue(KnotenId, out Knoten node)) { Knoten = node; }

                if (node == null)
                {
                    throw new ModellAusnahme("\nKnoten mit ID = " + KnotenId + " ist nicht im Modell enthalten");
                }
            }
            else
            {
                throw new ModellAusnahme("\nKnotenidentifikator für Randbedingung " + RandbedingungId +
                                         " ist nicht definiert");
            }
        }
    }
}
