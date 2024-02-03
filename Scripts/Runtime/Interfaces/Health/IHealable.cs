namespace HJ.Runtime
{
    public interface IHealable
    {
        /// <summary>
        /// Override this method to define custom behavior when an entity receives a health call.
        /// </summary>
        public void OnApplyHeal(int healAmount);

        /// <summary>
        /// Override this method to define custom behavior when applying maximum heal.
        /// </summary>
        public void ApplyHealMax();
    }
}
