using Application.Abstractions;
using Application.Abstractions.Authentication;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Acc.OpeningBalances
{
    public partial class OpeningBalanceService : BaseService, IOpeningBalanceService
    {
        private readonly IUserContext _userContext;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<OpeningBalance> _openingBalanceQuery;
        private readonly ICommandRepository<OpeningBalance> _openingBalanceCommand;
        private readonly IQueryRepository<OpeningBalanceAccount> _openingBalanceAccountQuery;
        private readonly ICommandRepository<OpeningBalanceAccount> _openingBalanceAccountCommand;
        private readonly IQueryRepository<OpeningBalanceAccountDetail> _openingBalanceDetailQuery;
        private readonly ICommandRepository<OpeningBalanceAccountDetail> _openingBalanceDetailCommand;
        private readonly ICommandRepository<OpeningBalanceAccountDetailSubkonto> _openingBalanceDetailSubkontoCommand;
        private readonly IQueryRepository<ChartAccount> _chartAccountQuery;
        private readonly IQueryRepository<ChartAccountSubkonto> _chartAccountSubkontoQuery;
        private readonly IQueryRepository<Currency> _currencyQuery;
        private readonly IQueryRepository<FaAsset> _faAssetQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyCardQuery;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<Product> _productQuery;
        private readonly IQueryRepository<PostingBatch> _postingBatchQuery;
        private readonly IQueryRepository<Warehouse> _warehouseQuery;
        private readonly IQueryRepository<User> _userQuery;
        private readonly IQueryRepository<ProductGroup> _productGroupQuery;
        private readonly IQueryRepository<VatRate> _vatRateQuery;
        private readonly IQueryRepository<SaleDoc> _saleDocQuery;
        private readonly IQueryRepository<PurchaseDoc> _purchaseDocQuery;
        private readonly IQueryRepository<BankOperation> _bankOperationQuery;
        private readonly IQueryRepository<CashOperation> _cashOperationQuery;
        private readonly IQueryRepository<CashBox> _cashBoxQuery;
        private readonly IQueryRepository<BankAccount> _bankAccountQuery;
        private readonly IQueryRepository<TaxType> _taxTypeQuery;

        public OpeningBalanceService(
            IUserContext userContext,
            IQueryBuilder queryBuilder,
            IQueryRepository<OpeningBalance> openingBalanceQuery,
            ICommandRepository<OpeningBalance> openingBalanceCommand,
            IQueryRepository<OpeningBalanceAccount> openingBalanceAccountQuery,
            ICommandRepository<OpeningBalanceAccount> openingBalanceAccountCommand,
            IQueryRepository<OpeningBalanceAccountDetail> openingBalanceDetailQuery,
            ICommandRepository<OpeningBalanceAccountDetail> openingBalanceDetailCommand,
            ICommandRepository<OpeningBalanceAccountDetailSubkonto> openingBalanceDetailSubkontoCommand,
            IQueryRepository<ChartAccount> chartAccountQuery,
            IQueryRepository<ChartAccountSubkonto> chartAccountSubkontoQuery,
            IQueryRepository<Currency> currencyQuery,
            IQueryRepository<FaAsset> faAssetQuery,
            IQueryRepository<CounterpartyCard> counterpartyCardQuery,
            IQueryRepository<Contract> contractQuery,
            IQueryRepository<Product> productQuery,
            IQueryRepository<PostingBatch> postingBatchQuery,
            IQueryRepository<Warehouse> warehouseQuery,
            IQueryRepository<User> userQuery,
            IQueryRepository<ProductGroup> productGroupQuery,
            IQueryRepository<VatRate> vatRateQuery,
            IQueryRepository<SaleDoc> saleDocQuery,
            IQueryRepository<PurchaseDoc> purchaseDocQuery,
            IQueryRepository<BankOperation> bankOperationQuery,
            IQueryRepository<CashOperation> cashOperationQuery,
            IQueryRepository<CashBox> cashBoxQuery,
            IQueryRepository<BankAccount> bankAccountQuery,
            IQueryRepository<TaxType> taxTypeQuery,
            ILogger<OpeningBalanceService> logger,
            IUnitOfWork unitOfWork)
            : base(logger, unitOfWork)
        {
            _userContext = userContext;
            _queryBuilder = queryBuilder;
            _openingBalanceQuery = openingBalanceQuery;
            _openingBalanceCommand = openingBalanceCommand;
            _openingBalanceAccountQuery = openingBalanceAccountQuery;
            _openingBalanceAccountCommand = openingBalanceAccountCommand;
            _openingBalanceDetailQuery = openingBalanceDetailQuery;
            _openingBalanceDetailCommand = openingBalanceDetailCommand;
            _openingBalanceDetailSubkontoCommand = openingBalanceDetailSubkontoCommand;
            _chartAccountQuery = chartAccountQuery;
            _chartAccountSubkontoQuery = chartAccountSubkontoQuery;
            _currencyQuery = currencyQuery;
            _faAssetQuery = faAssetQuery;
            _counterpartyCardQuery = counterpartyCardQuery;
            _contractQuery = contractQuery;
            _productQuery = productQuery;
            _postingBatchQuery = postingBatchQuery;
            _warehouseQuery = warehouseQuery;
            _userQuery = userQuery;
            _productGroupQuery = productGroupQuery;
            _vatRateQuery = vatRateQuery;
            _saleDocQuery = saleDocQuery;
            _purchaseDocQuery = purchaseDocQuery;
            _bankOperationQuery = bankOperationQuery;
            _cashOperationQuery = cashOperationQuery;
            _cashBoxQuery = cashBoxQuery;
            _bankAccountQuery = bankAccountQuery;
            _taxTypeQuery = taxTypeQuery;
        }

        public Task<Result<OpeningBalanceDto>> GetAsync(CancellationToken ct = default) =>
            ExecuteAsync(nameof(GetAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure<OpeningBalanceDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                var query = _queryBuilder.For<OpeningBalance>()
                    .Where(x => x.OrganizationId == _userContext.OrganizationId.Value &&
                                x.StateId == StateIdConst.ACTIVE)
                    .As<OpeningBalanceDto>()
                    .Build();

                var balance = await _openingBalanceQuery.GetAsync(query, ct);

                return balance is null
                    ? Result.Failure<OpeningBalanceDto>(OpeningBalanceErrors.NotFound(_userContext.LanguageId))
                    : Result.Success(balance);
            });

        public Task<Result<OpeningBalanceDetailDto>> GetDetailAsync(long id, long openingBalanceAccountId, CancellationToken ct = default) =>
            ExecuteAsync(nameof(GetDetailAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure<OpeningBalanceDetailDto>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                var query = _queryBuilder.For<OpeningBalanceAccount>()
                    .Where(x => x.Id == openingBalanceAccountId &&
                                x.OpeningBalanceId == id &&
                                x.OpeningBalance.OrganizationId == _userContext.OrganizationId.Value &&
                                x.OpeningBalance.StateId == StateIdConst.ACTIVE)
                    .As<OpeningBalanceDetailDto>()
                    .Build();

                var account = await _openingBalanceAccountQuery.GetAsync(query, ct);

                if (account is null)
                    return Result.Failure<OpeningBalanceDetailDto>(OpeningBalanceErrors.AccountNotFound(openingBalanceAccountId, _userContext.LanguageId));

                await PopulateSubkontoNamesAsync(account, _userContext.OrganizationId.Value, ct);
                return Result.Success(account);
            });

        public Task<Result<long>> CreateAsync(OpeningBalanceCreateDto dto, CancellationToken ct = default) =>
            ExecuteInTransactionAsync(nameof(CreateAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure<long>(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                var organizationId = _userContext.OrganizationId.Value;
                if (await _openingBalanceQuery.AnyAsync(x => x.OrganizationId == organizationId, ct))
                    return Result.Failure<long>(OpeningBalanceErrors.AlreadyExists(_userContext.LanguageId));

                var entity = new OpeningBalance
                {
                    OrganizationId = organizationId,
                    BalanceDate = DateOnly.FromDateTime(dto.BalanceDate),
                    Description = dto.Description,
                    StateId = StateIdConst.ACTIVE,
                    CreatedDate = DateTime.Now
                };

                await _openingBalanceCommand.CreateAsync(entity, ct);

                return Result.Success(entity.Id);
            }, ct);

        public Task<Result> UpdateAsync(long id, OpeningBalanceUpdateDto dto, CancellationToken ct = default) =>
            ExecuteInTransactionAsync(nameof(UpdateAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                if (dto.StateId is not StateIdConst.ACTIVE and not StateIdConst.PASSIVE)
                    return Result.Failure(OpeningBalanceErrors.InvalidState(dto.StateId, _userContext.LanguageId));

                var balance = await GetEntityAsync(id, _userContext.OrganizationId.Value, ct);
                if (balance is null)
                    return Result.Failure(OpeningBalanceErrors.NotFound(id, _userContext.LanguageId));

                balance.BalanceDate = DateOnly.FromDateTime(dto.BalanceDate);
                balance.Description = dto.Description;
                balance.StateId = dto.StateId;

                await _openingBalanceCommand.UpdateAsync(balance, ct);

                return Result.Success();
            }, ct);

        public Task<Result> SaveAccountAsync(long id, OpeningBalanceAccountSaveDto dto, CancellationToken ct = default) =>
            ExecuteInTransactionAsync(nameof(SaveAccountAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                var organizationId = _userContext.OrganizationId.Value;
                var balance = await GetEntityAsync(id, organizationId, ct);
                if (balance is null)
                    return Result.Failure(OpeningBalanceErrors.NotFound(id, _userContext.LanguageId));

                if (balance.StateId != StateIdConst.ACTIVE)
                    return Result.Failure(OpeningBalanceErrors.NotActive(id, _userContext.LanguageId));

                var chartAccount = await GetChartAccountAsync(dto.ChartAccountId, organizationId, ct);
                if (chartAccount is null)
                    return Result.Failure(OpeningBalanceErrors.ChartAccountNotFound(dto.ChartAccountId, _userContext.LanguageId));

                if (chartAccount.IsGroup)
                    return Result.Failure(OpeningBalanceErrors.ChartAccountIsGroup(dto.ChartAccountId, _userContext.LanguageId));

                var validationError = await ValidateAccountDetailsAsync(dto, chartAccount.Id, ct);
                if (validationError is not null)
                    return Result.Failure(validationError);

                var accountResult = await GetOrCreateAccountAsync(id, dto, ct);
                if (!accountResult.IsSuccess)
                    return Result.Failure(accountResult.Error);

                var account = accountResult.Value;
                var detailOwnershipError = await ValidateExistingDetailIdsAsync(account.Id, dto.Details, ct);
                if (detailOwnershipError is not null)
                    return Result.Failure(detailOwnershipError);

                await SyncAccountDetailsAsync(account, dto.Details, ct);

                await _openingBalanceAccountCommand.UpdateAsync(account, ct);

                return Result.Success();
            }, ct);

        public Task<Result> DeleteAsync(long id, CancellationToken ct = default) =>
            ExecuteInTransactionAsync(nameof(DeleteAsync), async () =>
            {
                if (_userContext.OrganizationId is null)
                    return Result.Failure(CommonErrors.UserHasNoOrganization(_userContext.LanguageId));

                var balance = await GetEntityAsync(id, _userContext.OrganizationId.Value, ct);
                if (balance is null)
                    return Result.Failure(OpeningBalanceErrors.NotFound(id, _userContext.LanguageId));

                balance.StateId = StateIdConst.PASSIVE;
                await _openingBalanceCommand.UpdateAsync(balance, ct);
                return Result.Success();
            }, ct);

        private async Task<OpeningBalance?> GetEntityAsync(long id, int organizationId, CancellationToken ct)
        {
            var query = _queryBuilder.For<OpeningBalance>()
                .Where(x => x.Id == id && x.OrganizationId == organizationId)
                .Build();

            return await _openingBalanceQuery.GetAsync(query, ct);
        }

        private async Task<ChartAccount?> GetChartAccountAsync(int chartAccountId, int organizationId, CancellationToken ct)
        {
            var query = _queryBuilder.For<ChartAccount>()
                .Where(x => x.Id == chartAccountId &&
                            x.OrganizationId == organizationId &&
                            x.StateId == StateIdConst.ACTIVE)
                .Build();

            return await _chartAccountQuery.GetAsync(query, ct);
        }

        private async Task<Error?> ValidateAccountDetailsAsync(OpeningBalanceAccountSaveDto dto, int chartAccountId, CancellationToken ct)
        {
            if (dto.Details.Count == 0)
                return OpeningBalanceErrors.DetailsRequired(_userContext.LanguageId);

            var duplicateDetailId = dto.Details
                .Where(x => x.Id.HasValue)
                .GroupBy(x => x.Id!.Value)
                .FirstOrDefault(x => x.Count() > 1)?
                .Key;
            if (duplicateDetailId.HasValue)
                return OpeningBalanceErrors.DuplicateDetail(duplicateDetailId.Value, _userContext.LanguageId);

            var currencyIds = dto.Details.Select(x => x.CurrencyId).Distinct().ToList();
            var currenciesQuery = _queryBuilder.For<Currency>()
                .Where(x => currencyIds.Contains(x.Id) && x.StateId == StateIdConst.ACTIVE)
                .As(x => x.Id)
                .Build();
            var activeCurrencyIds = (await _currencyQuery.GetAllAsync(currenciesQuery, ct)).ToHashSet();
            var invalidCurrencyId = currencyIds.FirstOrDefault(x => !activeCurrencyIds.Contains(x));
            if (invalidCurrencyId != 0)
                return OpeningBalanceErrors.CurrencyNotFound(invalidCurrencyId, _userContext.LanguageId);

            var configurationQuery = _queryBuilder.For<ChartAccountSubkonto>()
                .Where(x => x.AccountId == chartAccountId && x.StateId == StateIdConst.ACTIVE)
                .As(x => new { x.SubkontoTypeId, x.SortOrder })
                .Build();
            var configurations = await _chartAccountSubkontoQuery.GetAllAsync(configurationQuery, ct);
            var expectedTypeIds = configurations
                .Select(x => x.SubkontoTypeId)
                .OrderBy(x => x)
                .ToArray();

            foreach (var detail in dto.Details)
            {
                var actualTypeIds = detail.Subkontos
                    .Select(x => x.SubkontoTypeId)
                    .OrderBy(x => x)
                    .ToArray();

                if (actualTypeIds.Length != actualTypeIds.Distinct().Count())
                    return OpeningBalanceErrors.DuplicateSubkontoType(_userContext.LanguageId);

                if (!actualTypeIds.SequenceEqual(expectedTypeIds))
                    return OpeningBalanceErrors.SubkontoConfigurationMismatch(chartAccountId, _userContext.LanguageId);
            }

            return null;
        }

        private async Task<Result<OpeningBalanceAccount>> GetOrCreateAccountAsync(long openingBalanceId, OpeningBalanceAccountSaveDto dto, CancellationToken ct)
        {
            OpeningBalanceAccount? account = null;

            if (dto.Id.HasValue)
            {
                var accountQuery = _queryBuilder.For<OpeningBalanceAccount>()
                    .Where(x => x.Id == dto.Id.Value && x.OpeningBalanceId == openingBalanceId)
                    .Build();
                account = await _openingBalanceAccountQuery.GetAsync(accountQuery, ct);
                if (account is null)
                    return Result.Failure<OpeningBalanceAccount>(OpeningBalanceErrors.AccountNotFound(dto.Id.Value, _userContext.LanguageId));
            }

            var duplicateQuery = _queryBuilder.For<OpeningBalanceAccount>()
                .Where(x => x.OpeningBalanceId == openingBalanceId &&
                            x.ChartAccountId == dto.ChartAccountId &&
                            (!dto.Id.HasValue || x.Id != dto.Id.Value))
                .Build();
            if (await _openingBalanceAccountQuery.GetAsync(duplicateQuery, ct) is not null)
                return Result.Failure<OpeningBalanceAccount>(OpeningBalanceErrors.ChartAccountAlreadyAdded(dto.ChartAccountId, _userContext.LanguageId));

            if (account is not null)
            {
                account.ChartAccountId = dto.ChartAccountId;
                return Result.Success(account);
            }

            var debitAmount = dto.Details.Sum(x => x.DebitAmount);
            var creditAmount = dto.Details.Sum(x => x.CreditAmount);

            account = new OpeningBalanceAccount
            {
                OpeningBalanceId = openingBalanceId,
                ChartAccountId = dto.ChartAccountId,
                CreatedDate = DateTime.Now,
                DebitAmount = debitAmount > creditAmount ? (debitAmount - creditAmount) : decimal.Zero,
                CreditAmount = creditAmount > debitAmount ? (creditAmount - debitAmount) : decimal.Zero,
            };

            await _openingBalanceAccountCommand.CreateAsync(account, ct);

            return Result.Success(account);
        }

        private async Task<Error?> ValidateExistingDetailIdsAsync(long openingBalanceAccountId, List<OpeningBalanceAccountDetailSaveDto> details, CancellationToken ct)
        {
            var requestedIds = details
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToList();
            if (requestedIds.Count == 0)
                return null;

            var query = _queryBuilder.For<OpeningBalanceAccountDetail>()
                .Where(x => x.OpeningBalanceAccountId == openingBalanceAccountId && requestedIds.Contains(x.Id))
                .As(x => x.Id)
                .Build();
            var existingIds = (await _openingBalanceDetailQuery.GetAllAsync(query, ct)).ToHashSet();
            var missingId = requestedIds.FirstOrDefault(x => !existingIds.Contains(x));

            return missingId == 0
                ? null
                : OpeningBalanceErrors.DetailNotFound(missingId, _userContext.LanguageId);
        }

        private async Task SyncAccountDetailsAsync(OpeningBalanceAccount account, List<OpeningBalanceAccountDetailSaveDto> dtos, CancellationToken ct)
        {
            var existingQuery = _queryBuilder.For<OpeningBalanceAccountDetail>()
                .Where(x => x.OpeningBalanceAccountId == account.Id)
                .Build();
            var existingDetails = await _openingBalanceDetailQuery.GetAllAsync(existingQuery, ct);
            var existingById = existingDetails.ToDictionary(x => x.Id);
            var requestedIds = dtos.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();


            var toDelete = existingDetails.Where(x => !requestedIds.Contains(x.Id)).ToList();
            if (toDelete.Count > 0)
            {
                var deleteIds = toDelete.Select(x => x.Id).ToList();
                await _openingBalanceDetailSubkontoCommand.DeleteAsync(
                    x => deleteIds.Contains(x.OpeningBalanceAccountDetailId), ct);
                await _openingBalanceDetailCommand.DeleteAsync(toDelete, ct);
            }

            var toCreate = new List<OpeningBalanceAccountDetail>();
            var toUpdate = new List<OpeningBalanceAccountDetail>();
            var detailsByDtoIndex = new List<OpeningBalanceAccountDetail>(dtos.Count);

            for (var index = 0; index < dtos.Count; index++)
            {
                var dto = dtos[index];
                OpeningBalanceAccountDetail detail;

                if (dto.Id.HasValue)
                {
                    detail = existingById[dto.Id.Value];
                    toUpdate.Add(detail);
                }
                else
                {
                    detail = new OpeningBalanceAccountDetail
                    {
                        OpeningBalanceAccountId = account.Id,
                        CreatedDate = DateTime.Now
                    };
                    toCreate.Add(detail);
                }

                detail.DebitAmount = dto.DebitAmount;
                detail.CreditAmount = dto.CreditAmount;
                detail.Quantity = dto.Quantity;
                detail.CurrencyId = dto.CurrencyId;
                detail.CurrencyAmount = dto.CurrencyAmount;
                detail.ExchangeRate = dto.ExchangeRate;
                detail.Description = dto.Description;
                detail.SortOrder = index + 1;
                detailsByDtoIndex.Add(detail);
            }

            if (toCreate.Count > 0)
                await _openingBalanceDetailCommand.CreateAsync(toCreate, ct);

            if (toUpdate.Count > 0)
                await _openingBalanceDetailCommand.UpdateAsync(toUpdate, ct);

            var persistedDetailIds = detailsByDtoIndex.Select(x => x.Id).ToList();
            await _openingBalanceDetailSubkontoCommand.DeleteAsync(
                x => persistedDetailIds.Contains(x.OpeningBalanceAccountDetailId), ct);

            var configurationsQuery = _queryBuilder.For<ChartAccountSubkonto>()
                .Where(x => x.AccountId == account.ChartAccountId && x.StateId == StateIdConst.ACTIVE)
                .As(x => new { x.SubkontoTypeId, x.SortOrder })
                .Build();
            var sortOrdersByTypeId = (await _chartAccountSubkontoQuery.GetAllAsync(configurationsQuery, ct))
                .ToDictionary(x => x.SubkontoTypeId, x => x.SortOrder);

            var subkontos = new List<OpeningBalanceAccountDetailSubkonto>();
            for (var index = 0; index < dtos.Count; index++)
            {
                foreach (var subkonto in dtos[index].Subkontos)
                {
                    subkontos.Add(new OpeningBalanceAccountDetailSubkonto
                    {
                        OpeningBalanceAccountDetailId = detailsByDtoIndex[index].Id,
                        SubkontoTypeId = subkonto.SubkontoTypeId,
                        SubkontoId = subkonto.SubkontoId,
                        SortOrder = checked((short)sortOrdersByTypeId[subkonto.SubkontoTypeId]),
                        CreatedDate = DateTime.Now
                    });
                }
            }

            if (subkontos.Count > 0)
                await _openingBalanceDetailSubkontoCommand.CreateAsync(subkontos, ct);
        }
    }
}
