using System;
using Codeplex.Reactive;

namespace Oleg_ivo.MeloManager.Extensions
{
    public static class ReactiveExtensions
    {
        public static ReactiveCommand AddHandler(this ReactiveCommand command, Action action)
        {
            command.Subscribe(_ => action());
            return command;
        }

    }
}
