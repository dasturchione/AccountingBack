using Domain.Entities;
using SharedKernel.Constants;

namespace Application.Features.SaleDocs.EdoSalePreflight;

public static class EdoSaleCurrencySelectionPolicy
{
    public static string? Validate(Currency? selectedCurrency, string? providerCurrencyCode)
    {
        if (selectedCurrency is null || selectedCurrency.StateId != StateIdConst.ACTIVE)
            return "CURRENCY_MAPPING_INVALID";

        if (string.IsNullOrWhiteSpace(providerCurrencyCode))
            return null;

        return string.Equals(
            selectedCurrency.Code,
            providerCurrencyCode,
            StringComparison.OrdinalIgnoreCase)
            ? null
            : "CURRENCY_MAPPING_INVALID";
    }
}
