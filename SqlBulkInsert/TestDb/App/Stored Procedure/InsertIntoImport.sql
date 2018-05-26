-- Copyright (c) KhooverSoft. All rights reserved.
-- Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

CREATE PROCEDURE [App].[InsertIntoImport]
    @items [dbo].[ImportList] readonly
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [App].[Import] ([Id], [Variable], [Description])
    SELECT  x.[Id]
            ,x.[Variable]
            ,x.[Description]
    FROM    @items x;

END