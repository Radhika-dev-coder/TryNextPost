using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.Validators.Order
{
    public sealed class CreateForwardOrderRequestValidator
        : CreateOrderRequestBaseValidator<CreateForwardOrderRequest>
    {
        public CreateForwardOrderRequestValidator()
        {
            RuleFor(x => x.WeightGrams)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than 0");
            RuleFor(x => x.LengthCm).GreaterThan(0);
            RuleFor(x => x.BreadthCm).GreaterThan(0);
            RuleFor(x => x.HeightCm).GreaterThan(0);
        }
    }
}
