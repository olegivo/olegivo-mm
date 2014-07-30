using System.ServiceModel;

namespace Winamp.Wcf
{
    public interface IWinampServiceCallback : IWinampService
    {
        [OperationContract(IsOneWay = true)]
        void OnCurrentSongChanged(string filename);
    }
}