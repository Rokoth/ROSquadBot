create table if not exists public.task_settings(
	id int not null,
	code int not null,
	"name" varchar not null,
	"value" varchar not null
);

insert into public.task_settings(id, code, "name", "value")
values(1, 1, 'ShowResults', 'false');

insert into public.task_settings(id, code, "name", "value")
values(2, 2, 'ShowStrictResults', 'true');