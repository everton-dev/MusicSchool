import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MusicSchoolApiService, TeacherSummary } from '../../core/api/music-school-api.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatButtonModule, MatIconModule, StatusBadgeComponent, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.teachers' | appTranslate }}</p>
        <h1>{{ 'teachers.title' | appTranslate }}</h1>
        <p>{{ 'teachers.intro' | appTranslate }}</p>
      </div>
    </section>

    @if (usingMockData) {
      <div class="offline-banner" role="status">
        <mat-icon aria-hidden="true">cloud_off</mat-icon>
        <span>{{ 'USER.MOCK_MODE' | appTranslate }}</span>
      </div>
    }

    <section class="row card-list">
      @for (teacher of teachers; track teacher.id) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1 teacher-card" [class.unavailable]="!teacher.isAvailable">
            <div class="card-topline">
              <mat-icon aria-hidden="true">music_note</mat-icon>
              <span>{{ teacher.lessonTypes.join(', ') || ('TEACHER.NO_LESSONS' | appTranslate) }}</span>
              @if (!teacher.isAvailable) {
                <span class="leave-badge">{{ 'TEACHER.ON_LEAVE' | appTranslate }}</span>
              }
            </div>
            <h2>{{ teacher.name }}</h2>
            <p>{{ teacher.email }}</p>
            @if (teacher.isAvailable) {
              <app-status-badge tone="confirmed" labelKey="status.confirmed" />
            } @else {
              <p class="absence-reason">{{ teacher.absenceReason }}</p>
            }
            <div class="card-actions">
              <a mat-button [routerLink]="['/teachers/schedule', teacher.id]">
                <mat-icon aria-hidden="true">calendar_month</mat-icon>
                <span>{{ 'SCHEDULE.OPEN' | appTranslate }}</span>
              </a>
              <button mat-icon-button type="button" class="pause-action" [attr.title]="'SCHEDULE.PAUSE_REASON' | appTranslate" (click)="openPauseModal(teacher)">
                <mat-icon aria-hidden="true">pause_circle</mat-icon>
              </button>
            </div>
          </div>
        </article>
      }
    </section>

    @if (pauseTeacher) {
      <div class="pause-backdrop" (click)="closePauseModal()">
        <form class="pause-modal z-depth-3" [formGroup]="pauseForm" (ngSubmit)="savePause()" (click)="$event.stopPropagation()">
          <mat-icon aria-hidden="true">pause_circle</mat-icon>
          <h2>{{ 'TEACHER.PAUSE_TITLE' | appTranslate }}</h2>
          <p>{{ pauseTeacher.name }}</p>
          <label for="pauseReason">{{ 'SCHEDULE.PAUSE_REASON' | appTranslate }}</label>
          <textarea id="pauseReason" formControlName="reason" rows="4"></textarea>
          <div class="pause-actions">
            <button mat-button type="button" (click)="closePauseModal()">{{ 'actions.cancel' | appTranslate }}</button>
            <button mat-flat-button type="submit" class="pause-save">
              <mat-icon aria-hidden="true">check</mat-icon>
              <span>{{ 'actions.save' | appTranslate }}</span>
            </button>
          </div>
        </form>
      </div>
    }
  `,
  styleUrl: './teachers.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeachersComponent implements OnInit {
  private readonly api = inject(MusicSchoolApiService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  protected teachers: TeacherSummary[] = [];
  protected usingMockData = false;
  protected pauseTeacher: TeacherSummary | null = null;
  protected readonly pauseForm = this.fb.nonNullable.group({
    reason: ['', Validators.required]
  });

  ngOnInit(): void {
    this.loadTeachers();
  }

  protected openPauseModal(teacher: TeacherSummary): void {
    this.pauseTeacher = teacher;
    this.pauseForm.reset({ reason: teacher.absenceReason ?? '' });
  }

  protected closePauseModal(): void {
    this.pauseTeacher = null;
  }

  protected savePause(): void {
    if (!this.pauseTeacher || this.pauseForm.invalid) {
      this.pauseForm.markAllAsTouched();
      return;
    }

    const reason = this.pauseForm.controls.reason.value;
    if (this.usingMockData) {
      this.applyLocalPause(this.pauseTeacher, reason);
      return;
    }

    this.api.pauseTeacher(this.pauseTeacher.id, { reason }).subscribe({
      next: () => this.applyLocalPause(this.pauseTeacher!, reason),
      error: () => {
        this.enableMockData();
        this.applyLocalPause(this.pauseTeacher!, reason);
      }
    });
  }

  private loadTeachers(): void {
    this.api.getTeachers().subscribe({
      next: (teachers) => {
        this.teachers = teachers;
        this.usingMockData = false;
        this.cdr.markForCheck();
      },
      error: () => this.enableMockData()
    });
  }

  private applyLocalPause(teacher: TeacherSummary, reason: string): void {
    this.teachers = this.teachers.map((item) =>
      item.id === teacher.id ? { ...item, isAvailable: false, absenceReason: reason } : item);
    this.closePauseModal();
    this.cdr.markForCheck();
  }

  private enableMockData(): void {
    if (!this.usingMockData) {
      this.usingMockData = true;
      this.teachers = this.teachers.length > 0 ? this.teachers : this.createMockTeachers();
    }

    this.cdr.markForCheck();
  }

  private createMockTeachers(): TeacherSummary[] {
    return [
      {
        id: 'mock-teacher-ana',
        userId: 'mock-user-ana',
        name: 'Ana Correia',
        email: 'ana.teacher@example.test',
        lessonTypes: ['Piano', 'Music Theory'],
        lessonTypeOptions: [
          { instrumentId: 'mock-instrument-piano', name: 'Piano' },
          { instrumentId: 'mock-instrument-theory', name: 'Music Theory' }
        ],
        isAvailable: true
      },
      {
        id: 'mock-teacher-rui',
        userId: 'mock-user-rui',
        name: 'Rui Santos',
        email: 'rui.teacher@example.test',
        lessonTypes: ['Guitar'],
        lessonTypeOptions: [
          { instrumentId: 'mock-instrument-guitar', name: 'Guitar' }
        ],
        isAvailable: true
      },
      {
        id: 'mock-teacher-carla',
        userId: 'mock-user-carla',
        name: 'Carla Lopes',
        email: 'carla.teacher@example.test',
        lessonTypes: ['Violin'],
        lessonTypeOptions: [
          { instrumentId: 'mock-instrument-violin', name: 'Violin' }
        ],
        isAvailable: false,
        absenceReason: 'Vacation'
      }
    ];
  }
}
