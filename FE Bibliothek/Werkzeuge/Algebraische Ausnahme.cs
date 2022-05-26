using System;

namespace FEBibliothek.Werkzeuge
{
    [Serializable]
    public class AlgebraischeAusnahme : Exception
    {
        public AlgebraischeAusnahme(string message)
            : base(string.Format("Fehler in MatrixAlgebra " + message)) { }

        public AlgebraischeAusnahme() { }

        public AlgebraischeAusnahme(string message, Exception innerException)
            : base(string.Format(message), innerException) { }

        protected AlgebraischeAusnahme(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
