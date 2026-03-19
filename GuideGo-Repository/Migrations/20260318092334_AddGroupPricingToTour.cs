using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideGo_Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupPricingToTour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "group_price_per_person",
                table: "tours",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_group_size",
                table: "tours",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "group_price_per_person",
                table: "tours");

            migrationBuilder.DropColumn(
                name: "min_group_size",
                table: "tours");
        }
    }
}
