using TryNextPost.Application.DTO.Employee;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.IEmployee;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Employee
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ISellerContextService _sellerContextService;
        private readonly ISellerEmployeeRepository _employeeRepository;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;

        public EmployeeService(
            ISellerContextService sellerContextService,
            ISellerEmployeeRepository employeeRepository,
            IIdentityService identityService,
            IEmailService emailService)
        {
            _sellerContextService = sellerContextService;
            _employeeRepository = employeeRepository;
            _identityService = identityService;
            _emailService = emailService;
        }

        public async Task<EmployeeResponse> CreateAsync(string ownerUserId, CreateEmployeeRequest request)
        {
            await _sellerContextService.EnsureOwnerAsync(ownerUserId);
            var context = await _sellerContextService.ResolveAsync(ownerUserId);

            ValidatePermissions(request.Permissions);

            if (await _identityService.CheckEmailExistsAsync(request.Email))
                throw new InvalidOperationException("Email already registered.");

            var mobile = string.IsNullOrWhiteSpace(request.Mobile)
                ? $"9{Random.Shared.Next(100000000, 999999999)}"
                : request.Mobile.Trim();

            var createResult = await _identityService.CreateEmployeeUserAsync(
                request.Email.Trim(),
                request.Password,
                request.FullName.Trim(),
                mobile);

            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join(", ", createResult.Errors));

            var employee = new SellerEmployee
            {
                SellerId = context.SellerId,
                UserId = createResult.UserId,
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = ownerUserId,
                Permissions = request.Permissions
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(code => new EmployeePermission { PermissionCode = code })
                    .ToList()
            };

            await _employeeRepository.AddAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            await _emailService.SendEmployeeInviteEmail(
                request.Email.Trim(),
                request.FullName.Trim(),
                request.Password);

            return Map(employee);
        }

        public async Task<List<EmployeeResponse>> ListAsync(string ownerUserId)
        {
            await _sellerContextService.EnsureOwnerAsync(ownerUserId);
            var context = await _sellerContextService.ResolveAsync(ownerUserId);

            var employees = await _employeeRepository.GetBySellerIdAsync(context.SellerId);
            return employees.Select(Map).ToList();
        }

        public async Task<EmployeeResponse> UpdateAsync(
            string ownerUserId,
            long employeeId,
            UpdateEmployeeRequest request)
        {
            await _sellerContextService.EnsureOwnerAsync(ownerUserId);
            var context = await _sellerContextService.ResolveAsync(ownerUserId);

            var employee = await _employeeRepository.GetByIdAndSellerIdAsync(employeeId, context.SellerId)
                ?? throw new KeyNotFoundException(SystemMessage.NotFound);

            if (!string.IsNullOrWhiteSpace(request.FullName))
                employee.FullName = request.FullName.Trim();

            if (request.IsActive.HasValue)
                employee.IsActive = request.IsActive.Value;

            if (request.Permissions != null)
            {
                ValidatePermissions(request.Permissions);
                await _employeeRepository.ReplacePermissionsAsync(employee.EmployeeId, request.Permissions);
            }

            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = ownerUserId;

            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            var refreshed = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
            return Map(refreshed!);
        }

        public Task<List<string>> GetAvailablePermissionsAsync()
        {
            return Task.FromResult(EmployeePermissionCode.All.ToList());
        }

        private static void ValidatePermissions(IEnumerable<string> permissions)
        {
            foreach (var code in permissions)
            {
                if (!EmployeePermissionCode.IsValid(code))
                    throw new InvalidOperationException($"Invalid permission code: {code}");
            }
        }

        private static EmployeeResponse Map(SellerEmployee employee)
        {
            return new EmployeeResponse
            {
                EmployeeId = employee.EmployeeId,
                SellerId = employee.SellerId,
                UserId = employee.UserId,
                FullName = employee.FullName,
                Email = employee.Email,
                IsActive = employee.IsActive == true,
                Permissions = employee.Permissions?.Select(p => p.PermissionCode).ToList() ?? new List<string>(),
                CreatedAt = employee.CreatedAt
            };
        }
    }
}
