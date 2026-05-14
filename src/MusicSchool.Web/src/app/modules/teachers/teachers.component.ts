import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [MatButtonModule, MatIconModule, StatusBadgeComponent, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.teachers' | appTranslate }}</p>
        <h1>{{ 'teachers.title' | appTranslate }}</h1>
        <p>{{ 'teachers.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">person_add</mat-icon>
        <span>{{ 'actions.addTeacher' | appTranslate }}</span>
      </button>
    </section>

    <section class="row card-list">
      @for (teacher of teachers; track teacher.email) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1">
            <div class="card-topline">
              <mat-icon aria-hidden="true">music_note</mat-icon>
              <span>{{ teacher.instrument }}</span>
            </div>
            <h2>{{ teacher.name }}</h2>
            <p>{{ teacher.email }}</p>
            <app-status-badge tone="confirmed" labelKey="status.confirmed" />
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TeachersComponent {
  protected readonly teachers = [
    { name: 'Maria Silva', instrument: 'Piano, Voice', email: 'maria@oficinadosom.test' },
    { name: 'Rui Santos', instrument: 'Guitar', email: 'rui@oficinadosom.test' },
    { name: 'Carla Lopes', instrument: 'Violin', email: 'carla@oficinadosom.test' }
  ];
}
