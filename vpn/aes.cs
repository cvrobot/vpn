namespace Aes
{
  class AesHelp
  {
    /// <summary>  
    /// AES加密算法  
    /// </summary>  
    /// <param name="input">明文字符串</param>  
    /// <param name="key">密钥</param>  
    /// <returns>字符串</returns>  
    public static string EncryptByAES(string input, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32));
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = AES_IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(input);
                    }
                    byte[] bytes = msEncrypt.ToArray();
                    //return Convert.ToBase64String(bytes);//此方法不可用
                    return BitConverter.ToString(bytes);
                }
            }
        }
    }
    /// <summary>  
    /// AES解密  
    /// </summary>  
    /// <param name="input">密文字节数组</param>  
    /// <param name="key">密钥</param>  
    /// <returns>返回解密后的字符串</returns>  
    public static string DecryptByAES(string input, string key)
    {
        //byte[] inputBytes = Convert.FromBase64String(input); //Encoding.UTF8.GetBytes(input);
        string[] sInput = input.Split("-".ToCharArray());
        byte[] inputBytes = new byte[sInput.Length];
        for (int i = 0; i < sInput.Length; i++)
        {
            inputBytes[i] = byte.Parse(sInput[i], NumberStyles.HexNumber);
        }
        byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32)); 
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = AES_IV;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream(inputBytes))
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srEncrypt = new StreamReader(csEncrypt))
                    {
                        return srEncrypt.ReadToEnd();
                    }
                }
            }
        }           
    }

  }
}
