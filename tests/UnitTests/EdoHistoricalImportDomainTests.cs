using Domain.Entities;
using Domain.Exceptions;
using Application.Abstractions.Integration.Edo;
using Application.Features.PurchaseDocs;
using Integration.Edo.Historical;

namespace UnitTests;

public sealed class EdoHistoricalImportDomainTests
{
    [Fact]
    public void Bulk_import_statuses_define_durable_runnable_and_active_states()
    {
        Assert.True(EdoImportBulkImportStatus.IsRunnable(EdoImportBulkImportStatus.Queued));
        Assert.True(EdoImportBulkImportStatus.IsRunnable(EdoImportBulkImportStatus.Running));
        Assert.True(EdoImportBulkImportStatus.IsActive(EdoImportBulkImportStatus.Paused));
        Assert.False(EdoImportBulkImportStatus.IsActive(EdoImportBulkImportStatus.Completed));
        Assert.False(EdoImportBulkImportStatus.IsRunnable(EdoImportBulkImportStatus.Cancelled));
    }

    [Fact]
    public void Only_allowlisted_non_retryable_draft_failure_can_be_skipped()
    {
        var code = EdoImportDraftFailurePolicy.LineValuesInvalid;
        var skipped = EdoImportDraftFailurePolicy.ToSkipped(code);

        Assert.True(EdoImportDraftFailurePolicy.CanSkip(code));
        Assert.True(EdoImportDraftFailurePolicy.IsSkipped(skipped));
        Assert.Equal(code + "_SKIPPED", skipped);
        Assert.False(EdoImportDraftFailurePolicy.CanSkip("DRAFT_IMPORT_PROCESSING_FAILURE"));
        Assert.False(EdoImportDraftFailurePolicy.CanSkip(
            "DRAFT_IMPORT_PURCHASEFROMEDO_VALIDATIONFAILED"));
        Assert.Throws<ArgumentException>(() =>
            EdoImportDraftFailurePolicy.ToSkipped("DRAFT_IMPORT_PROCESSING_FAILURE"));

        var markingSkipped = EdoImportDraftFailurePolicy.ToSkipped(
            EdoImportDraftFailurePolicy.MarkingAlreadyUsed);
        Assert.Equal(EdoImportDraftFailurePolicy.MarkingAlreadyUsedSkipped, markingSkipped);
        Assert.True(EdoImportDraftFailurePolicy.IsSkipped(markingSkipped));
    }

    [Fact]
    public void Marking_conflict_skip_codes_are_controlled_and_reversible()
    {
        var codes = new[]
        {
            EdoImportMarkingPolicy.AlreadyUsed,
            EdoImportMarkingPolicy.CountMismatch,
            EdoImportMarkingPolicy.ProviderDataRequired,
            EdoImportMarkingPolicy.Duplicate,
            EdoImportMarkingPolicy.QuantityInvalid
        };

        foreach (var code in codes)
        {
            var skipped = EdoImportMarkingPolicy.ToSkippedConflict(code);

            Assert.True(EdoImportMarkingPolicy.CanSkipConflict(code));
            Assert.True(EdoImportMarkingPolicy.IsSkippedConflict(skipped));
            Assert.Equal(code, EdoImportMarkingPolicy.GetOriginalConflict(skipped));
        }

        Assert.False(EdoImportMarkingPolicy.CanSkipConflict("MARKING_MAPPING_REQUIRED"));
        Assert.False(EdoImportMarkingPolicy.IsSkippedConflict("MARKING_MAPPING_REQUIRED_SKIPPED"));
    }

    [Fact]
    public void Marked_goods_require_a_piece_tracked_local_product()
    {
        var failure = EdoImportMarkingPolicy.ValidateStructure(
            isPieceTracked: false,
            isService: false,
            quantity: 2,
            markings: ["mark-1", "mark-2"]);

        Assert.Equal(EdoImportMarkingPolicy.ProductPieceTrackingRequired, failure);
        Assert.Null(EdoImportMarkingPolicy.ValidateStructure(
            isPieceTracked: false,
            isService: false,
            quantity: 2,
            markings: []));
    }

