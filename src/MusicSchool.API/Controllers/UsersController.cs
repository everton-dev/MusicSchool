using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicSchool.API.Auth;
using MusicSchool.API.Contracts;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Lessons;
using MusicSchool.Domain.Common;
using MusicSchool.Domain.Families;
using MusicSchool.Domain.Instruments;
using MusicSchool.Domain.Lessons;
using MusicSchool.Domain.Students;
using MusicSchool.Domain.Teachers;
using MusicSchool.Domain.Users;
using MusicSchool.Infrastructure.Persistence;
using AppUser = MusicSchool.Domain.Users.User;

namespace MusicSchool.API.Controllers;

[ApiController]
[Authorize(Policy = AuthConstants.Policies.AdminOnly)]
[Route("api/users")]
public sealed class UsersController(
    MusicSchoolDbContext dbContext,
    ILessonSchedulingService lessonSchedulingService,
    IClock clock,
    CurrentTenant currentTenant) : ControllerBase
{
    private static readonly (DayOfWeek Day, TimeOnly Start, int Duration)[] StandardSlots =
    [
        (DayOfWeek.Monday, new TimeOnly(15, 30), 45),
        (DayOfWeek.Monday, new TimeOnly(16, 30), 45),
        (DayOfWeek.Tuesday, new TimeOnly(16, 30), 45),
        (DayOfWeek.Tuesday, new TimeOnly(17, 30), 45),
        (DayOfWeek.Wednesday, new TimeOnly(17, 00), 60),
        (DayOfWeek.Thursday, new TimeOnly(18, 00), 45)
    ];

    [HttpGet]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var skip = (normalizedPageNumber - 1) * normalizedPageSize;
        var query = dbContext.Users
            .Where(user => user.TenantId == tenantResult.Value)
            .OrderBy(user => user.DisplayName);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var users = await query.Skip(skip).Take(normalizedPageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        return Ok(new PagedResult<UserResponse>(
            users.Select(ToResponse).ToArray(),
            normalizedPageNumber,
            normalizedPageSize,
            totalCount));
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(
            item => item.Id == new UserId(userId) && item.TenantId == tenantResult.Value,
            cancellationToken).ConfigureAwait(false);

        return user is null
            ? NotFound(new ApiErrorResponse("User.NotFound", "User was not found."))
            : Ok(ToResponse(user));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(UserRegistrationRequest request, CancellationToken cancellationToken)
    {
        var tenantResult = EnsureTenant(request.TenantId);
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var userResult = AppUser.Create(
            tenantResult.Value,
            request.Email,
            request.Name,
            request.Profile,
            "en-US",
            clock.UtcNow,
            request.FullAddress,
            request.PostalCode,
            request.DocumentNumber,
            request.ContactPhone);

        if (userResult.IsFailure)
        {
            return ToActionResult(userResult);
        }

        var requiredDetailsResult = userResult.Value.EnsureRegistrationDetails();
        if (requiredDetailsResult.IsFailure)
        {
            return ToActionResult(requiredDetailsResult);
        }

        var duplicateEmail = await dbContext.Users.AnyAsync(
            user => user.TenantId == tenantResult.Value && user.Email == userResult.Value.Email,
            cancellationToken).ConfigureAwait(false);
        if (duplicateEmail)
        {
            return BadRequest(new ApiErrorResponse("User.EmailDuplicate", "A user with this email already exists."));
        }

        await dbContext.Users.AddAsync(userResult.Value, cancellationToken).ConfigureAwait(false);
        await EnsureRoleProfileAsync(userResult.Value, needsStudentProfile: request.ScheduleSelection is not null, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.Profile == UserRole.Guardian && request.HouseholdUserIds is { Count: > 0 })
        {
            var householdResult = await LinkHouseholdUsersAsync(userResult.Value, request.HouseholdUserIds, cancellationToken).ConfigureAwait(false);
            if (householdResult.IsFailure)
            {
                return ToActionResult(householdResult);
            }
        }

        if (request.ScheduleSelection is not null)
        {
            var scheduleResult = await ScheduleUserLessonAsync(userResult.Value, request.ScheduleSelection, cancellationToken).ConfigureAwait(false);
            if (scheduleResult.IsFailure)
            {
                return ToActionResult(scheduleResult);
            }
        }

        return CreatedAtAction(nameof(GetById), new { userId = userResult.Value.Id.Value }, ToResponse(userResult.Value));
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid userId, UserRegistrationRequest request, CancellationToken cancellationToken)
    {
        var tenantResult = EnsureTenant(request.TenantId);
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(
            item => item.Id == new UserId(userId) && item.TenantId == tenantResult.Value,
            cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("User.NotFound", "User was not found."));
        }

        var duplicateEmail = await dbContext.Users.AnyAsync(
            item => item.TenantId == tenantResult.Value && item.Id != user.Id && item.Email.Value == request.Email.Trim().ToLowerInvariant(),
            cancellationToken).ConfigureAwait(false);
        if (duplicateEmail)
        {
            return BadRequest(new ApiErrorResponse("User.EmailDuplicate", "A user with this email already exists."));
        }

        var updateResult = user.UpdateRegistration(
            request.Email,
            request.Name,
            request.Profile,
            request.FullAddress,
            request.PostalCode,
            request.DocumentNumber,
            request.ContactPhone);
        if (updateResult.IsFailure)
        {
            return ToActionResult(updateResult);
        }

        await EnsureRoleProfileAsync(user, needsStudentProfile: request.ScheduleSelection is not null, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.Profile == UserRole.Guardian && request.HouseholdUserIds is { Count: > 0 })
        {
            var householdResult = await LinkHouseholdUsersAsync(user, request.HouseholdUserIds, cancellationToken).ConfigureAwait(false);
            if (householdResult.IsFailure)
            {
                return ToActionResult(householdResult);
            }
        }

        if (request.ScheduleSelection is not null)
        {
            var scheduleResult = await ScheduleUserLessonAsync(user, request.ScheduleSelection, cancellationToken).ConfigureAwait(false);
            if (scheduleResult.IsFailure)
            {
                return ToActionResult(scheduleResult);
            }
        }

        return Ok(ToResponse(user));
    }

    [HttpPost("{userId:guid}/deactivate")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var user = await dbContext.Users.SingleOrDefaultAsync(
            item => item.Id == new UserId(userId) && item.TenantId == tenantResult.Value,
            cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return NotFound(new ApiErrorResponse("User.NotFound", "User was not found."));
        }

        user.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(ToResponse(user));
    }

    [HttpPost("{guardianUserId:guid}/household-users")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddHouseholdUser(
        Guid guardianUserId,
        AddGuardianHouseholdUserRequest request,
        CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        var guardian = await dbContext.Users.SingleOrDefaultAsync(
            user => user.Id == new UserId(guardianUserId) && user.TenantId == tenantResult.Value,
            cancellationToken).ConfigureAwait(false);
        if (guardian is null)
        {
            return NotFound(new ApiErrorResponse("Guardian.NotFound", "Guardian was not found."));
        }

        var householdResult = await LinkHouseholdUsersAsync(guardian, [request.HouseholdUserId], cancellationToken).ConfigureAwait(false);
        if (householdResult.IsFailure)
        {
            return ToActionResult(householdResult);
        }

        return Ok(ToResponse(guardian));
    }

    [HttpGet("teacher-schedule-options")]
    [ProducesResponseType<IReadOnlyCollection<TeacherScheduleOptionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTeacherScheduleOptions(
        [FromQuery] string instrumentQuery,
        CancellationToken cancellationToken)
    {
        var tenantResult = GetTenant();
        if (tenantResult.IsFailure)
        {
            return ToActionResult(tenantResult);
        }

        if (string.IsNullOrWhiteSpace(instrumentQuery))
        {
            return BadRequest(new ApiErrorResponse("Instrument.QueryRequired", "Instrument search text is required."));
        }

        var normalizedQuery = instrumentQuery.Trim();
        var instrumentsQuery = dbContext.Instruments.Where(instrument => instrument.TenantId == tenantResult.Value);
        if (Guid.TryParse(normalizedQuery, out var instrumentIdValue))
        {
            var instrumentId = new InstrumentId(instrumentIdValue);
            instrumentsQuery = instrumentsQuery.Where(instrument => instrument.Id == instrumentId);
        }
        else
        {
            instrumentsQuery = instrumentsQuery.Where(instrument => instrument.Name.Contains(normalizedQuery));
        }

        var instruments = await instrumentsQuery.OrderBy(instrument => instrument.Name).ToListAsync(cancellationToken).ConfigureAwait(false);
        if (instruments.Count == 0)
        {
            return Ok(Array.Empty<TeacherScheduleOptionResponse>());
        }

        var instrumentIds = instruments.Select(instrument => instrument.Id).ToHashSet();
        var teachers = await dbContext.Teachers
            .Include(teacher => teacher.Instruments)
            .Where(teacher =>
                teacher.TenantId == tenantResult.Value &&
                teacher.Instruments.Any(instrument => instrumentIds.Contains(instrument.InstrumentId)))
            .OrderBy(teacher => teacher.DisplayName)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        var teacherIds = teachers.Select(teacher => teacher.Id).ToHashSet();
        var lessons = await dbContext.Lessons
            .Where(lesson =>
                lesson.TenantId == tenantResult.Value &&
                teacherIds.Contains(lesson.TeacherId) &&
                instrumentIds.Contains(lesson.InstrumentId) &&
                lesson.Status != LessonStatus.Cancelled)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var studentIds = lessons.Select(lesson => lesson.StudentId).Distinct().ToArray();
        var studentNames = await dbContext.Students
            .Where(student => student.TenantId == tenantResult.Value && studentIds.Contains(student.Id))
            .ToDictionaryAsync(student => student.Id, student => student.DisplayName, cancellationToken).ConfigureAwait(false);

        var options = new List<TeacherScheduleOptionResponse>();
        foreach (var teacher in teachers)
        {
            foreach (var instrument in instruments.Where(instrument => teacher.Teaches(instrument.Id)))
            {
                foreach (var slot in StandardSlots)
                {
                    var slotEnd = slot.Start.AddMinutes(slot.Duration);
                    var assignedLesson = lessons.FirstOrDefault(lesson =>
                        lesson.TeacherId == teacher.Id &&
                        lesson.InstrumentId == instrument.Id &&
                        lesson.Schedule.DayOfWeek == slot.Day &&
                        lesson.Schedule.StartTime < slotEnd &&
                        lesson.Schedule.EndTime > slot.Start);

                    options.Add(new TeacherScheduleOptionResponse(
                        instrument.Id.Value,
                        instrument.Name,
                        teacher.Id.Value,
                        teacher.DisplayName,
                        slot.Day,
                        slot.Start,
                        slot.Duration,
                        "Europe/Lisbon",
                        assignedLesson is not null,
                        assignedLesson?.StudentId.Value,
                        assignedLesson is not null && studentNames.TryGetValue(assignedLesson.StudentId, out var studentName)
                            ? studentName
                            : null));
                }
            }
        }

        return Ok(options);
    }

    private async Task<Result> EnsureRoleProfileAsync(AppUser user, bool needsStudentProfile, CancellationToken cancellationToken)
    {
        if (user.Role == UserRole.Student || needsStudentProfile)
        {
            var existingStudent = await dbContext.Students.SingleOrDefaultAsync(
                student => student.TenantId == user.TenantId && student.UserId == user.Id,
                cancellationToken).ConfigureAwait(false);
            if (existingStudent is null)
            {
                var studentResult = Student.Create(user.TenantId, user.Id, user.DisplayName);
                if (studentResult.IsFailure)
                {
                    return Result.Failure(studentResult.Error);
                }

                await dbContext.Students.AddAsync(studentResult.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        if (user.Role == UserRole.Teacher)
        {
            var existingTeacher = await dbContext.Teachers.SingleOrDefaultAsync(
                teacher => teacher.TenantId == user.TenantId && teacher.UserId == user.Id,
                cancellationToken).ConfigureAwait(false);
            if (existingTeacher is null)
            {
                var teacherResult = Teacher.Create(user.TenantId, user.Id, user.DisplayName);
                if (teacherResult.IsFailure)
                {
                    return Result.Failure(teacherResult.Error);
                }

                await dbContext.Teachers.AddAsync(teacherResult.Value, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result.Success();
    }

    private async Task<Result> LinkHouseholdUsersAsync(AppUser guardian, IReadOnlyCollection<Guid> householdUserIds, CancellationToken cancellationToken)
    {
        if (guardian.Role != UserRole.Guardian)
        {
            return Result.Failure(new Error("Guardian.RoleRequired", "Only Guardian users can have household members."));
        }

        var familyGroup = await dbContext.FamilyGroups
            .Include(group => group.Relationships)
            .Where(group =>
                group.TenantId == guardian.TenantId &&
                group.Relationships.Any(relationship => relationship.GuardianUserId == guardian.Id))
            .OrderBy(group => group.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (familyGroup is null)
        {
            var familyResult = FamilyGroup.Create(guardian.TenantId, $"{guardian.DisplayName} household", clock.UtcNow);
            if (familyResult.IsFailure)
            {
                return familyResult;
            }

            familyGroup = familyResult.Value;
            await dbContext.FamilyGroups.AddAsync(familyGroup, cancellationToken).ConfigureAwait(false);
        }

        foreach (var householdUserId in householdUserIds.Where(id => id != Guid.Empty).Distinct())
        {
            var householdUser = await dbContext.Users.SingleOrDefaultAsync(
                user => user.Id == new UserId(householdUserId) && user.TenantId == guardian.TenantId,
                cancellationToken).ConfigureAwait(false);
            if (householdUser is null)
            {
                return Result.Failure(new Error("HouseholdUser.NotFound", "Household user was not found."));
            }

            var student = await EnsureStudentForUserAsync(householdUser, cancellationToken).ConfigureAwait(false);
            if (student.IsFailure)
            {
                return Result.Failure(student.Error);
            }

            if (familyGroup.Relationships.Any(relationship =>
                    relationship.GuardianUserId == guardian.Id && relationship.StudentId == student.Value.Id))
            {
                continue;
            }

            var relationshipResult = familyGroup.AddRelationship(
                guardian.Id,
                student.Value.Id,
                FamilyRelationshipKind.Guardian,
                isPrimaryPayer: true);
            if (relationshipResult.IsFailure)
            {
                return relationshipResult;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }

    private async Task<Result<Student>> EnsureStudentForUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.SingleOrDefaultAsync(
            item => item.TenantId == user.TenantId && item.UserId == user.Id,
            cancellationToken).ConfigureAwait(false);
        if (student is not null)
        {
            return Result<Student>.Success(student);
        }

        var studentResult = Student.Create(user.TenantId, user.Id, user.DisplayName);
        if (studentResult.IsFailure)
        {
            return studentResult;
        }

        await dbContext.Students.AddAsync(studentResult.Value, cancellationToken).ConfigureAwait(false);
        return Result<Student>.Success(studentResult.Value);
    }

    private async Task<Result> ScheduleUserLessonAsync(AppUser user, UserScheduleSelectionRequest request, CancellationToken cancellationToken)
    {
        var student = await EnsureStudentForUserAsync(user, cancellationToken).ConfigureAwait(false);
        if (student.IsFailure)
        {
            return Result.Failure(student.Error);
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var scheduleResult = await lessonSchedulingService.ScheduleIndividualLessonAsync(
            new ScheduleIndividualLessonCommand(
                user.TenantId.Value,
                request.TeacherId,
                student.Value.Id.Value,
                request.InstrumentId,
                request.DayOfWeek,
                request.StartTime,
                request.DurationMinutes,
                request.TimeZoneId),
            cancellationToken).ConfigureAwait(false);

        return scheduleResult.IsSuccess ? Result.Success() : Result.Failure(scheduleResult.Error);
    }

    private Result<TenantId> GetTenant()
    {
        return currentTenant.TenantId is null
            ? Result<TenantId>.Failure(new Error("Tenant.Missing", "Authenticated tenant context is required."))
            : Result<TenantId>.Success(currentTenant.TenantId.Value);
    }

    private Result<TenantId> EnsureTenant(Guid tenantId)
    {
        var currentTenantResult = GetTenant();
        if (currentTenantResult.IsFailure)
        {
            return currentTenantResult;
        }

        var requestedTenantId = new TenantId(tenantId);
        return requestedTenantId == currentTenantResult.Value
            ? Result<TenantId>.Success(requestedTenantId)
            : Result<TenantId>.Failure(new Error("Tenant.Mismatch", "Requested tenant does not match the authenticated tenant."));
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        var response = new ApiErrorResponse(result.Error.Code, result.Error.Message);
        return result.Error.Code.EndsWith(".NotFound", StringComparison.Ordinal)
            ? NotFound(response)
            : BadRequest(response);
    }

    private static UserResponse ToResponse(AppUser user)
    {
        return new UserResponse(
            user.Id.Value,
            user.DisplayName,
            user.Role.ToString(),
            user.FullAddress,
            user.PostalCode,
            user.DocumentNumber,
            user.ContactPhone,
            user.Email.Value,
            user.IsActive);
    }
}
