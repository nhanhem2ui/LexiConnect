using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class Fix_key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Majors_MajorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentReviews_AspNetUsers_ReviewerId1",
                table: "DocumentReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTags_Documents_DocumentId",
                table: "DocumentTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Majors_Universities_UniversityId",
                table: "Majors");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_AspNetUsers_UserId1",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_Countries_CountryId",
                table: "Universities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_AspNetUsers_UserId1",
                table: "UserFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Documents_document_id",
                table: "UserFavorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFollowers",
                table: "UserFollowers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_UserFavorites_UserId1",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_UserId1",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTags_DocumentId",
                table: "DocumentTags");

            migrationBuilder.DropIndex(
                name: "IX_DocumentReviews_ReviewerId1",
                table: "DocumentReviews");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PointTransactions");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "DocumentTags");

            migrationBuilder.DropColumn(
                name: "ReviewerId1",
                table: "DocumentReviews");

            migrationBuilder.RenameColumn(
                name: "document_id",
                table: "UserFavorites",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserFavorites",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_document_id",
                table: "UserFavorites",
                newName: "IX_UserFavorites_DocumentId");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserFollowers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 1);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserFavorites",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RecentVieweds",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ViewedAt",
                table: "RecentVieweds",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "user_id",
                table: "PointTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "reviewer_id",
                table: "DocumentReviews",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFollowers",
                table: "UserFollowers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DocumentDocumentTags",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    TagsTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentDocumentTags", x => new { x.DocumentId, x.TagsTagId });
                    table.ForeignKey(
                        name: "FK_DocumentDocumentTags_DocumentTags_TagsTagId",
                        column: x => x.TagsTagId,
                        principalTable: "DocumentTags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentDocumentTags_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFollowers_FollowerId_FollowingId",
                table: "UserFollowers",
                columns: new[] { "FollowerId", "FollowingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UserId_DocumentId",
                table: "UserFavorites",
                columns: new[] { "UserId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecentVieweds_UserId",
                table: "RecentVieweds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_user_id",
                table: "PointTransactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReviews_reviewer_id",
                table: "DocumentReviews",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDocumentTags_TagsTagId",
                table: "DocumentDocumentTags",
                column: "TagsTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Majors_MajorId",
                table: "AspNetUsers",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId",
                table: "AspNetUsers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReviews_AspNetUsers_reviewer_id",
                table: "DocumentReviews",
                column: "reviewer_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_Universities_UniversityId",
                table: "Majors",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_AspNetUsers_user_id",
                table: "PointTransactions",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecentVieweds_AspNetUsers_UserId",
                table: "RecentVieweds",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_Countries_CountryId",
                table: "Universities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_AspNetUsers_UserId",
                table: "UserFavorites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Documents_DocumentId",
                table: "UserFavorites",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Majors_MajorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentReviews_AspNetUsers_reviewer_id",
                table: "DocumentReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Majors_Universities_UniversityId",
                table: "Majors");

            migrationBuilder.DropForeignKey(
                name: "FK_PointTransactions_AspNetUsers_user_id",
                table: "PointTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RecentVieweds_AspNetUsers_UserId",
                table: "RecentVieweds");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_Countries_CountryId",
                table: "Universities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_AspNetUsers_UserId",
                table: "UserFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Documents_DocumentId",
                table: "UserFavorites");

            migrationBuilder.DropTable(
                name: "DocumentDocumentTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFollowers",
                table: "UserFollowers");

            migrationBuilder.DropIndex(
                name: "IX_UserFollowers_FollowerId_FollowingId",
                table: "UserFollowers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_UserFavorites_UserId_DocumentId",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_RecentVieweds_UserId",
                table: "RecentVieweds");

            migrationBuilder.DropIndex(
                name: "IX_PointTransactions_user_id",
                table: "PointTransactions");

            migrationBuilder.DropIndex(
                name: "IX_DocumentReviews_reviewer_id",
                table: "DocumentReviews");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserFollowers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserFavorites");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RecentVieweds");

            migrationBuilder.DropColumn(
                name: "ViewedAt",
                table: "RecentVieweds");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserFavorites",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "UserFavorites",
                newName: "document_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_DocumentId",
                table: "UserFavorites",
                newName: "IX_UserFavorites_document_id");

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)")
                .Annotation("Relational:ColumnOrder", 0);

            migrationBuilder.AlterColumn<int>(
                name: "document_id",
                table: "UserFavorites",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "UserFavorites",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "user_id",
                table: "PointTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "PointTransactions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "DocumentTags",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "reviewer_id",
                table: "DocumentReviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ReviewerId1",
                table: "DocumentReviews",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFollowers",
                table: "UserFollowers",
                columns: new[] { "FollowerId", "FollowingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites",
                columns: new[] { "user_id", "document_id" });

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 1,
                column: "DocumentId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 2,
                column: "DocumentId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 3,
                column: "DocumentId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 4,
                column: "DocumentId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 5,
                column: "DocumentId",
                value: null);

            migrationBuilder.UpdateData(
                table: "DocumentTags",
                keyColumn: "TagId",
                keyValue: 6,
                column: "DocumentId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UserId1",
                table: "UserFavorites",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId1",
                table: "PointTransactions",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTags_DocumentId",
                table: "DocumentTags",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReviews_ReviewerId1",
                table: "DocumentReviews",
                column: "ReviewerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Majors_MajorId",
                table: "AspNetUsers",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId",
                table: "AspNetUsers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentReviews_AspNetUsers_ReviewerId1",
                table: "DocumentReviews",
                column: "ReviewerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTags_Documents_DocumentId",
                table: "DocumentTags",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Majors_Universities_UniversityId",
                table: "Majors",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PointTransactions_AspNetUsers_UserId1",
                table: "PointTransactions",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Universities_Countries_CountryId",
                table: "Universities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_AspNetUsers_UserId1",
                table: "UserFavorites",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorites_Documents_document_id",
                table: "UserFavorites",
                column: "document_id",
                principalTable: "Documents",
                principalColumn: "DocumentId");
        }
    }
}
