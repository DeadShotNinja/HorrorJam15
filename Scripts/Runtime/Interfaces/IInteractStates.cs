using UnityEngine;

namespace HJ.Runtime 
{
    public struct TitleParams
    {
        public string Title;
        public string Button1;
        public string Button2;
    }

    public class StateParams
    {
        public string StateKey;
        public StorableCollection StateData;
    }

    public interface IInteractStart
    {
        public void InteractStart();
    }

    public interface IInteractHold
    {
        public void InteractHold(Vector3 point);
    }

    public interface IInteractTimed
    {
        public float InteractTime { get; set; }
        public bool NoInteract { get; }
        public void InteractTimed();
    }

    public interface IInteractStop
    {
        public void InteractStop();
    }

    public interface IInteractStartPlayer
    {
        public void InteractStartPlayer(GameObject player);
    }

    public interface IStateInteract
    {
        public StateParams OnStateInteract();
    }

    public interface IInteractTitle
    {
        public TitleParams InteractTitle();
    }
}
