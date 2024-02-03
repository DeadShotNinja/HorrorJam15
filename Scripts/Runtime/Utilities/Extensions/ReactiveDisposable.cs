using System;
using System.Collections.Generic;

namespace HJ.Runtime
{
    public class ReactiveDisposable : List<IDisposable>
    {
        public void Dispose()
        {
            foreach (var disposable in this)
            {
                disposable.Dispose();
            }
        }
    }

    public static class ReactiveDisposableExtensions
    {
        public static void AddTo(this IDisposable disposable, ReactiveDisposable disposables)
        {
            disposables.Add(disposable);
        }
    }
}