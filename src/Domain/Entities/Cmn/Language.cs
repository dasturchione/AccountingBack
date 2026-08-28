using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("cmn_language")]
public partial class Language
{
    [Key]
    [Column("id")]
    public short Id { get; set; }

    [Column("code")]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Column("name")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("native_name")]
    [StringLength(100)]
    public string NativeName { get; set; } = null!;

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("state_id")]
    public short StateId { get; set; }

    [Column("created_date", TypeName = "timestamp without time zone")]
    public DateTime CreatedDate { get; set; }

    [InverseProperty(nameof(Translation.Language))]
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();

    [InverseProperty(nameof(PaymentMethodTranslation.Language))]
    public virtual ICollection<PaymentMethodTranslation> PaymentMethodTranslations { get; set; } = new List<PaymentMethodTranslation>();

    [InverseProperty(nameof(FiscalCashRegisterTypeTranslation.Language))]
    public virtual ICollection<FiscalCashRegisterTypeTranslation> FiscalCashRegisterTypeTranslations { get; set; } = new List<FiscalCashRegisterTypeTranslation>();

    [InverseProperty(nameof(DocumentAccountRoleTranslation.Language))]
    public virtual ICollection<DocumentAccountRoleTranslation> DocumentAccountRoleTranslations { get; set; } = new List<DocumentAccountRoleTranslation>();

    [InverseProperty(nameof(FaReceiptTypeTranslation.Language))]
    public virtual ICollection<FaReceiptTypeTranslation> FaReceiptTypeTranslations { get; set; } = new List<FaReceiptTypeTranslation>();

    [InverseProperty(nameof(ProductGroupTranslation.Language))]
    public virtual ICollection<ProductGroupTranslation> ProductGroupTranslations { get; set; } = new List<ProductGroupTranslation>();

    [InverseProperty(nameof(FaDisposalTypeTranslation.Language))]
    public virtual ICollection<FaDisposalTypeTranslation> FaDisposalTypeTranslations { get; set; } = new List<FaDisposalTypeTranslation>();

    [InverseProperty(nameof(AccountTypeTranslation.Language))]
    public virtual ICollection<AccountTypeTranslation> AccountTypeTranslations { get; set; } = new List<AccountTypeTranslation>();

    [InverseProperty(nameof(CurrencyTranslation.Language))]
    public virtual ICollection<CurrencyTranslation> CurrencyTranslations { get; set; } = new List<CurrencyTranslation>();

    [InverseProperty(nameof(DocumentStatusTranslation.Language))]
    public virtual ICollection<DocumentStatusTranslation> DocumentStatusTranslations { get; set; } = new List<DocumentStatusTranslation>();

    [InverseProperty(nameof(DocumentTypeTranslation.Language))]
    public virtual ICollection<DocumentTypeTranslation> DocumentTypeTranslations { get; set; } = new List<DocumentTypeTranslation>();

    [InverseProperty(nameof(OperationTypeTranslation.Language))]
    public virtual ICollection<OperationTypeTranslation> OperationTypeTranslations { get; set; } = new List<OperationTypeTranslation>();

    [InverseProperty(nameof(MovementDirectionTranslation.Language))]
    public virtual ICollection<MovementDirectionTranslation> MovementDirectionTranslations { get; set; } = new List<MovementDirectionTranslation>();

    [InverseProperty(nameof(BankOperationCategoryTranslation.Language))]
    public virtual ICollection<BankOperationCategoryTranslation> BankOperationCategoryTranslations { get; set; } = [];

    [InverseProperty(nameof(CostingMethodTranslation.Language))]
    public virtual ICollection<CostingMethodTranslation> CostingMethodTranslations { get; set; } = new List<CostingMethodTranslation>();

    [InverseProperty(nameof(ContractTypeTranslation.Language))]
    public virtual ICollection<ContractTypeTranslation> ContractTypeTranslations { get; set; } = new List<ContractTypeTranslation>();

    [InverseProperty(nameof(DocumentAccountTypeTranslation.Language))]
    public virtual ICollection<DocumentAccountTypeTranslation> DocumentAccountTypeTranslations { get; set; } = new List<DocumentAccountTypeTranslation>();

    [InverseProperty(nameof(UserKindTranslation.Language))]
    public virtual ICollection<UserKindTranslation> UserKindTranslations { get; set; } = new List<UserKindTranslation>();

    [InverseProperty("DefaultLanguage")]
    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    [InverseProperty(nameof(SubkontoTypeTranslation.Language))]
    public virtual ICollection<SubkontoTypeTranslation> SubkontoTypeTranslations { get; set; } = new List<SubkontoTypeTranslation>();

    [InverseProperty(nameof(ChartAccountPresetTranslation.Language))]
    public virtual ICollection<ChartAccountPresetTranslation> ChartAccountPresetTranslations { get; set; } = new List<ChartAccountPresetTranslation>();

    [InverseProperty(nameof(ChartAccountPresetAccountTranslation.Language))]
    public virtual ICollection<ChartAccountPresetAccountTranslation> ChartAccountPresetAccountTranslations { get; set; } = new List<ChartAccountPresetAccountTranslation>();

    [InverseProperty(nameof(PaymentTypeTranslation.Language))]
    public virtual ICollection<PaymentTypeTranslation> PaymentTypeTranslations { get; set; } = new List<PaymentTypeTranslation>();

    [ForeignKey("StateId")]
    [InverseProperty("Languages")]
    public virtual State State { get; set; } = null!;

    [InverseProperty("Language")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
