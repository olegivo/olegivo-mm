using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.Extensions
{
    public static class FileSystemWatcherExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="watcher"></param>
        /// <param name="changeType">Тип изменений. Может содержать несколько флагов, фильтрация идёт по ИЛИ</param>
        /// <param name="throttleTime"></param>
        /// <returns></returns>
        public static IObservable<string> ToObservable(this FileSystemWatcher watcher, WatcherChangeTypes changeType = WatcherChangeTypes.All, TimeSpan? throttleTime = null)
        {
            Enforce.ArgumentNotNull(watcher, "watcher");
            Action<FileSystemEventHandler> attach = null;
            Action<FileSystemEventHandler> detach = null;
            foreach (var item in Enum.GetValues(typeof(WatcherChangeTypes)).Cast<WatcherChangeTypes>().Where(item => item!=WatcherChangeTypes.All && changeType.HasFlag(item)))
            {
                switch (item)
                {
                    case WatcherChangeTypes.Created:
                        attach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Created += h));
                        detach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Created -= h));
                        break;
                    case WatcherChangeTypes.Deleted:
                        attach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Deleted += h));
                        detach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Deleted -= h));
                        break;
                    case WatcherChangeTypes.Changed:
                        attach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Changed += h));
                        detach = (Action<FileSystemEventHandler>)Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Changed -= h));
                        break;
                    //TODO:renamed пока не реализован
                    //case WatcherChangeTypes.Renamed:
                    //    attach = (Action<FileSystemEventHandler>) Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Renamed += h));
                    //    detach = (Action<FileSystemEventHandler>)Delegate.Combine(attach, new Action<FileSystemEventHandler>(h => watcher.Renamed -= h));
                    //    break;
                }
            }
            return
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(attach, detach)
                    .Where(pattern => changeType.HasFlag(pattern.EventArgs.ChangeType))
                    .Throttle(throttleTime ?? TimeSpan.Zero)
                    .Select(pattern => pattern.EventArgs.FullPath);
        }
    }
}