    private static readonly DateTime Now = new(2026, 8, 11, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public void JobAllowsDefinedWorkflowTransitions()
    {
        var job = CreateJob();

        job.TransitionTo(EdoImportJobStatus.Scanning, Now.AddMinutes(1));
        job.TransitionTo(EdoImportJobStatus.PreflightReady, Now.AddMinutes(2));
        job.TransitionTo(EdoImportJobStatus.Importing, Now.AddMinutes(3));
        job.TransitionTo(EdoImportJobStatus.Partial, Now.AddMinutes(4));
        job.TransitionTo(EdoImportJobStatus.Importing, Now.AddMinutes(5));
        job.TransitionTo(EdoImportJobStatus.Completed, Now.AddMinutes(6));

        Assert.Equal(EdoImportJobStatus.Completed, job.Status);
        Assert.Equal(Now.AddMinutes(1), job.ScanStartedAt);
        Assert.Equal(Now.AddMinutes(1), job.StartedAt);
        Assert.Equal(Now.AddMinutes(6), job.CompletedAt);
    }

    [Fact]
    public void JobRejectsInvalidTransitionWithControlledDomainError()
    {
        var job = CreateJob();

        var exception = Assert.Throws<EdoImportStateTransitionException>(() =>
            job.TransitionTo(EdoImportJobStatus.Completed, Now.AddMinutes(1)));

        Assert.Equal(EdoImportStateTransitionException.ErrorCode, exception.Code);
        Assert.Equal(EdoImportJobStatus.Queued, exception.CurrentStatus);
        Assert.Equal(EdoImportJobStatus.Completed, exception.RequestedStatus);
    }

    [Fact]
    public void CandidateAllowsDefinedWorkflowTransitions()
    {
        var candidate = CreateCandidate();

        candidate.TransitionTo(EdoImportCandidateStatus.MappingRequired, Now.AddMinutes(1));
        candidate.TransitionTo(EdoImportCandidateStatus.Ready, Now.AddMinutes(2));
        candidate.TransitionTo(EdoImportCandidateStatus.Importing, Now.AddMinutes(3));
        candidate.TransitionTo(EdoImportCandidateStatus.Imported, Now.AddMinutes(4));

        Assert.Equal(EdoImportCandidateStatus.Imported, candidate.Status);
        Assert.Equal(Now.AddMinutes(4), candidate.UpdatedDate);
    }

    [Fact]
    public void CandidateRejectsInvalidTransitionWithControlledDomainError()
    {
        var candidate = CreateCandidate();

        var exception = Assert.Throws<EdoImportStateTransitionException>(() =>
            candidate.TransitionTo(EdoImportCandidateStatus.Imported, Now.AddMinutes(1)));

        Assert.Equal(EdoImportStateTransitionException.ErrorCode, exception.Code);
        Assert.Equal(EdoImportCandidateStatus.Discovered, exception.CurrentStatus);
        Assert.Equal(EdoImportCandidateStatus.Imported, exception.RequestedStatus);
    }

    [Fact]
    public void FingerprintsAndPurchaseLinksRemainNullableForBackwardCompatibility()
    {
        var candidate = CreateCandidate();

        Assert.Null(candidate.HeaderFingerprint);
        Assert.Null(candidate.ContentFingerprint);
        Assert.Null(candidate.SharedDocumentIdentity);
        Assert.Null(candidate.EdoDocumentId);
        Assert.Null(candidate.ExistingPurchaseId);
        Assert.Null(candidate.ImportedPurchaseId);
    }

    [Fact]
    public void DurableModelsDoNotExposeRawPayloadOrSecretFields()
    {
        var forbiddenFragments = new[]
        {
            "raw", "payload", "json", "token", "cookie", "authorization", "pkcs7", "signature", "credential"
        };
        var modelTypes = new[]
        {
            typeof(EdoImportJob),
            typeof(EdoImportJobProvider),
            typeof(EdoImportCandidate),
            typeof(EdoImportCandidateLine),
            typeof(EdoImportCandidateMarking)
        };

        foreach (var property in modelTypes.SelectMany(type => type.GetProperties()))
        {
            Assert.DoesNotContain(
                forbiddenFragments,
                fragment => property.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Theory]
    [InlineData(EdoImportProviderCheckpointStatus.Completed, EdoImportJobStatus.PreflightReady)]
    [InlineData(EdoImportProviderCheckpointStatus.Partial, EdoImportJobStatus.Partial)]
    [InlineData(EdoImportProviderCheckpointStatus.WaitingAuth, EdoImportJobStatus.WaitingAuth)]
    [InlineData(EdoImportProviderCheckpointStatus.Failed, EdoImportJobStatus.Failed)]
    public void SingleProviderCheckpointMapsToExpectedJobStatus(
        string providerStatus,
        string expectedJobStatus)
    {
        var provider = new EdoImportJobProvider { Status = providerStatus };

        Assert.Equal(expectedJobStatus, EdoImportPreflightProcessor.ResolveFinalJobStatus(provider));
    }

    [Fact]
    public void HistoricalFailureCodesSeparatePageDetailAndEligibilityDiagnostics()
    {
        Assert.Equal(
            "EDOCS_HISTORICAL_LIST_HTTP_422_INCOMPLETE_LAST_DETAIL_DOCUMENT_OUTSIDE_DATE_RANGE",
            EdoImportPreflightProcessor.BuildPageFailureCode(
                "EDOCS_HISTORICAL_LIST_HTTP_422",
                incomplete: true,
                "DOCUMENT_OUTSIDE_DATE_RANGE"));
        Assert.Equal(
            "EDOCS_DETAIL_VALIDATION_PROVIDER_DOCUMENT_IDENTITY_MISMATCH",
            EdoImportPreflightProcessor.BuildDetailFailureCode(
                "EDOCS",
                "PROVIDER_DOCUMENT_IDENTITY_MISMATCH"));
        Assert.Equal(
            "EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID",
            EdoImportPreflightProcessor.BuildDetailFailureCode(
                "EDOCS",
                "EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID"));
        Assert.True(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("UNSUPPORTED_DOCUMENT_TYPE"));
        Assert.True(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("DOCUMENT_OUTSIDE_DATE_RANGE"));
        Assert.True(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("PROVIDER_LINE_NUMBER_INVALID"));
        Assert.True(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("PROVIDER_LINE_NUMBER_DUPLICATE"));
        Assert.True(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("DOCUMENT_LINES_REQUIRED"));
        Assert.False(EdoImportPreflightProcessor.IsExpectedEligibilityExclusion("PROVIDER_DOCUMENT_IDENTITY_MISMATCH"));
        Assert.Equal(
            "EDOCS_PAGINATION_INCOMPLETE_LAST_DETAIL_UNSUPPORTED_DOCUMENT_TYPE",
            EdoImportPreflightProcessor.BuildPaginationIncompleteCode(
                "EDOCS",
                "UNSUPPORTED_DOCUMENT_TYPE"));
    }

    [Fact]
    public void HistoricalDetailFailureCodeIsQualifiedOnce()
    {
        var alreadyQualified = EdoHistoricalSourceSupport.BuildFailureDetail(
            EdoProviderCode.EDOCS,
            new EdoHistoricalMappingException("EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID"),
            "EDOCS_HISTORICAL_DETAIL");
        var legacyQualified = EdoHistoricalSourceSupport.BuildFailureDetail(
            EdoProviderCode.EDOCS,
            new EdoHistoricalMappingException("EDOCS_DETAIL_LINE_NUMBER_INVALID"),
            "EDOCS_HISTORICAL_DETAIL");

        Assert.Equal(
            "EDOCS_HISTORICAL_DETAIL_DECIMAL_INVALID",
            alreadyQualified.SafeFailureCode);
        Assert.Equal(
            "EDOCS_HISTORICAL_DETAIL_LINE_NUMBER_INVALID",
            legacyQualified.SafeFailureCode);
        Assert.Equal(
            "EDOCS_HISTORICAL_DETAIL_LINE_NUMBER_INVALID",
            EdoImportPreflightProcessor.BuildDetailFailureCode(
                "EDOCS",
            legacyQualified.SafeFailureCode!));
    }

    [Fact]
    public void MarkingConflictApplyValidatorRequiresExplicitUniqueSkipSelections()
    {
        var validator = new EdoImportMarkingConflictApplyRequestDtoValidator();
        var valid = validator.Validate(new EdoImportMarkingConflictApplyRequestDto
        {
            Confirm = true,
            ExpectedConflictHash = new string('a', 64),
            Items = [new EdoImportMarkingConflictApplyItemDto
            {
                CandidateId = 7058,
                Action = "SKIP"
            }]
        });
        var invalid = validator.Validate(new EdoImportMarkingConflictApplyRequestDto
        {
            Confirm = false,
            ExpectedConflictHash = "stale",
            Items =
            [
                new EdoImportMarkingConflictApplyItemDto { CandidateId = 7058, Action = "REUSE" },
                new EdoImportMarkingConflictApplyItemDto { CandidateId = 7058, Action = "SKIP" }
            ]
        });

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
    }

    [Fact]
    public void DraftImportValidatorRequiresConfirmationHashAndBoundedBatch()
    {
        var validator = new EdoImportDraftBatchRequestDtoValidator();

        Assert.True(validator.Validate(new EdoImportDraftBatchRequestDto
        {
            Confirm = true,
            ExpectedImportPlanHash = new string('a', 64),
            BatchSize = 50
        }).IsValid);
        Assert.False(validator.Validate(new EdoImportDraftBatchRequestDto
        {
            Confirm = false,
            ExpectedImportPlanHash = "stale",
            BatchSize = 51
        }).IsValid);
        Assert.True(typeof(IEdoHistoricalPurchaseDraftFactory)
            .IsAssignableFrom(typeof(PurchaseDocService)));
    }

    [Fact]
    public void Draft_failure_apply_validator_requires_confirmation_hash_and_unique_skip_items()
    {
        var validator = new EdoImportDraftFailureApplyRequestDtoValidator();
        var valid = validator.Validate(new EdoImportDraftFailureApplyRequestDto
        {
            Confirm = true,
            ExpectedFailureHash = new string('a', 64),
            Items = [new EdoImportDraftFailureApplyItemDto
            {
                CandidateId = 7031,
                Action = "SKIP"
            }]
        });
        var invalid = validator.Validate(new EdoImportDraftFailureApplyRequestDto
        {
            Confirm = false,
            ExpectedFailureHash = "bad",
            Items =
            [
                new EdoImportDraftFailureApplyItemDto { CandidateId = 7031, Action = "RETRY" },
                new EdoImportDraftFailureApplyItemDto { CandidateId = 7031, Action = "SKIP" }
            ]
        });

        Assert.True(valid.IsValid);
        Assert.False(invalid.IsValid);
        Assert.True(validator.Validate(new EdoImportDraftFailureApplyRequestDto
        {
            Confirm = true,
            ExpectedFailureHash = new string('b', 64),
            Items = [new EdoImportDraftFailureApplyItemDto
            {
                CandidateId = 7032,
                Action = "MARK_DUPLICATE"
            }]
        }).IsValid);
    }

    private static EdoImportJob CreateJob() =>
        new(11, 7, new DateOnly(2026, 1, 1), new DateOnly(2026, 8, 11), Now);

    private static EdoImportCandidate CreateCandidate() =>
        new(1, 11, "DIDOX", "provider-document-1", Now);
}
