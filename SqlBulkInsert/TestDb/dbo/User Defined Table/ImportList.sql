-- Copyright (c) KhooverSoft. All rights reserved.
-- Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

CREATE TYPE [dbo].[ImportList] AS TABLE
(
    [Id] [dbo].[IdType] NOT NULL,
    [Variable] [dbo].[VariableType] NOT NULL,
    [Description] [dbo].[DesType] NULL
);