using Application.Features.BankOperations;
using Application.Features.BankParsers;
using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace UnitTests;

public sealed class BankDocumentNumberTests
{
    [Fact]
    public void BankOperation_MapsBankDocumentNumberToNullableVarchar150Column()
    {
        var property = typeof(BankOperation).GetProperty("BankDocumentNumber");

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("bank_document_number", property.GetCustomAttributes(typeof(ColumnAttribute), false)
            .Cast<ColumnAttribute>()
            .Single()
            .Name);
        Assert.Equal(150, property.GetCustomAttributes(typeof(StringLengthAttribute), false)
            .Cast<StringLengthAttribute>()
            .Single()
            .MaximumLength);
    }

    [Fact]
    public void BankOperationCrud_ExposesMapsAndValidatesBankDocumentNumber()
    {
        Assert.NotNull(typeof(BankOperationBaseDto).GetProperty("BankDocumentNumber"));
        Assert.NotNull(typeof(BankOperationDto).GetProperty("BankDocumentNumber"));
        Assert.NotNull(typeof(BankOperationListDto).GetProperty("BankDocumentNumber"));

        AssertProjectionMapsBankDocumentNumber(new BankOperationDtoProjection().Build());
        AssertProjectionMapsBankDocumentNumber(new BankOperationListDtoProjection().Build());

        var dto = new BankOperationBaseDto
        {
            BankAccountId = 1,
            DirectionId = 1,
            CurrencyId = 1,
            Amount = 1m
        };
        typeof(BankOperationBaseDto).GetProperty("BankDocumentNumber")!
            .SetValue(dto, new string('X', 151));

        var result = new BankOperationBaseDtoValidator().Validate(dto);

        Assert.Contains(result.Errors, error => error.PropertyName == "BankDocumentNumber");

        var listDto = new BankOperationListDto { DocNumber = "INTERNAL-1" };
        typeof(BankOperationListDto).GetProperty("BankDocumentNumber")!
            .SetValue(listDto, "BANK-42");
        var predicate = new BankOperationListDtoByListFilterCriteriaBuilder()
            .Build(new BankOperationListFilter { Search = "bank-42" })
            .Compile();
        Assert.True(predicate(listDto));
    }

    [Fact]
    public void Parse_ReturnsBankDocumentNumberAndKeepsLegacyDocNumber()
    {
        using var workbook = BankStatementTemplateParserTests.CreateUzsanoatqurilishbankWorkbook();

        var export = BankStatementTemplateParser.Parse(
            workbook,
            BankStatementTemplateTestData.CreateUzsanoatqurilishbank());

        var transaction = Assert.Single(export.Accounts).Transactions[0];
        var property = typeof(TransactionDto).GetProperty("BankDocumentNumber");

        Assert.NotNull(property);
        Assert.Equal("bankDocumentNumber", property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false)
            .Cast<JsonPropertyNameAttribute>()
            .Single()
            .Name);
        Assert.Equal(transaction.DocNumber, property.GetValue(transaction));
        Assert.Equal("27", transaction.DocNumber);
    }

    private static void AssertProjectionMapsBankDocumentNumber<TDto>(
        Expression<Func<BankOperation, TDto>> projection)
    {
        var body = Assert.IsType<MemberInitExpression>(projection.Body);
        var binding = Assert.Single(body.Bindings, x => x.Member.Name == "BankDocumentNumber");
        var assignment = Assert.IsType<MemberAssignment>(binding);
        var source = Assert.IsAssignableFrom<MemberExpression>(assignment.Expression);
        Assert.Equal("BankDocumentNumber", source.Member.Name);
    }
}
