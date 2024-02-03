using UnityEngine;

namespace HJ.Runtime
{
    public interface IOnDragStart
    {
        void OnDragStart();
    }

    public interface IOnDragEnd
    {
        void OnDragEnd();
    }

    public interface IOnDragUpdate
    {
        void OnDragUpdate(Vector3 velocity);
    }
}