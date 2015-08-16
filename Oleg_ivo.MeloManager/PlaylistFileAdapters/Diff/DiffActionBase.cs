using System;
using System.Collections.Generic;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public abstract class DiffActionBase : IDiffAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected DiffActionBase(IList<IDiffAction> childDiffActions, DiffType diffType = DiffType.None)
        {
            ChildDiffActions = childDiffActions;
            DiffType = diffType;
        }

        public void Apply()
        {
            if (PreAction != null) PreAction();
            ApplySelf();
            ApplyChildren();
            if (PostAction != null) PostAction();
        }

        protected abstract void ApplySelf();

        protected void ApplyChildren()
        {
            if (ChildDiffActions != null)
            {
                foreach (var childDiffAction in ChildDiffActions)
                {
                    childDiffAction.Apply();
                }
            }
        }

        public IList<IDiffAction> ChildDiffActions { get; private set; }
        public DiffType DiffType { get; protected set; }

        public Action PreAction { get; set; }
        public Action PostAction { get; set; }
    }
}