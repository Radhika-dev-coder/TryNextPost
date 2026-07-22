using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TryNextPost.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncAuthAndWeightPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Manual auth migration was never applied; add columns if missing (safe re-run).
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[UserSessions]') AND name = N'RefreshTokenHash')
                BEGIN
                    ALTER TABLE [UserSessions] ADD [RefreshTokenHash] nvarchar(max) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[UserSessions]') AND name = N'RefreshTokenExpiryAt')
                BEGIN
                    ALTER TABLE [UserSessions] ADD [RefreshTokenExpiryAt] datetime2 NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[UserSessions]') AND name = N'RefreshTokenHash')
                BEGIN
                    ALTER TABLE [UserSessions] DROP COLUMN [RefreshTokenHash];
                END
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[UserSessions]') AND name = N'RefreshTokenExpiryAt')
                BEGIN
                    ALTER TABLE [UserSessions] DROP COLUMN [RefreshTokenExpiryAt];
                END
                """);
        }
    }
}
