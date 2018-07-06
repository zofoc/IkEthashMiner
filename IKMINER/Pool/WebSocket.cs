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
using System.Net;
using System.Text;
using System.Threading;

using IKMINER.Json;
using IKMINER.Ethash;
using IKMINER.Logging;
using Newtonsoft.Json;

namespace IKMINER.Pool
{
    public class WebSocket
    {
        protected int GlobalId;
        protected readonly Uri Uri;
        protected readonly WebClient WebClient;
        protected Logger Logger = new Logger();

        public EventHandler OnShareAccepted;
        public EventHandler OnShareRejected;

        public WebSocket(string connectionString)
        {
            Logger.INFO("{0}", connectionString);
            this.Uri = new Uri(connectionString);
            this.WebClient = new WebClient();
            this.WebClient.Headers.Add("Content-Type", "application/json");
        }

        public string JobId = "0";
        public BlockHeader GetWork(BlockHeader previous)
        {

            string json = this.Rpc("eth_getWork", null);
            GetWorkReply getWorkResponse = JsonConvert.DeserializeObject<GetWorkReply>(json);
            if (getWorkResponse == null)
            {
                Logger.ERROR("eth_getWork returned null.");
                return null;
            }

            string header = getWorkResponse.result[0].ToString();
            string seed = getWorkResponse.result[1].ToString();
            string boundry = getWorkResponse.result[2].ToString();

            if (previous.HeaderString.Equals(header) && previous.SeedString.Equals(seed) && previous.BoundaryString.Equals(boundry))
                return null;

            string id = (Math.Abs(header.GetHashCode() + seed.GetHashCode() + boundry.GetHashCode())).ToString("X2").ToLower();

            if(JobId != id)
            {
                JobId = id.ToLower();
                Logger.POOL("Current pool job: #{0}", JobId);
            }
            
            return new BlockHeader(header, seed, boundry);
        }

        public bool SubmitWork(LightDag light, ulong[] results, byte[] headerbytes)
        {
            bool result = true;
            int num = 0;
            while ((long)num < results.LongLength)
            {
                if (results[num] > 0uL)
                {
                    byte[] mixHash = light.GetMixHash(headerbytes, results[num]);
                    string text = "0x" + results[num].ToString("x2").PadLeft(16, '0');
                    string text2 = "0x" + BlockHeader.ToHex(headerbytes);
                    string text3 = "0x" + BlockHeader.ToHex(mixHash);
                    try
                    {
                        string json = this.Rpc("eth_submitWork", text, text2, text3);
                        SubmitWorkReply submitWorkResponse = JsonConvert.DeserializeObject<SubmitWorkReply>(json);
                        if (submitWorkResponse.result)
                        {
                            OnShareAccepted?.Invoke(null, null);
                        }
                        else
                        {
                            OnShareRejected?.Invoke(null, null);
                            result = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ERROR(ex.Message);
                        result = false;
                    }
                }
                num++;
            }
            return result;
        }

        public string Rpc(string methodName, params string[] parameters)
        {
            Request obj = new Request(Interlocked.Increment(ref this.GlobalId), methodName, parameters);
            string text = JsonConvert.SerializeObject(obj);
            text = text.Replace("parameters", "params");
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            string result;

            try
            {
                object webClient = this.WebClient;
                byte[] bytes2;

                lock (webClient)
                {
                    bytes2 = this.WebClient.UploadData(this.Uri, "POST", bytes);
                }

                result = Encoding.UTF8.GetString(bytes2);
            }
            catch (Exception ex)
            {
                Logger.ERROR(ex.Message);
                result = null;
            }

            return result;
        }
    }
}
