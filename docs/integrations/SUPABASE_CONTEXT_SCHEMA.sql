create schema ai_context;
revoke all on schema ai_context from public, anon, authenticated;
create table ai_context.projects (
 project_key text primary key,
 name text not null,
 repository_url text,
 revision bigint not null default 0 check(revision>=0),
 created_at timestamptz not null default now(),
 updated_at timestamptz not null default now()
);
create table ai_context.checkpoints (
 project_key text not null references ai_context.projects(project_key),
 revision bigint not null check(revision>0),
 payload jsonb not null check(jsonb_typeof(payload)='object'),
 writer text not null check(length(writer)>0),
 source_metadata jsonb not null default '{}'::jsonb check(jsonb_typeof(source_metadata)='object'),
 created_at timestamptz not null default now(),
 primary key(project_key,revision)
);
create table ai_context.tasks (
 project_key text not null references ai_context.projects(project_key),
 task_key text not null,
 title text not null,
 status text not null check(status in ('pending','in_progress','blocked','implemented_pending_validation','validated','done','cancelled')),
 details jsonb not null default '{}'::jsonb check(jsonb_typeof(details)='object'),
 dependencies jsonb not null default '[]'::jsonb check(jsonb_typeof(dependencies)='array'),
 evidence jsonb not null default '[]'::jsonb check(jsonb_typeof(evidence)='array'),
 version bigint not null default 0 check(version>=0),
 lease_owner text,
 lease_token uuid,
 lease_until timestamptz,
 updated_at timestamptz not null default now(),
 primary key(project_key,task_key),
 check((lease_owner is null and lease_token is null and lease_until is null) or (lease_owner is not null and lease_token is not null and lease_until is not null))
);
create index tasks_project_status on ai_context.tasks(project_key,status);
create table ai_context.documents (
 project_key text not null references ai_context.projects(project_key),
 path text not null,
 content text not null,
 sha256 text not null check(sha256 ~ '^[0-9a-f]{64}$'),
 source_commit text not null,
 captured_at timestamptz not null default now(),
 primary key(project_key,path)
);
create table ai_context.events (
 event_id bigint generated always as identity primary key,
 project_key text not null references ai_context.projects(project_key),
 task_key text,
 event_type text not null,
 actor text not null,
 details jsonb not null default '{}'::jsonb check(jsonb_typeof(details)='object'),
 created_at timestamptz not null default now(),
 foreign key(project_key,task_key) references ai_context.tasks(project_key,task_key)
);
create index events_project_task on ai_context.events(project_key,task_key) where task_key is not null;
create index events_project_time on ai_context.events(project_key,created_at desc,event_id desc);
create table ai_context.artifacts (
 project_key text not null references ai_context.projects(project_key),
 artifact_key text not null,
 kind text not null,
 uri text not null,
 source_commit text,
 validation_state text not null,
 details jsonb not null default '{}'::jsonb check(jsonb_typeof(details)='object'),
 primary key(project_key,artifact_key)
);
alter table ai_context.projects enable row level security;
alter table ai_context.checkpoints enable row level security;
alter table ai_context.tasks enable row level security;
alter table ai_context.documents enable row level security;
alter table ai_context.events enable row level security;
alter table ai_context.artifacts enable row level security;
revoke all on all tables in schema ai_context from public, anon, authenticated;
revoke all on all sequences in schema ai_context from public, anon, authenticated;
alter default privileges in schema ai_context revoke all on tables from public, anon, authenticated;
alter default privileges in schema ai_context revoke all on sequences from public, anon, authenticated;
alter default privileges in schema ai_context revoke execute on functions from public, anon, authenticated;

create function ai_context.commit_checkpoint(p_project_key text,p_expected_revision bigint,p_payload jsonb,p_writer text,p_source_metadata jsonb default '{}'::jsonb)
returns bigint language plpgsql security invoker set search_path=pg_catalog as $$
declare current_revision bigint; next_revision bigint;
begin
 select revision into current_revision from ai_context.projects where project_key=p_project_key for update;
 if not found then raise exception 'unknown project'; end if;
 if current_revision is distinct from p_expected_revision then raise exception 'stale checkpoint: expected %, current %',p_expected_revision,current_revision using errcode='40001'; end if;
 if p_payload is null or jsonb_typeof(p_payload)<>'object' or jsonb_typeof(p_payload->'project') is distinct from 'object' or jsonb_typeof(p_payload->'current_state') is distinct from 'object' or jsonb_typeof(p_payload->'resume') is distinct from 'object' or jsonb_typeof(p_payload->'operating_rules') is distinct from 'array' then raise exception 'checkpoint missing required continuity fields'; end if;
 next_revision:=current_revision+1;
 insert into ai_context.checkpoints(project_key,revision,payload,writer,source_metadata) values(p_project_key,next_revision,p_payload,p_writer,p_source_metadata);
 update ai_context.projects set revision=next_revision,updated_at=now() where project_key=p_project_key;
 insert into ai_context.events(project_key,event_type,actor,details) values(p_project_key,'checkpoint_committed',p_writer,jsonb_build_object('revision',next_revision,'sources',p_source_metadata));
 return next_revision;
