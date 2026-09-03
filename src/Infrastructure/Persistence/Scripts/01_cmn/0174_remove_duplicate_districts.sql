begin;

create temporary table tmp_cmn_district_duplicate_map
(
    duplicate_id int primary key,
    canonical_id int not null
) on commit drop;

insert into tmp_cmn_district_duplicate_map (duplicate_id, canonical_id)
with coded_districts as
(
    select
        id,
        min(id) over (partition by region_id, code) as canonical_id
    from cmn_district
    where code is not null
),
uncoded_districts as
(
    select
        id,
        min(id) over
        (
            partition by region_id, lower(trim(short_name)), lower(trim(full_name))
        ) as canonical_id
    from cmn_district
    where code is null
)
select id, canonical_id
from coded_districts
where id <> canonical_id
union all
select id, canonical_id
from uncoded_districts
where id <> canonical_id;

update org_organization organization
set district_id = duplicate.canonical_id
from tmp_cmn_district_duplicate_map duplicate
where organization.district_id = duplicate.duplicate_id;

update org_branch branch
set district_id = duplicate.canonical_id
from tmp_cmn_district_duplicate_map duplicate
where branch.district_id = duplicate.duplicate_id;

update counterparty_card counterparty
set district_id = duplicate.canonical_id
from tmp_cmn_district_duplicate_map duplicate
where counterparty.district_id = duplicate.duplicate_id;

update cmn_bank_branch bank_branch
set district_id = duplicate.canonical_id
from tmp_cmn_district_duplicate_map duplicate
where bank_branch.district_id = duplicate.duplicate_id;

delete from cmn_district district
using tmp_cmn_district_duplicate_map duplicate
where district.id = duplicate.duplicate_id;

do
$$
begin
    if exists
    (
        select 1
        from cmn_district
        where code is not null
        group by region_id, code
        having count(*) > 1
    ) then
        raise exception 'cmn_district still contains duplicate region and code pairs';
    end if;
end
$$;

drop index if exists idx_cmn_district_region_code;

create unique index if not exists uidx_cmn_district_region_code
    on cmn_district using btree (region_id, code)
    where code is not null;

commit;
