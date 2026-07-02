using Application.Abstractions;
using Domain.Entities;
using SharedKernel.Constants;
using SharedKernel.Query;
using System.Text.Json;

namespace Application.Features.Register.PostingEngines
{
    public class BankOperationContextBuilder : IPostingContextBuilder<List<BankOperation>>
    {
        private readonly IQueryBuilder _queryBuilder;
        private readonly IQueryRepository<BankAccount> _bankAccountQuery;
        private readonly IQueryRepository<CounterpartyCard> _counterpartyQuery;
        private readonly IQueryRepository<Contract> _contractQuery;
        private readonly IQueryRepository<PaymentType> _paymentTypeQuery;

        public BankOperationContextBuilder(
            IQueryBuilder queryBuilder,
            IQueryRepository<BankAccount> bankAccountQuery,
            IQueryRepository<CounterpartyCard> counterpartyQuery,
            IQueryRepository<Contract> contractQuery,
            IQueryRepository<PaymentType> paymentTypeQuery)
        {
            _queryBuilder = queryBuilder;
            _bankAccountQuery = bankAccountQuery;
            _counterpartyQuery = counterpartyQuery;
            _contractQuery = contractQuery;
            _paymentTypeQuery = paymentTypeQuery;
        }

        public async Task<List<PostingContext>> BuildAsync(List<BankOperation> documents)
        {
            var result = new List<PostingContext>();

            if (documents.Count == 0)
                return result;

            var bankAccountIds = documents.Select(x => x.BankAccountId).Distinct().ToList();
            var counterpartyIds = documents
                .Select(x => (int?)x.CounterpartyId)
                .Concat(documents.SelectMany(x => x.BankOperationLines.Select(l => l.CounterpartyId)))
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var contractIds = documents
                .Where(x => x.ContractId.HasValue)
                .Select(x => x.ContractId!.Value)
                .Distinct()
                .ToList();

            var paymentTypeIds = documents
                .Select(x => x.PaymentTypeId)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var bankAccountMap = await GetBankAccountMapAsync(bankAccountIds);
            var counterpartyMap = await GetCounterpartyMapAsync(counterpartyIds);
            var contractMap = await GetContractMapAsync(contractIds);
            var paymentTypeMap = await GetPaymentTypeMapAsync(paymentTypeIds);

            foreach (var operation in documents)
            {
                var ruleId = ResolveRuleId(operation.OperationTypeId);

                var operationLines = operation.BankOperationLines.Count > 0
                    ? operation.BankOperationLines.ToList()
                    : new List<BankOperationLine>
                    {
                        new()
                        {
                            Amount = operation.Amount,
                            CounterpartyId = operation.CounterpartyId,
                        }
                    };

                foreach (var line in operationLines.Where(x => x.Amount != 0))
                {
                    var counterpartyId = line.CounterpartyId ?? operation.CounterpartyId;
                    var context = new PostingContext
                    {
                        OrganizationId = operation.OrganizationId,
                        AccountingPolicyId = AccountingPolicyIdConst.STANDARD_UZ,
                        RuleId = ruleId,
                        DocumentId = operation.Id,
                        DocDate = operation.DocDate,
                        CurrencyId = operation.CurrencyId,
                        JournalNumber = operation.DocNumber,
                        PaymentMethod = ResolvePaymentMethod(operation.PaymentTypeId, paymentTypeMap),
                        Amounts = new Dictionary<string, decimal>
                        {
                            [AmountSourceConst.Total] = line.Amount
                        },
                        Subkontos = BuildSubkontos(operation, counterpartyId, bankAccountMap, counterpartyMap, contractMap)
                    };

                    result.Add(context);
                }
            }

            return result;
        }

        private static short ResolveRuleId(short operationTypeId) =>
            operationTypeId switch
            {
                OperationTypeIdConst.IN => PostingRuleIdConst.DEBIT_OPERATION,
                OperationTypeIdConst.OUT => PostingRuleIdConst.CREDIT_OPERATION,
                _ => throw new ArgumentOutOfRangeException(nameof(operationTypeId), operationTypeId, "Unsupported bank operation type for accounting posting.")
            };

