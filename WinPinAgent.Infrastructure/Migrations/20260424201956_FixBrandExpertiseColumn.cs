using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinPinAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBrandExpertiseColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""Users"" 
        DROP COLUMN ""BrandExpertise"";
        
        ALTER TABLE ""Users"" 
        ADD COLUMN ""BrandExpertise"" text[] NOT NULL DEFAULT '{}';
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""Users"" 
        ALTER COLUMN ""BrandExpertise"" TYPE jsonb 
        USING ""BrandExpertise""::jsonb;
    ");
        }
    }
}
