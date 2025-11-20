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
    public class StartDeliveryValidator : AbstractValidator<StartDeliveryStatus>
    {
        public StartDeliveryValidator()
        {
            RuleFor(po => po.Status).Equal(ProcessingStatus.Completed);
            RuleFor(po => po.Stage).Equal(Stage.Assembly);
        }
    }
}