        private static string ResolvePaymentMethod(short? paymentTypeId, Dictionary<short, string> paymentTypeMap) =>
            paymentTypeId is { } id && paymentTypeMap.TryGetValue(id, out var value)
                ? value.ToLowerInvariant()
                : "bank";

        private static List<SubkontoValue> BuildSubkontos(
            BankOperation operation,
            int? counterpartyId,
            Dictionary<int, string> bankAccountMap,
            Dictionary<int, string> counterpartyMap,
            Dictionary<long, (string Number, DateTime Date)> contractMap)
        {
            var subkontos = new List<SubkontoValue>
            {
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.BANK_ACCOUNT,
                    DisplayValue = bankAccountMap.GetValueOrDefault(operation.BankAccountId),
                    EntityId = operation.BankAccountId,
                    SortOrder = 1,
                },
                new()
                {
                    SubkontoTypeId = SubkontoTypeIdConst.BANK_OPERATION,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = operation.DocNumber,
                        date = operation.DocDate
                    }),
                    EntityId = operation.Id,
                    SortOrder = 2,
                }
            };

            if (counterpartyId.HasValue && counterpartyMap.TryGetValue(counterpartyId.Value, out var counterpartyName))
            {
                subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.COUNTER_PARTY,
                    DisplayValue = counterpartyName,
                    EntityId = counterpartyId.Value,
                    SortOrder = 3
                });
            }

            if (operation.ContractId.HasValue && contractMap.TryGetValue(operation.ContractId.Value, out var contractData))
            {
                subkontos.Add(new SubkontoValue
                {
                    SubkontoTypeId = SubkontoTypeIdConst.CONTRACT,
                    DisplayValue = JsonSerializer.Serialize(new
                    {
                        number = contractData.Number,
                        date = contractData.Date
                    }),
                    EntityId = operation.ContractId.Value,
                    SortOrder = 4
                });
            }

            return subkontos;
        }

        private async Task<Dictionary<int, string>> GetBankAccountMapAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            var query = _queryBuilder.For<BankAccount>()
                .Where(x => ids.Contains(x.Id))
                .As(x => new
                {
                    x.Id,
                    Name = string.IsNullOrWhiteSpace(x.Name) ? x.AccountNumber : $"{x.Name} ({x.AccountNumber})"
                })
                .Build();

            var items = await _bankAccountQuery.GetAllAsync(query);
            return items.ToDictionary(x => x.Id, x => x.Name);
        }

        private async Task<Dictionary<int, string>> GetCounterpartyMapAsync(List<int> ids)
        {
            if (ids.Count == 0)
                return new Dictionary<int, string>();

            var query = _queryBuilder.For<CounterpartyCard>()
                .Where(x => ids.Contains(x.Id))
                .As(x => new { x.Id, x.ShortName })
                .Build();

            var items = await _counterpartyQuery.GetAllAsync(query);
            return items.ToDictionary(x => x.Id, x => x.ShortName);
        }

        private async Task<Dictionary<long, (string Number, DateTime Date)>> GetContractMapAsync(List<long> ids)
        {
            if (ids.Count == 0)
                return new Dictionary<long, (string Number, DateTime Date)>();

            var query = _queryBuilder.For<Contract>()
                .Where(x => ids.Contains(x.Id))
                .As(x => new ContractData
                {
                    Id = x.Id,
                    Number = x.ContractNumber,
                    Date = x.ContractDate
                })
                .Build();

            var items = await _contractQuery.GetAllAsync(query);
            return items.ToDictionary(x => x.Id, x => (x.Number, x.Date));
        }

        private async Task<Dictionary<short, string>> GetPaymentTypeMapAsync(List<short> ids)
        {
            if (ids.Count == 0)
                return new Dictionary<short, string>();

            var query = _queryBuilder.For<PaymentType>()
                .Where(x => ids.Contains(x.Id))
                .As(x => new { x.Id, x.Code })
                .Build();

            var items = await _paymentTypeQuery.GetAllAsync(query);
            return items.ToDictionary(x => x.Id, x => x.Code);
        }

        private sealed class ContractData
        {
            public long Id { get; set; }
            public string Number { get; set; } = null!;
            public DateTime Date { get; set; }
        }
    }
}
