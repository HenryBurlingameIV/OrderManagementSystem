using FluentValidation;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderManagementSystem.Shared.Exceptions;
using OrderProcessingService.Application.Contracts;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Services
{
    public class ProcessingOrderQueryService : IProcessingOrderQueryService
    {
        private readonly IEFRepository<ProcessingOrder, Guid> _repository;
        private readonly ILogger<ProcessingOrderQueryService> _logger;
        private readonly IValidator<GetPaginatedProcessingOrdersRequest> _paginationValidator;

        public ProcessingOrderQueryService(
            IEFRepository<ProcessingOrder, Guid> repository,
            IValidator<GetPaginatedProcessingOrdersRequest> paginationValidator,
            ILogger<ProcessingOrderQueryService> logger)
        {
            _repository = repository;
            _logger = logger;
            _paginationValidator = paginationValidator;
        }
        public async  Task<ProcessingOrderViewModel> GetProcesingOrderById(Guid id, CancellationToken ct)
        {
            var result = await _repository.GetFirstOrDefaultAsync<ProcessingOrderViewModel>(
                selector: (po) => po.ToViewModel(),
                filter: (po) => po.Id == id,
                asNoTraсking: true,
                ct: ct);

            if(result is null)
            {
                throw new NotFoundException($"ProcessingOrder with ID {id} not found");
            }

            _logger.LogInformation("ProcessingOrder with ID {@ProcessingOrderId} successfully found", id);

            return result;
        }

        public async Task<PaginatedResult<ProcessingOrderViewModel>> GetProcessingOrdersPaginatedAsync(GetPaginatedProcessingOrdersRequest query, CancellationToken ct)
        {
            await _paginationValidator.ValidateAndThrowAsync(query);
            var paginationRequest = new PaginationRequest()
            {
                PageSize = query.PageSize,
                PageNumber = query.PageNumber,
            };
            Expression<Func<ProcessingOrder, bool>> filter = null;

            filter = (po) =>
                    (!query.Stage.HasValue || po.Stage == query.Stage.Value) &&
                     (!query.Status.HasValue || po.Status == query.Status);


            return await _repository.GetPaginated<ProcessingOrderViewModel>(
                request: paginationRequest,
                selector: (po) => po.ToViewModel(),
                filter: filter,
                orderBy: (q) => q.OrderByDescending(po => po.CreatedAt));
        }
    }
}
