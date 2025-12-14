using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballField.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewImagesAndHelpfulVotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "REVIEW_HELPFUL_VOTE",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEW_HELPFUL_VOTE", x => x.id);
                    table.ForeignKey(
                        name: "FK_REVIEW_HELPFUL_VOTE_REVIEW_review_id",
                        column: x => x.review_id,
                        principalTable: "REVIEW",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_REVIEW_HELPFUL_VOTE_USER_user_id",
                        column: x => x.user_id,
                        principalTable: "USER",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "REVIEW_IMAGE",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<int>(type: "int", nullable: false),
                    image_url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "DATEADD(HOUR, 7, GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REVIEW_IMAGE", x => x.id);
                    table.ForeignKey(
                        name: "FK_REVIEW_IMAGE_REVIEW_review_id",
                        column: x => x.review_id,
                        principalTable: "REVIEW",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_REVIEW_HELPFUL_VOTE_user_id",
                table: "REVIEW_HELPFUL_VOTE",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHelpfulVote_ReviewId_UserId",
                table: "REVIEW_HELPFUL_VOTE",
                columns: new[] { "review_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImage_ReviewId",
                table: "REVIEW_IMAGE",
                column: "review_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "REVIEW_HELPFUL_VOTE");

            migrationBuilder.DropTable(
                name: "REVIEW_IMAGE");
        }
    }
}
