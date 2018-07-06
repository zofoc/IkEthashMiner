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
using System.Globalization;

namespace IKMINER.Logging
{
    public class Logger
    {
        public void INFO(string message, params object[] @params)
        {
            Write("INFO", ConsoleColor.White, message, @params);
        }

        public void POOL(string message, params object[] @params)
        {
            Write("POOL", ConsoleColor.Cyan, message, @params);
        }

        public void TRACE(string message, params object[] @params)
        {
            Write("TRACE", ConsoleColor.Gray, message, @params);
        }

        public void WARN(string message, params object[] @params)
        {
            Write("WARNING", ConsoleColor.Yellow, message, @params);
        }

        public void ERROR(string message, params object[] @params)
        {
            Write("ERROR", ConsoleColor.Red, message, @params);
        }

        public void SUCCESS(string message, params object[] @params)
        {
            Write("SUCCESS", ConsoleColor.Green, message, @params);
        }

        public void HASHRATE(string message, params object[] @params)
        {
            Write("INFO", ConsoleColor.DarkCyan, message, @params);
        }

        private void Write(string id, ConsoleColor color, string message, params object[] @params)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("[{0}] [{1}] {2}", id, DateTime.Now.ToString(CultureInfo.InvariantCulture), string.Format(message, @params));
            Console.ResetColor();
        }
    }
}
