using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        var responses = new List<UserResponse>(users.Count);
        foreach (var user in users)
        {
            responses.Add(await ToResponseAsync(user, autoStudentCreatedCount: 0, cancellationToken).ConfigureAwait(false));
        }

        return Ok(new PagedResult<UserResponse>(
            responses,
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
            : Ok(await ToResponseAsync(user, autoStudentCreatedCount: 0, cancellationToken).ConfigureAwait(false));
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
            request.ContactPhone,
            request.DocType,
            request.BirthDate);

        if (userResult.IsFailure)
        {
            return ToActionResult(userResult);
        }

        var requiredDetailsResult = userResult.Value.EnsureRegistrationDetails();
        if (requiredDetailsResult.IsFailure)
        {
            return ToActionResult(requiredDetailsResult);
        }

        var duplicateResult = await EnsureNoDuplicateEmailsAsync(
            tenantResult.Value,
            mainUserId: null,
            request,
            cancellationToken).ConfigureAwait(false);
        if (duplicateResult.IsFailure)
        {
            return ToActionResult(duplicateResult);
        }

        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Users.AddAsync(userResult.Value, cancellationToken).ConfigureAwait(false);
        var roleProfileResult = await EnsureRoleProfileAsync(
            userResult.Value,
            needsStudentProfile: request.IsStudent || request.ScheduleSelection is not null,
            cancellationToken).ConfigureAwait(false);
        if (roleProfileResult.IsFailure)
        {
            return ToActionResult(roleProfileResult);
        }

        var autoStudentCreatedCount = 0;

        if (request.Profile == UserRole.Guardian && request.HouseholdUserIds is { Count: > 0 })
        {
            var householdResult = await LinkHouseholdUsersAsync(userResult.Value, request.HouseholdUserIds, cancellationToken).ConfigureAwait(false);
            if (householdResult.IsFailure)
            {
                return ToActionResult(householdResult);
            }
        }

        if (request.Profile == UserRole.Guardian && request.HouseholdMembers is { Count: > 0 })
        {
            var householdMemberResult = await SyncHouseholdMembersAsync(
                userResult.Value,
                request.HouseholdMembers,
                deactivateRemovedMembers: false,
                cancellationToken).ConfigureAwait(false);
            if (householdMemberResult.IsFailure)
            {
                return ToActionResult(householdMemberResult);
            }

            autoStudentCreatedCount = householdMemberResult.Value;
        }

        if (request.Profile == UserRole.Teacher)
        {
            var teacherLessonTypesResult = await SyncTeacherLessonTypesAsync(userResult.Value, request.LessonTypes, cancellationToken).ConfigureAwait(false);
            if (teacherLessonTypesResult.IsFailure)
            {
                return ToActionResult(teacherLessonTypesResult);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.ScheduleSelection is not null)
        {
            var scheduleResult = await ScheduleUserLessonAsync(userResult.Value, request.ScheduleSelection, cancellationToken).ConfigureAwait(false);
            if (scheduleResult.IsFailure)
            {
                return ToActionResult(scheduleResult);
            }
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { userId = userResult.Value.Id.Value },
            await ToResponseAsync(userResult.Value, autoStudentCreatedCount, cancellationToken).ConfigureAwait(false));
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

        var duplicateResult = await EnsureNoDuplicateEmailsAsync(
            tenantResult.Value,
            user.Id,
            request,
            cancellationToken).ConfigureAwait(false);
        if (duplicateResult.IsFailure)
        {
            return ToActionResult(duplicateResult);
        }

        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken).ConfigureAwait(false);
        var updateResult = user.UpdateRegistration(
            request.Email,
            request.Name,
            request.Profile,
            request.FullAddress,
            request.PostalCode,
            request.DocumentNumber,
            request.ContactPhone,
            request.DocType,
            request.BirthDate);
        if (updateResult.IsFailure)
        {
            return ToActionResult(updateResult);
        }

        var roleProfileResult = await EnsureRoleProfileAsync(
            user,
            needsStudentProfile: request.IsStudent || request.ScheduleSelection is not null,
            cancellationToken).ConfigureAwait(false);
        if (roleProfileResult.IsFailure)
        {
            return ToActionResult(roleProfileResult);
        }

        var autoStudentCreatedCount = 0;

        if (request.Profile == UserRole.Guardian && request.HouseholdUserIds is { Count: > 0 })
        {
            var householdResult = await LinkHouseholdUsersAsync(user, request.HouseholdUserIds, cancellationToken).ConfigureAwait(false);
            if (householdResult.IsFailure)
            {
                return ToActionResult(householdResult);
            }
        }

        if (request.Profile == UserRole.Guardian)
        {
            var householdMemberResult = await SyncHouseholdMembersAsync(
                user,
                request.HouseholdMembers ?? Array.Empty<HouseholdMemberRequest>(),
                deactivateRemovedMembers: true,
                cancellationToken).ConfigureAwait(false);
            if (householdMemberResult.IsFailure)
            {
                return ToActionResult(householdMemberResult);
            }

            autoStudentCreatedCount = householdMemberResult.Value;
        }

        if (request.Profile == UserRole.Teacher)
        {
            var teacherLessonTypesResult = await SyncTeacherLessonTypesAsync(user, request.LessonTypes, cancellationToken).ConfigureAwait(false);
            if (teacherLessonTypesResult.IsFailure)
            {
                return ToActionResult(teacherLessonTypesResult);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (request.ScheduleSelection is not null)
        {
            var scheduleResult = await ScheduleUserLessonAsync(user, request.ScheduleSelection, cancellationToken).ConfigureAwait(false);
            if (scheduleResult.IsFailure)
            {
                return ToActionResult(scheduleResult);
            }
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return Ok(await ToResponseAsync(user, autoStudentCreatedCount, cancellationToken).ConfigureAwait(false));
    }

    [HttpPatch("{userId:guid}/status")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid userId, UserStatusUpdateRequest request, CancellationToken cancellationToken)
    {
        return await SetUserStatusAsync(userId, request.IsActive, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("{userId:guid}/deactivate")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
    {
        return await SetUserStatusAsync(userId, isActive: false, cancellationToken).ConfigureAwait(false);
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

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(await ToResponseAsync(guardian, autoStudentCreatedCount: 0, cancellationToken).ConfigureAwait(false));
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

    private async Task<IActionResult> SetUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
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

        await using var transaction = await BeginTransactionIfSupportedAsync(cancellationToken).ConfigureAwait(false);

        if (isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
            if (user.Role == UserRole.Guardian)
            {
                var linkedStudentUsers = await GetGuardianHouseholdUsersAsync(user, cancellationToken).ConfigureAwait(false);
                foreach (var linkedStudentUser in linkedStudentUsers)
                {
                    linkedStudentUser.Deactivate();
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        return Ok(await ToResponseAsync(user, autoStudentCreatedCount: 0, cancellationToken).ConfigureAwait(false));
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

    private async Task<Result<int>> SyncHouseholdMembersAsync(
        AppUser guardian,
        IReadOnlyCollection<HouseholdMemberRequest> householdMembers,
        bool deactivateRemovedMembers,
        CancellationToken cancellationToken)
    {
        if (guardian.Role != UserRole.Guardian)
        {
            return Result<int>.Failure(new Error("Guardian.RoleRequired", "Only Guardian users can have household members."));
        }

        var familyGroupResult = await EnsureFamilyGroupAsync(guardian, cancellationToken).ConfigureAwait(false);
        if (familyGroupResult.IsFailure)
        {
            return Result<int>.Failure(familyGroupResult.Error);
        }

        var familyGroup = familyGroupResult.Value;
        var retainedStudentIds = new List<StudentId>();
        var createdCount = 0;

        foreach (var member in householdMembers.Where(item => !string.IsNullOrWhiteSpace(item.Email)).DistinctBy(item => item.Email.Trim().ToLowerInvariant()))
        {
            if (member.UserId == guardian.Id.Value)
            {
                return Result<int>.Failure(new Error("HouseholdUser.Invalid", "Guardian cannot be linked as their own household student."));
            }

            var memberUser = await ResolveHouseholdMemberUserAsync(guardian, member, cancellationToken).ConfigureAwait(false);
            if (memberUser.IsFailure)
            {
                return Result<int>.Failure(memberUser.Error);
            }

            if (memberUser.Value.Created)
            {
                createdCount++;
            }

            var studentResult = await EnsureStudentForUserAsync(memberUser.Value.User, cancellationToken).ConfigureAwait(false);
            if (studentResult.IsFailure)
            {
                return Result<int>.Failure(studentResult.Error);
            }

            var updateStudentResult = studentResult.Value.UpdateProfile(member.Name, member.BirthDate);
            if (updateStudentResult.IsFailure)
            {
                return Result<int>.Failure(updateStudentResult.Error);
            }

            retainedStudentIds.Add(studentResult.Value.Id);

            if (familyGroup.Relationships.Any(relationship =>
                    relationship.GuardianUserId == guardian.Id && relationship.StudentId == studentResult.Value.Id))
            {
                continue;
            }

            var relationshipResult = familyGroup.AddRelationship(
                guardian.Id,
                studentResult.Value.Id,
                FamilyRelationshipKind.Guardian,
                isPrimaryPayer: true);
            if (relationshipResult.IsFailure)
            {
                return Result<int>.Failure(relationshipResult.Error);
            }
        }

        if (deactivateRemovedMembers)
        {
            var removedStudentIds = familyGroup.RemoveRelationshipsNotIn(guardian.Id, retainedStudentIds);
            if (removedStudentIds.Count > 0)
            {
                var removedStudents = await dbContext.Students
                    .Where(student => student.TenantId == guardian.TenantId && removedStudentIds.Contains(student.Id))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);
                var removedUserIds = removedStudents.Select(student => student.UserId).ToArray();
                var removedUsers = await dbContext.Users
                    .Where(user => user.TenantId == guardian.TenantId && removedUserIds.Contains(user.Id))
                    .ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var removedUser in removedUsers)
                {
                    removedUser.Deactivate();
                }
            }
        }

        return Result<int>.Success(createdCount);
    }

    private async Task<Result> SyncTeacherLessonTypesAsync(
        AppUser user,
        IReadOnlyCollection<string>? lessonTypes,
        CancellationToken cancellationToken)
    {
        if (lessonTypes is null || lessonTypes.Count == 0)
        {
            return Result.Success();
        }

        var teacher = await dbContext.Teachers
            .Include(item => item.Instruments)
            .SingleOrDefaultAsync(item => item.TenantId == user.TenantId && item.UserId == user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (teacher is null)
        {
            return Result.Failure(new Error("Teacher.NotFound", "Teacher profile was not found."));
        }

        var normalizedLessonTypes = lessonTypes
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedLessonTypes.Length == 0)
        {
            return Result.Success();
        }

        var existingInstruments = await dbContext.Instruments
            .Where(instrument => instrument.TenantId == user.TenantId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var selectedInstrumentIds = new List<InstrumentId>();

        foreach (var lessonType in normalizedLessonTypes)
        {
            var instrument = existingInstruments.FirstOrDefault(item => item.Name.Equals(lessonType, StringComparison.OrdinalIgnoreCase));
            if (instrument is null)
            {
                var instrumentResult = Instrument.Create(user.TenantId, lessonType);
                if (instrumentResult.IsFailure)
                {
                    return Result.Failure(instrumentResult.Error);
                }

                instrument = instrumentResult.Value;
                existingInstruments.Add(instrument);
                await dbContext.Instruments.AddAsync(instrument, cancellationToken).ConfigureAwait(false);
            }

            selectedInstrumentIds.Add(instrument.Id);
        }

        return teacher.ReplaceInstruments(selectedInstrumentIds);
    }

    private async Task<Result<FamilyGroup>> EnsureFamilyGroupAsync(AppUser guardian, CancellationToken cancellationToken)
    {
        var familyGroup = await dbContext.FamilyGroups
            .Include(group => group.Relationships)
            .Where(group =>
                group.TenantId == guardian.TenantId &&
                group.Relationships.Any(relationship => relationship.GuardianUserId == guardian.Id))
            .OrderBy(group => group.CreatedOnUtc)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (familyGroup is not null)
        {
            return Result<FamilyGroup>.Success(familyGroup);
        }

        var familyResult = FamilyGroup.Create(guardian.TenantId, $"{guardian.DisplayName} household", clock.UtcNow);
        if (familyResult.IsFailure)
        {
            return familyResult;
        }

        await dbContext.FamilyGroups.AddAsync(familyResult.Value, cancellationToken).ConfigureAwait(false);
        return Result<FamilyGroup>.Success(familyResult.Value);
    }

    private async Task<Result<(AppUser User, bool Created)>> ResolveHouseholdMemberUserAsync(
        AppUser guardian,
        HouseholdMemberRequest member,
        CancellationToken cancellationToken)
    {
        var emailResult = EmailAddress.Create(member.Email);
        if (emailResult.IsFailure)
        {
            return Result<(AppUser User, bool Created)>.Failure(emailResult.Error);
        }

        AppUser? user = null;
        if (member.UserId is { } memberUserId && memberUserId != Guid.Empty)
        {
            user = await dbContext.Users.SingleOrDefaultAsync(
                item => item.Id == new UserId(memberUserId) && item.TenantId == guardian.TenantId,
                cancellationToken).ConfigureAwait(false);
            if (user is null)
            {
                return Result<(AppUser User, bool Created)>.Failure(new Error("HouseholdUser.NotFound", "Household user was not found."));
            }
        }
        else
        {
            user = await dbContext.Users.SingleOrDefaultAsync(
                item => item.TenantId == guardian.TenantId && item.Email == emailResult.Value,
                cancellationToken).ConfigureAwait(false);
        }

        if (user is null)
        {
            var createUserResult = AppUser.Create(
                guardian.TenantId,
                member.Email,
                member.Name,
                UserRole.Student,
                guardian.PreferredCulture,
                clock.UtcNow,
                guardian.FullAddress,
                guardian.PostalCode,
                member.DocumentNumber,
                guardian.ContactPhone,
                member.DocType,
                member.BirthDate);
            if (createUserResult.IsFailure)
            {
                return Result<(AppUser User, bool Created)>.Failure(createUserResult.Error);
            }

            await dbContext.Users.AddAsync(createUserResult.Value, cancellationToken).ConfigureAwait(false);
            return Result<(AppUser User, bool Created)>.Success((createUserResult.Value, true));
        }

        if (user.Id == guardian.Id)
        {
            return Result<(AppUser User, bool Created)>.Failure(new Error("HouseholdUser.Invalid", "Guardian cannot be linked as their own household student."));
        }

        var updateResult = user.UpdateRegistration(
            member.Email,
            member.Name,
            UserRole.Student,
            user.FullAddress,
            user.PostalCode,
            member.DocumentNumber,
            user.ContactPhone,
            member.DocType,
            member.BirthDate);
        return updateResult.IsFailure
            ? Result<(AppUser User, bool Created)>.Failure(updateResult.Error)
            : Result<(AppUser User, bool Created)>.Success((user, false));
    }

    private async Task<Result> LinkHouseholdUsersAsync(AppUser guardian, IReadOnlyCollection<Guid> householdUserIds, CancellationToken cancellationToken)
    {
        if (guardian.Role != UserRole.Guardian)
        {
            return Result.Failure(new Error("Guardian.RoleRequired", "Only Guardian users can have household members."));
        }

        var familyGroupResult = await EnsureFamilyGroupAsync(guardian, cancellationToken).ConfigureAwait(false);
        if (familyGroupResult.IsFailure)
        {
            return Result.Failure(familyGroupResult.Error);
        }

        var familyGroup = familyGroupResult.Value;

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

    private async Task<Result> EnsureNoDuplicateEmailsAsync(
        TenantId tenantId,
        UserId? mainUserId,
        UserRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var mainEmailResult = EmailAddress.Create(request.Email);
        if (mainEmailResult.IsFailure)
        {
            return Result.Failure(mainEmailResult.Error);
        }

        var mainEmailExists = await dbContext.Users.AnyAsync(
            user =>
                user.TenantId == tenantId &&
                user.Email == mainEmailResult.Value &&
                (!mainUserId.HasValue || user.Id != mainUserId.Value),
            cancellationToken).ConfigureAwait(false);
        if (mainEmailExists)
        {
            return Result.Failure(new Error("User.EmailDuplicate", "A user with this email already exists."));
        }

        var householdEmails = request.HouseholdMembers?
            .Where(member => !string.IsNullOrWhiteSpace(member.Email))
            .Select(member => member.Email.Trim().ToLowerInvariant())
            .ToArray() ?? [];
        if (householdEmails.Length != householdEmails.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            return Result.Failure(new Error("HouseholdUser.EmailDuplicate", "Household member emails must be unique."));
        }

        if (householdEmails.Contains(mainEmailResult.Value.Value, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(new Error("HouseholdUser.EmailDuplicate", "Guardian and household member emails must be different."));
        }

        return Result.Success();
    }

    private async Task<IReadOnlyCollection<AppUser>> GetGuardianHouseholdUsersAsync(AppUser guardian, CancellationToken cancellationToken)
    {
        var studentIds = await dbContext.FamilyRelationships
            .Where(relationship => relationship.GuardianUserId == guardian.Id)
            .Select(relationship => relationship.StudentId)
            .Distinct()
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (studentIds.Count == 0)
        {
            return Array.Empty<AppUser>();
        }

        var userIds = await dbContext.Students
            .Where(student => student.TenantId == guardian.TenantId && studentIds.Contains(student.Id))
            .Select(student => student.UserId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (userIds.Count == 0)
        {
            return Array.Empty<AppUser>();
        }

        return await dbContext.Users
            .Where(user => user.TenantId == guardian.TenantId && userIds.Contains(user.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfSupportedAsync(CancellationToken cancellationToken)
    {
        return dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false)
            : null;
    }

    private async Task<UserResponse> ToResponseAsync(AppUser user, int autoStudentCreatedCount, CancellationToken cancellationToken)
    {
        var householdMembers = user.Role == UserRole.Guardian
            ? await GetHouseholdMemberResponsesAsync(user, cancellationToken).ConfigureAwait(false)
            : Array.Empty<HouseholdMemberResponse>();
        var lessonTypes = user.Role == UserRole.Teacher
            ? await GetTeacherLessonTypesAsync(user, cancellationToken).ConfigureAwait(false)
            : Array.Empty<string>();

        return new UserResponse(
            user.Id.Value,
            user.DisplayName,
            user.Role.ToString(),
            user.FullAddress,
            user.PostalCode,
            user.DocumentType,
            user.DocumentNumber,
            user.ContactPhone,
            user.Email.Value,
            user.BirthDate,
            user.IsActive,
            householdMembers,
            lessonTypes,
            autoStudentCreatedCount);
    }

    private async Task<IReadOnlyCollection<HouseholdMemberResponse>> GetHouseholdMemberResponsesAsync(AppUser guardian, CancellationToken cancellationToken)
    {
        var studentIds = await dbContext.FamilyRelationships
            .Where(relationship => relationship.GuardianUserId == guardian.Id)
            .Select(relationship => relationship.StudentId)
            .Distinct()
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (studentIds.Count == 0)
        {
            return Array.Empty<HouseholdMemberResponse>();
        }

        var students = await dbContext.Students
            .Where(student => student.TenantId == guardian.TenantId && studentIds.Contains(student.Id))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var studentUserIds = students.Select(student => student.UserId).ToArray();
        var users = await dbContext.Users
            .Where(user => user.TenantId == guardian.TenantId && studentUserIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken).ConfigureAwait(false);

        return students
            .Where(student => users.ContainsKey(student.UserId))
            .Select(student =>
            {
                var studentUser = users[student.UserId];
                return new HouseholdMemberResponse(
                    studentUser.Id.Value,
                    studentUser.DisplayName,
                    student.BirthDate ?? studentUser.BirthDate,
                    studentUser.DocumentType,
                    studentUser.DocumentNumber,
                    studentUser.Email.Value,
                    studentUser.IsActive);
            })
            .OrderBy(member => member.Name)
            .ToArray();
    }

    private async Task<IReadOnlyCollection<string>> GetTeacherLessonTypesAsync(AppUser user, CancellationToken cancellationToken)
    {
        var teacher = await dbContext.Teachers
            .Include(item => item.Instruments)
            .SingleOrDefaultAsync(item => item.TenantId == user.TenantId && item.UserId == user.Id, cancellationToken)
            .ConfigureAwait(false);
        if (teacher is null || teacher.Instruments.Count == 0)
        {
            return Array.Empty<string>();
        }

        var instrumentIds = teacher.Instruments.Select(instrument => instrument.InstrumentId).ToArray();
        return await dbContext.Instruments
            .Where(instrument => instrument.TenantId == user.TenantId && instrumentIds.Contains(instrument.Id))
            .OrderBy(instrument => instrument.Name)
            .Select(instrument => instrument.Name)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
    }
}
