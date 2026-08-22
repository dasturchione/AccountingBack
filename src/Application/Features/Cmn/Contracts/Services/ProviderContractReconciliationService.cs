using Application.Abstractions;
using Application.Abstractions.Authentication;
using Application.Features.AuditLogs;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using SharedKernel.Constants;
using SharedKernel.Exceptions;
using SharedKernel.Query;
using SharedKernel.Results;

namespace Application.Features.Contracts;

public sealed class ProviderContractReconciliationService(
    IUserContext userContext,
    IQueryBuilder queryBuilder,
    IQueryRepository<CounterpartyCard> counterpartyQuery,
    IQueryRepository<Contract> contractQuery,
    ICommandRepository<Contract> contractCommand,
    IAuditLogService auditLog,
    ILogger<ProviderContractReconciliationService> logger,
    IUnitOfWork unitOfWork) : BaseService(logger, unitOfWork), IProviderContractReconciliationService
{
    private const string AuditTable = "cmn_contract_provider_identity";

    public Task<Result<ProviderContractReconciliationResultDto>> ReconcileAsync(
        ProviderContractReconciliationCreateDto dto,
        CancellationToken ct = default) =>
        ExecuteInTransactionAsync("ReconcileProviderContract", async () =>
        {
            var validation = ProviderContractReconciliationRules.Validate(dto, userContext.LanguageId);
            if (validation is not null)
                return Result.Failure<ProviderContractReconciliationResultDto>(validation);

            if (userContext.OrganizationId is not { } organizationId)
                return Result.Failure<ProviderContractReconciliationResultDto>(
                    CommonErrors.UserHasNoOrganization(userContext.LanguageId));

            var providerCode = ProviderContractReconciliationRules.EdocsProviderCode;
            var providerNumber = ProviderContractReconciliationRules.NormalizeProviderNumber(dto.ProviderContractNumber)!;
            var idempotencyKey = ProviderContractReconciliationRules.BuildIdempotencyKey(
                organizationId,
                dto.CounterpartyId,
                providerCode,
                providerNumber,
                dto.ProviderContractDate);

            var counterpartySpec = queryBuilder.For<CounterpartyCard>()
                .Where(x => x.Id == dto.CounterpartyId
                    && x.OrganizationId == organizationId
                    && x.StateId == StateIdConst.ACTIVE)
                .Build();
            var counterparty = await counterpartyQuery.GetAsync(counterpartySpec, ct);
            if (counterparty is null)
                return Result.Failure<ProviderContractReconciliationResultDto>(
                    Error.NotFound("EDO_CONTRACT_COUNTERPARTY_NOT_FOUND", "The counterparty is not available in the current organization."));

            var identitySpec = queryBuilder.For<Contract>()
                .Where(x => x.OrganizationId == organizationId
                    && x.CounterpartyId == dto.CounterpartyId
                    && x.ProviderCode == providerCode
                    && x.ProviderContractNumber == providerNumber
                    && x.ProviderContractDate == dto.ProviderContractDate)
                .Build();
            var existing = await contractQuery.GetAllAsync(identitySpec, ct);
            var identityState = ProviderContractReconciliationRules.ClassifyExistingIdentity(existing);
            var active = existing.FirstOrDefault(x => x.StateId == StateIdConst.ACTIVE);
            if (identityState == "ALREADY_EXISTS" && active is not null)
            {
                await WriteAuditAsync(active.Id, organizationId, dto, providerNumber, idempotencyKey, "REUSED");
                return Result.Success(MapResult(active.Id, organizationId, dto, providerNumber, idempotencyKey, "ALREADY_EXISTS"));
            }

            if (identityState == "INACTIVE")
                return Result.Failure<ProviderContractReconciliationResultDto>(
                    Error.Conflict("EDO_CONTRACT_PROVIDER_IDENTITY_INACTIVE", "An inactive contract already owns this provider identity; reactivate it explicitly before reconciliation."));

            var entity = new Contract
            {
                OrganizationId = organizationId,
                CounterpartyId = dto.CounterpartyId,
                ContractTypeId = dto.ContractTypeId,
                ContractNumber = string.Empty,
                ProviderCode = providerCode,
                ProviderContractNumber = providerNumber,
                ProviderContractDate = dto.ProviderContractDate,
                ContractDate = dto.ContractDate.Date,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date.AddDays(1).AddTicks(-1),
                Comment = dto.Comment,
                StateId = StateIdConst.ACTIVE,
                CreatedDate = DateTime.Now
            };

            try
            {
                await contractCommand.CreateAsync(entity, ct);
            }
            catch (UniqueConstraintViolationException)
            {
                return Result.Failure<ProviderContractReconciliationResultDto>(
                    Error.Conflict("EDO_CONTRACT_PROVIDER_IDENTITY_EXISTS", "The provider contract identity is already owned by another contract. Retry the same request to read its safe existing result."));
            }

            await WriteAuditAsync(entity.Id, organizationId, dto, providerNumber, idempotencyKey, "CREATED");
            return Result.Success(MapResult(entity.Id, organizationId, dto, providerNumber, idempotencyKey, "CREATED"));
        }, ct);

    private async Task WriteAuditAsync(
        long contractId,
        int organizationId,
        ProviderContractReconciliationCreateDto dto,
        string providerNumber,
        string idempotencyKey,
        string result)
    {
        auditLog.SetNewValues(new
        {
            workflow = "EDOCS_PROVIDER_CONTRACT_RECONCILIATION",
            organizationId,
            contractId,
            counterpartyId = dto.CounterpartyId,
            providerCode = ProviderContractReconciliationRules.EdocsProviderCode,
            providerContractNumber = providerNumber,
            providerContractDate = dto.ProviderContractDate,
            idempotencyKey,
            result
        });

        await auditLog.CreateAsync(
            AuditTable,
            idempotencyKey,
            AuditLogOperationTypeConst.Create,
            "Explicit EDOCS provider contract reconciliation.",
            organizationId);
    }

    private static ProviderContractReconciliationResultDto MapResult(
        long contractId,
        int organizationId,
        ProviderContractReconciliationCreateDto dto,
        string providerNumber,
        string idempotencyKey,
        string status) => new()
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            Status = status,
            IdempotencyKey = idempotencyKey,
            ProviderCode = ProviderContractReconciliationRules.EdocsProviderCode,
            ProviderContractNumber = providerNumber,
            ProviderContractDate = dto.ProviderContractDate
        };
}
