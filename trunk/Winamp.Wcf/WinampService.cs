using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.ServiceModel;
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
        }

        public Subject<string> LoadPlaylistSubject { get; private set; }

        public IObservable<string> CurrentFileChanges
        {
            get { return currentFileChanges; }
            set
            {
                if (currentFileChanges == value) return;
                currentFileChanges = value;
                if (CurrentFileChanges == null) return;
                if (currentFileChangesObserver != null)
                    currentFileChangesObserver.Dispose();
                currentFileChangesObserver =
                    CurrentFileChanges.Subscribe(
                        filename => IterateCallbacks(callback => callback.OnCurrentSongChanged(filename)),
                        exception => log.Error(exception));
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
        public void RemoveCallback(IWinampServiceCallback callback)
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
                    catch (CommunicationObjectFaultedException ex)
                    {
                        log.Error("Обратный вызов с клиентом нарушен. Удаляем обратный вызов и ждём повторной регистрации клиента.", (Exception)ex);
                        callbacks.Remove(callback);
                    }
        }

        private bool disposed;
        private IObservable<string> currentFileChanges;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(disposed) return;
            LoadPlaylistSubject.Dispose();
            currentFileChangesObserver.Dispose();
            disposed = true;
        }
    }
}
