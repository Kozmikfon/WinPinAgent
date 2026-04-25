using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinPinAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "PartRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "PartRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "PartRequests");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "PartRequests");
        }
    }
}
