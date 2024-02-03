using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public interface ISaveableCustom
    {
        StorableCollection OnCustomSave();
        void OnCustomLoad(JToken data);
    }
}