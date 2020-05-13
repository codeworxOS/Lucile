namespace Lucile.Data.Metadata
{
    public class ForeignKey : IForeignKey
    {
        public ForeignKey(ScalarProperty principal, ScalarProperty dependant)
        {
            Principal = principal;
            Dependant = dependant;
        }

        public ScalarProperty Dependant { get; }

        public ScalarProperty Principal { get; }

        IScalarProperty IForeignKey.Principal => Principal;

        IScalarProperty IForeignKey.Dependant => Dependant;
    }
}