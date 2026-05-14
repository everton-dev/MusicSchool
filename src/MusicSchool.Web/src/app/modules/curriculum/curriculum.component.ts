import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressBarModule, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.curriculum' | appTranslate }}</p>
        <h1>{{ 'curriculum.title' | appTranslate }}</h1>
        <p>{{ 'curriculum.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">library_add</mat-icon>
        <span>{{ 'actions.addCurriculum' | appTranslate }}</span>
      </button>
    </section>

    <section class="row card-list">
      @for (node of nodes; track node.student + node.title) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1 curriculum-card">
            <div class="card-topline">
              <mat-icon aria-hidden="true">library_music</mat-icon>
              <span>{{ node.instrument }}</span>
            </div>
            <h2>{{ node.title }}</h2>
            <p>{{ 'label.student' | appTranslate }}: {{ node.student }}</p>
            <mat-progress-bar mode="determinate" [value]="node.progress" />
            <strong>{{ node.progress }}%</strong>
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CurriculumComponent {
  protected readonly nodes = [
    { student: 'Sofia Martins', title: 'Rhythm foundations', instrument: 'Piano', progress: 68 },
    { student: 'Leo Costa', title: 'Chord fluency', instrument: 'Guitar', progress: 42 },
    { student: 'Ana Pereira', title: 'Sight reading', instrument: 'Violin', progress: 54 }
  ];
}
