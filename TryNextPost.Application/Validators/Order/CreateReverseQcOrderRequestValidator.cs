using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.Validators.Order
{
    public sealed class CreateReverseQcOrderRequestValidator
        : CreateOrderRequestBaseValidator<CreateReverseQcOrderRequest>
    {
        public CreateReverseQcOrderRequestValidator()
        {
            RuleFor(x => x.ProductCategory)
                .NotEmpty().WithMessage("Product category is required");
            RuleFor(x => x.ReferenceImageUrls)
                .NotEmpty().WithMessage("At least one reference image is required")
                .Must(urls => urls.Count <= 4)
                .WithMessage("Maximum four reference images are allowed");
        }
    }
}
