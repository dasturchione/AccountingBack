using Application.Features.BankOperations;
using Domain.Entities;

namespace UnitTests;

public sealed class BankOperationClassificationDtoTests
{
    [Theory]
    [InlineData(typeof(BankOperationBaseDto), "ClassificationCategoryId", typeof(short?))]
    [InlineData(typeof(BankOperationBaseDto), "ClassificationRuleId", typeof(int?))]
    [InlineData(typeof(BankOperationDto), "ClassificationCategoryId", typeof(short?))]
    [InlineData(typeof(BankOperationDto), "ClassificationCode", typeof(string))]
    [InlineData(typeof(BankOperationDto), "ClassificationName", typeof(string))]
    [InlineData(typeof(BankOperationDto), "ClassificationRuleId", typeof(int?))]
    [InlineData(typeof(BankOperationDto), "ClassificationRuleCode", typeof(string))]
    [InlineData(typeof(BankOperationListDto), "ClassificationCategoryId", typeof(short?))]
    [InlineData(typeof(BankOperationListDto), "ClassificationCode", typeof(string))]
    [InlineData(typeof(BankOperationListDto), "ClassificationName", typeof(string))]
    [InlineData(typeof(BankOperationListDto), "ClassificationRuleId", typeof(int?))]
    [InlineData(typeof(BankOperationListDto), "ClassificationRuleCode", typeof(string))]
    public void Dto_ContainsClassificationProperty(Type dtoType, string propertyName, Type propertyType)
    {
        var property = dtoType.GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
    }

    [Fact]
    public void Projections_ReturnClassificationValues()
    {
        var entity = new BankOperation
        {
            ClassificationCategoryId = 3,
            ClassificationCategory = new BankOperationCategory
            {
                Id = 3,
                Code = "ACQUIRING",
                Name = "Terminal / acquiring"
            },
            ClassificationRuleId = 15,
            ClassificationRule = new BankOperationClassificationRule
            {
                Id = 15,
                Code = "ACQUIRING"
            },
            Organization = new Organization(),
            BankAccount = new BankAccount { Bank = new Bank() },
            Direction = new MovementDirection(),
            Currency = new Currency(),
            Status = new DocumentStatus(),
            State = new State()
        };

        var detail = new BankOperationDtoProjection().Build().Compile()(entity);
        var list = new BankOperationListDtoProjection().Build().Compile()(entity);

        Assert.Equal((short)3, detail.ClassificationCategoryId);
        Assert.Equal("ACQUIRING", detail.ClassificationCode);
        Assert.Equal("Terminal / acquiring", detail.ClassificationName);
        Assert.Equal(15, detail.ClassificationRuleId);
        Assert.Equal("ACQUIRING", detail.ClassificationRuleCode);
        Assert.Equal(detail.ClassificationCategoryId, list.ClassificationCategoryId);
        Assert.Equal(detail.ClassificationCode, list.ClassificationCode);
        Assert.Equal(detail.ClassificationRuleCode, list.ClassificationRuleCode);
    }
}
