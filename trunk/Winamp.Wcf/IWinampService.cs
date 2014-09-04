using System;
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

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginPlay(AsyncCallback callback, object asyncState);

        void EndPlay(IAsyncResult result);

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginPlayPause(AsyncCallback callback, object asyncState);
        void EndPlayPause(IAsyncResult result);

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginStop(AsyncCallback callback, object asyncState);
        void EndStop(IAsyncResult result);

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginPreviousTrack(AsyncCallback callback, object asyncState);
        void EndPreviousTrack(IAsyncResult result);

        [OperationContract(IsOneWay = true, AsyncPattern = true)]
        IAsyncResult BeginNextTrack(AsyncCallback callback, object asyncState);
        void EndNextTrack(IAsyncResult result);
    }
}
