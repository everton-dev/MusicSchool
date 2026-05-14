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
        <p class="eyebrow">{{ 'nav.payments' | appTranslate }}</p>
        <h1>{{ 'payments.title' | appTranslate }}</h1>
        <p>{{ 'payments.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">add_card</mat-icon>
        <span>{{ 'actions.addPayment' | appTranslate }}</span>
      </button>
    </section>

    @if (payments.length) {
      <section class="row card-list">
        @for (payment of payments; track payment.guardian + payment.amount) {
          <article class="col s12 m6 l4">
            <div class="ods-card z-depth-1">
              <div class="card-topline">
                <mat-icon aria-hidden="true">receipt_long</mat-icon>
                <span>{{ payment.method }}</span>
                <app-status-badge tone="review" labelKey="status.review" />
              </div>
              <h2>{{ payment.guardian }}</h2>
              <p>{{ 'label.student' | appTranslate }}: {{ payment.student }}</p>
              <strong>{{ payment.amount }}</strong>
              <div class="card-action-icons">
                <button mat-icon-button type="button" class="action-icon pay-action" [attr.aria-label]="'actions.pay' | appTranslate">
                  <mat-icon aria-hidden="true">payments</mat-icon>
                </button>
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
        titleKey="empty.paymentsTitle"
        messageKey="empty.paymentsMessage"
        actionKey="actions.addPayment" />
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentsComponent {
  protected readonly payments = [
    { guardian: 'Miguel Martins', student: 'Sofia Martins', method: 'Bank transfer', amount: 'EUR 180.00' },
    { guardian: 'Beatriz Costa', student: 'Leo Costa', method: 'MBWay', amount: 'EUR 90.00' },
    { guardian: 'Paulo Pereira', student: 'Ana Pereira', method: 'Bank transfer', amount: 'EUR 120.00' }
  ];
}
