using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CommentsDownloader.Migrations
{
    public partial class initial_create : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(nullable: true),
                    ModifiedBy = table.Column<string>(nullable: true),
                    Version = table.Column<byte[]>(nullable: true),
                    Deleted = table.Column<DateTime>(nullable: true),
                    RequestUrl = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    TempFileDirectory = table.Column<string>(nullable: true),
                    CommentsFetched = table.Column<bool>(nullable: false),
                    MailSent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentRequests");
        }
    }
}
