namespace HJ.Runtime
{
    public interface IDynamicUnlock
    {
        public void OnTryUnlock(DynamicObject dynamicObject);
    }
}