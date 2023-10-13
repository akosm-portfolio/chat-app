using System;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    class EncryptPassword
    {
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            //string -> byte
            byte[] plainTextByte = Encoding.UTF8.GetBytes(plainText);
            byte[] saltByte = Encoding.UTF8.GetBytes(salt);

            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainTextByte.Length + saltByte.Length];

            for (int i = 0; i < plainTextByte.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainTextByte[i];
            }
            for (int i = 0; i < saltByte.Length; i++)
            {
                plainTextWithSaltBytes[plainTextByte.Length + i] = saltByte[i];
            }
            return Convert.ToBase64String(algorithm.ComputeHash(plainTextWithSaltBytes));
        }

        public static string GenerateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] salt = new byte[size];
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
    }
}