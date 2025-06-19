using Bogus;
using StudentPerformance.Api.Data.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace StudentPerformance.Api.Data
{
    public static class DataSeeder
    {
        public static void SeedData(ApplicationDbContext context)
        {
            // context.Database.EnsureCreated(); // No need, using Migrate() in Program.cs

            // Define variables (no change)
            Role adminRole, teacherRole, studentRole;
            List<User> allUsers;
            List<User> teacherUsers;
            List<User> studentUsers;
            List<Group> groups;
            List<Subject> subjects;
            List<Semester> semesters;
            List<Student> students;
            List<Teacher> teachers;
            List<TeacherSubjectGroupAssignment> teacherSubjectGroupAssignments;
            List<Assignment> assignments;


            // 1. Seed Roles
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Role { Name = "Teacher", Description = "Educator", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Role { Name = "Student", Description = "Enrolled Student", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                context.Roles.AddRange(roles);
                context.SaveChanges(); // Сохраняем сразу, чтобы получить ID и сделать их доступными
            }
            // Всегда получаем актуальные роли из БД после возможного засеивания
            adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
            teacherRole = context.Roles.FirstOrDefault(r => r.Name == "Teacher");
            studentRole = context.Roles.FirstOrDefault(r => r.Name == "Student");

            // 2. Seed Users
            if (!context.Users.Any())
            {
                var userFaker = new Faker<User>()
                    .RuleFor(u => u.Username, f => f.Internet.UserName())
                    .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("password123"))
                    .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                    .RuleFor(u => u.LastName, f => f.Name.LastName())
                    .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                    .RuleFor(u => u.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
                    .RuleFor(u => u.UpdatedAt, f => f.Date.Recent(1).ToUniversalTime());

                var adminUsersList = userFaker.Generate(1).Select(u => { u.RoleId = adminRole.RoleId; return u; }).ToList();
                var teacherUsersList = userFaker.Generate(3).Select(u => { u.RoleId = teacherRole.RoleId; return u; }).ToList();
                var studentUsersList = userFaker.Generate(20).Select(u => { u.RoleId = studentRole.RoleId; return u; }).ToList();

                context.Users.AddRange(adminUsersList);
                context.Users.AddRange(teacherUsersList);
                context.Users.AddRange(studentUsersList);
                context.SaveChanges(); // Сохраняем сразу, чтобы получить ID
            }
            // Всегда получаем актуальных пользователей из БД после возможного засеивания
            allUsers = context.Users.ToList();
            teacherUsers = allUsers.Where(u => u.RoleId == teacherRole.RoleId).ToList();
            studentUsers = allUsers.Where(u => u.RoleId == studentRole.RoleId).ToList();


            // 3. Seed Groups
            if (!context.Groups.Any())
            {
                var groupFaker = new Faker<Group>()
                    .RuleFor(g => g.Name, f => $"Группа {f.Random.AlphaNumeric(3).ToUpper()}")
                    .RuleFor(g => g.Code, f => f.Finance.Account(4))
                    .RuleFor(g => g.Description, f => f.Lorem.Sentence())
                    .RuleFor(g => g.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
                    .RuleFor(g => g.UpdatedAt, f => f.Date.Recent(1).ToUniversalTime());
                context.Groups.AddRange(groupFaker.Generate(5));
                context.SaveChanges(); // Сохраняем сразу
            }
            groups = context.Groups.ToList(); // Всегда получаем актуальные группы

            // 4. Seed Subjects
            if (!context.Subjects.Any())
            {
                var subjectFaker = new Faker<Subject>()
                    .RuleFor(s => s.Name, f => f.Commerce.ProductName())
                    .RuleFor(s => s.Code, f => f.Random.AlphaNumeric(5).ToUpper())
                    .RuleFor(s => s.Description, f => f.Lorem.Sentence())
                    .RuleFor(s => s.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
                    .RuleFor(s => s.UpdatedAt, f => f.Date.Recent(1).ToUniversalTime());
                context.Subjects.AddRange(subjectFaker.Generate(10));
                context.SaveChanges(); // Сохраняем сразу
            }
            subjects = context.Subjects.ToList(); // Всегда получаем актуальные предметы

            // 5. Seed Semesters
            if (!context.Semesters.Any())
            {
                var currentYear = DateTime.UtcNow.Year;
                var semestersToSeed = new List<Semester>
                {
                    new Semester { Name = $"{currentYear} Весенний", StartDate = new DateTime(currentYear, 2, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(currentYear, 6, 30, 0, 0, 0, DateTimeKind.Utc), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Semester { Name = $"{currentYear} Осенний", StartDate = new DateTime(currentYear, 9, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(currentYear, 12, 31, 0, 0, 0, DateTimeKind.Utc), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Semester { Name = $"{currentYear - 1} Весенний", StartDate = new DateTime(currentYear - 1, 2, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(currentYear - 1, 6, 30, 0, 0, 0, DateTimeKind.Utc), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Semester { Name = $"{currentYear - 1} Осенний", StartDate = new DateTime(currentYear - 1, 9, 1, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(currentYear - 1, 12, 31, 0, 0, 0, DateTimeKind.Utc), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                context.Semesters.AddRange(semestersToSeed);
                context.SaveChanges(); // Сохраняем сразу
            }
            semesters = context.Semesters.ToList(); // Всегда получаем актуальные семестры


            // 6. Seed Students (связываем с существующими Users и Groups)
            // Важно: Этот блок должен выполняться только если studentUsers (пользователи с ролью Student) не пуст И groups не пуст.
            // Если studentUsers пуст, то generatedStudents будет пустым, и AddRange ничего не добавит.
            // Если groups пуст, то studentDataFaker.PickRandom(groups) вызовет ошибку "The list is empty".
            if (!context.Students.Any() && studentUsers.Any() && groups.Any())
            {
                var generatedStudents = new List<Student>();
                var studentDataFaker = new Faker();
                foreach (var user in studentUsers)
                {
                    generatedStudents.Add(new Student
                    {
                        UserId = user.Id,
                        GroupId = studentDataFaker.PickRandom(groups).GroupId,
                        DateOfBirth = studentDataFaker.Date.Past(20, DateTime.UtcNow.AddYears(-18)).ToUniversalTime(),
                        EnrollmentDate = studentDataFaker.Date.Past(2, DateTime.UtcNow.AddYears(-1)).ToUniversalTime(),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                context.Students.AddRange(generatedStudents);
                context.SaveChanges();
            }
            students = context.Students.ToList(); // Всегда получаем актуальных студентов


            // 7. Seed Teachers (связываем с существующими Users)
            // Важно: Этот блок должен выполняться только если teacherUsers (пользователи с ролью Teacher) не пуст.
            if (!context.Teachers.Any() && teacherUsers.Any())
            {
                var generatedTeachers = new List<Teacher>();
                var teacherDataFaker = new Faker();
                foreach (var user in teacherUsers)
                {
                    generatedTeachers.Add(new Teacher
                    {
                        UserId = user.Id,
                        Department = teacherDataFaker.Commerce.Department(),
                        Position = teacherDataFaker.Name.JobTitle(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                context.Teachers.AddRange(generatedTeachers);
                context.SaveChanges();
            }
            teachers = context.Teachers.ToList(); // Всегда получаем актуальных учителей


            // 8. Seed TeacherSubjectGroupAssignments
            // Важно: Этот блок должен выполняться только если teachers, subjects, groups, semesters не пусты.
            if (!context.TeacherSubjectGroupAssignments.Any() && teachers.Any() && subjects.Any() && groups.Any() && semesters.Any())
            {
                var uniqueAssignments = new HashSet<(int TeacherId, int SubjectId, int GroupId, int SemesterId)>();
                var generatedAssignments = new List<TeacherSubjectGroupAssignment>();
                var faker = new Faker();

                int maxAttempts = 100;
                int desiredAssignments = 20;

                for (int i = 0; generatedAssignments.Count < desiredAssignments && i < maxAttempts; i++)
                {
                    var teacher = faker.PickRandom(teachers);
                    var subject = faker.PickRandom(subjects);
                    var group = faker.PickRandom(groups);
                    var semester = faker.PickRandom(semesters);

                    var currentCombination = (teacher.TeacherId, subject.SubjectId, group.GroupId, semester.SemesterId);

                    if (uniqueAssignments.Add(currentCombination))
                    {
                        generatedAssignments.Add(new TeacherSubjectGroupAssignment
                        {
                            TeacherId = teacher.TeacherId,
                            SubjectId = subject.SubjectId,
                            GroupId = group.GroupId,
                            SemesterId = semester.SemesterId,
                            CreatedAt = faker.Date.Past(1).ToUniversalTime(),
                            UpdatedAt = faker.Date.Recent(1).ToUniversalTime()
                        });
                    }
                }
                context.TeacherSubjectGroupAssignments.AddRange(generatedAssignments);
                context.SaveChanges();
            }
            teacherSubjectGroupAssignments = context.TeacherSubjectGroupAssignments.ToList();


            // 9. Seed Assignments
            // Важно: Этот блок должен выполняться только если teacherSubjectGroupAssignments не пуст.
            if (!context.Assignments.Any() && teacherSubjectGroupAssignments.Any())
            {
                var assignmentFaker = new Faker<Assignment>()
                    .RuleFor(a => a.TeacherSubjectGroupAssignmentId, f => f.PickRandom(teacherSubjectGroupAssignments).TeacherSubjectGroupAssignmentId)
                    .RuleFor(a => a.Title, f => f.Commerce.ProductName())
                    .RuleFor(a => a.Description, f => f.Lorem.Sentence())
                    .RuleFor(a => a.Type, f => f.PickRandom("Quiz", "Homework", "Project", "Exam"))
                    .RuleFor(a => a.MaxScore, f => f.Random.Decimal(5, 100))
                    .RuleFor(a => a.DueDate, f => f.Date.Future(1).ToUniversalTime())
                    .RuleFor(a => a.SubmissionDate, (f, a) => f.Date.Between(a.CreatedAt, a.DueDate).ToUniversalTime())
                    .RuleFor(a => a.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
                    .RuleFor(a => a.UpdatedAt, f => f.Date.Recent(1).ToUniversalTime());
                context.Assignments.AddRange(assignmentFaker.Generate(50));
                context.SaveChanges();
            }
            assignments = context.Assignments.ToList();

            // 10. Seed Attendances
            // Важно: Этот блок должен выполняться только если students и teacherSubjectGroupAssignments не пусты.
            if (!context.Attendances.Any() && students.Any() && teacherSubjectGroupAssignments.Any())
            {
                var attendanceFaker = new Faker<Attendance>()
                    .RuleFor(a => a.StudentId, f => f.PickRandom(students).StudentId)
                    .RuleFor(a => a.TeacherSubjectGroupAssignmentId, f => f.PickRandom(teacherSubjectGroupAssignments).TeacherSubjectGroupAssignmentId)
                    .RuleFor(a => a.Date, f => f.Date.Recent(30).ToUniversalTime())
                    .RuleFor(a => a.Status, f => f.PickRandom("Present", "Absent", "Late", "Excused"))
                    .RuleFor(a => a.Remarks, f => f.Lorem.Sentence(3))
                    .RuleFor(a => a.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
                    .RuleFor(a => a.UpdatedAt, f => f.Date.Recent(1).ToUniversalTime());
                context.Attendances.AddRange(attendanceFaker.Generate(100));
                context.SaveChanges();
            }

            // 11. Seed Grades
            // Важно: Этот блок должен выполняться только если students, subjects, semesters, teachers, teacherSubjectGroupAssignments не пусты.
            if (!context.Grades.Any() && students.Any() && subjects.Any() && semesters.Any() && teachers.Any() && teacherSubjectGroupAssignments.Any())
            {
                var baseFaker = new Faker();

                foreach (var student in students)
                {
                    var possibleAssignmentsForStudent = teacherSubjectGroupAssignments
                        .Where(tsga => tsga.GroupId == student.GroupId)
                        .ToList();

                    if (possibleAssignmentsForStudent.Any())
                    {
                        foreach (var tsga in possibleAssignmentsForStudent)
                        {
                            var subjectForGrade = subjects.FirstOrDefault(s => s.SubjectId == tsga.SubjectId);
                            var semesterForGrade = semesters.FirstOrDefault(sem => sem.SemesterId == tsga.SemesterId);
                            var teacherForGrade = teachers.FirstOrDefault(t => t.TeacherId == tsga.TeacherId);

                            // Дополнительная проверка, чтобы избежать NullReferenceException, если ForEach PickRandom выдал null (хотя PickRandom не должен)
                            if (subjectForGrade != null && semesterForGrade != null && teacherForGrade != null)
                            {
                                for (int i = 0; i < baseFaker.Random.Int(1, 3); i++)
                                {
                                    context.Grades.Add(new Grade
                                    {
                                        StudentId = student.StudentId,
                                        SubjectId = subjectForGrade.SubjectId, // Убрал ?
                                        SemesterId = semesterForGrade.SemesterId, // Убрал ?
                                        TeacherId = teacherForGrade.TeacherId, // Убрал ?
                                        TeacherSubjectGroupAssignmentId = tsga.TeacherSubjectGroupAssignmentId,
                                        Value = baseFaker.Random.Decimal(0, 100),
                                        ControlType = baseFaker.PickRandom("Quiz", "Exam", "Lab", "Project"),
                                        DateReceived = baseFaker.Date.Recent(60).ToUniversalTime(),
                                        Status = baseFaker.PickRandom("Passed", "Failed", "Pending"),
                                        Notes = baseFaker.Lorem.Sentence(),
                                        CreatedAt = DateTime.UtcNow,
                                        UpdatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
