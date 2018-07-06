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

using Cloo;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IKMINER.OpenCL
{
    public class AcceleratorDevice
    {
        public static AcceleratorDevice[] All = ComputePlatform.Platforms.Select(new Func<ComputePlatform, ReadOnlyCollection<ComputeDevice>>(AcceleratorDevice._All_m__0)).SelectMany(new Func<ReadOnlyCollection<ComputeDevice>, IEnumerable<ComputeDevice>>(AcceleratorDevice._All_m__1)).Select(new Func<ComputeDevice, AcceleratorDevice>(AcceleratorDevice._All_m__2)).ToArray<AcceleratorDevice>();

        public string Name
        {
            get;
            private set;
        }

        public string Vendor
        {
            get;
            private set;
        }

        public ComputeDevice Device
        {
            get;
            private set;
        }

        public ComputeDeviceTypes Type
        {
            get;
            private set;
        }

        public AcceleratorDevice(ComputeDevice Device)
        {
            this.Device = Device;
            this.Name = Device.Name;
            this.Vendor = Device.Vendor;
            this.Type = Device.Type;
        }

        public override string ToString()
        {
            return this.Name;
        }

        private static ReadOnlyCollection<ComputeDevice> _All_m__0(ComputePlatform x)
        {
            return x.Devices;
        }

        private static IEnumerable<ComputeDevice> _All_m__1(ReadOnlyCollection<ComputeDevice> i)
        {
            return i;
        }

        private static AcceleratorDevice _All_m__2(ComputeDevice x)
        {
            return new AcceleratorDevice(x);
        }
    }
}
