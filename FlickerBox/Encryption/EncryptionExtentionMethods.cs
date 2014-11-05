using System;
using System.Security.Cryptography;
using System.Text;

namespace FlickerBox.Encryption
{
    public static class EncryptionExtentionMethods
    {
        static public string Encrypt(this string toEncode)
        {
            string encoded;
            using (SHA256 shaM = new SHA256Managed())
            {
                StringBuilder Sb = new StringBuilder();
                Encoding enc = Encoding.UTF8;

                var hashedBytes = shaM.ComputeHash(enc.GetBytes(toEncode));
                foreach (Byte b in hashedBytes)
                    Sb.Append(b.ToString("x2"));
                encoded = Sb.ToString();
            }
            return encoded;
        }
    }
}
