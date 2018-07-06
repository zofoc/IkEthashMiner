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

namespace IKMINER.Cryptography
{
    public class RoundConstants
    {
        public ulong this[int index]
        {
            get
            {
                return Constants[index];
            }
            set
            {
                Constants[index] = value;
            }
        }

        private readonly ulong[] Constants = new ulong[]
            {
                1uL,
                32898uL,
                9223372036854808714uL,
                9223372039002292224uL,
                32907uL,
                2147483649uL,
                9223372039002292353uL,
                9223372036854808585uL,
                138uL,
                136uL,
                2147516425uL,
                2147483658uL,
                2147516555uL,
                9223372036854775947uL,
                9223372036854808713uL,
                9223372036854808579uL,
                9223372036854808578uL,
                9223372036854775936uL,
                32778uL,
                9223372039002259466uL,
                9223372039002292353uL,
                9223372036854808704uL,
                2147483649uL,
                9223372039002292232uL
            };
    }
}
