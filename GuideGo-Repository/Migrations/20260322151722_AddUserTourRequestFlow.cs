using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideGo_Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTourRequestFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "guide_request_status",
                table: "tours",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("UPDATE tours SET guide_request_status = 'Accepted' WHERE guide_request_status IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "guide_request_status",
                table: "tours",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_custom_request",
                table: "tours",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "requested_by_user_id",
                table: "tours",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guide_request_status",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "is_custom_request",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "requested_by_user_id",
                table: "tours");
        }
    }
}
