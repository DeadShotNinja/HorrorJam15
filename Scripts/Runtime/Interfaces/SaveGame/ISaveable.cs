using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public interface ISaveable
    {
        StorableCollection OnSave();
        void OnLoad(JToken data);
    }

    public interface IRuntimeSaveable : ISaveable
    {
        UniqueID UniqueID { get; set; }
    }
}