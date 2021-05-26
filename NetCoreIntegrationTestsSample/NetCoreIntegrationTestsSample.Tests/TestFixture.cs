using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreIntegrationTestsSample.Infrastructure;
using NetCoreIntegrationTestsSample.Tests.Infrastructure;
using NetCoreIntegrationTestsSample.Tests.TestCases.Expenses;
using Newtonsoft.Json;
using Polly;
using Xunit.Abstractions;

using V1 = NetCoreIntegrationTestsSample.Expenses.Contracts.Expenses.V1;

namespace NetCoreIntegrationTestsSample.Tests
{
  public abstract class TestFixture : IDisposable
    {
        public IHost Host { get; set; }
        public string HostAddress;
        public RabbitMqQueue _rabbitMqQueue;
        public string _integrationTestsExpenseQueue = "test-expense-queue";
        public ExchangeServiceApi ExchangeServiceApi;
        
        protected TestFixture(HostFixture hostFixture, ITestOutputHelper output)
        {
            Host = hostFixture.Host;
            HostAddress = hostFixture.HostAddress;
            
            SetupDatabase(hostFixture.AppConfiguration);
            
            var rabbitMqConfig = hostFixture.AppConfiguration
                .GetSection(ConfigKeys.RabbitMq)
                .Get<RabbitMqConfig>();
            
            _rabbitMqQueue = new RabbitMqQueue(
                rabbitMqConfig.HostUrl, 
                rabbitMqConfig.User, 
                rabbitMqConfig.Password);

            _rabbitMqQueue.ConfigureListeningQueue(
                _integrationTestsExpenseQueue, 
                typeof(V1.Messages.ExpenseCreatedEvent));
            
            ExchangeServiceApi = new ExchangeServiceApi(hostFixture.AppConfiguration
                .GetSection(ConfigKeys.ExchangeService)
                .Get<ExchangeServiceConfig>());
        }

        private void SetupDatabase(IConfiguration config)
        {
            var dbConfig = config.GetSection(ConfigKeys.Database).Get<DatabaseConfig>();

            DatabaseCleanup.ClearDatabase(dbConfig.ConnectionString);
        }
        
        public async Task post_to_sut_api<TRequest>(string path, TRequest request, Action<TRequest> requestModifier = null)
        {
            requestModifier?.Invoke(request);

            var response = await HostAddress
                .AppendPathSegment(path)
                .AllowAnyHttpStatus()
                .PostJsonAsync(request);

            if (!response.HasOneOfStatuses(HttpStatusCode.OK))
            {
                var error = await response.Content.ToJson<ErrorDetails>();
                throw new TestApiCallErrorException(error);
            }
        }

        public async Task<TResponse> post_to_sut_api_with_response<TRequest, TResponse>(string path, TRequest request, Action<TRequest> requestModifier = null)
        {
            requestModifier?.Invoke(request);

            var response = await HostAddress
                .AppendPathSegment(path)
                .AllowAnyHttpStatus()
                .PostJsonAsync(request);
            
            if (!response.HasOneOfStatuses(HttpStatusCode.OK))
            {
                var error = await response.Content.ToJson<ErrorDetails>();
                throw new TestApiCallErrorException(error);
            }
      
            return await response.Content.ToJson<TResponse>();
        }
        
        public async Task wait_for_api_response<TResponse>(
        			Func<Task<TResponse>> apiCallFunc,
        			Action<TResponse> assertions)
        {
        	await wait_for_passed_condition_or_throw_after_timeout(
        		async () =>
        		{
        			try
        			{
        				var apiResult = await apiCallFunc();
        				assertions(apiResult);
        				return true;
        			}
        			catch (Exception e)
        			{
        				return false;
        			}
        		},
        		checkingIntervalInMilliseconds: 500,
        		retryCount: 15,
        		conditionNotMetErrorMessage: "API results dont meet the assertions.");
        }

        protected async Task assert_message_in_queue<TMessage>(Action<TMessage> assertions)
            where TMessage : class
        {
            Func<TMessage, bool> condition = message =>
            {
                try
                {
                    assertions(message);

                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            };

            await wait_for_message_in_queue(_integrationTestsExpenseQueue, condition);
        }
        
        protected async Task<TMessage> wait_for_message_in_queue<TMessage>(
            string queueName, Func<TMessage, bool> condition) where TMessage : class
        {
            TMessage expectedMessage = null;

            await wait_for_passed_condition_or_throw_after_timeout(() =>
                {
                    var rawMessage = _rabbitMqQueue.ReadRawMessageFromQueue(queueName);
                    
                    if (rawMessage == null)
                        return false;

                    expectedMessage = JsonConvert.DeserializeObject<MassTransitDefaultMessage<TMessage>>(rawMessage).Message;

                    return condition(expectedMessage);
                },
                checkingIntervalInMilliseconds: 1000 ,
                retryCount: 100,
                conditionNotMetErrorMessage: $"Condition failed: message that meets the requirements has not been found in Queue: {queueName}");

            return expectedMessage;
        }
        
        protected async Task wait_for_passed_condition_or_throw_after_timeout(
            Func<Task<bool>> checkingAction,
            int checkingIntervalInMilliseconds,
            int retryCount,
            string conditionNotMetErrorMessage)
        {
            var conditionFulfilled = await Policy.HandleResult<bool>(result => result == false)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromMilliseconds(checkingIntervalInMilliseconds))
                .ExecuteAsync(checkingAction);

            if(!conditionFulfilled)
                throw new IntegrationTestException(conditionNotMetErrorMessage);
        }

        protected async Task wait_for_passed_condition_or_throw_after_timeout(
            Func<bool> checkingAction,
            int checkingIntervalInMilliseconds,
            int retryCount,
            string conditionNotMetErrorMessage)
        {
            var conditionFulfilled = Policy.HandleResult<bool>(result => result == false)
                .WaitAndRetry(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromMilliseconds(checkingIntervalInMilliseconds))
                .Execute(checkingAction);

            if(!conditionFulfilled)
                throw new IntegrationTestException(conditionNotMetErrorMessage);
        }
        
        public virtual void Dispose()
        {
            ExchangeServiceApi.Dispose();
            Host?.Dispose();
        }
    }
  
  public static class HttpResponseMessageExtensions
  {
      public static bool HasOneOfStatuses(this HttpResponseMessage response, params HttpStatusCode[] statuses)
      {
          return statuses.ToList().Contains(response.StatusCode);
      }
        
      public static async Task<TModel> ToJson<TModel>(this HttpContent content)
      {
          var responseString = await content.ReadAsStringAsync();
          return JsonConvert.DeserializeObject<TModel>(responseString);
      }
  }
  
  public static class CollectionExtensions
  {
      public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
      {
          foreach (var item in items) action(item);
      }
  }
  
  class MassTransitDefaultMessage<TMessage>
  {
      public TMessage Message { get; set; }
  }
}