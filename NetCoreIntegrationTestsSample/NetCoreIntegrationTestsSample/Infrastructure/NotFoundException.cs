using System;

namespace NetCoreIntegrationTestsSample.Infrastructure
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
            
        }

        public NotFoundException(string message): base(message)
        {
            
        }
    }
}