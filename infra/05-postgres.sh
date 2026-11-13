# 查询所有的 Sequence
SELECT 
    schemaname,
    sequencename,
    data_type,
    last_value,        -- 序列最后一次生成的值
    start_value,
    increment_by
FROM pg_sequences
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY schemaname, sequencename;

# 将序列重新设为从 1 开始
ALTER SEQUENCE person_id_seq RESTART;
ALTER SEQUENCE core_organization_id_seq RESTART;

# Add default permission / permission items for CRM
INSERT INTO permission_group (name, roles, items)
	VALUES ('Administrators', 28672, '{1000,2000,3000,6000,7000,8000,9000,10000,11000,18000}')

INSERT INTO public.permission_item VALUES (1000, 1, 'All');
INSERT INTO public.permission_item VALUES (2000, 2, 'All');
INSERT INTO public.permission_item VALUES (2001, 2, 'List');
INSERT INTO public.permission_item VALUES (2002, 2, 'Query');
INSERT INTO public.permission_item VALUES (2003, 2, 'View');
INSERT INTO public.permission_item VALUES (2011, 2, 'Add');
INSERT INTO public.permission_item VALUES (2012, 2, 'Edit');
INSERT INTO public.permission_item VALUES (2013, 2, 'Delete');
INSERT INTO public.permission_item VALUES (2083, 2, 'AddContact');
INSERT INTO public.permission_item VALUES (2091, 2, 'QueryProfile');
INSERT INTO public.permission_item VALUES (2092, 2, 'ViewProfile');
INSERT INTO public.permission_item VALUES (2093, 2, 'AddProfile');
INSERT INTO public.permission_item VALUES (2097, 2, 'AddComment');
INSERT INTO public.permission_item VALUES (3000, 3, 'All');
INSERT INTO public.permission_item VALUES (6000, 6, 'All');
INSERT INTO public.permission_item VALUES (7000, 7, 'All');
INSERT INTO public.permission_item VALUES (8000, 8, 'All');
INSERT INTO public.permission_item VALUES (9000, 9, 'All');
INSERT INTO public.permission_item VALUES (10000, 10, 'All');
INSERT INTO public.permission_item VALUES (11000, 11, 'All');
INSERT INTO public.permission_item VALUES (18000, 18, 'All');

# Reverse engineering and model visualization tools for EF Core in Visual Studio
# https://github.com/ErikEJ/EFCorePowerTools, Install it and EF Core Power Pack with Extension Manager