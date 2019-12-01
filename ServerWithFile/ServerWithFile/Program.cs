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
            CreateSocketsAndCheckFiles createSockets = new CreateSocketsAndCheckFiles();
            createSockets.Start(2);

        }
    }
}
