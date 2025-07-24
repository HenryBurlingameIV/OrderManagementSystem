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
    public class StartAssemblyValidator : AbstractValidator<StartAssemblyStatus>
    {
        public StartAssemblyValidator() 
        {
            RuleFor(po => po.Status).Equal(ProcessingStatus.New);
            RuleFor(po => po.Stage).Equal(Stage.Assembly);
        }
    }
}
