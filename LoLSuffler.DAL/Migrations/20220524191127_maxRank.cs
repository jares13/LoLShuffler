using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoLShuffler.DAL.Migrations
{
    public partial class maxRank : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxRang",
                table: "Filters",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRang",
                table: "Filters");
        }
    }
}
