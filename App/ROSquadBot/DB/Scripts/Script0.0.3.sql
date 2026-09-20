alter table public.user
add column if not exists "number" int not null default 0;

alter table public.user
add column if not exists isblocked boolean not null default false;
