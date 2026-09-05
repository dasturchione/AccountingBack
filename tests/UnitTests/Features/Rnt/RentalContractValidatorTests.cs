using System.Text.Json;
using Application.Features.Rnt.RentalContracts;

namespace UnitTests.Features.Rnt;

public sealed class RentalContractValidatorTests
{
    [Fact]
    public void CreateValidatorAcceptsPeriodAmountWithoutPeriodValueOrContractAmount()
    {
        const string json = """
        {
          "isFreeOfCharge": false,
          "lessors": [
            {
              "lessorKindCode": "INDIVIDUAL",
              "fullName": "Ali Valiyev",
              "pinfl": "12345678901234"
            }
          ],
          "contractNumber": "R-PERIOD-AMOUNT",
          "contractDate": "2026-09-04",
          "startDate": "2026-09-04",
          "endDate": "2026-12-04",
          "currencyId": 1,
          "objects": [
            {
              "rentalObjectTypeId": 1,
              "objectName": "Bino",
              "startDate": "2026-09-04",
              "periodUnit": "MONTH",
              "periodAmount": 5000000,
              "taxBaseAmount": 6000000,
              "taxRate": 12
            }
          ]
        }
        """;

        var dto = JsonSerializer.Deserialize<RentalContractCreateDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dto);
        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
    }

    [Fact]
    public void CreateValidatorAcceptsIndefiniteContractAndObject()
    {
        const string json = """
        {
          "isFreeOfCharge": true,
          "lessors": [
            {
              "lessorKindCode": "INDIVIDUAL",
              "fullName": "Ali Valiyev",
              "pinfl": "12345678901234"
            }
          ],
          "contractNumber": "R-INDEFINITE",
          "contractDate": "2026-09-04",
          "startDate": "2026-09-04",
          "currencyId": 1,
          "objects": [
            {
              "rentalObjectTypeId": 1,
              "objectName": "Bino",
              "startDate": "2026-09-04",
              "periodUnit": "MONTH",
              "periodAmount": 0,
              "taxBaseAmount": 0,
              "taxRate": 0
            }
          ]
        }
        """;

        var dto = JsonSerializer.Deserialize<RentalContractCreateDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dto);
        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
    }

    [Fact]
    public void CreateValidatorAcceptsFreeIndividualContractWithNewNestedRequestShape()
    {
        const string json = """
        {
          "isFreeOfCharge": true,
          "lessors": [
            {
              "lessorKindCode": "INDIVIDUAL",
              "fullName": "Ali Valiyev",
              "pinfl": "12345678901234",
              "phoneNumber": "+998901234567",
              "registeredAddress": "Navoiy viloyati",
              "residentialAddress": "Navoiy shahri"
            }
          ],
          "contractNumber": "R-1",
          "contractDate": "2026-09-04",
          "startDate": "2026-09-04",
          "endDate": "2027-09-03",
          "currencyId": 1,
          "objects": [
            {
              "rentalObjectTypeId": 1,
              "objectName": "Bino",
              "totalArea": 48.22,
              "rentedArea": 20,
              "startDate": "2026-09-04",
              "endDate": "2027-09-03",
              "periodUnit": "MONTH",
              "periodAmount": 0,
              "taxBaseAmount": 0,
              "taxRate": 0,
              "utilities": [
                { "utilityServiceId": 1, "payerCode": "LESSOR" },
                { "utilityServiceId": 4, "payerCode": "LESSEE" }
              ]
            }
          ]
        }
        """;

        var dto = JsonSerializer.Deserialize<RentalContractCreateDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(dto);
        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage)));
    }

    [Fact]
    public void CreateValidatorRejectsDifferentPinflValuesWithTheSameInn()
    {
        var dto = new RentalContractCreateDto
        {
            IsFreeOfCharge = true,
            ContractNumber = "R-2",
            ContractDate = new DateTime(2026, 9, 4),
            StartDate = new DateTime(2026, 9, 4),
            EndDate = new DateTime(2027, 9, 3),
            CurrencyId = 1,
            Lessors =
            [
                new RentalLessorInputDto
                {
                    LessorKindCode = "INDIVIDUAL",
                    FullName = "First lessor",
                    Inn = "301111111",
                    Pinfl = "11111111111111"
                },
                new RentalLessorInputDto
                {
                    LessorKindCode = "INDIVIDUAL",
                    FullName = "Second lessor",
                    Inn = "301111111",
                    Pinfl = "22222222222222"
                }
            ],
            Objects =
            [
                new RentalContractObjectInputDto
                {
                    RentalObjectTypeId = 1,
                    ObjectName = "Bino",
                    StartDate = new DateTime(2026, 9, 4),
                    EndDate = new DateTime(2027, 9, 3),
                    PeriodAmount = 0,
                    TaxBaseAmount = 0,
                    TaxRate = 0
                }
            ]
        };

        var result = new RentalContractCreateDtoValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RentalContractBaseDto.Lessors));
    }
}
