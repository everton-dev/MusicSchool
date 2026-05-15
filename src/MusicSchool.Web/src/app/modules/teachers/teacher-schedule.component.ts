import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import {
  CreateTeacherScheduleLessonRequest,
  MusicSchoolApiService,
  TeacherLessonTypeOption,
  TeacherSchedule,
  TeacherScheduleLesson,
  TeacherScheduleStudent,
  TeacherSummary
} from '../../core/api/music-school-api.service';
import { I18nService } from '../../core/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatIconModule, TranslatePipe],
  template: `
    <section class="feature-heading schedule-heading">
      <div>
        <p class="eyebrow">{{ 'SCHEDULE.TITLE' | appTranslate }}</p>
        <h1>{{ teacher?.name ?? ('nav.teachers' | appTranslate) }}</h1>
        <p>{{ 'SCHEDULE.INTRO' | appTranslate }}</p>
      </div>

      <a mat-button routerLink="/teachers">
        <mat-icon aria-hidden="true">arrow_back</mat-icon>
        <span>{{ 'actions.back' | appTranslate }}</span>
      </a>
    </section>

    @if (usingMockData) {
      <div class="offline-banner" role="status">
        <mat-icon aria-hidden="true">cloud_off</mat-icon>
        <span>{{ 'USER.MOCK_MODE' | appTranslate }}</span>
      </div>
    }

    @if (feedbackMessage) {
      <div class="success-banner" role="status">
        <mat-icon aria-hidden="true">check_circle</mat-icon>
        <span>{{ feedbackMessage }}</span>
      </div>
    }

    @if (conflictMessage) {
      <div class="conflict-banner" role="alert">
        <mat-icon aria-hidden="true">error</mat-icon>
        <span>{{ conflictMessage }}</span>
      </div>
    }

    <section class="schedule-layout">
      <form class="ods-card schedule-form" [formGroup]="scheduleForm" (ngSubmit)="saveLesson()">
        <h2>{{ 'SCHEDULE.NEW_LESSON' | appTranslate }}</h2>

        <label for="studentId">{{ 'label.student' | appTranslate }}</label>
        <select id="studentId" formControlName="studentId">
          @for (student of students; track student.studentId) {
            <option [value]="student.studentId">{{ student.name }}</option>
          }
        </select>

        <label for="instrumentId">{{ 'TEACHER.LESSONS' | appTranslate }}</label>
        <select id="instrumentId" formControlName="instrumentId">
          @for (lessonType of lessonTypeOptions; track lessonType.instrumentId) {
            <option [value]="lessonType.instrumentId">{{ lessonType.name }}</option>
          }
        </select>

        <label for="dayOfWeek">{{ 'SCHEDULE.DAY' | appTranslate }}</label>
        <select id="dayOfWeek" formControlName="dayOfWeek">
          @for (day of days; track day.value) {
            <option [value]="day.value">{{ day.labelKey | appTranslate }}</option>
          }
        </select>

        <label for="startTime">{{ 'SCHEDULE.START_TIME' | appTranslate }}</label>
        <input id="startTime" type="time" formControlName="startTime" />

        <label for="durationMinutes">{{ 'SCHEDULE.DURATION' | appTranslate }}</label>
        <select id="durationMinutes" formControlName="durationMinutes">
          <option [value]="30">30min</option>
          <option [value]="60">1h</option>
          <option [value]="90">1h 30min</option>
          <option [value]="120">2h</option>
        </select>

        <label for="recurrenceRule">{{ 'SCHEDULE.RECURRENCE' | appTranslate }}</label>
        <select id="recurrenceRule" formControlName="recurrenceRule">
          <option value="Weekly">{{ 'SCHEDULE.WEEKLY' | appTranslate }}</option>
          <option value="Daily">{{ 'SCHEDULE.DAILY' | appTranslate }}</option>
        </select>

        <button mat-flat-button type="submit" class="schedule-save">
          <mat-icon aria-hidden="true">event_available</mat-icon>
          <span>{{ 'SCHEDULE.SAVE' | appTranslate }}</span>
        </button>
      </form>

      <section class="calendar-panel ods-card">
        <div class="calendar-header">
          <h2>{{ 'SCHEDULE.WEEK_VIEW' | appTranslate }}</h2>
          @if (activePauses.length > 0) {
            <span class="pause-chip">{{ 'TEACHER.ON_LEAVE' | appTranslate }}</span>
          }
        </div>

        <div class="week-grid">
          @for (day of days; track day.value) {
            <article class="day-column">
              <h3>{{ day.labelKey | appTranslate }}</h3>
              @for (lesson of lessonsForDay(day.value); track lesson.id) {
                <div class="lesson-block">
                  <strong>{{ lesson.startTime }} - {{ lesson.endTime }}</strong>
                  <span>{{ lesson.studentName }}</span>
                  <small>{{ lesson.instrumentName }} · {{ lesson.durationMinutes }}m</small>
                </div>
              } @empty {
                <p class="empty-day">{{ 'empty.noDataFound' | appTranslate }}</p>
              }
            </article>
          }
        </div>
      </section>
    </section>
  `,
  styleUrl: './teacher-schedule.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeacherScheduleComponent implements OnInit {
  private readonly api = inject(MusicSchoolApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly i18n = inject(I18nService);

  protected readonly days = [
    { value: 1, labelKey: 'SCHEDULE.DAY.MONDAY' },
    { value: 2, labelKey: 'SCHEDULE.DAY.TUESDAY' },
    { value: 3, labelKey: 'SCHEDULE.DAY.WEDNESDAY' },
    { value: 4, labelKey: 'SCHEDULE.DAY.THURSDAY' },
    { value: 5, labelKey: 'SCHEDULE.DAY.FRIDAY' },
    { value: 6, labelKey: 'SCHEDULE.DAY.SATURDAY' },
    { value: 0, labelKey: 'SCHEDULE.DAY.SUNDAY' }
  ];

  protected readonly scheduleForm = this.fb.nonNullable.group({
    studentId: ['', Validators.required],
    instrumentId: ['', Validators.required],
    dayOfWeek: [1, Validators.required],
    startTime: ['16:00', Validators.required],
    durationMinutes: [60, Validators.required],
    recurrenceRule: ['Weekly' as 'Weekly' | 'Daily', Validators.required]
  });

  protected teacherId = '';
  protected teacher: TeacherSummary | null = null;
  protected schedule: TeacherSchedule | null = null;
  protected students: TeacherScheduleStudent[] = [];
  protected lessonTypeOptions: TeacherLessonTypeOption[] = [];
  protected usingMockData = false;
  protected conflictMessage = '';
  protected feedbackMessage = '';

  protected get activePauses() {
    return this.schedule?.pauses.filter((pause) => pause.isActive) ?? [];
  }

  ngOnInit(): void {
    this.teacherId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadPage();
  }

  protected lessonsForDay(day: number): TeacherScheduleLesson[] {
    return (this.schedule?.lessons ?? []).filter((lesson) => this.toDayNumber(lesson.dayOfWeek) === day);
  }

  protected saveLesson(): void {
    if (this.scheduleForm.invalid) {
      this.scheduleForm.markAllAsTouched();
      return;
    }

    const request = this.toRequest();
    if (this.hasFrontendConflict(request)) {
      this.conflictMessage = this.i18n.translate('SCHEDULE.CONFLICT');
      this.cdr.markForCheck();
      return;
    }

    if (this.usingMockData) {
      this.addLocalLesson(request);
      return;
    }

    this.api.createTeacherScheduleLesson(this.teacherId, request).subscribe({
      next: (schedule) => {
        this.schedule = schedule;
        this.feedbackMessage = schedule.billingUpdated ? this.i18n.translate('SCHEDULE.BILLING_UPDATED') : '';
        this.conflictMessage = '';
        this.cdr.markForCheck();
      },
      error: () => {
        this.enableMockData();
        this.addLocalLesson(request);
      }
    });
  }

  private loadPage(): void {
    this.api.getTeachers().subscribe({
      next: (teachers) => {
        this.teacher = teachers.find((item) => item.id === this.teacherId) ?? null;
        this.lessonTypeOptions = this.teacher?.lessonTypeOptions ?? [];
        this.loadSchedule();
      },
      error: () => {
        this.enableMockData();
        this.cdr.markForCheck();
      }
    });
  }

  private loadSchedule(): void {
    this.api.getTeacherSchedule(this.teacherId).subscribe({
      next: (schedule) => {
        this.schedule = schedule;
        this.students = schedule.students;
        this.applyDefaults();
        this.cdr.markForCheck();
      },
      error: () => {
        this.enableMockData();
        this.cdr.markForCheck();
      }
    });
  }

  private applyDefaults(): void {
    this.scheduleForm.patchValue({
      studentId: this.students[0]?.studentId ?? '',
      instrumentId: this.lessonTypeOptions[0]?.instrumentId ?? ''
    });
  }

  private addLocalLesson(request: CreateTeacherScheduleLessonRequest): void {
    const student = this.students.find((item) => item.studentId === request.studentId);
    const lessonType = this.lessonTypeOptions.find((item) => item.instrumentId === request.instrumentId);
    const days = request.recurrenceRule === 'Daily' ? this.days.map((day) => day.value) : [request.dayOfWeek];
    const lessons = days.map((day) => ({
      id: this.createId(),
      studentId: request.studentId,
      studentName: student?.name ?? 'Student',
      instrumentId: request.instrumentId,
      instrumentName: lessonType?.name ?? 'Lesson',
      dayOfWeek: day,
      startTime: request.startTime,
      endTime: this.addMinutes(request.startTime, request.durationMinutes),
      durationMinutes: request.durationMinutes,
      recurrenceRule: request.recurrenceRule,
      status: 'Scheduled'
    }));

    this.schedule = {
      teacherId: this.teacherId,
      lessons: [...(this.schedule?.lessons ?? []), ...lessons],
      pauses: this.schedule?.pauses ?? [],
      students: this.students,
      billingUpdated: true
    };
    this.feedbackMessage = this.i18n.translate('SCHEDULE.BILLING_UPDATED');
    this.conflictMessage = '';
    this.cdr.markForCheck();
  }

  private hasFrontendConflict(request: CreateTeacherScheduleLessonRequest): boolean {
    if (this.activePauses.length > 0) {
      return true;
    }

    const requestedDays = request.recurrenceRule === 'Daily' ? this.days.map((day) => day.value) : [request.dayOfWeek];
    const requestedEnd = this.addMinutes(request.startTime, request.durationMinutes);
    return (this.schedule?.lessons ?? []).some((lesson) =>
      requestedDays.includes(this.toDayNumber(lesson.dayOfWeek)) &&
      lesson.status !== 'Cancelled' &&
      this.overlaps(lesson.startTime, lesson.endTime, request.startTime, requestedEnd));
  }

  private toRequest(): CreateTeacherScheduleLessonRequest {
    return {
      tenantId: localStorage.getItem('music-school-tenant-id') ?? '11111111-1111-1111-1111-111111111111',
      studentId: this.scheduleForm.controls.studentId.value,
      instrumentId: this.scheduleForm.controls.instrumentId.value,
      dayOfWeek: Number(this.scheduleForm.controls.dayOfWeek.value),
      startTime: this.scheduleForm.controls.startTime.value,
      durationMinutes: Number(this.scheduleForm.controls.durationMinutes.value),
      recurrenceRule: this.scheduleForm.controls.recurrenceRule.value
    };
  }

  private enableMockData(): void {
    if (this.usingMockData) {
      return;
    }

    this.usingMockData = true;
    this.teacher = this.createMockTeacher();
    this.lessonTypeOptions = this.teacher.lessonTypeOptions;
    this.students = [
      { studentId: 'mock-student-sofia', userId: 'mock-user-sofia', name: 'Sofia Martins' },
      { studentId: 'mock-student-tiago', userId: 'mock-user-tiago', name: 'Tiago Martins' }
    ];
    this.schedule = {
      teacherId: this.teacherId,
      lessons: [
        {
          id: 'mock-lesson-1',
          studentId: 'mock-student-sofia',
          studentName: 'Sofia Martins',
          instrumentId: this.lessonTypeOptions[0].instrumentId,
          instrumentName: this.lessonTypeOptions[0].name,
          dayOfWeek: 2,
          startTime: '16:00',
          endTime: '17:00',
          durationMinutes: 60,
          recurrenceRule: 'Weekly',
          status: 'Scheduled'
        }
      ],
      pauses: this.teacher.isAvailable ? [] : [
        {
          id: 'mock-pause-1',
          teacherId: this.teacherId,
          reason: this.teacher.absenceReason ?? 'Vacation',
          startsOnUtc: new Date().toISOString(),
          isActive: true
        }
      ],
      students: this.students,
      billingUpdated: false
    };
    this.applyDefaults();
  }

  private createMockTeacher(): TeacherSummary {
    return {
      id: this.teacherId || 'mock-teacher-ana',
      userId: 'mock-user-ana',
      name: 'Ana Correia',
      email: 'ana.teacher@example.test',
      lessonTypes: ['Piano', 'Music Theory'],
      lessonTypeOptions: [
        { instrumentId: 'mock-instrument-piano', name: 'Piano' },
        { instrumentId: 'mock-instrument-theory', name: 'Music Theory' }
      ],
      isAvailable: true
    };
  }

  private toDayNumber(day: number | string): number {
    if (typeof day === 'number') {
      return day;
    }

    return this.days.find((item) => item.labelKey.toLowerCase().includes(day.toLowerCase()))?.value ?? Number(day);
  }

  private overlaps(existingStart: string, existingEnd: string, requestedStart: string, requestedEnd: string): boolean {
    return this.toMinutes(existingStart) < this.toMinutes(requestedEnd) && this.toMinutes(existingEnd) > this.toMinutes(requestedStart);
  }

  private addMinutes(time: string, minutes: number): string {
    const totalMinutes = this.toMinutes(time) + minutes;
    const hours = Math.floor(totalMinutes / 60).toString().padStart(2, '0');
    const mins = (totalMinutes % 60).toString().padStart(2, '0');
    return `${hours}:${mins}`;
  }

  private toMinutes(time: string): number {
    const [hours, minutes] = time.split(':').map(Number);
    return (hours * 60) + minutes;
  }

  private createId(): string {
    return globalThis.crypto?.randomUUID?.() ?? `mock-${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }
}
