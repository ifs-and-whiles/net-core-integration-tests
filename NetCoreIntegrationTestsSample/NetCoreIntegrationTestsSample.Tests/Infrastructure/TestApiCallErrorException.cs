using System;
using NetCoreIntegrationTestsSample.Infrastructure;
using Newtonsoft.Json;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    public class TestApiCallErrorException : Exception
    {
        public ErrorDetails ErrorDetails { get; }

        public TestApiCallErrorException(ErrorDetails error)
        {
            ErrorDetails = error;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(ErrorDetails);
        }
    }
}