using System;

namespace HJ.Runtime
{
    public interface IReticleProvider
    {
        (Type, Reticle, bool) OnProvideReticle();
    }
}