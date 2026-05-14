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
        <p class="eyebrow">{{ 'nav.families' | appTranslate }}</p>
        <h1>{{ 'families.title' | appTranslate }}</h1>
        <p>{{ 'families.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">group_add</mat-icon>
        <span>{{ 'actions.addFamily' | appTranslate }}</span>
      </button>
    </section>

    <section class="row card-list">
      @for (family of families; track family.guardian) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1">
            <div class="card-topline">
              <mat-icon aria-hidden="true">family_restroom</mat-icon>
              <span>{{ 'label.guardian' | appTranslate }}</span>
            </div>
            <h2>{{ family.guardian }}</h2>
            <p>{{ family.students }}</p>
            <app-status-badge tone="pending" labelKey="status.pending" />
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FamiliesComponent {
  protected readonly families = [
    { guardian: 'Miguel Martins', students: 'Sofia Martins, Ines Martins' },
    { guardian: 'Beatriz Costa', students: 'Leo Costa' },
    { guardian: 'Paulo Pereira', students: 'Ana Pereira' }
  ];
}
