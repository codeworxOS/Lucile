namespace Lucile.Data.Metadata
{
    public class ForeignKey
    {
        public ForeignKey(ScalarProperty principal, ScalarProperty dependant)
        {
            Principal = principal;
            Dependant = dependant;
        }

        public ScalarProperty Dependant { get; }

        public ScalarProperty Principal { get; }
    }
}