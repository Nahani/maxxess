using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DB
{
    class Program
    {
        static AccesBD_SQL access = AccesBD_SQL.Instance;
        static void Main(string[] args)
        {

            Client c = access.getClientById("001");
            Console.WriteLine(c.Adresse1);

            c = access.getClientByName("Polo");
            Console.WriteLine(c.Adresse1);

        }

    }
}