import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

@Component({
  standalone: true,
  imports: [MatButtonModule, MatIconModule, TranslatePipe],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.users' | appTranslate }}</p>
        <h1>{{ 'users.title' | appTranslate }}</h1>
        <p>{{ 'users.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab">
        <mat-icon aria-hidden="true">person_add</mat-icon>
        <span>{{ 'actions.addUser' | appTranslate }}</span>
      </button>
    </section>

    <section class="row card-list">
      @for (user of users; track user.email) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1">
            <div class="card-topline">
              <mat-icon aria-hidden="true">account_circle</mat-icon>
              <span>{{ user.role }}</span>
            </div>
            <h2>{{ user.name }}</h2>
            <p>{{ user.email }}</p>
            <div class="card-actions">
              <button mat-button type="button">
                <mat-icon aria-hidden="true">edit</mat-icon>
                <span>{{ 'actions.edit' | appTranslate }}</span>
              </button>
            </div>
          </div>
        </article>
      }
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersComponent {
  protected readonly users = [
    { name: 'Clara Admin', role: 'Admin', email: 'admin@oficinadosom.test' },
    { name: 'Miguel Martins', role: 'Guardian', email: 'miguel@example.test' },
    { name: 'Sofia Martins', role: 'Student', email: 'sofia@example.test' }
  ];
}
