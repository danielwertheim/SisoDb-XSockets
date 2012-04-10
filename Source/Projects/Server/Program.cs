using System;
using Core;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Runtime.Resources = new Resources();

            try
            {
                new Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);
                Console.ReadLine();
            }
        }
    }
}
