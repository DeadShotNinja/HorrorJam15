namespace HJ
{
    public abstract class FSMState
    {
        public virtual void OnStateEnter() { }
        public virtual void OnStateUpdate() { }
        public virtual void OnStateFixedUpdate() { }
        public virtual void OnStateExit() { }
        public virtual void OnDrawGizmos() { }
    }
}
