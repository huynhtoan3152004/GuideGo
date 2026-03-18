using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideGo_Repository.Migrations
{
    /// <inheritdoc />
    public partial class PaymentOneToManyBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "payments_booking_id_fkey",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_booking_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "booking_id",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "payment_id",
                table: "bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_bookings_payment_id",
                table: "bookings",
                column: "payment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_payments_payment_id",
                table: "bookings",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_payments_payment_id",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "ix_bookings_payment_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "payment_id",
                table: "bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "booking_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                table: "payments",
                column: "booking_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_bookings_booking_id",
                table: "payments",
                column: "booking_id",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
