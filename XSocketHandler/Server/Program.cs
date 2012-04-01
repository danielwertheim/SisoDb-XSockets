using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
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
