using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideGo_Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveAndUpdatedAtToTours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "tours",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "tours",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "tours");
        }
    }
}
