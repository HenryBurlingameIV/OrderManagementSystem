using FluentValidation;
using OrderProcessingService.Application.DTO;
using OrderProcessingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingService.Application.Validators
{
    public class GetPaginatedProcessingOrderRequestValidator :AbstractValidator<GetPaginatedProcessingOrdersRequest>
    {
        public GetPaginatedProcessingOrderRequestValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThan(0);
            RuleFor(r => r.PageSize).InclusiveBetween(1, 100);
        }
    }
}
