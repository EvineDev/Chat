using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Migrations
{
    public partial class AddedAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.AddColumn<byte[]>(
                name: "Password",
                table: "Users",
                nullable: false,
                defaultValue: new byte[] { });

            migrationBuilder.AddColumn<byte[]>(
                name: "Salt",
                table: "Users",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "Sessions");

            migrationBuilder.AddColumn<byte[]>(
                name: "SessionKey",
                table: "Sessions",
                nullable: false,
                defaultValue: new byte[] { });

            migrationBuilder.AddColumn<byte[]>(
                name: "RefreshKey",
                table: "Sessions",
                nullable: false,
                defaultValue: new byte[] {  });

            migrationBuilder.AddColumn<byte[]>(
                name: "RefreshSalt",
                table: "Sessions",
                nullable: false,
                defaultValue: new byte[] {  });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshKey",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "RefreshSalt",
                table: "Sessions");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte[]));

            migrationBuilder.AlterColumn<string>(
                name: "SessionKey",
                table: "Sessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(byte[]));
        }
    }
}
