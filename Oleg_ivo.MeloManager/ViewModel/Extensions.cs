using System;
using System.Windows;
using System.Windows.Threading;

namespace Oleg_ivo.MeloManager.ViewModel
{
    public static class Extensions
    {
        public static void InvokeOnDispatcher(this Action action, Dispatcher dispatcher = null)
        {
            if (dispatcher == null) dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }
    }
}