using System.Security.Cryptography;
using System.Text;

namespace API.Models.AppConfig
{
    public class Security
    {
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public static bool VerifyHashedPassword(string? hashedPassword, string? password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }
        private static bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
        public static string DecryptText(string sEncryptText)
        {
            if (sEncryptText == "")
            {
                return sEncryptText;
            }
            string strEncrKey = "&*hTp~x4";
            byte[] IV = new byte[8] { 33, 48, 22, 120, 144, 171, 205, 225 };
            byte[] array = new byte[0];
            byte[] array2 = new byte[strEncrKey.Length];
            MemoryStream memoryStream = new MemoryStream();
            array = Encoding.UTF8.GetBytes(strEncrKey);
            DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
            array2 = Convert.FromBase64String(sEncryptText);
            try
            {
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateDecryptor(array, IV), CryptoStreamMode.Write);
                cryptoStream.Write(array2, 0, array2.Length);
                cryptoStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static string EncryptText(string sText)
        {
            if (sText == "")
            {
                return sText;
            }

            byte[] array = new byte[0];
            MemoryStream memoryStream = new MemoryStream();
            DESCryptoServiceProvider dESCryptoServiceProvider = new DESCryptoServiceProvider();
            try
            {
                string strEncrKey = "&*hTp~x4";
                byte[] IV = new byte[8] { 33, 48, 22, 120, 144, 171, 205, 225 };
                array = Encoding.UTF8.GetBytes(strEncrKey);
                byte[] bytes = Encoding.UTF8.GetBytes(sText);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dESCryptoServiceProvider.CreateEncryptor(array, IV), CryptoStreamMode.Write);
                cryptoStream.Write(bytes, 0, bytes.Length);
                cryptoStream.FlushFinalBlock();
                cryptoStream.Dispose();
                return Convert.ToBase64String(memoryStream.ToArray());
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

    }
}
