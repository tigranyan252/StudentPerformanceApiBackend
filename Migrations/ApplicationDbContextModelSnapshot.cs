﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StudentPerformance.Api.Data;

#nullable disable

namespace StudentPerformance.Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Assignment", b =>
                {
                    b.Property<int>("AssignmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AssignmentId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("MaxScore")
                        .HasColumnType("decimal(5, 2)");

                    b.Property<DateTime?>("SubmissionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TeacherSubjectGroupAssignmentId")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Type")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("AssignmentId");

                    b.HasIndex("TeacherSubjectGroupAssignmentId");

                    b.ToTable("Assignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Attendance", b =>
                {
                    b.Property<int>("AttendanceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AttendanceId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Remarks")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<int>("TeacherSubjectGroupAssignmentId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("AttendanceId");

                    b.HasIndex("TeacherSubjectGroupAssignmentId");

                    b.HasIndex("StudentId", "TeacherSubjectGroupAssignmentId", "Date")
                        .IsUnique();

                    b.ToTable("Attendances");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Grade", b =>
                {
                    b.Property<int>("GradeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GradeId"));

                    b.Property<int?>("AssignmentId")
                        .HasColumnType("integer");

                    b.Property<string>("ControlType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateReceived")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Notes")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int?>("SemesterId")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<int?>("SubjectId")
                        .HasColumnType("integer");

                    b.Property<int?>("TeacherId")
                        .HasColumnType("integer");

                    b.Property<int>("TeacherSubjectGroupAssignmentId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Value")
                        .HasColumnType("decimal(5, 2)");

                    b.HasKey("GradeId");

                    b.HasIndex("AssignmentId");

                    b.HasIndex("SemesterId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TeacherId");

                    b.HasIndex("TeacherSubjectGroupAssignmentId");

                    b.HasIndex("StudentId", "AssignmentId")
                        .IsUnique()
                        .HasFilter("[AssignmentId] IS NOT NULL");

                    b.ToTable("Grades");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Group", b =>
                {
                    b.Property<int>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GroupId"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("GroupId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RoleId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("RoleId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Semester", b =>
                {
                    b.Property<int>("SemesterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SemesterId"));

                    b.Property<string>("Code")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("SemesterId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Semesters");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Student", b =>
                {
                    b.Property<int>("StudentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("StudentId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EnrollmentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("StudentId");

                    b.HasIndex("GroupId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Students");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Subject", b =>
                {
                    b.Property<int>("SubjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("SubjectId"));

                    b.Property<string>("Code")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("SubjectId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Teacher", b =>
                {
                    b.Property<int>("TeacherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TeacherId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Department")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Position")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("TeacherId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Teachers");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", b =>
                {
                    b.Property<int>("TeacherSubjectGroupAssignmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TeacherSubjectGroupAssignmentId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("GroupId")
                        .HasColumnType("integer");

                    b.Property<int>("SemesterId")
                        .HasColumnType("integer");

                    b.Property<int>("SubjectId")
                        .HasColumnType("integer");

                    b.Property<int>("TeacherId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("TeacherSubjectGroupAssignmentId");

                    b.HasIndex("GroupId");

                    b.HasIndex("SemesterId");

                    b.HasIndex("SubjectId");

                    b.HasIndex("TeacherId", "SubjectId", "GroupId", "SemesterId")
                        .IsUnique();

                    b.ToTable("TeacherSubjectGroupAssignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasMaxLength(150)
                        .HasColumnType("character varying(150)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Assignment", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", "TeacherSubjectGroupAssignment")
                        .WithMany("Assignments")
                        .HasForeignKey("TeacherSubjectGroupAssignmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("TeacherSubjectGroupAssignment");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Attendance", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.Student", "Student")
                        .WithMany("Attendances")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", "TeacherSubjectGroupAssignment")
                        .WithMany("Attendances")
                        .HasForeignKey("TeacherSubjectGroupAssignmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Student");

                    b.Navigation("TeacherSubjectGroupAssignment");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Grade", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.Assignment", "Assignment")
                        .WithMany("Grades")
                        .HasForeignKey("AssignmentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.Semester", "Semester")
                        .WithMany("Grades")
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.Student", "Student")
                        .WithMany("Grades")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("StudentPerformance.Api.Data.Entities.Subject", "Subject")
                        .WithMany("Grades")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.Teacher", "Teacher")
                        .WithMany("Grades")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", "TeacherSubjectGroupAssignment")
                        .WithMany("Grades")
                        .HasForeignKey("TeacherSubjectGroupAssignmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Assignment");

                    b.Navigation("Semester");

                    b.Navigation("Student");

                    b.Navigation("Subject");

                    b.Navigation("Teacher");

                    b.Navigation("TeacherSubjectGroupAssignment");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Student", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.Group", "Group")
                        .WithMany("Students")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.User", "User")
                        .WithOne("Student")
                        .HasForeignKey("StudentPerformance.Api.Data.Entities.Student", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Teacher", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.User", "User")
                        .WithOne("Teacher")
                        .HasForeignKey("StudentPerformance.Api.Data.Entities.Teacher", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.Group", "Group")
                        .WithMany("TeacherSubjectGroupAssignments")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("StudentPerformance.Api.Data.Entities.Semester", "Semester")
                        .WithMany("TeacherSubjectGroupAssignments")
                        .HasForeignKey("SemesterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StudentPerformance.Api.Data.Entities.Subject", "Subject")
                        .WithMany("TeacherSubjectGroupAssignments")
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("StudentPerformance.Api.Data.Entities.Teacher", "Teacher")
                        .WithMany("TeacherSubjectGroupAssignments")
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Semester");

                    b.Navigation("Subject");

                    b.Navigation("Teacher");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.User", b =>
                {
                    b.HasOne("StudentPerformance.Api.Data.Entities.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Assignment", b =>
                {
                    b.Navigation("Grades");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Group", b =>
                {
                    b.Navigation("Students");

                    b.Navigation("TeacherSubjectGroupAssignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Semester", b =>
                {
                    b.Navigation("Grades");

                    b.Navigation("TeacherSubjectGroupAssignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Student", b =>
                {
                    b.Navigation("Attendances");

                    b.Navigation("Grades");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Subject", b =>
                {
                    b.Navigation("Grades");

                    b.Navigation("TeacherSubjectGroupAssignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.Teacher", b =>
                {
                    b.Navigation("Grades");

                    b.Navigation("TeacherSubjectGroupAssignments");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.TeacherSubjectGroupAssignment", b =>
                {
                    b.Navigation("Assignments");

                    b.Navigation("Attendances");

                    b.Navigation("Grades");
                });

            modelBuilder.Entity("StudentPerformance.Api.Data.Entities.User", b =>
                {
                    b.Navigation("Student");

                    b.Navigation("Teacher");
                });
#pragma warning restore 612, 618
        }
    }
}
