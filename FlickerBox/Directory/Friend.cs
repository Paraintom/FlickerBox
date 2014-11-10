using Newtonsoft.Json;

namespace FlickerBox.Directory
{
    public class Friend
    {
        public string Name { get; set; }
        [JsonIgnore]
        public string Passphrase { get; set; }
        [JsonIgnore]
        public string PublicId { get; set; }

        public override string ToString()
        {
            return string.Format("[Name:{0},Passphrase:{1},PublicId:{2}]", Name, Passphrase, PublicId);
        }
    }
}
