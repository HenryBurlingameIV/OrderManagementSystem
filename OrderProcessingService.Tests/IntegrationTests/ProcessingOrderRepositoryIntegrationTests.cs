using OrderProcessingService.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.IntegrationTests
{
    public class ProcessingOrderRepositoryIntegrationTests : IClassFixture<ProcessingOrderRepositoryFixture>, IAsyncLifetime
    {
        private readonly ProcessingOrderRepositoryFixture _fixture;

        public ProcessingOrderRepositoryIntegrationTests(ProcessingOrderRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDataBase();
        }
    }
}
