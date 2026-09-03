insert into cmn_bank_statement_template
(
    bank_id,
    code,
    name,
    version,
    sheet_name_match_type,
    sheet_name_pattern,
    data_start_row_offset,
    state_id
)
values
    (
        (select id from cmn_bank where code = 'TRASTBANK'),
        'TRASTBANK_XLSX_V1',
        'Trastbank Excel statement V1',
        1,
        'ANY',
        null,
        5,
        1
    ),
    (
        (select id from cmn_bank where code = 'UZSANOATQURILISHBANK'),
        'UZSANOATQURILISHBANK_XLSX_V1',
        'Uzsanoatqurilishbank Excel statement V1',
        1,
        'ANY',
        null,
        1,
        1
    );

insert into cmn_bank_statement_template_header_rule
(
    template_id,
    row_offset,
    column_index,
    match_type,
    expected_value,
    normalization_code,
    is_required
)
values
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 0, 1, 'CONTAINS', '/', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 1, 1, 'CONTAINS', 'Сведения', 'NORMALIZE_WHITESPACE', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 2, 1, 'REGEX', '^[CС]чет\s*:', 'NORMALIZE_WHITESPACE', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 4, 1, 'EXACT', 'Дата', 'TRIM', true),

    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 1, 'EXACT', 'Дата', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 2, 'CONTAINS', 'Номер документа', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 3, 'CONTAINS', 'МФО корресп', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 4, 'CONTAINS', 'Счет корреспондента', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 5, 'CONTAINS', 'Наименование корресп', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 6, 'CONTAINS', 'ИНН корреспондента', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 7, 'CONTAINS', 'Назначение платежа', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 8, 'EXACT', 'Дебет', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 0, 9, 'EXACT', 'Кредит', 'TRIM', true);

insert into cmn_bank_statement_template_row_rule
(
    template_id,
    priority,
    row_kind,
    column_index,
    operator_code,
    compare_value,
    normalization_code
)
values
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 1, 'TOTAL', 1, 'CONTAINS', 'Итоговый оборот', 'TRIM'),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 2, 'DATA', 1, 'IS_DATE', null, 'TRIM'),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 3, 'STOP', 1, 'REGEX', '.*', 'NONE'),

    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 1, 'TOTAL', 1, 'CONTAINS', 'Итоговый оборот за период', 'TRIM'),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 2, 'DATA', 1, 'IS_DATE', null, 'TRIM'),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 3, 'SKIP', 1, 'REGEX', '.*', 'NONE');

insert into cmn_bank_statement_template_field
(
    template_id,
    section_code,
    target_code,
    source_type,
    anchor_code,
    row_offset,
    column_index,
    extract_regex,
    extract_group,
    value_type,
    format,
    transform_code,
    is_required
)
values
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'BANK_MFO', 'RELATIVE_CELL', 'HEADER', 0, 1, '^\s*(?<mfo>[^/]+)\s*/', 'mfo', 'STRING', null, 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'BANK_NAME', 'RELATIVE_CELL', 'HEADER', 0, 1, '^\s*[^/]+/\s*(?<name>.*)$', 'name', 'STRING', null, 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'ACCOUNT_NUMBER', 'RELATIVE_CELL', 'HEADER', 2, 1, '[CС]чет\s*:\s*(?<account>\d+)', 'account', 'STRING', null, 'NORMALIZE_KEY', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'COMPANY_NAME', 'RELATIVE_CELL', 'HEADER', 2, 1, '[CС]чет\s*:\s*\d+\s+(?<name>.*?)\s+ИНН\s*:', 'name', 'STRING', null, 'NORMALIZE_WHITESPACE', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'COMPANY_INN', 'RELATIVE_CELL', 'HEADER', 2, 1, 'ИНН\s*:\s*(?<inn>\d+)', 'inn', 'STRING', null, 'NORMALIZE_KEY', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'PERIOD_FROM', 'RELATIVE_CELL', 'HEADER', 1, 1, '[cс]\s+(?<from>\d{2}\.\d{2}\.\d{4})\s+по', 'from', 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'PERIOD_TO', 'RELATIVE_CELL', 'HEADER', 1, 1, '\s+по\s+(?<to>\d{2}\.\d{2}\.\d{4})', 'to', 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'OPENING_BALANCE', 'RELATIVE_CELL', 'HEADER', 3, 1, null, null, 'DECIMAL', null, 'AMOUNT_FROM_TEXT', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'STATEMENT', 'CLOSING_BALANCE', 'RELATIVE_CELL', 'HEADER', 3, 2, null, null, 'DECIMAL', null, 'AMOUNT_FROM_TEXT', false),

    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'DATE', 'RELATIVE_CELL', 'DATA_ROW', 0, 1, null, null, 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_ACCOUNT', 'RELATIVE_CELL', 'DATA_ROW', 0, 2, '^\s*(?<account>[^/]*)', 'account', 'STRING', null, 'NORMALIZE_KEY', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_INN', 'RELATIVE_CELL', 'DATA_ROW', 0, 2, '^[^/]*/\s*(?<inn>[^/]*)', 'inn', 'STRING', null, 'NORMALIZE_KEY', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_NAME', 'RELATIVE_CELL', 'DATA_ROW', 0, 2, '^[^/]*/[^/]*/\s*(?<name>.*)$', 'name', 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'DOC_NUMBER', 'RELATIVE_CELL', 'DATA_ROW', 0, 3, null, null, 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'OPERATION_CODE', 'RELATIVE_CELL', 'DATA_ROW', 0, 4, null, null, 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_MFO', 'RELATIVE_CELL', 'DATA_ROW', 0, 5, null, null, 'STRING', null, 'NORMALIZE_MFO', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'DEBIT', 'RELATIVE_CELL', 'DATA_ROW', 0, 7, null, null, 'DECIMAL', null, 'NONE', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'CREDIT', 'RELATIVE_CELL', 'DATA_ROW', 0, 6, null, null, 'DECIMAL', null, 'NONE', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TRANSACTION', 'PURPOSE', 'RELATIVE_CELL', 'DATA_ROW', 0, 8, null, null, 'STRING', null, 'TRIM', false),

    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TOTAL', 'TOTAL_DEBIT', 'RELATIVE_CELL', 'TOTAL_ROW', 0, 7, null, null, 'DECIMAL', null, 'NONE', false),
    ((select id from cmn_bank_statement_template where code = 'TRASTBANK_XLSX_V1'), 'TOTAL', 'TOTAL_CREDIT', 'RELATIVE_CELL', 'TOTAL_ROW', 0, 6, null, null, 'DECIMAL', null, 'NONE', false);

