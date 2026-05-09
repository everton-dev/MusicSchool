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
  documentNumber: string;
  contactPhone: string;
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
  documentNumber: string;
  contactPhone: string;
  email: string;
  householdUserIds?: string[];
  scheduleSelection?: UserScheduleSelection;
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

  private pageParams(pageNumber: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
  }
}
