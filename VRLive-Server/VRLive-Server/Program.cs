using MagicOnion.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting.Server;

namespace VRLive_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://*:80", "http://hostname:12345", "http://hostname:5000");
                webBuilder.UseStartup<Startup>();
            });
    }
}