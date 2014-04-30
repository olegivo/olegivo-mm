using System.ComponentModel;

namespace Oleg_ivo.MeloManager.ViewModel
{
    public class DeletingEventArgs<T> : CancelEventArgs
    {
        public T Deleting { get; private set; }

        public DeletingEventArgs(T deleting) : this(deleting, false)
        {
        }

        public DeletingEventArgs(T deleting, bool cancel) : base(cancel)
        {
            Deleting = deleting;
        }
    }
}