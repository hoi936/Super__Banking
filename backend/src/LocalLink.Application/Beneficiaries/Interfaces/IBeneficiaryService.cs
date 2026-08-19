using LocalLink.Application.Beneficiaries.DTOs;

namespace LocalLink.Application.Beneficiaries.Interfaces;

public interface IBeneficiaryService
{
    Task<IReadOnlyList<BeneficiaryDto>> GetMyBeneficiariesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BeneficiaryDto> AddBeneficiaryAsync(Guid userId, CreateBeneficiaryRequest request, CancellationToken cancellationToken = default);
    Task DeleteBeneficiaryAsync(Guid userId, Guid beneficiaryId, CancellationToken cancellationToken = default);
}
