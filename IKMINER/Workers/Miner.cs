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
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using Cloo;
using IKMINER.Pool;
using IKMINER.OpenCL;
using IKMINER.Ethash;
using IKMINER.Logging;
using System.Threading.Tasks;

namespace IKMINER.Workers
{
    public class Miner : IDisposable
    {
        private Logger Logger = new Logger();
        private int AcceptedShares;
        private int RejectedShares;
        public static string Speed = "00.00MH/s";
        public bool Running = true;
        private const uint DEFAULT_LOCAL_WORKSIZE = 128u;
        private const uint DEFAULT_GLOBAL_WORKSIZE_MULTIPLIER = 4096u;
        private const uint ETHASH_MIX_BYTES = 128u;
        public LightDag light;
        private int _workId;
        private bool GeneratingDag;
        private uint[] EmptyBuffer = new uint[64];
        private TimeSpan Refresh = TimeSpan.FromSeconds(5);
        private uint WorkGroupSize = 128u;
        private uint InitialGlobalWorkSize = 524288u;
        private uint GlobalWorkSize;
        private int MaxSearchResults = 63;
        private ComputeBuffer<byte> LightBuffer;
        private ComputeBuffer<byte> DagBuffer;
        private ComputeBuffer<byte> HeaderBuffer;
        private ComputeBuffer<uint> SearchBuffer;
        private ComputeContext ComputeContext;
        private ComputeCommandQueue ComputeQueue;
        private ComputeKernel Kernal;
        private AcceleratorDevice Device;
        private WebSocket PoolClient;
        private Timer FarmCheck;
        private BlockHeader PreviousWork = new BlockHeader();
        private static Func<AcceleratorDevice, bool> __f__am_cache0;
        private bool Stop = false;

        public Miner(string url)
        {
            this.PoolClient = new WebSocket(url);
            this.PoolClient.OnShareAccepted += ShareAccepted;
            this.PoolClient.OnShareRejected += ShareRejected;

            IEnumerable<AcceleratorDevice> arg_86_0 = AcceleratorDevice.All.Where(d => d.Device.ExecutionCapabilities == ComputeDeviceExecutionCapabilities.OpenCLKernel && !d.Vendor.Contains("NVIDIA")).ToArray();

            if (Miner.__f__am_cache0 == null)
                Miner.__f__am_cache0 = new Func<AcceleratorDevice, bool>(Miner._Miner_m__0);

            this.Device = arg_86_0.First(Miner.__f__am_cache0);
            this.ComputeContext = new ComputeContext(this.Device.Type, new ComputeContextPropertyList(this.Device.Device.Platform), null, IntPtr.Zero);
            this.ComputeQueue = new ComputeCommandQueue(this.ComputeContext, this.ComputeContext.Devices[0], ComputeCommandQueueFlags.None);
            this.FarmCheck = new Timer(new TimerCallback(this.WorkLoop), null, 300, 300);
        }

        private void ShareRejected(object sender, EventArgs e)
        {
            Logger.WARN("Rejected share :-(");
            RejectedShares++;
        }

        private void ShareAccepted(object sender, EventArgs e)
        {
            Logger.SUCCESS("Accepted share :-)");
            AcceptedShares++;
        }

        private async void WorkLoop(object state)
        {
            if (!this.Running || this.GeneratingDag)
                return;

            BlockHeader work = this.PoolClient.GetWork(this.PreviousWork);
            if (work != null)
            {
                ulong start_nonce = 1uL;
                while (!this.GeneratingDag)
                {
                    if (this.ComputeContext == null || work.SeedString != this.PreviousWork.SeedString)
                    {
                        this.GeneratingDag = true;
                        this.light = LightDag.GetLight(work.SeedString, work.Seedbytes);
                        this.InitializeMiner(this.light);
                        this.GeneratingDag = false;
                    }
 
                    this._workId++;
                    ulong upper64OfBoundary = Convert.ToUInt64(work.BoundaryString.Substring(2, 10), 16);
                    ulong[] array = await this.Search(work.HeaderHash, upper64OfBoundary, this._workId, start_nonce);

                    if (array == null)
                    {
                        this.PreviousWork = work;
                        return;
                    }

                    this.PoolClient.SubmitWork(this.light, array, work.HeaderHash);
                    start_nonce = array.Max<ulong>() + 1uL;
                }

                this.PreviousWork = work; //Check this
            }

            if(Stop)
                this.Running = false;
        }

        public void SetKernel(string OpenCLBody, string EntryPoint)
        {
            ComputeProgram computeProgram = new ComputeProgram(this.ComputeContext, OpenCLBody);
            try
            {
                computeProgram.Build(null, null, null, IntPtr.Zero);
                this.Kernal = computeProgram.CreateKernel(EntryPoint);
            }
            catch (BuildProgramFailureComputeException)
            {
                string buildLog = computeProgram.GetBuildLog(this.Device.Device);
                throw new ArgumentException(buildLog);
            }
        }

