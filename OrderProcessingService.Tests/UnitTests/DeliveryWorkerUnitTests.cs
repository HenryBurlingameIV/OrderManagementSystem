using Hangfire;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Infrastructure.BackgroundWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class DeliveryWorkerUnitTests
    {
        private readonly Mock<IProcessingOrderRepository> _mockRepository;
        private readonly Mock<IBackgroundJobClient> _mockBackgroundJobClient;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<DeliveryWorker>> _mockLogger;
        private readonly DeliveryWorker _deliveryWorker;

        public DeliveryWorkerUnitTests()
        {
            _mockRepository = new Mock<IProcessingOrderRepository>();
            _mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<DeliveryWorker>>();
            _deliveryWorker = new DeliveryWorker(
                _mockBackgroundJobClient.Object,
                _mockRepository.Object,
                _mockOrderServiceApi.Object,
                _mockLogger.Object
                );
        }
    }
}
