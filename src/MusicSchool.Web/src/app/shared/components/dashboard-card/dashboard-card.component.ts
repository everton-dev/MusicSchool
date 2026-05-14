import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
  selector: 'app-dashboard-card',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, TranslatePipe],
  templateUrl: './dashboard-card.component.html',
  styleUrl: './dashboard-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardCardComponent {
  @Input({ required: true }) icon = '';
  @Input({ required: true }) titleKey = '';
  @Input({ required: true }) value = '';
  @Input({ required: true }) metaKey = '';
  @Input() tone: 'wine' | 'red' | 'cyan' | 'purple' = 'wine';
  @Input() actionKey = '';
}
