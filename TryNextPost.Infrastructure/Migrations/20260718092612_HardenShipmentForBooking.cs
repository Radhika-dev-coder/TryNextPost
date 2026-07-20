using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenShipmentForBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: prior failed run already dropped this index.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Shipments_AwbNumber'
                      AND object_id = OBJECT_ID(N'[dbo].[Shipments]')
                )
                    DROP INDEX [IX_Shipments_AwbNumber] ON [Shipments];
                """);

            // Convert bigint/long → nvarchar(100) only when still numeric.
            // Separate Sql() calls are required: SQL Server compiles an entire batch
            // before execution, so ADD + UPDATE of a new column in one batch fails
            // with "Invalid column name 'AwbNumberTemp'".
            // Dynamic SQL (EXEC) is used where a statement references a column that
            // may not exist yet / anymore, because IF alone does not skip compile-time checks.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                      AND c.name = N'AwbNumber'
                      AND t.name IN (N'bigint', N'int', N'smallint', N'tinyint', N'decimal', N'numeric', N'float', N'real')
                )
                AND COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [AwbNumberTemp] nvarchar(100) NULL');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND EXISTS (
                       SELECT 1
                       FROM sys.columns c
                       INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                       WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                         AND c.name = N'AwbNumber'
                         AND t.name IN (N'bigint', N'int', N'smallint', N'tinyint', N'decimal', N'numeric', N'float', N'real')
                   )
                    EXEC(N'
                        UPDATE [Shipments]
                        SET [AwbNumberTemp] = CASE
                            WHEN [AwbNumber] = 0 THEN NULL
                            ELSE CONVERT(nvarchar(100), [AwbNumber])
                        END;
                    ');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND EXISTS (
                       SELECT 1
                       FROM sys.columns c
                       INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                       WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                         AND c.name = N'AwbNumber'
                         AND t.name IN (N'bigint', N'int', N'smallint', N'tinyint', N'decimal', N'numeric', N'float', N'real')
                   )
                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [AwbNumber]');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumber') IS NULL
                    EXEC sp_rename N'[Shipments].[AwbNumberTemp]', N'AwbNumber', N'COLUMN';
                """);

            // Add booking columns if missing (idempotent). Use EXEC so compile is deferred.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'ChargedAmount') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [ChargedAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Shipments_ChargedAmount] DEFAULT (0)');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'CourierReference') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [CourierReference] nvarchar(100) NULL');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'LabelUrl') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [LabelUrl] nvarchar(max) NULL');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'ServiceCode') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [ServiceCode] nvarchar(100) NULL');
                """);

            // Recreate filtered unique index if missing.
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Shipments_AwbNumber'
                      AND object_id = OBJECT_ID(N'[dbo].[Shipments]')
                )
                    EXEC(N'
                        CREATE UNIQUE INDEX [IX_Shipments_AwbNumber]
                        ON [Shipments] ([AwbNumber])
                        WHERE [AwbNumber] IS NOT NULL;
                    ');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Shipments_AwbNumber'
                      AND object_id = OBJECT_ID(N'[dbo].[Shipments]')
                )
                    DROP INDEX [IX_Shipments_AwbNumber] ON [Shipments];
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'ChargedAmount') IS NOT NULL
                BEGIN
                    DECLARE @df sysname;
                    SELECT @df = dc.name
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c
                        ON dc.parent_object_id = c.object_id
                       AND dc.parent_column_id = c.column_id
                    WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[Shipments]')
                      AND c.name = N'ChargedAmount';

                    IF @df IS NOT NULL
                        EXEC(N'ALTER TABLE [Shipments] DROP CONSTRAINT [' + @df + N']');

                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [ChargedAmount]');
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'CourierReference') IS NOT NULL
                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [CourierReference]');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'LabelUrl') IS NOT NULL
                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [LabelUrl]');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'ServiceCode') IS NOT NULL
                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [ServiceCode]');
                """);

            // Best-effort rollback: nvarchar → bigint. Separate batches + dynamic SQL.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                      AND c.name = N'AwbNumber'
                      AND t.name IN (N'nvarchar', N'varchar', N'nchar', N'char')
                )
                AND COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NULL
                    EXEC(N'ALTER TABLE [Shipments] ADD [AwbNumberTemp] bigint NOT NULL CONSTRAINT [DF_Shipments_AwbNumberTemp] DEFAULT (0)');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND EXISTS (
                       SELECT 1
                       FROM sys.columns c
                       INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                       WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                         AND c.name = N'AwbNumber'
                         AND t.name IN (N'nvarchar', N'varchar', N'nchar', N'char')
                   )
                    EXEC(N'
                        UPDATE [Shipments]
                        SET [AwbNumberTemp] = CASE
                            WHEN [AwbNumber] IS NULL OR TRY_CONVERT(bigint, [AwbNumber]) IS NULL THEN 0
                            ELSE TRY_CONVERT(bigint, [AwbNumber])
                        END;
                    ');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND EXISTS (
                       SELECT 1
                       FROM sys.columns c
                       INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                       WHERE c.object_id = OBJECT_ID(N'[dbo].[Shipments]')
                         AND c.name = N'AwbNumber'
                         AND t.name IN (N'nvarchar', N'varchar', N'nchar', N'char')
                   )
                    EXEC(N'ALTER TABLE [Shipments] DROP COLUMN [AwbNumber]');
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumberTemp') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Shipments]', N'AwbNumber') IS NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.default_constraints
                        WHERE name = N'DF_Shipments_AwbNumberTemp'
                          AND parent_object_id = OBJECT_ID(N'[dbo].[Shipments]')
                    )
                        ALTER TABLE [Shipments] DROP CONSTRAINT [DF_Shipments_AwbNumberTemp];

                    EXEC sp_rename N'[Shipments].[AwbNumberTemp]', N'AwbNumber', N'COLUMN';
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_Shipments_AwbNumber'
                      AND object_id = OBJECT_ID(N'[dbo].[Shipments]')
                )
                    EXEC(N'CREATE UNIQUE INDEX [IX_Shipments_AwbNumber] ON [Shipments] ([AwbNumber])');
                """);
        }
    }
}
