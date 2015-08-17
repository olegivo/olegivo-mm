using System;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class SimpleItemDiffAction<TItem> : ItemDiffAction<TItem> where TItem : class
    {
        private readonly Action<TItem> action;

        public SimpleItemDiffAction(Func<TItem> itemProvider, Action<TItem> action, DiffType diffType = DiffType.None) 
            : base(itemProvider, null, diffType)
        {
            this.action = Enforce.ArgumentNotNull(action, "action");
        }

        protected override void ApplySelf()
        {
            base.ApplySelf();
            action(Item);
        }
    }
}