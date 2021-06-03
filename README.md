# .NET Core Integration Tests

The project presents how to prepare integration tests in .net core environment. The tests covers all the most common parts of modern applications:
1. Handling requests with REST Api
2. Sending messages with RabbitMQ queue
3. Storing data with PostgreSQL database
4. Calling External REST Api

The repository contains a ready-to-use solution with pre-configured docker files.

The goal of integration tests should always be to test the application with its external dependencies, as broadly as possible. The less mocks you have to use the better and more robust your tests are.

![IntegrationTests-Page-2 (1)](https://user-images.githubusercontent.com/17302468/120616356-1219ae00-c451-11eb-8a5f-c18351e4ec24.png)
