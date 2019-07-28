using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FoxCipher
{
    public class CryptoManager
    {
        private string Password { get; set; }
        private byte[] LinkedListKey { get; set; }

        public CryptoManager(string password)
        {
            Password = password;
        }

        public byte[] Encrypt(string data) => EncryptRaw(data);
        public string Decrypt(byte[] data) => DecryptRaw(data);
        private string Sha265(byte[] data) => BitConverter.ToString(new SHA256Managed().ComputeHash(data)).Replace("-", "");
        private byte[] StringToByteArray(string hex) => Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();

        public string EncryptToMessage(string data)
        {
            byte[] protectedData = EncryptRaw(data);

            StringBuilder messageBuilder = new StringBuilder();
            char[] hexChars = (BitConverter.ToString(protectedData).Replace("-", "")).ToCharArray();

            messageBuilder.AppendLine("-----BEGIN FOX MESSAGE-----");
            messageBuilder.AppendLine("Version: 2.4");
            messageBuilder.AppendLine("Faux Bytes: Version 2.1");
            messageBuilder.AppendLine("Checksum: " + Sha265(protectedData));

            for (int i = 0; i < hexChars.Length; i += 64)
                messageBuilder.AppendLine(new string(hexChars.Skip(i).Take(Math.Min(64, hexChars.Length - i)).ToArray()));

            messageBuilder.AppendLine("-----END FOX MESSAGE-----");

            return messageBuilder.ToString();
        }

        public string DecryptFromMessage(string message)
        {
            string[] lines = message.Split('\n');
            byte[] data = StringToByteArray(string.Join("", lines.Skip(4).Take(lines.Length - 6)).Replace("\r", ""));

            if (Sha265(data) != lines[3].Split(':')[1].TrimStart().Replace("\r", ""))
                throw new InvalidOperationException("Data corrupt");

            return DecryptRaw(data);
        }

        private byte[] EncryptRaw(string data)
        {
            ChunkManager manager = new ChunkManager(new PasswordDeriveBytes(Password, System.Text.Encoding.UTF8.GetBytes("F0x")).GetBytes(128));
            return manager.Encrypt(Encoding.UTF8.GetBytes(data));
        }

        private string DecryptRaw(byte[] data)
        {
            ChunkManager manager = new ChunkManager(new PasswordDeriveBytes(Password, System.Text.Encoding.UTF8.GetBytes("F0x")).GetBytes(128));
            return Encoding.UTF8.GetString(manager.Decrypt(data));
        }
    }
}