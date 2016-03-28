using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapIntegratedService.Database
{
    /// <summary>
    /// © 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved.
    /// This class handles the encryption routines
    /// </summary>
    public static class MM_Encryption
    {
        /// <summary>
        /// Decrypt the specified encrypted text
        /// </summary>
        /// <param name="EncryptedText">The encrypted text</param>
        /// <returns></returns>
        public static String Decrypt(String EncryptedText)
        {
            //TODO: Update with initialization vector and key
            TripleDESCryptoServiceProvider TDes = new TripleDESCryptoServiceProvider();
            byte[] inBytes = Convert.FromBase64String(EncryptedText);
            return Encoding.UTF8.GetString(TDes.CreateDecryptor().TransformFinalBlock(inBytes, 0, inBytes.Length));

        }

        /// <summary>
        /// Encrypt the specified clear text
        /// </summary>
        /// <param name="ClearText">The decrypted text</param>
        /// <returns></returns>
        public static String Encrypt(String ClearText)
        {
            //TODO: Update with initialization vector and key
            TripleDESCryptoServiceProvider TDes = new TripleDESCryptoServiceProvider();
            byte[] EncryptedBytes = Encoding.UTF8.GetBytes(ClearText);
            return Convert.ToBase64String(TDes.CreateEncryptor().TransformFinalBlock(EncryptedBytes, 0, EncryptedBytes.Length));
        }
    }
}
