using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    [Serializable]
    public abstract class DynamicObjectType
    {
        [field: SerializeField]
        public DynamicObject DynamicObject { get; internal set; }

        public bool IsHolding { get; protected set; }

        public virtual bool IsOpened { get; } = false;

        protected DynamicObject.DynamicStatus DynamicStatus => DynamicObject.DynamicStatusEnum;
        protected DynamicObject.InteractType InteractType => DynamicObject.InteractTypeEnum;
        protected DynamicObject.StatusChange StatusChange => DynamicObject.StatusChangeEnum;

        protected Transform Target => DynamicObject.Target;
        protected Animator Animator => DynamicObject.Animator;
        protected HingeJoint Joint => DynamicObject.Joint;
        protected Rigidbody Rigidbody => DynamicObject.Rigidbody;

        /// <summary>
        /// Override this parameter to set whether you want to show or hide gizmos on the dynamic object.
        /// </summary>
        public virtual bool ShowGizmos { get; } = false;

        /// <summary>
        /// Override this method to set custom parameters at Awake.
        /// </summary>
        public virtual void OnDynamicInit() { }

        /// <summary>
        /// Override this method to set custom parameters when you interact with the dynamic object.
        /// </summary>
        public virtual void OnDynamicStart(PlayerManager player) { }

        /// <summary>
        /// Override this method to set the behavior when a open event is called.
        /// </summary>
        public virtual void OnDynamicOpen() { }

        /// <summary>
        /// Override this method to set the behavior when a close event is called.
        /// </summary>
        public virtual void OnDynamicClose() { }

        /// <summary>
        /// Override this method to define custom actions when the dynamic object is locked.
        /// </summary>
        public virtual void OnDynamicLocked() { }

        /// <summary>
        /// Override this method to define your own behavior at Update.
        /// </summary>
        public virtual void OnDynamicUpdate() { }

        /// <summary>
        /// Override this method to define a custom behavior when you hold the interact button on a dynamic object.
        /// </summary>
        public virtual void OnDynamicHold(Vector2 mouseDelta) { }

        /// <summary>
        /// Override this method to clear the parameters.
        /// </summary>
        public virtual void OnDynamicEnd() { }

        /// <summary>
        /// Override this method if you want to define your own gizmos drawing.
        /// </summary>
        public virtual void OnDrawGizmos() { }

        /// <summary>
        /// Try to unlock the dynamic object.
        /// </summary>
        public virtual void TryUnlock()
        {
            if (StatusChange == DynamicObject.StatusChange.InventoryItem)
            {
                if (DynamicObject.UnlockItem.HasItem)
                {
                    if (!DynamicObject.KeepUnlockItem)
                        DynamicObject.Inventory.RemoveItem(DynamicObject.UnlockItem);

                    DynamicObject.SetLockedStatus(false);
                    DynamicObject.UnlockedEvent?.Invoke();
                    DynamicObject.PlaySound(DynamicSoundType.Unlock);
                }
                else
                {
                    DynamicObject.LockedEvent?.Invoke();
                    DynamicObject.PlaySound(DynamicSoundType.Locked);
                    OnDynamicLocked();

                    if (DynamicObject.ShowLockedText)
                        DynamicObject.GameManager.ShowHintMessage(DynamicObject.LockedText, 3f);
                }
            }
            else if (StatusChange == DynamicObject.StatusChange.CustomScript && DynamicObject.UnlockScript != null)
            {
                IDynamicUnlock dynamicUnlock = (IDynamicUnlock)DynamicObject.UnlockScript;
                dynamicUnlock.OnTryUnlock(DynamicObject);
            }
            else
            {
                DynamicObject.LockedEvent?.Invoke();
                DynamicObject.PlaySound(DynamicSoundType.Locked);
                OnDynamicLocked();

                if (DynamicObject.ShowLockedText)
                    DynamicObject.GameManager.ShowHintMessage(DynamicObject.LockedText, 3f);
            }
        }

        /// <summary>
        /// This method collects the data that is to be saved.
        /// </summary>
        public abstract StorableCollection OnSave();

        /// <summary>
        /// This method is called when the loading process is executed.
        /// </summary>
        public abstract void OnLoad(JToken token);
    }
}