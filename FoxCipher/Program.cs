using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoxCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.Title = "FoxCrypt";

            while (true)
            {
                Console.Write("Message: ");
                string messagePlain = Console.ReadLine();
                Console.Write("Password: ");
                string password = Console.ReadLine();


                CryptoManager fox = new CryptoManager(password);
                Console.WriteLine($"Encrypting: '{messagePlain}'\n");

                stopwatch.Reset();
                stopwatch.Start();

                string msg = fox.EncryptToMessage(messagePlain);
                string decrypted = fox.DecryptFromMessage(msg);

                stopwatch.Stop();

                Console.WriteLine(msg);
                Console.WriteLine($"Decrypted: '{fox.DecryptFromMessage(msg)}' in {stopwatch.ElapsedMilliseconds}Ms");
                Console.Write("Press any key to continue...");
                Console.ReadKey();

                Console.Clear();
            }
        }
    }
}
