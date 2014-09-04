using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.ServiceModel;
using System.Threading;
using NLog;

namespace Winamp.Wcf
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WinampService : IWinampService, IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private IDisposable currentFileChangesObserver;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WinampService()
        {
            LoadPlaylistSubject = new Subject<string>();
            ActionSubject = new Subject<ActionType>();
            disposer = new CompositeDisposable(LoadPlaylistSubject, ActionSubject);
        }

        public Subject<string> LoadPlaylistSubject { get; private set; }

        public Subject<ActionType> ActionSubject { get; private set; }

        public IObservable<string> CurrentFileChanges
        {
            get { return currentFileChanges; }
            set
            {
                if (currentFileChanges == value) return;
                currentFileChanges = value;
                if (CurrentFileChanges == null) return;
                if (currentFileChangesObserver != null)
                {
                    disposer.Remove(currentFileChangesObserver);
                    currentFileChangesObserver.Dispose();
                }
                currentFileChangesObserver =
                    CurrentFileChanges.Subscribe(
                        filename => IterateCallbacks(callback => callback.OnCurrentSongChanged(filename)),
                        exception => log.Error(exception));
                disposer.Add(currentFileChangesObserver);
            }
        }

/*
        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public void MessageBox(string message)
        {
            var clientCallback = OperationContext.Current.GetCallbackChannel<IWinampServiceCallback>();
            AddCallback(clientCallback);
            //clientCallback.OnCurrentSongChanged();
            System.Windows.MessageBox.Show(message);
        }
*/

        public void LoadPlaylist(string playlistFilename)
        {
            AddCurrentCallback();
            LoadPlaylistSubject.OnNext(playlistFilename);
        }

        public void Ping()
        {
            AddCurrentCallback();
        }

        private static CompletedAsyncResult<object> CreateAsyncResult()
        {
            return CreateAsyncResult((object)null);
        }

        private static CompletedAsyncResult<T> CreateAsyncResult<T>(T data)
        {
            return new CompletedAsyncResult<T>(data);
        }

        public IAsyncResult BeginPlay(AsyncCallback callback, object asyncState)
        {
            AddCurrentCallback();
            ActionSubject.OnNext(ActionType.Play);
            return CreateAsyncResult();
        }

        public void EndPlay(IAsyncResult result)
        {
        }

        public IAsyncResult BeginPlayPause(AsyncCallback callback, object asyncState)
        {
            AddCurrentCallback();
            ActionSubject.OnNext(ActionType.PlayPause);
            return CreateAsyncResult();
        }

        public void EndPlayPause(IAsyncResult result)
        {
        }

        public IAsyncResult BeginStop(AsyncCallback callback, object asyncState)
        {
            AddCurrentCallback();
            ActionSubject.OnNext(ActionType.Stop);
            return CreateAsyncResult();
        }

        public void EndStop(IAsyncResult result)
        {
        }

        public IAsyncResult BeginPreviousTrack(AsyncCallback callback, object asyncState)
        {
            AddCurrentCallback();
            ActionSubject.OnNext(ActionType.PreviousTrack);
            return CreateAsyncResult();
        }

        public void EndPreviousTrack(IAsyncResult result)
        {
        }

        public IAsyncResult BeginNextTrack(AsyncCallback callback, object asyncState)
        {
            AddCurrentCallback();
            ActionSubject.OnNext(ActionType.NextTrack);
            return CreateAsyncResult();
        }

        public void EndNextTrack(IAsyncResult result)
        {
        }

        private readonly List<IWinampServiceCallback> callbacks = new List<IWinampServiceCallback>();

        /// <summary>
        /// Есть обратные вызовы
        /// </summary>
        public bool HasCallbacks
        {
            get { lock (callbacks) return callbacks.Any(); }
        }

        /// <summary>
        /// Добавить обратный вызов клиента
        /// </summary>
        /// <param name="callback"></param>
        private void AddCallback(IWinampServiceCallback callback)
        {
            lock (callbacks)
            {
                if (!callbacks.Contains(callback)) 
                    callbacks.Add(callback);
            }
        }

        private void AddCurrentCallback()
        {
            var clientCallback = OperationContext.Current.GetCallbackChannel<IWinampServiceCallback>();
            AddCallback(clientCallback);
        }

        /// <summary>
        /// Удалить обратный вызов клиента
        /// </summary>
        /// <param name="callback"></param>
        protected void RemoveCallback(IWinampServiceCallback callback)
        {
            lock (callbacks)
            {
                if (callbacks.Contains(callback))
                    callbacks.Remove(callback);
            }
        }

        protected void IterateCallbacks(Action<IWinampServiceCallback> callbackAction)
        {
            lock (callbacks)
                foreach (var callback in callbacks.ToList())
                    try
                    {
                        callbackAction(callback);
                    }
                    catch (CommunicationException ex)
                    {
                        log.Error("Обратный вызов с клиентом нарушен. Удаляем обратный вызов и ждём повторной регистрации клиента.", (Exception)ex);
                        callbacks.Remove(callback);
                    }
        }

        private IObservable<string> currentFileChanges;
        private readonly CompositeDisposable disposer;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            disposer.Dispose();
        }
    }

    class CompletedAsyncResult<T> : IAsyncResult
    {
        public static CompletedAsyncResult<T> Create(T data)
        {
            return new CompletedAsyncResult<T>(data);
        }

        readonly T data;

        public CompletedAsyncResult(T data)
        { this.data = data; }

        public T Data
        { get { return data; } }

        #region IAsyncResult Members
        public object AsyncState
        { get { return data; } }

        public WaitHandle AsyncWaitHandle
        { get { throw new Exception("The method or operation is not implemented."); } }

        public bool CompletedSynchronously
        { get { return true; } }

        public bool IsCompleted
        { get { return true; } }
        #endregion
    }
}
