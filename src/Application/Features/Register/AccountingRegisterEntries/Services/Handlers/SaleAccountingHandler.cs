using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AccountingRegisterEntries;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;
using System.Text.Json;

namespace Application.Features.Register.AccountingRegisterEntries
{
    public class SaleAccountingHandler : IAccountingDocumentHandler<SaleDoc>
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly ISaleSubkontoNamesResolver _resolver;
        private readonly IQueryRepository<PostingRule> _postingRuleQuery;
        public SaleAccountingHandler(IUserContext userContext, 
                                     IQueryBuilder queryBuilder,
                                     ISaleSubkontoNamesResolver resolver,
                                     IQueryRepository<PostingRule> postingRuleQuery)
        {
            _resolver = resolver;
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _postingRuleQuery = postingRuleQuery;
        }

        public async Task<Result<List<AccountingRegisterEntry>>> HandleAsync(SaleDoc document, CancellationToken ct = default)
        {
            var rule = await GetRuleAsync(ct);
            if (rule is null)
                return Result.Failure<List<AccountingRegisterEntry>>(AccountingRegisterEntryErrors.PostingRuleNotFound(_userContext.LanguageId));

            var entries = new List<AccountingRegisterEntry>();

            var subkontoContext = await _resolver.FillSubkontoContext(document);

            var costRuleLine = rule.PostingRuleLines.Where(p => p.AmountSource == PostingAmountFields.CostAmount).First();

            foreach (var productDetail in subkontoContext.Products)
            {
                entries.AddRange(BuildPurchaseEntry(subkontoContext, productDetail, costRuleLine));
            }

            foreach (var subkontoTable in subkontoContext.Tables)
            {
                foreach (var ruleLine in rule.PostingRuleLines.Where(p => p.AmountSource != PostingAmountFields.CostAmount).OrderBy(o => o.SortOrder))
                {
                    entries.Add(BuildEntry(subkontoContext, subkontoTable, ruleLine));
                }
            }

            return Result.Success(entries);
        }

        private List<AccountingRegisterEntry> BuildPurchaseEntry(SaleSubkontoContext context, SaleProductSubkontoContext productContext, PostingRuleLine ruleLine)
        {
            var entries = new List<AccountingRegisterEntry>();

            foreach (var purchaseData in productContext.Purchases)
            {
                entries.Add(new AccountingRegisterEntry
                {
                    Amount = purchaseData.PurchaseAmount,

                    OrganizationId = context.OrganizationId,
                    DocumentTypeId = DocumentTypeIdConst.SALE,
                    OperationTypeId = OperationTypeIdConst.OUT,

                    CreatedDate = DateTime.Now,
                    DocDate = context.DocDate,

                    DebitAccountId = ruleLine.DebitAccountId,
                    CreditAccountId = ruleLine.CreditAccountId,

                    DebitQuantity = purchaseData.Quantity,
                    CreditQuantity = purchaseData.Quantity,

                    CurrencyId = context.CurrencyId,
                    DocumentId = context.Id,

                    Content = ruleLine.ContentTemplate,

                    RegisterEntrySubkontos = new List<RegisterEntrySubkonto>
                    {
                        new RegisterEntrySubkonto
                        {
                            SortOrder = 1,
                            EntityId = productContext.ProductId,
                            DisplayValue = productContext.ProductName,
                            SubkontoTypeId = SubkontoTypeIdConst.PRODUCT,
                            Side = SubkontoSideConst.CREDIT,
                            CreatedDate = DateTime.Now,
                        },
                        new RegisterEntrySubkonto
                        {
                            SortOrder = 2,
                            EntityId = context.WarehouseId,
                            DisplayValue = context.WarehouseName,
                            SubkontoTypeId = SubkontoTypeIdConst.WAREHOUSE,
                            Side = SubkontoSideConst.CREDIT,
                            CreatedDate = DateTime.Now,
                        },
                        new RegisterEntrySubkonto
                        {
                            SortOrder = 3,
                            EntityId = purchaseData.PurchaseId,
                            DisplayValue = JsonSerializer.Serialize(new
                            {
                                purchase = new
                                {
                                    number = purchaseData.PurchaseDocNumber,
                                    date = purchaseData.PurchaseDate
                                }
                            })
                        }
                    }
                });
            }

            return entries;
        }

        private AccountingRegisterEntry BuildEntry(SaleSubkontoContext context, SaleGroupedSubkontoContext table, PostingRuleLine ruleLine)
        {
            var amount = GetAmount(table, ruleLine.AmountSource);
            var quantity = GetQuantity(table, ruleLine.QuantitySource);

            return new AccountingRegisterEntry
            {
                Amount = amount,

                OrganizationId = context.OrganizationId,
                DocumentTypeId = DocumentTypeIdConst.SALE,
                OperationTypeId = OperationTypeIdConst.OUT,

                CreatedDate = DateTime.Now,
                DocDate = context.DocDate,

                DebitAccountId = ruleLine.DebitAccountId,
                CreditAccountId = ruleLine.CreditAccountId,

                DebitQuantity = quantity,
                CreditQuantity = quantity,

                CurrencyId = context.CurrencyId,
                DocumentId = context.Id,

                Content = ruleLine.ContentTemplate,

                RegisterEntrySubkontos = BuildSubkontos(context, ruleLine)
            };
        }

        private List<RegisterEntrySubkonto> BuildSubkontos(SaleSubkontoContext context, PostingRuleLine ruleLine)
        {
            var list = new List<RegisterEntrySubkonto>();

            if (context.ClientId != null)
                list.Add(new RegisterEntrySubkonto
                {
                    EntityId = context.Id,
                    Side = SubkontoSideConst.DEBIT,
                    SortOrder = list.Count + 1,
                    DisplayValue = context.ClientName,
                    CreatedDate = DateTime.Now,
                    SubkontoTypeId = SubkontoTypeIdConst.DOCUMENT
                });

            list.Add(new RegisterEntrySubkonto
            {
                EntityId = context.Id,
                Side = SubkontoSideConst.DEBIT,
                SortOrder = list.Count + 1,
                DisplayValue = JsonSerializer.Serialize(new
                {
                    sale = new
                    {
                        number = context.DocNumber,
                        date = context.DocDate
                    }
                }),
                CreatedDate = DateTime.Now,
                SubkontoTypeId = SubkontoTypeIdConst.DOCUMENT
            });

            return list;
        }

        private decimal GetAmount(SaleGroupedSubkontoContext productContext, string source)
        {
            return source switch
            {
                PostingAmountFields.Amount => productContext.Amount,
                PostingAmountFields.VatAmount => productContext.VatAmount,
                _ => 0
            };
        }

        private decimal? GetQuantity(SaleGroupedSubkontoContext productContext, string? source)
        {
            if (source is null)
                return null;

            return source switch
            {
                PostingQuantityFields.Quantity => productContext.Quantity,
                _ => null
            };
        }

        private async Task<PostingRule?> GetRuleAsync(CancellationToken ct = default)
        {
            var query = _queryBuilder
                .For<PostingRule>()
                .Where(x => x.DocumentTypeId == DocumentTypeIdConst.SALE)
                .Build();

            query.AddIncludes(e => e.Include(i => i.PostingRuleLines));

            return await _postingRuleQuery.GetAsync(query, ct);
        }
    }
}
