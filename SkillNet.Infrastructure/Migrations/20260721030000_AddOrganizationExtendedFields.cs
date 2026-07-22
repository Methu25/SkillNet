using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillNet.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260721030000_AddOrganizationExtendedFields")]
    public partial class AddOrganizationExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: safe to run even if columns were already added manually
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'Description')
                    ALTER TABLE [Organization] ADD [Description] nvarchar(max) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'CompanySize')
                    ALTER TABLE [Organization] ADD [CompanySize] nvarchar(50) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'FoundedYear')
                    ALTER TABLE [Organization] ADD [FoundedYear] int NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'ContactEmail')
                    ALTER TABLE [Organization] ADD [ContactEmail] nvarchar(254) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'ContactPhone')
                    ALTER TABLE [Organization] ADD [ContactPhone] nvarchar(30) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'LinkedInUrl')
                    ALTER TABLE [Organization] ADD [LinkedInUrl] nvarchar(255) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'City')
                    ALTER TABLE [Organization] ADD [City] nvarchar(100) NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Organization]') AND name = N'Country')
                    ALTER TABLE [Organization] ADD [Country] nvarchar(100) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Description", table: "Organization");
            migrationBuilder.DropColumn(name: "CompanySize", table: "Organization");
            migrationBuilder.DropColumn(name: "FoundedYear", table: "Organization");
            migrationBuilder.DropColumn(name: "ContactEmail", table: "Organization");
            migrationBuilder.DropColumn(name: "ContactPhone", table: "Organization");
            migrationBuilder.DropColumn(name: "LinkedInUrl", table: "Organization");
            migrationBuilder.DropColumn(name: "City", table: "Organization");
            migrationBuilder.DropColumn(name: "Country", table: "Organization");
        }
    }
}
