-- Copyright (c) KhooverSoft. All rights reserved.
-- Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

CREATE PROCEDURE [App].[Reset]
AS
BEGIN
    SET NOCOUNT ON;

    TRUNCATE TABLE [App].[Import];
    TRUNCATE TABLE [App].[Import2];
END