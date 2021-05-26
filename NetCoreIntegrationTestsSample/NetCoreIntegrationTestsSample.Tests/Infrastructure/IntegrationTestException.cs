using System;

namespace NetCoreIntegrationTestsSample.Tests.Infrastructure
{
    public class IntegrationTestException : Exception
    {
        public IntegrationTestException(string message) : base(message)
        {
            
        }
    }
}