insert into cmn_bank_statement_template_field
(
    template_id,
    section_code,
    target_code,
    source_type,
    anchor_code,
    absolute_row_index,
    column_index,
    extract_regex,
    extract_group,
    value_type,
    transform_code,
    is_required
)
values
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'BANK_NAME', 'ABSOLUTE_CELL', 'SHEET', 1, 1, '^\s*/?\s*(?<name>.*)$', 'name', 'STRING', 'TRIM', true);

insert into cmn_bank_statement_template_field
(
    template_id,
    section_code,
    target_code,
    source_type,
    anchor_code,
    column_index,
    search_direction,
    search_limit,
    locator_match_type,
    locator_value,
    extract_regex,
    extract_group,
    value_type,
    format,
    transform_code,
    is_required
)
values
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'ACCOUNT_NUMBER', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'REGEX', '^[CС]чет\s*:', '[CС]чет\s*:\s*(?<account>\d+)', 'account', 'STRING', null, 'NORMALIZE_KEY', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'COMPANY_NAME', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'REGEX', '^[CС]чет\s*:', '[CС]чет\s*:\s*\d+\s+(?<name>.*?)\s+ИНН\s*:', 'name', 'STRING', null, 'NORMALIZE_WHITESPACE', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'COMPANY_INN', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'REGEX', '^[CС]чет\s*:', 'ИНН\s*:\s*(?<inn>\d+)', 'inn', 'STRING', null, 'NORMALIZE_KEY', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'PERIOD_FROM', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'REGEX', 'Сведения\s+о\s+работе\s+счета', '[cс]\s+(?<from>\d{2}\.\d{2}\.\d{4})\s+по', 'from', 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'PERIOD_TO', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'REGEX', 'Сведения\s+о\s+работе\s+счета', '\s+по\s+(?<to>\d{2}\.\d{2}\.\d{4})', 'to', 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'OPENING_BALANCE', 'SEARCH_CELL', 'HEADER', 1, 'UP', 10, 'CONTAINS', 'Остаток на начало периода', null, null, 'DECIMAL', null, 'AMOUNT_FROM_TEXT', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'STATEMENT', 'CLOSING_BALANCE', 'SEARCH_CELL', 'HEADER', 2, 'UP', 10, 'CONTAINS', 'Остаток на конец периода', null, null, 'DECIMAL', null, 'AMOUNT_FROM_TEXT', false);

insert into cmn_bank_statement_template_field
(
    template_id,
    section_code,
    target_code,
    source_type,
    anchor_code,
    row_offset,
    column_index,
    value_type,
    format,
    transform_code,
    is_required
)
values
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'DATE', 'RELATIVE_CELL', 'DATA_ROW', 0, 1, 'DATE', 'dd.MM.yyyy', 'TRIM', true),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'DOC_NUMBER', 'RELATIVE_CELL', 'DATA_ROW', 0, 2, 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_MFO', 'RELATIVE_CELL', 'DATA_ROW', 0, 3, 'STRING', null, 'NORMALIZE_MFO', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_ACCOUNT', 'RELATIVE_CELL', 'DATA_ROW', 0, 4, 'STRING', null, 'NORMALIZE_KEY', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_NAME', 'RELATIVE_CELL', 'DATA_ROW', 0, 5, 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'COUNTERPARTY_INN', 'RELATIVE_CELL', 'DATA_ROW', 0, 6, 'STRING', null, 'NORMALIZE_KEY', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'PURPOSE', 'RELATIVE_CELL', 'DATA_ROW', 0, 7, 'STRING', null, 'TRIM', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'DEBIT', 'RELATIVE_CELL', 'DATA_ROW', 0, 9, 'DECIMAL', null, 'NONE', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TRANSACTION', 'CREDIT', 'RELATIVE_CELL', 'DATA_ROW', 0, 8, 'DECIMAL', null, 'NONE', false),

    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TOTAL', 'TOTAL_DEBIT', 'RELATIVE_CELL', 'TOTAL_ROW', 0, 9, 'DECIMAL', null, 'NONE', false),
    ((select id from cmn_bank_statement_template where code = 'UZSANOATQURILISHBANK_XLSX_V1'), 'TOTAL', 'TOTAL_CREDIT', 'RELATIVE_CELL', 'TOTAL_ROW', 0, 8, 'DECIMAL', null, 'NONE', false);
