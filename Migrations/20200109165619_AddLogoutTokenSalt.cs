using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Migrations
{
    public partial class AddLogoutTokenSalt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "TokenSalt",
                table: "LogoutToken",
                nullable: false,
                defaultValue: new byte[] {  });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenSalt",
                table: "LogoutToken");
        }
    }
}
