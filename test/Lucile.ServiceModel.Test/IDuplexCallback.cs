using System.ServiceModel;

namespace Lucile.ServiceModel.Test
{
    public interface IDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnFinished();

        [OperationContract(IsOneWay = true)]
        void OnProgress(decimal progress);
    }
}