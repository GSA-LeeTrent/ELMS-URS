using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace lmsextreg.Migrations
{
    public partial class InitialDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lmsext");

            migrationBuilder.CreateTable(
                name: "Agency",
                schema: "lmsext",
                columns: table => new
                {
                    AgencyID = table.Column<string>(nullable: false),
                    AgencyName = table.Column<string>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    OPMCode = table.Column<string>(nullable: true),
                    TreasuryCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agency", x => x.AgencyID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "lmsext",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentStatus",
                schema: "lmsext",
                columns: table => new
                {
                    StatusCode = table.Column<string>(nullable: false),
                    StatusLabel = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentStatus", x => x.StatusCode);
                });

            migrationBuilder.CreateTable(
                name: "EventType",
                schema: "lmsext",
                columns: table => new
                {
                    EventTypeCode = table.Column<string>(nullable: false),
                    EventTypeLabel = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.EventTypeCode);
                });

            migrationBuilder.CreateTable(
                name: "LMSProgram",
                schema: "lmsext",
                columns: table => new
                {
                    LMSProgramID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CommonInbox = table.Column<string>(nullable: true),
                    LongName = table.Column<string>(nullable: false),
                    ShortName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSProgram", x => x.LMSProgramID);
                });

            migrationBuilder.CreateTable(
                name: "SubAgency",
                schema: "lmsext",
                columns: table => new
                {
                    SubAgencyID = table.Column<string>(nullable: false),
                    AgencyID = table.Column<string>(nullable: false),
                    DisplayOrder = table.Column<int>(nullable: false),
                    OPMCode = table.Column<string>(nullable: true),
                    SubAgencyName = table.Column<string>(nullable: false),
                    TreasuryCode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubAgency", x => x.SubAgencyID);
                    table.ForeignKey(
                        name: "FK_SubAgency_Agency_AgencyID",
                        column: x => x.AgencyID,
                        principalSchema: "lmsext",
                        principalTable: "Agency",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "lmsext",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StatusTransition",
                schema: "lmsext",
                columns: table => new
                {
                    StatusTransitionID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    FromStatusCode = table.Column<string>(nullable: false),
                    ToStatusCode = table.Column<string>(nullable: false),
                    TransitionCode = table.Column<string>(nullable: false),
                    TransitionLabel = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusTransition", x => x.StatusTransitionID);
                    table.ForeignKey(
                        name: "FK_StatusTransition_EnrollmentStatus_FromStatusCode",
                        column: x => x.FromStatusCode,
                        principalSchema: "lmsext",
                        principalTable: "EnrollmentStatus",
                        principalColumn: "StatusCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatusTransition_EnrollmentStatus_ToStatusCode",
                        column: x => x.ToStatusCode,
                        principalSchema: "lmsext",
                        principalTable: "EnrollmentStatus",
                        principalColumn: "StatusCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "lmsext",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    AgencyID = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    DateAccountExpires = table.Column<DateTime>(nullable: false),
                    DatePasswordExpires = table.Column<DateTime>(nullable: false),
                    DateRegistered = table.Column<DateTime>(nullable: false),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    JobTitle = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    MiddleName = table.Column<string>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    RulesOfBehaviorAgreedTo = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    SubAgencyID = table.Column<string>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Agency_AgencyID",
                        column: x => x.AgencyID,
                        principalSchema: "lmsext",
                        principalTable: "Agency",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_SubAgency_SubAgencyID",
                        column: x => x.SubAgencyID,
                        principalSchema: "lmsext",
                        principalTable: "SubAgency",
                        principalColumn: "SubAgencyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "lmsext",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "lmsext",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "lmsext",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "lmsext",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventLog",
                schema: "lmsext",
                columns: table => new
                {
                    EventLogID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    DataValues = table.Column<string>(nullable: true),
                    DateTimeCreated = table.Column<DateTime>(nullable: false),
                    EventTypeCode = table.Column<string>(nullable: false),
                    UserCreatedID = table.Column<string>(nullable: false),
                    UserCreatedName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.EventLogID);
                    table.ForeignKey(
                        name: "FK_EventLog_EventType_EventTypeCode",
                        column: x => x.EventTypeCode,
                        principalSchema: "lmsext",
                        principalTable: "EventType",
                        principalColumn: "EventTypeCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventLog_AspNetUsers_UserCreatedID",
                        column: x => x.UserCreatedID,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramApprover",
                schema: "lmsext",
                columns: table => new
                {
                    LMSProgramID = table.Column<int>(nullable: false),
                    ApproverUserId = table.Column<string>(nullable: false),
                    EmailNotify = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramApprover", x => new { x.LMSProgramID, x.ApproverUserId });
                    table.ForeignKey(
                        name: "FK_ProgramApprover_AspNetUsers_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramApprover_LMSProgram_LMSProgramID",
                        column: x => x.LMSProgramID,
                        principalSchema: "lmsext",
                        principalTable: "LMSProgram",
                        principalColumn: "LMSProgramID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramEnrollment",
                schema: "lmsext",
                columns: table => new
                {
                    ProgramEnrollmentID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ApproverUserId = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DateLastUpdated = table.Column<DateTime>(nullable: false),
                    LMSProgramID = table.Column<int>(nullable: false),
                    StatusCode = table.Column<string>(nullable: false),
                    StudentUserId = table.Column<string>(nullable: false),
                    UserCreated = table.Column<string>(nullable: false),
                    UserLastUpdated = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramEnrollment", x => x.ProgramEnrollmentID);
                    table.ForeignKey(
                        name: "FK_ProgramEnrollment_AspNetUsers_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProgramEnrollment_LMSProgram_LMSProgramID",
                        column: x => x.LMSProgramID,
                        principalSchema: "lmsext",
                        principalTable: "LMSProgram",
                        principalColumn: "LMSProgramID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramEnrollment_EnrollmentStatus_StatusCode",
                        column: x => x.StatusCode,
                        principalSchema: "lmsext",
                        principalTable: "EnrollmentStatus",
                        principalColumn: "StatusCode",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramEnrollment_AspNetUsers_StudentUserId",
                        column: x => x.StudentUserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnrollmentHistory",
                schema: "lmsext",
                columns: table => new
                {
                    EnrollmentHistoryID = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ActorRemarks = table.Column<string>(nullable: true),
                    ActorUserId = table.Column<string>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    ProgramEnrollmentID = table.Column<int>(nullable: false),
                    StatusTransitionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrollmentHistory", x => x.EnrollmentHistoryID);
                    table.ForeignKey(
                        name: "FK_EnrollmentHistory_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalSchema: "lmsext",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentHistory_ProgramEnrollment_ProgramEnrollmentID",
                        column: x => x.ProgramEnrollmentID,
                        principalSchema: "lmsext",
                        principalTable: "ProgramEnrollment",
                        principalColumn: "ProgramEnrollmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrollmentHistory_StatusTransition_StatusTransitionID",
                        column: x => x.StatusTransitionID,
                        principalSchema: "lmsext",
                        principalTable: "StatusTransition",
                        principalColumn: "StatusTransitionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "lmsext",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "lmsext",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "lmsext",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "lmsext",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "lmsext",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AgencyID",
                schema: "lmsext",
                table: "AspNetUsers",
                column: "AgencyID");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "lmsext",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "lmsext",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SubAgencyID",
                schema: "lmsext",
                table: "AspNetUsers",
                column: "SubAgencyID");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentHistory_ActorUserId",
                schema: "lmsext",
                table: "EnrollmentHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentHistory_ProgramEnrollmentID",
                schema: "lmsext",
                table: "EnrollmentHistory",
                column: "ProgramEnrollmentID");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentHistory_StatusTransitionID",
                schema: "lmsext",
                table: "EnrollmentHistory",
                column: "StatusTransitionID");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentStatus_StatusLabel",
                schema: "lmsext",
                table: "EnrollmentStatus",
                column: "StatusLabel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_EventTypeCode",
                schema: "lmsext",
                table: "EventLog",
                column: "EventTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EventLog_UserCreatedID",
                schema: "lmsext",
                table: "EventLog",
                column: "UserCreatedID");

            migrationBuilder.CreateIndex(
                name: "IX_EventType_EventTypeLabel",
                schema: "lmsext",
                table: "EventType",
                column: "EventTypeLabel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgramApprover_ApproverUserId",
                schema: "lmsext",
                table: "ProgramApprover",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramEnrollment_ApproverUserId",
                schema: "lmsext",
                table: "ProgramEnrollment",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramEnrollment_StatusCode",
                schema: "lmsext",
                table: "ProgramEnrollment",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramEnrollment_StudentUserId",
                schema: "lmsext",
                table: "ProgramEnrollment",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramEnrollment_LMSProgramID_StudentUserId",
                schema: "lmsext",
                table: "ProgramEnrollment",
                columns: new[] { "LMSProgramID", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusTransition_ToStatusCode",
                schema: "lmsext",
                table: "StatusTransition",
                column: "ToStatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_StatusTransition_FromStatusCode_ToStatusCode",
                schema: "lmsext",
                table: "StatusTransition",
                columns: new[] { "FromStatusCode", "ToStatusCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubAgency_AgencyID",
                schema: "lmsext",
                table: "SubAgency",
                column: "AgencyID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "EnrollmentHistory",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "EventLog",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "ProgramApprover",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "ProgramEnrollment",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "StatusTransition",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "EventType",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "LMSProgram",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "EnrollmentStatus",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "SubAgency",
                schema: "lmsext");

            migrationBuilder.DropTable(
                name: "Agency",
                schema: "lmsext");
        }
    }
}
