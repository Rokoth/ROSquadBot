alter table public.user
add column if not exists "number" int not null default false;