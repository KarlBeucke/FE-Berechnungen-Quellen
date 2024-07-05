using System;
using System.Runtime.Serialization;

namespace FEBibliothek.Modell
{
    [Serializable]
    public class BerechnungAusnahme : Exception
    {
        public BerechnungAusnahme(string message)
            : base(string.Format("Fehler in der Berechnung:" + message)) { }

        public BerechnungAusnahme() { }

        public BerechnungAusnahme(string message, Exception innerException)
            : base(string.Format(message), innerException) { }

        protected BerechnungAusnahme(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
