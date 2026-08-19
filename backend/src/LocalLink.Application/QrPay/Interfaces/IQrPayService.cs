using LocalLink.Application.QrPay.DTOs;

namespace LocalLink.Application.QrPay.Interfaces;

public interface IQrPayService
{
    Task<QrPayPayloadDto> CreateReceivePayloadAsync(
        Guid currentUserId,
        CreateQrPayloadRequest request,
        CancellationToken cancellationToken = default);

    Task<ParsedQrPayDto> ParsePayloadAsync(
        ParseQrPayloadRequest request,
        CancellationToken cancellationToken = default);
}
