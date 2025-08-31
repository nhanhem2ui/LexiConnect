using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObjects.Migrations
{
    /// <inheritdoc />
    public partial class UserFollower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Major_MajorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_SubscriptionPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_University_UniversityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Course_Major_MajorId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_AspNetUsers_ApprovedBy",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_AspNetUsers_UploaderId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Course_CourseId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTag_Document_DocumentId",
                table: "DocumentTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Major_University_UniversityId",
                table: "Major");

            migrationBuilder.DropForeignKey(
                name: "FK_University_Country_CountryId",
                table: "University");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorite_AspNetUsers_UserId1",
                table: "UserFavorite");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorite_Document_document_id",
                table: "UserFavorite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavorite",
                table: "UserFavorite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_University",
                table: "University");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlan",
                table: "SubscriptionPlan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Major",
                table: "Major");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentTag",
                table: "DocumentTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Document",
                table: "Document");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Course",
                table: "Course");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Country",
                table: "Country");

            migrationBuilder.RenameTable(
                name: "UserFavorite",
                newName: "UserFavorites");

            migrationBuilder.RenameTable(
                name: "University",
                newName: "Universities");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlan",
                newName: "SubscriptionPlans");

            migrationBuilder.RenameTable(
                name: "Major",
                newName: "Majors");

            migrationBuilder.RenameTable(
                name: "DocumentTag",
                newName: "DocumentTags");

            migrationBuilder.RenameTable(
                name: "Document",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "Course",
                newName: "Courses");

            migrationBuilder.RenameTable(
                name: "Country",
                newName: "Countries");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorite_UserId1",
                table: "UserFavorites",
                newName: "IX_UserFavorites_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorite_document_id",
                table: "UserFavorites",
                newName: "IX_UserFavorites_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_University_CountryId",
                table: "Universities",
                newName: "IX_Universities_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlan_Name",
                table: "SubscriptionPlans",
                newName: "IX_SubscriptionPlans_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Major_UniversityId",
                table: "Majors",
                newName: "IX_Majors_UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_Major_Code_UniversityId",
                table: "Majors",
                newName: "IX_Majors_Code_UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTag_Name",
                table: "DocumentTags",
                newName: "IX_DocumentTags_Name");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTag_DocumentId",
                table: "DocumentTags",
                newName: "IX_DocumentTags_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_UploaderId",
                table: "Documents",
                newName: "IX_Documents_UploaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_CourseId",
                table: "Documents",
                newName: "IX_Documents_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_ApprovedBy",
                table: "Documents",
                newName: "IX_Documents_ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Course_MajorId",
                table: "Courses",
                newName: "IX_Courses_MajorId");

            migrationBuilder.RenameIndex(
                name: "IX_Course_CourseCode_MajorId",
                table: "Courses",
                newName: "IX_Courses_CourseCode_MajorId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId1",
                table: "UserFavorites",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites",
                columns: new[] { "user_id", "document_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Universities",
                table: "Universities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans",
                column: "PlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Majors",
                table: "Majors",
                column: "MajorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentTags",
                table: "DocumentTags",
                column: "TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courses",
                table: "Courses",
                column: "CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DocumentReviews",
                columns: table => new
                {
                    review_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    document_id = table.Column<int>(type: "int", nullable: false),
                    reviewer_id = table.Column<int>(type: "int", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    helpful_count = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewerId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentReviews", x => x.review_id);
                    table.ForeignKey(
                        name: "FK_DocumentReviews_AspNetUsers_ReviewerId1",
                        column: x => x.ReviewerId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentReviews_Documents_document_id",
                        column: x => x.document_id,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    transaction_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    points_change = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    reference_id = table.Column<int>(type: "int", nullable: true),
                    reference_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    balance_after = table.Column<int>(type: "int", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.transaction_id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecentVieweds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentVieweds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecentVieweds_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecentVieweds_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "DocumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFollowers",
                columns: table => new
                {
                    FollowerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FollowingId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollowers", x => new { x.FollowerId, x.FollowingId });
                    table.ForeignKey(
                        name: "FK_UserFollowers_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFollowers_AspNetUsers_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReviews_document_id",
                table: "DocumentReviews",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentReviews_ReviewerId1",
                table: "DocumentReviews",
                column: "ReviewerId1");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId1",
                table: "PointTransactions",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_RecentVieweds_CourseId",
                table: "RecentVieweds",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_RecentVieweds_DocumentId",
                table: "RecentVieweds",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollowers_FollowingId",
                table: "UserFollowers",
                column: "FollowingId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Majors_MajorId",
                table: "AspNetUsers",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlans_SubscriptionPlanId",
                table: "AspNetUsers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Universities_UniversityId",
                table: "AspNetUsers",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_UploaderId",
                table: "Documents",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Courses_CourseId",
                table: "Documents",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId");

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
                name: "FK_AspNetUsers_Universities_UniversityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Majors_MajorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_ApprovedBy",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_UploaderId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Courses_CourseId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTags_Documents_DocumentId",
                table: "DocumentTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Majors_Universities_UniversityId",
                table: "Majors");

            migrationBuilder.DropForeignKey(
                name: "FK_Universities_Countries_CountryId",
                table: "Universities");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_AspNetUsers_UserId1",
                table: "UserFavorites");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavorites_Documents_document_id",
                table: "UserFavorites");

            migrationBuilder.DropTable(
                name: "DocumentReviews");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "RecentVieweds");

            migrationBuilder.DropTable(
                name: "UserFollowers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavorites",
                table: "UserFavorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Universities",
                table: "Universities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubscriptionPlans",
                table: "SubscriptionPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Majors",
                table: "Majors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentTags",
                table: "DocumentTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.RenameTable(
                name: "UserFavorites",
                newName: "UserFavorite");

            migrationBuilder.RenameTable(
                name: "Universities",
                newName: "University");

            migrationBuilder.RenameTable(
                name: "SubscriptionPlans",
                newName: "SubscriptionPlan");

            migrationBuilder.RenameTable(
                name: "Majors",
                newName: "Major");

            migrationBuilder.RenameTable(
                name: "DocumentTags",
                newName: "DocumentTag");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Document");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Course");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Country");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_UserId1",
                table: "UserFavorite",
                newName: "IX_UserFavorite_UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_UserFavorites_document_id",
                table: "UserFavorite",
                newName: "IX_UserFavorite_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_Universities_CountryId",
                table: "University",
                newName: "IX_University_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_SubscriptionPlans_Name",
                table: "SubscriptionPlan",
                newName: "IX_SubscriptionPlan_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Majors_UniversityId",
                table: "Major",
                newName: "IX_Major_UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_Majors_Code_UniversityId",
                table: "Major",
                newName: "IX_Major_Code_UniversityId");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTags_Name",
                table: "DocumentTag",
                newName: "IX_DocumentTag_Name");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentTags_DocumentId",
                table: "DocumentTag",
                newName: "IX_DocumentTag_DocumentId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_UploaderId",
                table: "Document",
                newName: "IX_Document_UploaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_CourseId",
                table: "Document",
                newName: "IX_Document_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_ApprovedBy",
                table: "Document",
                newName: "IX_Document_ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_MajorId",
                table: "Course",
                newName: "IX_Course_MajorId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_CourseCode_MajorId",
                table: "Course",
                newName: "IX_Course_CourseCode_MajorId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId1",
                table: "UserFavorite",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavorite",
                table: "UserFavorite",
                columns: new[] { "user_id", "document_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_University",
                table: "University",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubscriptionPlan",
                table: "SubscriptionPlan",
                column: "PlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Major",
                table: "Major",
                column: "MajorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentTag",
                table: "DocumentTag",
                column: "TagId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Document",
                table: "Document",
                column: "DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Course",
                table: "Course",
                column: "CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Country",
                table: "Country",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Major_MajorId",
                table: "AspNetUsers",
                column: "MajorId",
                principalTable: "Major",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_SubscriptionPlanId",
                table: "AspNetUsers",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "PlanId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_University_UniversityId",
                table: "AspNetUsers",
                column: "UniversityId",
                principalTable: "University",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Major_MajorId",
                table: "Course",
                column: "MajorId",
                principalTable: "Major",
                principalColumn: "MajorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_AspNetUsers_ApprovedBy",
                table: "Document",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_AspNetUsers_UploaderId",
                table: "Document",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Course_CourseId",
                table: "Document",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTag_Document_DocumentId",
                table: "DocumentTag",
                column: "DocumentId",
                principalTable: "Document",
                principalColumn: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Major_University_UniversityId",
                table: "Major",
                column: "UniversityId",
                principalTable: "University",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_University_Country_CountryId",
                table: "University",
                column: "CountryId",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorite_AspNetUsers_UserId1",
                table: "UserFavorite",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavorite_Document_document_id",
                table: "UserFavorite",
                column: "document_id",
                principalTable: "Document",
                principalColumn: "DocumentId");
        }
    }
}
