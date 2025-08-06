using Castle.Core.Logging;
using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Infrastructure.BackgroundWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class AssemblyWorkerUnitTests
    {
        private readonly Mock<IProcessingOrderRepository> _mockRepository;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
        private readonly Mock<IOrderServiceApi> _mockOrderServoceApi;
        private readonly Mock<ILogger<AssemblyWorker>> _mockLogger;
        private readonly IOrderBackgroundWorker<StartAssemblyCommand> _asseblyWorker;

        public AssemblyWorkerUnitTests() 
        {
            _mockRepository = new Mock<IProcessingOrderRepository>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _mockOrderServoceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<AssemblyWorker>>();
            _asseblyWorker = new AssemblyWorker(
                _mockBackgroundJobClient.Object,
                _mockRepository.Object,
                _mockOrderServoceApi.Object,
                _mockLogger.Object
                );
        }
    }
}
