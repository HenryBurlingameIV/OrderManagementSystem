using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Application.Services;
using OrderProcessingService.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Tests.UnitTests
{
    public class OrderProcessorUnitTests
    {
        private readonly Mock<IProcessingOrderRepository> _mockRepository;
        private readonly Mock<IOrderBackgroundWorker<StartAssemblyCommand>> _mockAssemblyWorker;
        private readonly Mock<IOrderServiceApi> _mockOrderServiceApi;
        private readonly Mock<ILogger<OrderProcessor>> _mockLogger;
        private readonly Mock<IOrderBackgroundWorker<StartDeliveryCommand>> _mockDeliveryWorker;
        private readonly StartAssemblyValidator _assemblyStatusValidator;
        private readonly StartDeliveryValidator _deliveryStatusValidator;
        private readonly OrderProcessor _orderProcessor;

        public OrderProcessorUnitTests() 
        {
            _mockRepository = new Mock<IProcessingOrderRepository>();
            _mockAssemblyWorker = new Mock<IOrderBackgroundWorker<StartAssemblyCommand>>();
            _mockOrderServiceApi = new Mock<IOrderServiceApi>();
            _mockLogger = new Mock<ILogger<OrderProcessor>>();
            _mockDeliveryWorker = new Mock<IOrderBackgroundWorker<StartDeliveryCommand>>();
            _assemblyStatusValidator = new StartAssemblyValidator();
            _deliveryStatusValidator = new StartDeliveryValidator();
            _orderProcessor = new OrderProcessor(
                _mockRepository.Object,
                _mockAssemblyWorker.Object,
                _mockDeliveryWorker.Object,
                _mockOrderServiceApi.Object,
                _mockLogger.Object,
                _assemblyStatusValidator,
                _deliveryStatusValidator
                );
        }
    }
}
