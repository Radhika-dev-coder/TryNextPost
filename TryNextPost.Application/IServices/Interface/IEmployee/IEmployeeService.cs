using TryNextPost.Application.DTO.Employee;

namespace TryNextPost.Application.IServices.Interface.IEmployee
{
    public interface IEmployeeService
    {
        Task<EmployeeResponse> CreateAsync(string ownerUserId, CreateEmployeeRequest request);
        Task<List<EmployeeResponse>> ListAsync(string ownerUserId);
        Task<EmployeeResponse> UpdateAsync(string ownerUserId, long employeeId, UpdateEmployeeRequest request);
        Task<List<string>> GetAvailablePermissionsAsync();
    }
}
