namespace Lucile.Dynamic
{
    public interface ICommitable
    {
        void Commit();

        void Rollback();
    }
}
