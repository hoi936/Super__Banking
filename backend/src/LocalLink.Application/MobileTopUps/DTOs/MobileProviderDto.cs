namespace LocalLink.Application.MobileTopUps.DTOs;

public class MobileProviderDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<MobileProductDto> Products { get; set; } = Array.Empty<MobileProductDto>();
}
