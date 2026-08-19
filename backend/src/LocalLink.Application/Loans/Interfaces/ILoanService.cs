using LocalLink.Application.Loans.DTOs;

namespace LocalLink.Application.Loans.Interfaces;

public interface ILoanService
{
    Task<IReadOnlyList<LoanApplicationDto>> GetMyApplicationsAsync(Guid currentUserId, string? status = null, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> GetMyApplicationAsync(Guid currentUserId, Guid id, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> CreateApplicationAsync(Guid currentUserId, CreateLoanApplicationRequest request, CancellationToken cancellationToken = default);
}
