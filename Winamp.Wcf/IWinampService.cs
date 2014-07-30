using System.ServiceModel;

namespace Winamp.Wcf
{
    [ServiceContract(CallbackContract = typeof(IWinampServiceCallback))]
    public interface IWinampService
    {
/*
        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);
*/

        [OperationContract(IsOneWay = true)]
        void LoadPlaylist(string playlistFilename);

        [OperationContract(IsOneWay = true)]
        void Ping();
    }
}
