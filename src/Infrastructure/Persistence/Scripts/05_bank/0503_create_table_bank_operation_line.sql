create table bank_operation_line 
(
	id						BIGSERIAL PRIMARY KEY,
	bank_operation_id		BIGINT NOT NULL REFERENCES bank_operation(id) ON DELETE CASCADE,
	order_number			SMALLINT NOT NULL,
	payment_purpose_id		SMALLINT NOT NULL REFERENCES acc_payment_purpose(id),
	counterparty_id			INT REFERENCES counterparty_card(id),
	amount					NUMERIC(18,2) NOT NULL,
	comment					VARCHAR(500),

	CONSTRAINT uq_bank_operation_line_order UNIQUE (bank_operation_id, order_number)
);

create index idx_bank_operation_line_payment_purpose_id
    on bank_operation_line (payment_purpose_id);

create index idx_bank_operation_line_counterparty_id
    on bank_operation_line (counterparty_id);


CREATE OR REPLACE FUNCTION fn_validate_bank_operation_lines_sum()
RETURNS TRIGGER AS $$
DECLARE
	v_total NUMERIC(18,2);
	v_op_amount NUMERIC(18,2);
BEGIN
	SELECT amount INTO v_op_amount FROM bank_operation WHERE id = COALESCE(NEW.bank_operation_id, OLD.bank_operation_id);
	SELECT COALESCE(SUM(amount), 0) INTO v_total FROM bank_operation_line WHERE bank_operation_id = COALESCE(NEW.bank_operation_id, OLD.bank_operation_id);

	IF v_total > v_op_amount THEN
		RAISE EXCEPTION 'Sum of bank_operation_line.amount (%) exceeds bank_operation.amount (%)', v_total, v_op_amount;
	END IF;

	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_bank_operation_lines_sum
AFTER INSERT OR UPDATE ON bank_operation_line
FOR EACH ROW EXECUTE FUNCTION fn_validate_bank_operation_lines_sum();