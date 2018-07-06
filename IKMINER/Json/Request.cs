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

namespace IKMINER.Json
{
    public class Request
    {
        public string jsonrpc { get; set; } = "2.0";
        public string method { get; set; }
        public string[] parameters { get; set; }
        public int id { get; set; }

        public Request(int id, string method, string[] parameters)
        {
            this.id = id;
            this.method = method;
            this.parameters = parameters;
            this.jsonrpc = "2.0";
        }
    }
}
