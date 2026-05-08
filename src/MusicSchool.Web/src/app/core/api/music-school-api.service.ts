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

  private pageParams(pageNumber: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
  }
}
