-- Copyright (c) KhooverSoft. All rights reserved.
-- Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

CREATE TABLE [App].[Import2]
(
    [Id] [dbo].[IdType] NOT NULL PRIMARY KEY,
    [Variable] [dbo].[IntValueType] NOT NULL,
    [Description] [dbo].[DesType] NULL,
);