        private void InitializeMiner(LightDag light)
        {
            Logger.INFO("Allocating device memory....");
            this.GlobalWorkSize = this.InitialGlobalWorkSize;

            if (this.GlobalWorkSize % this.WorkGroupSize != 0u)
                this.GlobalWorkSize = (this.GlobalWorkSize / this.WorkGroupSize + 1u) * this.WorkGroupSize;
            
            ulong datasize = LightDag.GetDatasize(light.block_number);
            ulong dagSize = (ulong)((uint)(datasize / 128uL));
            ulong lightSize = (ulong)((uint)(light.cache_size / 64uL));
            Kernel.SetEthereumKernelParameters(this.WorkGroupSize, dagSize, lightSize, this.MaxSearchResults);
            this.LightBuffer = new ComputeBuffer<byte>(this.ComputeContext, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, light.cache);
            this.DagBuffer = new ComputeBuffer<byte>(this.ComputeContext, ComputeMemoryFlags.ReadOnly, (long)datasize);
            this.HeaderBuffer = new ComputeBuffer<byte>(this.ComputeContext, ComputeMemoryFlags.ReadOnly, 32L);
            this.SearchBuffer = new ComputeBuffer<uint>(this.ComputeContext, ComputeMemoryFlags.WriteOnly, (long)(this.MaxSearchResults + 1));
            this.ComputeQueue.WriteToBuffer<byte>(light.cache, this.LightBuffer, true, null);
            this.SetKernel(Kernel.EthereumKernel, "ethash_calculate_dag_item");

            Logger.INFO("DAG Generation: 0%");
            ulong num = datasize / 64uL;
            ulong num2 = num / (ulong)this.GlobalWorkSize;
            if (num % (ulong)this.GlobalWorkSize > 0uL)
                num2 += 1uL;

            this.Kernal.SetMemoryArgument(1, this.LightBuffer);
            this.ComputeQueue.WriteToBuffer<byte>(light.cache, this.LightBuffer, true, null);
            this.Kernal.SetMemoryArgument(2, this.DagBuffer);
            this.Kernal.SetValueArgument<int>(3, 0);

            uint num3 = 0u;
            while ((ulong)num3 < num2)
            {
                this.Kernal.SetValueArgument<uint>(0, num3 * this.GlobalWorkSize);
                this.ComputeQueue.Execute(this.Kernal, new long[1], new long[]
                {
                    (long)((ulong)this.GlobalWorkSize)
                }, null, null);
                this.ComputeQueue.Finish();               
                num3 += 1u;

                Logger.INFO("DAG Generation: {0}%", ((num3 * 100) / num2));
            }

            Logger.SUCCESS("DAG Generation Complete.");
            this.SetKernel(Kernel.EthereumKernel, "ethash_search");
            this.Kernal.SetMemoryArgument(2, this.DagBuffer);
            this.Kernal.SetValueArgument<uint>(5, 0u);
        }

        private Stopwatch stopwatch = new Stopwatch();
        private async Task<ulong[]> Search(byte[] headerHash, ulong upper64OfBoundary, int workPackage, ulong start_nonce)
        {
            var result = await Task.Run<ulong[]>(() =>
            {
                this.Kernal.SetMemoryArgument(0, this.SearchBuffer);
                this.Kernal.SetMemoryArgument(1, this.HeaderBuffer);
                this.ComputeQueue.WriteToBuffer<byte>(headerHash, this.HeaderBuffer, true, null);
                this.Kernal.SetValueArgument<ulong>(3, start_nonce);
                this.Kernal.SetValueArgument<ulong>(4, upper64OfBoundary);

                uint num = 0u;
                uint[] array = new uint[this.MaxSearchResults + 1];
                string text = "00.00";
                stopwatch.Start();
                while (true)
                {
                    this.Kernal.SetValueArgument<ulong>(3, start_nonce);
                    this.ComputeQueue.WriteToBuffer<uint>(this.EmptyBuffer, this.SearchBuffer, true, null);
                    this.ComputeQueue.Execute(this.Kernal, null, new long[]
                    {
                    (long)((ulong)this.GlobalWorkSize)
                    }, new long[]
                    {
                    (long)((ulong)this.WorkGroupSize)
                    }, null);
                    this.ComputeQueue.ReadFromBuffer<uint>(this.SearchBuffer, ref array, true, null);
                    this.ComputeQueue.Finish();
                    num += this.GlobalWorkSize;
                    if (stopwatch.Elapsed >= this.Refresh)
                    {
                        stopwatch.Stop();
                        string text2 = (num / stopwatch.Elapsed.TotalMilliseconds / 1000.0).ToString("##.##", CultureInfo.InvariantCulture);
                        Miner.Speed = text2;
                        num = 0u;
                        stopwatch.Reset();
                        stopwatch.Start();

                        text = text2;
                        Logger.HASHRATE("Speed: {0}MH/s Accepted: {1} Rejected: {2}]", text2, AcceptedShares, RejectedShares);
                    }
                    if (array[0] > 0u)
                    {
                        break;
                    }
                    if (this._workId != workPackage || !this.Running)
                    {
                        goto IL_22D;
                    }
                    start_nonce += (ulong)this.GlobalWorkSize;
                }
                ulong[] array2 = new ulong[this.MaxSearchResults];
                int num2 = 0;
                while ((long)num2 != Math.Min((long)((ulong)array[0]), (long)this.MaxSearchResults))
                {
                    array2[num2] = start_nonce + (ulong)array[num2 + 1];
                    num2++;
                }
                return array2;
                IL_22D:
                Miner.Speed = "00.00";
                return null;
            });

            return result;
        }

        private static bool _Miner_m__0(AcceleratorDevice x)
        {
            return x.Type == ComputeDeviceTypes.Gpu;
        }

        public void Dispose()
        {
            Stop = true;

            while (Running)
            {
                Console.WriteLine("Waiting for thread...");
                Thread.Sleep(200);
            }

            this.FarmCheck?.Dispose();
            this.FarmCheck = null;
            this.stopwatch?.Stop();
            this.DagBuffer?.Dispose();
            this.HeaderBuffer?.Dispose();
            this.Kernal?.Dispose();
            this.LightBuffer?.Dispose();
            this.ComputeQueue?.Dispose();
            this.SearchBuffer?.Dispose();
            this.ComputeContext?.Dispose();           
        }
    }
}
