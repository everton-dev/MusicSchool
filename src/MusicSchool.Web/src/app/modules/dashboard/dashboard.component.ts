import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DashboardCardComponent } from '../../shared/components/dashboard-card/dashboard-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [DashboardCardComponent, MatButtonModule, MatIconModule, StatusBadgeComponent, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">Oficina do Som</p>
        <h1>{{ 'dashboard.title' | appTranslate }}</h1>
        <p>{{ 'dashboard.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="quick-schedule-fab">
        <mat-icon aria-hidden="true">add_task</mat-icon>
        <span>{{ 'dashboard.quickSchedule' | appTranslate }}</span>
      </button>
    </section>

    @if (isLoading) {
      <section class="row metrics-row" aria-busy="true">
        @for (item of skeletonCards; track item) {
          <div class="col s12 m6 l4">
            <div class="skeleton skeleton-card"></div>
          </div>
        }
      </section>
    } @else {
      <section class="row metrics-row">
        @for (card of cards; track card.titleKey) {
          <div class="col s12 m6 l4">
            <app-dashboard-card
              [icon]="card.icon"
              [titleKey]="card.titleKey"
              [value]="card.value"
              [metaKey]="card.metaKey"
              [tone]="card.tone"
              actionKey="actions.open" />
          </div>
        }
      </section>
    }

    <section class="ods-panel z-depth-1 today-panel">
      <div>
        <p class="eyebrow">{{ 'dashboard.today' | appTranslate }}</p>
        <h2>18:00 · Piano · Studio 2</h2>
        <p>Sofia Martins · Maria Silva</p>
      </div>
      <app-status-badge tone="confirmed" labelKey="status.confirmed" />
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  protected readonly isLoading = false;
  protected readonly skeletonCards = [1, 2, 3, 4];

  protected readonly cards = [
    { icon: 'music_note', titleKey: 'dashboard.lessons', value: '128', metaKey: 'dashboard.lessonsMeta', tone: 'wine' as const },
    { icon: 'diversity_3', titleKey: 'dashboard.families', value: '42', metaKey: 'dashboard.familiesMeta', tone: 'red' as const },
    { icon: 'payments', titleKey: 'dashboard.payments', value: '6', metaKey: 'dashboard.paymentsMeta', tone: 'cyan' as const },
    { icon: 'trending_up', titleKey: 'dashboard.curriculum', value: '74%', metaKey: 'dashboard.curriculumMeta', tone: 'purple' as const }
  ];
}
