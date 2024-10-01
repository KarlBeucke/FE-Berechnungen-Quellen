namespace FEBibliothek.Modell
{
    [Serializable]
    public class ParseAusnahme : Exception
    {
        public ParseAusnahme(string message)
            : base(string.Format("Fehler in Eingabezeile " + message)) { }

        public ParseAusnahme() { }

        public ParseAusnahme(string message, Exception innerException)
            : base(string.Format(message), innerException) { }

        protected ParseAusnahme(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
