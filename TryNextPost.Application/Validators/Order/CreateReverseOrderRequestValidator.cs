using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.Validators.Order
{
    public class CreateReverseOrderRequestValidator
        : CreateOrderRequestBaseValidator<CreateReverseOrderRequest>
    {
        public CreateReverseOrderRequestValidator()
        {
            // Reverse mein weight/dimensions optional — koi rule nahi.
            // Agar value aayi to negative na ho:
            RuleFor(x => x.WeightGrams).GreaterThanOrEqualTo(0);
            RuleFor(x => x.LengthCm).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BreadthCm).GreaterThanOrEqualTo(0);
            RuleFor(x => x.HeightCm).GreaterThanOrEqualTo(0);
        }
    }
}
