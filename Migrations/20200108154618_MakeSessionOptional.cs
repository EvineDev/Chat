using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Chat.Migrations
{
    public partial class MakeSessionOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.AddColumn<Guid>(
				name: "UserId",
				table: "Messages",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.Sql("update \"Messages\" as m set \"UserId\" = (select s.\"UserId\" from \"Sessions\" as s where m.\"SessionId\" = s.\"Id\" limit 1)");

			migrationBuilder.DropForeignKey(
                name: "FK_Messages_Sessions_SessionId",
                table: "Messages");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Messages",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

			migrationBuilder.CreateIndex(
				name: "IX_Messages_UserId",
				table: "Messages",
				column: "UserId");

			migrationBuilder.AddForeignKey(
				name: "FK_Messages_Sessions_SessionId",
				table: "Messages",
				column: "SessionId",
				principalTable: "Sessions",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_Messages_Users_UserId",
				table: "Messages",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Sessions_SessionId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_UserId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_UserId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Messages");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "Messages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Sessions_SessionId",
                table: "Messages",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
