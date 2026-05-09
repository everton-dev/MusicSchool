using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Curriculum;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Instruments;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Payments;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class MusicSchoolDbContext(DbContextOptions<MusicSchoolDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Instrument> Instruments => Set<Instrument>();

    public DbSet<FamilyGroup> FamilyGroups => Set<FamilyGroup>();

    public DbSet<FamilyRelationship> FamilyRelationships => Set<FamilyRelationship>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<CurriculumNode> CurriculumNodes => Set<CurriculumNode>();

    public DbSet<StudentCurriculumProgress> StudentCurriculumProgress => Set<StudentCurriculumProgress>();

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUsers(modelBuilder.Entity<User>());
        ConfigureStudents(modelBuilder.Entity<Student>());
        ConfigureTeachers(modelBuilder.Entity<Teacher>());
        ConfigureTeacherInstruments(modelBuilder.Entity<TeacherInstrument>());
        ConfigureInstruments(modelBuilder.Entity<Instrument>());
        ConfigureFamilyGroups(modelBuilder.Entity<FamilyGroup>());
        ConfigureFamilyRelationships(modelBuilder.Entity<FamilyRelationship>());
        ConfigureLessons(modelBuilder.Entity<Lesson>());
        ConfigureCurriculumNodes(modelBuilder.Entity<CurriculumNode>());
        ConfigureStudentCurriculumProgress(modelBuilder.Entity<StudentCurriculumProgress>());
        ConfigurePayments(modelBuilder.Entity<Payment>());
    }

    private static void ConfigureUsers(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).HasConversion(id => id.Value, value => new UserId(value)).ValueGeneratedNever();
        builder.Property(user => user.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(user => user.Email).HasConversion(email => email.Value, value => EmailAddress.Create(value).Value).HasMaxLength(256).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(user => user.PreferredCulture).HasMaxLength(16).IsRequired();
        builder.Property(user => user.FullAddress).HasMaxLength(300).IsRequired();
        builder.Property(user => user.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(user => user.DocumentNumber).HasMaxLength(80).IsRequired();
        builder.Property(user => user.ContactPhone).HasMaxLength(40).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.CreatedOnUtc).IsRequired();
        builder.HasIndex(user => new { user.TenantId, user.Email }).IsUnique();
    }

    private static void ConfigureStudents(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);
        builder.Property(student => student.Id).HasConversion(id => id.Value, value => new StudentId(value)).ValueGeneratedNever();
        builder.Property(student => student.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(student => student.UserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(student => student.DisplayName).HasMaxLength(200).IsRequired();
        builder.HasIndex(student => new { student.TenantId, student.UserId }).IsUnique();
    }

    private static void ConfigureTeachers(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(teacher => teacher.Id);
        builder.Property(teacher => teacher.Id).HasConversion(id => id.Value, value => new TeacherId(value)).ValueGeneratedNever();
        builder.Property(teacher => teacher.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(teacher => teacher.UserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(teacher => teacher.DisplayName).HasMaxLength(200).IsRequired();
        builder.HasIndex(teacher => new { teacher.TenantId, teacher.UserId }).IsUnique();
        builder.HasMany(teacher => teacher.Instruments).WithOne().HasForeignKey(instrument => instrument.TeacherId);
        builder.Navigation(teacher => teacher.Instruments).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureInstruments(EntityTypeBuilder<Instrument> builder)
    {
        builder.ToTable("Instruments");
        builder.HasKey(instrument => instrument.Id);
        builder.Property(instrument => instrument.Id).HasConversion(id => id.Value, value => new InstrumentId(value)).ValueGeneratedNever();
        builder.Property(instrument => instrument.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(instrument => instrument.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(instrument => new { instrument.TenantId, instrument.Name }).IsUnique();
    }

    private static void ConfigureTeacherInstruments(EntityTypeBuilder<TeacherInstrument> builder)
    {
        builder.ToTable("TeacherInstruments");
        builder.HasKey(teacherInstrument => new { teacherInstrument.TeacherId, teacherInstrument.InstrumentId });
        builder.Property(teacherInstrument => teacherInstrument.TeacherId).HasConversion(id => id.Value, value => new TeacherId(value));
        builder.Property(teacherInstrument => teacherInstrument.InstrumentId).HasConversion(id => id.Value, value => new InstrumentId(value));
        builder.HasIndex(teacherInstrument => teacherInstrument.InstrumentId);
    }

    private static void ConfigureFamilyGroups(EntityTypeBuilder<FamilyGroup> builder)
    {
        builder.ToTable("FamilyGroups");
        builder.HasKey(familyGroup => familyGroup.Id);
        builder.Property(familyGroup => familyGroup.Id).HasConversion(id => id.Value, value => new FamilyGroupId(value)).ValueGeneratedNever();
        builder.Property(familyGroup => familyGroup.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(familyGroup => familyGroup.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(familyGroup => familyGroup.CreatedOnUtc).IsRequired();
        builder.HasMany(familyGroup => familyGroup.Relationships).WithOne().HasForeignKey(relationship => relationship.FamilyGroupId);
        builder.Navigation(familyGroup => familyGroup.Relationships).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureFamilyRelationships(EntityTypeBuilder<FamilyRelationship> builder)
    {
        builder.ToTable("FamilyRelationships");
        builder.HasKey(relationship => relationship.Id);
        builder.Property(relationship => relationship.Id).HasConversion(id => id.Value, value => new FamilyRelationshipId(value)).ValueGeneratedNever();
        builder.Property(relationship => relationship.FamilyGroupId).HasConversion(id => id.Value, value => new FamilyGroupId(value));
        builder.Property(relationship => relationship.GuardianUserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.Property(relationship => relationship.StudentId).HasConversion(id => id.Value, value => new StudentId(value));
        builder.Property(relationship => relationship.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(relationship => relationship.IsPrimaryPayer).IsRequired();
        builder.HasIndex(relationship => new { relationship.FamilyGroupId, relationship.GuardianUserId, relationship.StudentId }).IsUnique();
    }

    private static void ConfigureLessons(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");
        builder.HasKey(lesson => lesson.Id);
        builder.Property(lesson => lesson.Id).HasConversion(id => id.Value, value => new LessonId(value)).ValueGeneratedNever();
        builder.Property(lesson => lesson.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(lesson => lesson.TeacherId).HasConversion(id => id.Value, value => new TeacherId(value));
        builder.Property(lesson => lesson.StudentId).HasConversion(id => id.Value, value => new StudentId(value));
        builder.Property(lesson => lesson.InstrumentId).HasConversion(id => id.Value, value => new InstrumentId(value));
        builder.Property(lesson => lesson.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(lesson => lesson.CreatedOnUtc).IsRequired();
        builder.Property(lesson => lesson.CancellationReason).HasMaxLength(500);
        builder.OwnsOne(lesson => lesson.Schedule, schedule =>
        {
            schedule.ToTable("Schedules");
            schedule.WithOwner().HasForeignKey("LessonId");
            schedule.Property<LessonId>("LessonId").HasConversion(id => id.Value, value => new LessonId(value));
            schedule.HasKey("LessonId");
            schedule.Property(item => item.DayOfWeek).HasConversion<int>().HasColumnName("DayOfWeek").IsRequired();
            schedule.Property(item => item.StartTime).HasColumnName("StartTime").IsRequired();
            schedule.Property(item => item.DurationMinutes).HasColumnName("DurationMinutes").IsRequired();
            schedule.Property(item => item.TimeZoneId).HasColumnName("TimeZoneId").HasMaxLength(100).IsRequired();
        });
        builder.HasIndex(lesson => new { lesson.TenantId, lesson.TeacherId, lesson.Status });
        builder.HasIndex(lesson => new { lesson.TenantId, lesson.StudentId, lesson.Status });
    }

    private static void ConfigureCurriculumNodes(EntityTypeBuilder<CurriculumNode> builder)
    {
        builder.ToTable("CurriculumNodes");
        builder.HasKey(node => node.Id);
        builder.Property(node => node.Id).HasConversion(id => id.Value, value => new CurriculumNodeId(value)).ValueGeneratedNever();
        builder.Property(node => node.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(node => node.InstrumentId).HasConversion(id => id.Value, value => new InstrumentId(value));
        builder.Property(node => node.ParentNodeId).HasConversion(
            id => id.HasValue ? id.Value.Value : (Guid?)null,
            value => value.HasValue ? new CurriculumNodeId(value.Value) : (CurriculumNodeId?)null);
        builder.Property(node => node.Title).HasMaxLength(200).IsRequired();
        builder.Property(node => node.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(node => node.BlobName).HasMaxLength(512);
        builder.Property(node => node.ResourceFileType).HasConversion<string>().HasMaxLength(32);
        builder.Property(node => node.ResourceFileName).HasMaxLength(255);
        builder.Property(node => node.ResourceContentType).HasMaxLength(100);
        builder.Property(node => node.ResourceUploadedByTeacherId).HasConversion(
            id => id.HasValue ? id.Value.Value : (Guid?)null,
            value => value.HasValue ? new TeacherId(value.Value) : (TeacherId?)null);
        builder.HasIndex(node => new { node.TenantId, node.InstrumentId, node.ParentNodeId, node.SortOrder });
    }

    private static void ConfigureStudentCurriculumProgress(EntityTypeBuilder<StudentCurriculumProgress> builder)
    {
        builder.ToTable("StudentCurriculumProgress");
        builder.HasKey(progress => progress.Id);
        builder.Property(progress => progress.Id).HasConversion(id => id.Value, value => new StudentCurriculumProgressId(value)).ValueGeneratedNever();
        builder.Property(progress => progress.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(progress => progress.StudentId).HasConversion(id => id.Value, value => new StudentId(value));
        builder.Property(progress => progress.CurriculumNodeId).HasConversion(id => id.Value, value => new CurriculumNodeId(value));
        builder.Property(progress => progress.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(progress => progress.UpdatedOnUtc).IsRequired();
        builder.HasIndex(progress => new { progress.TenantId, progress.StudentId, progress.CurriculumNodeId }).IsUnique();
    }

    private static void ConfigurePayments(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).HasConversion(id => id.Value, value => new PaymentId(value)).ValueGeneratedNever();
        builder.Property(payment => payment.TenantId).HasConversion(id => id.Value, value => new TenantId(value));
        builder.Property(payment => payment.StudentId).HasConversion(id => id.Value, value => new StudentId(value));
        builder.Property(payment => payment.GuardianUserId).HasConversion(id => id.Value, value => new UserId(value));
        builder.OwnsOne(payment => payment.Amount, amount =>
        {
            amount.Property(item => item.Amount).HasColumnName("Amount").HasColumnType("decimal(10,2)").IsRequired();
            amount.Property(item => item.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
        });
        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(payment => payment.Description).HasMaxLength(300).IsRequired();
        builder.Property(payment => payment.PaymentReference).HasMaxLength(100);
        builder.Property(payment => payment.RejectionReason).HasMaxLength(500);
        builder.Property(payment => payment.CreatedOnUtc).IsRequired();
        builder.HasIndex(payment => new { payment.TenantId, payment.GuardianUserId, payment.Status });
        builder.HasIndex(payment => new { payment.TenantId, payment.StudentId, payment.DueDate });
    }
}
