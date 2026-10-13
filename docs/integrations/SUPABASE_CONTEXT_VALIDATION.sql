begin;
do $$
declare a text:='validation-a-'||gen_random_uuid(); b text:='validation-b-'||gen_random_uuid(); h jsonb; l jsonb; packet jsonb; expected bigint; rejected boolean; r text;
begin
 insert into ai_context.projects(project_key,name,repository_url) values(a,'A',null),(b,'B',null);
 packet:=jsonb_build_object('project',jsonb_build_object('project_key',a),'current_state','{}'::jsonb,'resume','{}'::jsonb,'operating_rules','[]'::jsonb);
 perform ai_context.commit_checkpoint(a,0,packet,'validation');
 foreach expected in array array[0::bigint,null::bigint] loop
  rejected:=false;
  begin perform ai_context.commit_checkpoint(a,expected,packet,'validation'); exception when serialization_failure then rejected:=true; end;
  if not rejected then raise exception 'invalid CAS accepted'; end if;
 end loop;
 if (select revision from ai_context.projects where project_key=a)<>1 or (select count(*) from ai_context.checkpoints where project_key=a)<>1 then raise exception 'rejected CAS changed state'; end if;
 rejected:=false;
 begin perform ai_context.commit_checkpoint(a,1,'{"project":null,"current_state":null,"resume":null,"operating_rules":null}','validation'); exception when raise_exception then rejected:=true; end;
 if not rejected then raise exception 'null fields accepted'; end if;
 insert into ai_context.tasks(project_key,task_key,title,status) values(a,'same','A','pending'),(b,'same','B','pending'),(b,'b-only','B-only','pending');
 foreach expected in array array[5::bigint,null::bigint] loop
  rejected:=false;
  begin perform ai_context.claim_task(a,'same',expected,'validation',60); exception when serialization_failure then rejected:=true; end;
  if not rejected then raise exception 'invalid task version accepted'; end if;
 end loop;
 l:=ai_context.claim_task(a,'same',0,'validation',60);
 rejected:=false;
 begin perform ai_context.claim_task(a,'same',1,'other-ai',60); exception when raise_exception then rejected:=true; end;
 if not rejected then raise exception 'concurrent lease accepted'; end if;
 foreach expected in array array[0::bigint,null::bigint] loop
  rejected:=false;
  begin perform ai_context.update_task(a,'same',expected,(l->>'lease_token')::uuid,'done','{}','[]','validation'); exception when serialization_failure then rejected:=true; end;
  if not rejected then raise exception 'invalid update version accepted'; end if;
 end loop;
 rejected:=false;
 begin perform ai_context.update_task(a,'same',1,gen_random_uuid(),'done','{}','[]','validation'); exception when raise_exception then rejected:=true; end;
 if not rejected then raise exception 'wrong token accepted'; end if;
 update ai_context.tasks set lease_until=clock_timestamp()-interval '1 second' where project_key=a and task_key='same';
 rejected:=false;
 begin perform ai_context.update_task(a,'same',1,(l->>'lease_token')::uuid,'done','{}','[]','validation'); exception when raise_exception then rejected:=true; end;
 if not rejected then raise exception 'expired lease accepted'; end if;
 update ai_context.tasks set lease_until=clock_timestamp()+interval '60 seconds' where project_key=a and task_key='same';
 perform ai_context.update_task(a,'same',1,(l->>'lease_token')::uuid,'done','{}','[]','validation');
 if (select version from ai_context.tasks where project_key=b and task_key='same')<>0 then raise exception 'cross-project task update'; end if;
 rejected:=false;
 begin insert into ai_context.events(project_key,task_key,event_type,actor) values(a,'b-only','test','validation'); exception when foreign_key_violation then rejected:=true; end;
 if not rejected then raise exception 'cross-project FK accepted'; end if;
 insert into ai_context.documents(project_key,path,content,sha256,source_commit) values(a,'same','A',repeat('a',64),'validation'),(b,'same','B',repeat('b',64),'validation');
 insert into ai_context.artifacts(project_key,artifact_key,kind,uri,validation_state) values(a,'same','test','A','test'),(b,'same','test','B','test');
 h:=ai_context.get_handoff(a);
 if jsonb_array_length(h->'tasks')<>1 or h->'tasks'->0->>'title'<>'A' or jsonb_array_length(h->'documents')<>1 or h->'documents'->0->>'sha256'<>repeat('a',64) or jsonb_array_length(h->'artifacts')<>1 or h->'artifacts'->0->>'uri'<>'A' then raise exception 'handoff leaked other project'; end if;
 if (select revision from ai_context.projects where project_key=b)<>0 then raise exception 'checkpoint leaked'; end if;
 foreach r in array array['anon','authenticated'] loop
  rejected:=false;
  execute format('set local role %I',r);
  begin execute 'select ai_context.get_handoff(''huntrx-honmoon'')'; exception when insufficient_privilege then rejected:=true; end;
  execute 'reset role';
  if not rejected then raise exception 'unauthorized read accepted for %',r; end if;
 end loop;
end $$;
rollback;
select 'passed: CAS, NULL guards, task leases, cross-project isolation, unauthorized role denial; test rows rolled back' as validation_result;
