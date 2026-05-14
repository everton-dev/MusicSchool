import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { TranslatePipe } from '../../pipes/translate.pipe';

export type StatusBadgeTone = 'confirmed' | 'pending' | 'review';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatusBadgeComponent {
  @Input() tone: StatusBadgeTone = 'pending';
  @Input({ required: true }) labelKey = '';
}
