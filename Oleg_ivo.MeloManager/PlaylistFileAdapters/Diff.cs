using System;
using System.Collections.Generic;
using System.Linq;
using Oleg_ivo.Base.Autofac;
using Oleg_ivo.Base.Extensions;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters
{
    public abstract class DiffActionBase : IDiffAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected DiffActionBase(IList<IDiffAction> childDiffActions, DiffType diffType)
        {
            ChildDiffActions = childDiffActions;
            DiffType = diffType;
        }

        public virtual void Apply()
        {
            if (PreAction != null) PreAction();
            if (ChildDiffActions != null)
            {
                foreach (var childDiffAction in ChildDiffActions)
                {
                    childDiffAction.Apply();
                }
            }
            if (PostAction != null) PostAction();
        }

        public IList<IDiffAction> ChildDiffActions { get; private set; }
        public DiffType DiffType { get; private set; }

        public Action PreAction { get; set; }
        public Action PostAction { get; set; }
    }

    public class ContainerDiffAction : DiffActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ContainerDiffAction(IList<IDiffAction> childDiffActions, DiffType diffType) : base(childDiffActions, diffType)
        {
            Enforce.ArgumentNotNull(childDiffActions, "childDiffActions");
        }
    }

    public class DiffAction<TTarget, TItem> : DiffActionBase where TItem : class
    {
        private readonly Func<TItem> itemProvider;
        private readonly Action<TTarget, TItem> action;
        private readonly Func<TTarget> targetProvider;
        private TTarget target;
        private TItem item;

        public DiffAction(Func<TTarget> targetProvider, Func<TItem> itemProvider, Action<TTarget, TItem> action, DiffType diffType, IList<IDiffAction> childDiffActions = null) : base(childDiffActions, diffType)
        {
            this.targetProvider = Enforce.ArgumentNotNull(targetProvider, "targetProvider");
            this.itemProvider = Enforce.ArgumentNotNull(itemProvider, "itemProvider");
            this.action = Enforce.ArgumentNotNull(action, "action");
            if(childDiffActions != null)
            {
                if (diffType==DiffType.Added && childDiffActions.Any(child=>child.DiffType!=DiffType.Added))
                    throw new InvalidOperationException("Тип различия вложенных действий отличен от Added");
                if (diffType == DiffType.Deleted && childDiffActions.Any(child => child.DiffType != DiffType.Deleted))
                    throw new InvalidOperationException("Тип различия вложенных действий отличен от Deleted");
            }
        }

        public override void Apply()
        {
            target = targetProvider();
            item = itemProvider();
            action(target, item);
            base.Apply();
        }

        public TItem ApplyAndReturnItem()
        {
            Apply();
            return item;
        }

    }

    public class DiffCreator
    {
        public static IList<IDiffAction> CreateCollectionDiff<T>(IEnumerable<T> oldVersion, IEnumerable<T> newVersion, Func<T, IDiffAction> onAdd, Func<T, IDiffAction> onDelete, Func<T, T, IDiffAction> onEquals, IEqualityComparer<T> comparer = null)
        {
            var foj = oldVersion.FullOuterJoin(newVersion, comparer);
            IList<IDiffAction> diffActions = new List<IDiffAction>();
            foreach (var item in foj)
            {
                if (item.Item1 == null)
                {
                    diffActions.Add(onAdd(item.Item2));
                }
                else if (item.Item2 == null)
                {
                    diffActions.Add(onDelete(item.Item1));
                }
                else
                {
                    diffActions.Add(onEquals(item.Item1, item.Item2));
                }
            }

            return diffActions;
        }
    }

    public class DiffFunc<TResult>
    {
        
    }

    public interface IDiffAction
    {
        IList<IDiffAction> ChildDiffActions { get; }
        DiffType DiffType { get; }
        void Apply();
    }

    [Flags]
    public enum DiffType
    {
        None = 0,
        Added = 1,
        Modified = 2,
        Deleted = 4
    }
}