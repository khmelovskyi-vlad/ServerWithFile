using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateSockets createSockets = new CreateSockets();
            createSockets.Start(5);

        }
    }
}
