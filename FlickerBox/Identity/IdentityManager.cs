using System;
using System.IO;
using System.Linq;
using System.Text;
using FlickerBox.Encryption;
using NLog;

namespace FlickerBox.Identity
{
    public class IdentityManager : IIdentityManager
    {
        public string PublicId { get; private set; }
        public string PrivateId { get; private set; }
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private const int IdSize = 39;
        public const string FileName = "BoxIdentity.txt";

        public IdentityManager()
        {
            if (File.Exists(FileName))
            {
                log.Info("The file exist, opening it ...");
                string persistedId = File.ReadLines(FileName).FirstOrDefault();
                if(String.IsNullOrEmpty(persistedId) || persistedId.Length != IdSize)
                    throw new ApplicationException("We found a id in a file witch is not valid : "+persistedId);
                SetPrivateID(persistedId);
            }
            else
            {
                log.Info("The file does not exist, creating it!");
                string newId = RandomString(IdSize);
                File.AppendAllLines(FileName,new[]{newId});
                SetPrivateID(newId);
            }
            log.Info("PublicId : {0}", PublicId);
        }

        private void SetPrivateID(string persistedId)
        {
            PrivateId = persistedId;
            PublicId = persistedId.Encrypt();
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}