end $$;

create function ai_context.get_handoff(p_project_key text)
returns jsonb language sql stable security invoker set search_path=pg_catalog as $$
 select jsonb_build_object('project',to_jsonb(p),'checkpoint',to_jsonb(c),'tasks',coalesce((select jsonb_agg(to_jsonb(t) order by t.task_key) from ai_context.tasks t where t.project_key=p.project_key),'[]'::jsonb),'documents',coalesce((select jsonb_agg(jsonb_build_object('path',d.path,'sha256',d.sha256,'source_commit',d.source_commit,'captured_at',d.captured_at) order by d.path) from ai_context.documents d where d.project_key=p.project_key),'[]'::jsonb),'artifacts',coalesce((select jsonb_agg(to_jsonb(a) order by a.artifact_key) from ai_context.artifacts a where a.project_key=p.project_key),'[]'::jsonb),'recent_events',coalesce((select jsonb_agg(to_jsonb(e) order by e.event_id desc) from (select * from ai_context.events where project_key=p.project_key order by event_id desc limit 30)e),'[]'::jsonb))
 from ai_context.projects p left join ai_context.checkpoints c on c.project_key=p.project_key and c.revision=p.revision where p.project_key=p_project_key
$$;

create function ai_context.claim_task(p_project_key text,p_task_key text,p_expected_version bigint,p_actor text,p_seconds integer default 900)
returns jsonb language plpgsql security invoker set search_path=pg_catalog as $$
declare t ai_context.tasks%rowtype; token uuid; lease_now timestamptz;
begin
 if p_seconds is null or p_seconds<60 or p_seconds>3600 or coalesce(length(p_actor),0)=0 then raise exception 'invalid lease'; end if;
 select * into t from ai_context.tasks where project_key=p_project_key and task_key=p_task_key for update;
 if not found then raise exception 'unknown task'; end if;
 if t.version is distinct from p_expected_version then raise exception 'stale task version' using errcode='40001'; end if;
 if t.status not in ('pending','in_progress','blocked','implemented_pending_validation') then raise exception 'task is not claimable'; end if;
 lease_now:=clock_timestamp();
 if t.lease_until>lease_now then raise exception 'task already leased'; end if;
 token:=gen_random_uuid();
 update ai_context.tasks set lease_owner=p_actor,lease_token=token,lease_until=lease_now+make_interval(secs=>p_seconds),version=version+1,updated_at=now() where project_key=p_project_key and task_key=p_task_key returning * into t;
 insert into ai_context.events(project_key,task_key,event_type,actor,details) values(p_project_key,p_task_key,'task_claimed',p_actor,jsonb_build_object('version',t.version,'lease_until',t.lease_until));
 return to_jsonb(t);
end $$;

create function ai_context.update_task(p_project_key text,p_task_key text,p_expected_version bigint,p_lease_token uuid,p_status text,p_details jsonb,p_evidence jsonb,p_actor text,p_release boolean default true)
returns bigint language plpgsql security invoker set search_path=pg_catalog as $$
declare t ai_context.tasks%rowtype; lease_now timestamptz;
begin
 if coalesce(length(p_actor),0)=0 or p_release is null then raise exception 'invalid task update'; end if;
 select * into t from ai_context.tasks where project_key=p_project_key and task_key=p_task_key for update;
 if not found then raise exception 'unknown task'; end if;
 if t.version is distinct from p_expected_version then raise exception 'stale task version' using errcode='40001'; end if;
 lease_now:=clock_timestamp();
 if t.lease_token is distinct from p_lease_token or t.lease_token is null or t.lease_until<=lease_now or t.lease_owner is distinct from p_actor then raise exception 'invalid or expired task lease'; end if;
 update ai_context.tasks set status=p_status,details=p_details,evidence=p_evidence,version=version+1,updated_at=now(),lease_owner=case when p_release then null else lease_owner end,lease_token=case when p_release then null else lease_token end,lease_until=case when p_release then null else lease_until end where project_key=p_project_key and task_key=p_task_key returning * into t;
 insert into ai_context.events(project_key,task_key,event_type,actor,details) values(p_project_key,p_task_key,'task_updated',p_actor,jsonb_build_object('version',t.version,'status',t.status,'released',p_release));
 return t.version;
end $$;
revoke all on all functions in schema ai_context from public, anon, authenticated;
