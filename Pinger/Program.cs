using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Pingerj
{
    /*
Pinger

- Given a router ip and an external server
- Given a ‘success’ filename and a ‘failure’ filename

Setup a timer. On tick: 
	- Ping the router and log success/failure
	- Ping the external server and log success/failure

     */
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Option("--router")]
        [Required]
        public string RouterIP{ get; }

        [Option("--external-server")]
        [Required]
        public string ExternalServer { get; }

        [Option("--success-file")]
        [Required]
        public string SuccessFile { get; }

        [Option("--failure-file")]
        [Required]
        public string FailureFile { get; }

        public async Task OnExecute()
        {
            while(true)
            {
                Ping(RouterIP);
                Ping(ExternalServer);

                await Task.Delay(3000);
            }
        }

        void Ping(string address)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            PingReply reply = pingSender.Send(address, timeout, buffer, options);
            LogReply(address, reply);
        }

        void LogReply(string address, PingReply reply)
        {
            var result = new PingResult
            {
                Address = address,
                Timestamp = DateTime.Now,
                RoundtripTime = reply.RoundtripTime,
                Status = reply.Status.ToString(),
                Success = reply.Status == IPStatus.Success
            };

            string filename = result.Success ? SuccessFile : FailureFile;
            string logMessage = JsonSerializer.Serialize(result);
            File.AppendAllLines(filename, new[] { logMessage });
        }
    }

    class PingResult
    {
        public bool Success { get; set; }
        public string Address { get; set; }
        public DateTime Timestamp { get; set; }
        public long RoundtripTime { get; internal set; }
        public string Status { get; internal set; }
    }
}
