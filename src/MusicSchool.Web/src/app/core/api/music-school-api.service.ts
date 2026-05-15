import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PagedResult<TItem> {
  items: TItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export interface FamilyGroupSummary {
  id: string;
  displayName: string;
  guardianCount: number;
  studentCount: number;
}

export interface LessonSummary {
  id: string;
  teacherId: string;
  studentId: string;
  instrumentId: string;
  dayOfWeek: string;
  startTime: string;
  durationMinutes: number;
  timeZoneId: string;
  status: string;
}

export interface CurriculumNodeSummary {
  id: string;
  instrumentId: string;
  parentNodeId?: string;
  title: string;
  type: string;
  sortOrder: number;
  hasResource: boolean;
}

export interface PaymentSummary {
  id: string;
  studentId: string;
  guardianUserId: string;
  amount: number;
  currency: string;
  method: string;
  status: string;
  dueDate: string;
}

export type UserProfile = 'Admin' | 'Teacher' | 'Guardian' | 'Student';

export interface UserSummary {
  id: string;
  name: string;
  profile: UserProfile;
  fullAddress: string;
  postalCode: string;
  docType: string;
  documentNumber: string;
  contactPhone: string;
  email: string;
  birthDate?: string;
  isActive: boolean;
  householdMembers: HouseholdMemberSummary[];
  lessonTypes: string[];
  autoStudentCreatedCount: number;
}

export interface HouseholdMemberSummary {
  userId: string;
  name: string;
  birthDate?: string;
  docType: string;
  documentNumber: string;
  email: string;
  isActive: boolean;
}

export interface UserScheduleSelection {
  teacherId: string;
  instrumentId: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  timeZoneId: string;
}

export interface UserRegistrationRequest {
  tenantId: string;
  name: string;
  profile: UserProfile;
  fullAddress: string;
  postalCode: string;
  docType: string;
  documentNumber: string;
  contactPhone: string;
  email: string;
  birthDate?: string;
  isStudent?: boolean;
  householdUserIds?: string[];
  householdMembers?: HouseholdMemberRequest[];
  lessonTypes?: string[];
  scheduleSelection?: UserScheduleSelection;
}

export interface HouseholdMemberRequest {
  userId?: string;
  name: string;
  birthDate?: string;
  docType: string;
  documentNumber: string;
  email: string;
}

export interface TeacherScheduleOption {
  instrumentId: string;
  instrumentName: string;
  teacherId: string;
  teacherName: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  timeZoneId: string;
  isTaken: boolean;
  assignedStudentId?: string;
  assignedStudentName?: string;
}

export interface TeacherSummary {
  id: string;
  userId: string;
  name: string;
  email: string;
  lessonTypes: string[];
  lessonTypeOptions: TeacherLessonTypeOption[];
  isAvailable: boolean;
  absenceReason?: string;
}

export interface TeacherLessonTypeOption {
  instrumentId: string;
  name: string;
}

export interface TeacherPauseRequest {
  reason: string;
}

export interface TeacherPauseSummary {
  id: string;
  teacherId: string;
  reason: string;
  startsOnUtc: string;
  endsOnUtc?: string;
  isActive: boolean;
}

export interface TeacherScheduleLesson {
  id: string;
  studentId: string;
  studentName: string;
  instrumentId: string;
  instrumentName: string;
  dayOfWeek: number | string;
  startTime: string;
  endTime: string;
  durationMinutes: number;
  recurrenceRule: 'Weekly' | 'Daily' | string;
  status: string;
}

export interface TeacherScheduleStudent {
  studentId: string;
  userId: string;
  name: string;
}

export interface TeacherSchedule {
  teacherId: string;
  lessons: TeacherScheduleLesson[];
  pauses: TeacherPauseSummary[];
  students: TeacherScheduleStudent[];
  billingUpdated: boolean;
}

export interface CreateTeacherScheduleLessonRequest {
  tenantId: string;
  studentId: string;
  instrumentId: string;
  dayOfWeek: number;
  startTime: string;
  durationMinutes: number;
  recurrenceRule: 'Weekly' | 'Daily';
}

@Injectable({ providedIn: 'root' })
export class MusicSchoolApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getFamilyGroups(pageNumber = 1, pageSize = 20): Observable<PagedResult<FamilyGroupSummary>> {
    return this.http.get<PagedResult<FamilyGroupSummary>>(`${this.apiBaseUrl}/api/family-groups`, {
      params: this.pageParams(pageNumber, pageSize)
    });
  }

  getLessons(pageNumber = 1, pageSize = 20): Observable<PagedResult<LessonSummary>> {
    return this.http.get<PagedResult<LessonSummary>>(`${this.apiBaseUrl}/api/lessons`, {
      params: this.pageParams(pageNumber, pageSize)
    });
  }

  getCurriculumNodes(instrumentId?: string): Observable<CurriculumNodeSummary[]> {
    const params = instrumentId ? new HttpParams().set('instrumentId', instrumentId) : undefined;

    return this.http.get<CurriculumNodeSummary[]>(`${this.apiBaseUrl}/api/curriculum-nodes`, { params });
  }

  getPayments(pageNumber = 1, pageSize = 20): Observable<PagedResult<PaymentSummary>> {
    return this.http.get<PagedResult<PaymentSummary>>(`${this.apiBaseUrl}/api/payments`, {
      params: this.pageParams(pageNumber, pageSize)
    });
  }

  getUsers(pageNumber = 1, pageSize = 20): Observable<PagedResult<UserSummary>> {
    return this.http.get<PagedResult<UserSummary>>(`${this.apiBaseUrl}/api/users`, {
      params: this.pageParams(pageNumber, pageSize)
    });
  }

  createUser(request: UserRegistrationRequest): Observable<UserSummary> {
    return this.http.post<UserSummary>(`${this.apiBaseUrl}/api/users`, request);
  }

  updateUser(userId: string, request: UserRegistrationRequest): Observable<UserSummary> {
    return this.http.put<UserSummary>(`${this.apiBaseUrl}/api/users/${userId}`, request);
  }

  deactivateUser(userId: string): Observable<UserSummary> {
    return this.http.post<UserSummary>(`${this.apiBaseUrl}/api/users/${userId}/deactivate`, {});
  }

  updateUserStatus(userId: string, isActive: boolean): Observable<UserSummary> {
    return this.http.patch<UserSummary>(`${this.apiBaseUrl}/api/users/${userId}/status`, { isActive });
  }

  addGuardianHouseholdUser(guardianUserId: string, householdUserId: string): Observable<UserSummary> {
    return this.http.post<UserSummary>(`${this.apiBaseUrl}/api/users/${guardianUserId}/household-users`, {
      householdUserId
    });
  }

  getTeacherScheduleOptions(instrumentQuery: string): Observable<TeacherScheduleOption[]> {
    return this.http.get<TeacherScheduleOption[]>(`${this.apiBaseUrl}/api/users/teacher-schedule-options`, {
      params: new HttpParams().set('instrumentQuery', instrumentQuery)
    });
  }

  getTeachers(): Observable<TeacherSummary[]> {
    return this.http.get<TeacherSummary[]>(`${this.apiBaseUrl}/api/teachers`);
  }

  pauseTeacher(teacherId: string, request: TeacherPauseRequest): Observable<TeacherPauseSummary> {
    return this.http.post<TeacherPauseSummary>(`${this.apiBaseUrl}/api/teachers/${teacherId}/pause`, request);
  }

  getTeacherSchedule(teacherId: string): Observable<TeacherSchedule> {
    return this.http.get<TeacherSchedule>(`${this.apiBaseUrl}/api/teachers/${teacherId}/schedule`);
  }

  createTeacherScheduleLesson(teacherId: string, request: CreateTeacherScheduleLessonRequest): Observable<TeacherSchedule> {
    return this.http.post<TeacherSchedule>(`${this.apiBaseUrl}/api/teachers/${teacherId}/schedule`, request);
  }

  private pageParams(pageNumber: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
  }
}
