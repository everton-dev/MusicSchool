import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [EmptyStateComponent, MatButtonModule, MatIconModule, StatusBadgeComponent, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.lessons' | appTranslate }}</p>
        <h1>{{ 'lessons.title' | appTranslate }}</h1>
        <p>{{ 'lessons.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">add_task</mat-icon>
        <span>{{ 'actions.addLesson' | appTranslate }}</span>
      </button>
    </section>

    @if (lessons.length) {
      <section class="row card-list">
        @for (lesson of lessons; track lesson.time + lesson.student) {
          <article class="col s12 m6 l4">
            <div class="ods-card z-depth-1">
              <div class="card-topline">
                <mat-icon aria-hidden="true">schedule</mat-icon>
                <strong>{{ lesson.time }}</strong>
                <app-status-badge tone="confirmed" labelKey="status.confirmed" />
              </div>
              <h2>{{ lesson.student }}</h2>
              <p>{{ 'label.teacher' | appTranslate }}: {{ lesson.teacher }}</p>
              <p>{{ lesson.instrument }}</p>
              <div class="card-action-icons">
                <button mat-icon-button type="button" class="action-icon edit-action" [attr.aria-label]="'actions.edit' | appTranslate">
                  <mat-icon aria-hidden="true">edit</mat-icon>
                </button>
                <button mat-icon-button type="button" class="action-icon delete-action" [attr.aria-label]="'actions.delete' | appTranslate">
                  <mat-icon aria-hidden="true">delete</mat-icon>
                </button>
              </div>
            </div>
          </article>
        }
      </section>
    } @else {
      <app-empty-state
        titleKey="empty.lessonsTitle"
        messageKey="empty.lessonsMessage"
        actionKey="actions.addLesson" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LessonsComponent {
  protected readonly lessons = [
    { time: '15:30', student: 'Sofia Martins', teacher: 'Maria Silva', instrument: 'Piano' },
    { time: '17:00', student: 'Leo Costa', teacher: 'Rui Santos', instrument: 'Guitar' },
    { time: '18:00', student: 'Ines Martins', teacher: 'Maria Silva', instrument: 'Voice' }
  ];
}
