using LocalLink.Application.Common.Models;
using LocalLink.Application.Loans.DTOs;

namespace LocalLink.Application.Loans.Interfaces;

public interface IAdminLoanService
{
    Task<PagedResult<LoanApplicationDto>> GetApplicationsAsync(int page = 1, int pageSize = 20, string? status = null, string? keyword = null, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> GetApplicationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> ApproveAsync(Guid id, ApproveLoanApplicationRequest request, Guid reviewerUserId, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> RejectAsync(Guid id, RejectLoanApplicationRequest request, Guid reviewerUserId, string? ipAddress = null, CancellationToken cancellationToken = default);
}
