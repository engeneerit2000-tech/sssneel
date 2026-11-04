using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations;

/// <summary>
/// Supports the case-insensitive invoice-reference lookup used while moving Oracle
/// invoice staging rows into the platform invoice table.
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20261028060000_AddNormalizedInvoiceRefIndex")]
public partial class AddNormalizedInvoiceRefIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Concurrent index operations must run outside the migration transaction so
        // production reads and writes are not blocked for the duration of the build.
        migrationBuilder.Sql(
            """
            DROP INDEX CONCURRENTLY IF EXISTS "IX_Invoices_InvoiceRef_Lower";
            """,
            suppressTransaction: true);

        migrationBuilder.Sql(
            """
            CREATE INDEX CONCURRENTLY "IX_Invoices_InvoiceRef_Lower"
                ON "Invoices" (lower("InvoiceRef"));
            """,
            suppressTransaction: true);

        migrationBuilder.Sql(
            """
            ANALYZE "Invoices";
            """,
            suppressTransaction: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP INDEX CONCURRENTLY IF EXISTS "IX_Invoices_InvoiceRef_Lower";
            """,
            suppressTransaction: true);
    }
}
