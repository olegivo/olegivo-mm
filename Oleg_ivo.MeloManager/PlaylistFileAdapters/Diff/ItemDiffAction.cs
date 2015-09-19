using System;
using System.Collections.Generic;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class ItemDiffAction<TItem> : DiffActionBase where TItem : class
    {
        private readonly Func<TItem> itemProvider;
        protected TItem Item;

        protected ItemDiffAction(Func<TItem> itemProvider, IList<IDiffAction> childDiffActions, DiffType diffType = DiffType.None)
            : base(childDiffActions, diffType)
        {
            this.itemProvider = Enforce.ArgumentNotNull(itemProvider, "itemProvider");
        }

        protected override void ApplySelf()
        {
            if(Item==null) Item = itemProvider();
        }

        public TItem ApplyAndReturnItem()
        {
            Apply();
            return Item;
        }

        public static ItemDiffAction<TItem> CreateEmptyStub()
        {
            return new ItemDiffAction<TItem>(() => default(TItem), new IDiffAction[]{});
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            ApplySelf();
            return string.Format("{0}", Item);
        }
    }
}