using System;
using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class DiffAction<TTarget, TItem> : ItemDiffAction<TItem> where TItem : class
    {
        private readonly Action<TTarget, TItem> action;
        private readonly Func<TTarget> targetProvider;
        private TTarget target;

        public DiffAction(Func<TTarget> targetProvider, Func<TItem> itemProvider, Action<TTarget, TItem> action, DiffType diffType, IList<IDiffAction> childDiffActions = null)
            : base(itemProvider, childDiffActions, diffType)
        {
            this.targetProvider = Enforce.ArgumentNotNull(targetProvider, "targetProvider");
            this.action = Enforce.ArgumentNotNull(action, "action");
            if(childDiffActions != null)
            {
                if (diffType==DiffType.Added && childDiffActions.Any(child=>child.DiffType!=DiffType.Added))
                    throw new InvalidOperationException("Тип различия вложенных действий отличен от Added");
                if (diffType == DiffType.Deleted && childDiffActions.Any(child => child.DiffType != DiffType.Deleted))
                    throw new InvalidOperationException("Тип различия вложенных действий отличен от Deleted");
            }
        }

        protected override void ApplySelf()
        {
            base.ApplySelf();
            target = targetProvider();
            action(target, Item);
        }
    }
}