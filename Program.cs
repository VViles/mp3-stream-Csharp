using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MKFileStream
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hello world");
            Class1 c1 = new Class1();
            c1.filestr = "D:/k.mk";
            c1.somename = "test";
            c1.ulevel = 55;
            c1.plevel = 3;
            c1.ip = "192.168.1.100";
            c1.port = 6002;
            c1.seedlist.Add("SEED_CODE1");
            c1.seedlist.Add("SEED_CODE2");
            c1.run();
            Console.ReadLine();
        }
    }
}
