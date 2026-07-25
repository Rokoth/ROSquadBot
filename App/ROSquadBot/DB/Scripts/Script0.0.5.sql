create table if not exists newscommand(
	id uuid not null default uuid_generate_v4(),
	title text not null,
	description text not null,
	state int not null,
	creatorid uuid not null,
	createddate timestamp not null,
	type int not null,
	begindate timestamp not null,
	enddate timestamp not null,
	number int not null,
	is_deleted boolean not nul default false
);

create table if not exists newscommandmessage(
	id uuid not null default uuid_generate_v4(),
	newsid uuid not null,
	tgmessageid bigint not null,
	textvalue text,
	is_deleted boolean not nul default false
};