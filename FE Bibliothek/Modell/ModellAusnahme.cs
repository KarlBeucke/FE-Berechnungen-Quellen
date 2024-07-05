using System;

namespace FEBibliothek.Modell
{
    [Serializable]
    public class ModellAusnahme : Exception
    {
        public ModellAusnahme(string message)
            : base(string.Format("Fehler in Modelldaten:" + message)) { }

        public ModellAusnahme() { }

        public ModellAusnahme(string message, Exception innerException)
            : base(string.Format(message), innerException) { }

        protected ModellAusnahme(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
