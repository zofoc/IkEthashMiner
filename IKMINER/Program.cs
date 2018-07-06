/*  Iker Ruiz Arnauda 2015
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<https://www.gnu.org/licenses/>.
*/

using System;
using IKMINER.Workers;
using System.Threading;
using System.Reflection;

namespace IKMINER
{
    class Program
    {
        private static Thread _T;
        private static Miner Miner;
        static void Main(string[] args)
        {
            Console.WriteLine("IKMINER {0}", Assembly.GetExecutingAssembly().GetName().Version);

            _T = new Thread(LaunchWorker);
            _T.Start();

            Console.ReadLine();
            Miner.Dispose();

            _T.Join(4000);
            _T.Interrupt();
        }

        private static void LaunchWorker()
        {
            Miner = new Miner("http://us.ubiqpool.io:8888/0xd223a550b0d19bf655fe6068aaa150538f34c449/Test");   
        }
    }
}
