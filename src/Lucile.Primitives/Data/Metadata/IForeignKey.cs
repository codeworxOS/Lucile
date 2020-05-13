namespace Lucile.Data.Metadata
{
    public interface IForeignKey
    {
        IScalarProperty Principal { get; }

        IScalarProperty Dependant { get; }
    }
}
