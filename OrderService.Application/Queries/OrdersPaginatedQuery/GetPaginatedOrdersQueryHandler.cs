using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagementSystem.Shared.Contracts;
using OrderManagementSystem.Shared.DataAccess.Pagination;
using OrderService.Application.DTO;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OrderService.Application.Queries.OrdersPaginatedQuery
{
    public class GetPaginatedOrdersQueryHandler(
        IEFRepository<Order, Guid> orderRepository,
        IValidator<GetPaginatedOrdersRequest> paginationValidator,
        ILogger<GetPaginatedOrdersQueryHandler> logger
        ) 
        : IRequestHandler<GetPaginatedOrdersQuery, PaginatedResult<OrderViewModel>>
    {
        public async Task<PaginatedResult<OrderViewModel>> Handle(GetPaginatedOrdersQuery query, CancellationToken cancellationToken)
        {
            await paginationValidator.ValidateAndThrowAsync(query.Request, cancellationToken);

            var pagination = new PaginationRequest()
            {
                PageSize = query.Request.PageSize,
                PageNumber = query.Request.PageNumber,
            };


            Expression<Func<Order, bool>>? filter = BuildFilter(query.Request.Search);

            Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = BuildOrderBy(query.Request.SortBy, query.Request.Descending);

            var result = await orderRepository.GetPaginated<OrderViewModel>(
                request: pagination,
                selector: (o) => o.ToViewModel(),
                filter: filter,
                orderBy: orderBy,
                ct: cancellationToken);

            logger.LogInformation("Retrieved {Count} orders out of {Total}",
            result.Items.Count, result.TotalCount);
            return result;
        }

        private Expression<Func<Order, bool>>? BuildFilter(string? search)
        {
            if(string.IsNullOrEmpty(search)) return null;
            var searchLower = search.ToLower();
            return (o) => o.Email.ToLower().Contains(searchLower)
               || o.Status.ToString().ToLower().Contains(searchLower);
        }

        private Func<IQueryable<Order>, IOrderedQueryable<Order>>? BuildOrderBy(string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return (q) => q.OrderByDescending(o => o.CreatedAtUtc);
            }

            var normalizedSortBy = sortBy?.ToLower().Trim();


            return normalizedSortBy switch
            {
                "email" => descending
                    ? q => q.OrderByDescending(o => o.Email)
                    : q => q.OrderBy(o => o.Email),
                "price" => descending
                    ? q => q.OrderByDescending(o => o.TotalPrice)
                    : q => q.OrderBy(o => o.TotalPrice),
                "status" => descending
                    ? q => q.OrderByDescending(o => o.Status)
                    : q => q.OrderBy(o => o.Status),
                "created" => descending
                    ? q => q.OrderByDescending(o => o.CreatedAtUtc)
                    : q => q.OrderBy(o => o.CreatedAtUtc),
                _ => (q) => q.OrderByDescending(o => o.CreatedAtUtc)
            };
        }
    }
}
