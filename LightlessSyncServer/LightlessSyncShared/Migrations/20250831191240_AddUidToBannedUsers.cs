using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LightlessSyncServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUidToBannedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "banned_uid",
                table: "banned_users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "banned_uid",
                table: "banned_users",
                type: "character varying(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
