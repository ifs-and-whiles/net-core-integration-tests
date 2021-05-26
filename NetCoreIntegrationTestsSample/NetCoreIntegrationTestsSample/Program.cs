using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreIntegrationTestsSample.Infrastructure;

namespace NetCoreIntegrationTestsSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = Configuration.Read();

            ApiHost.Run(config);
        }

    }
}