using System;
using Oleg_ivo.Base.Autofac;

namespace Oleg_ivo.MeloManager.PlaylistFileAdapters.Diff
{
    public class SimpleDiffAction : DiffActionBase
    {
        private readonly Action action;

        public SimpleDiffAction(Action action, DiffType diffType = DiffType.None) 
            : base(null, diffType)
        {
            this.action = Enforce.ArgumentNotNull(action, "action");
        }

        protected override void ApplySelf()
        {
            action();
        }
    }
}