using Microsoft.EntityFrameworkCore.Migrations;

namespace codes_netCore.Migrations
{
    public partial class removeNotNullOnCodeValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Codes",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Codes",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
