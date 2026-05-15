import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MusicSchoolApiService, UserRegistrationRequest, UserSummary } from '../../core/api/music-school-api.service';
import { I18nService } from '../../core/services/i18n.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { UserModalComponent, UserModalPayload } from './user-modal.component';

@Component({
  standalone: true,
  imports: [MatButtonModule, MatIconModule, TranslatePipe, UserModalComponent],
  template: `
    <section class="feature-heading">
      <div>
        <p class="eyebrow">{{ 'nav.users' | appTranslate }}</p>
        <h1>{{ 'users.title' | appTranslate }}</h1>
        <p>{{ 'users.intro' | appTranslate }}</p>
      </div>

      <button mat-fab extended type="button" class="page-action-fab" (click)="openUserModal()">
        <mat-icon aria-hidden="true">person_add</mat-icon>
        <span>{{ 'actions.addUser' | appTranslate }}</span>
      </button>
    </section>

    @if (successMessage) {
      <div class="success-banner" role="status">
        <mat-icon aria-hidden="true">check_circle</mat-icon>
        <span>{{ successMessage }}</span>
      </div>
    }

    @if (usingMockData) {
      <div class="offline-banner" role="status">
        <mat-icon aria-hidden="true">cloud_off</mat-icon>
        <span>{{ 'USER.MOCK_MODE' | appTranslate }}</span>
      </div>
    }

    <section class="row card-list">
      @for (user of users; track user.email) {
        <article class="col s12 m6 l4">
          <div class="ods-card z-depth-1 user-card" [class.inactive-user]="!user.isActive">
            <div class="card-topline">
              <mat-icon aria-hidden="true">account_circle</mat-icon>
              <span>{{ ('USER.TYPE.' + user.profile) | appTranslate }}</span>
              <span class="user-status" [class.active]="user.isActive" [class.inactive]="!user.isActive">
                {{ (user.isActive ? 'USER.STATUS.ACTIVE' : 'USER.STATUS.INACTIVE') | appTranslate }}
              </span>
            </div>
            <h2>{{ user.name }}</h2>
            <p>{{ user.email }}</p>
            <div class="card-actions">
              <button mat-button type="button" (click)="openUserModal(user)">
                <mat-icon aria-hidden="true">edit</mat-icon>
                <span>{{ 'actions.edit' | appTranslate }}</span>
              </button>
              <button
                mat-icon-button
                type="button"
                class="status-toggle"
                [class.activate]="!user.isActive"
                [class.inactivate]="user.isActive"
                [attr.title]="'USER.STATUS_TOGGLE' | appTranslate"
                (click)="requestStatusToggle(user)"
              >
                <mat-icon aria-hidden="true">{{ user.isActive ? 'power_settings_new' : 'restart_alt' }}</mat-icon>
              </button>
            </div>
          </div>
        </article>
      }
    </section>

    @if (isUserModalOpen) {
      <app-user-modal [user]="editingUser" (cancelled)="closeUserModal()" (saved)="saveUser($event)" />
    }

    @if (pendingStatusUser) {
      <div class="confirm-backdrop" (click)="pendingStatusUser = null">
        <section class="confirm-modal z-depth-3" (click)="$event.stopPropagation()">
          <mat-icon aria-hidden="true">warning</mat-icon>
          <h2>{{ 'USER.CONFIRM_TITLE' | appTranslate }}</h2>
          <p>{{ 'USER.CONFIRM_INACTIVATE_CASCADE' | appTranslate }}</p>
          <div class="confirm-actions">
            <button mat-button type="button" (click)="pendingStatusUser = null">{{ 'actions.cancel' | appTranslate }}</button>
            <button mat-flat-button type="button" class="confirm-danger" (click)="confirmStatusToggle()">
              <mat-icon aria-hidden="true">power_settings_new</mat-icon>
              <span>{{ 'actions.proceed' | appTranslate }}</span>
            </button>
          </div>
        </section>
      </div>
    }
  `,
  styleUrl: './users.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UsersComponent implements OnInit {
  private readonly api = inject(MusicSchoolApiService);
  private readonly i18n = inject(I18nService);
  private readonly cdr = inject(ChangeDetectorRef);

  protected isUserModalOpen = false;
  protected editingUser: UserSummary | null = null;
  protected pendingStatusUser: UserSummary | null = null;
  protected successMessage = '';
  protected usingMockData = false;

  protected users: UserSummary[] = [];

  ngOnInit(): void {
    this.loadUsers();
  }

  protected openUserModal(user: UserSummary | null = null): void {
    this.editingUser = user;
    this.isUserModalOpen = true;
  }

  protected closeUserModal(): void {
    this.isUserModalOpen = false;
    this.editingUser = null;
  }

  protected saveUser(payload: UserModalPayload): void {
    if (this.usingMockData) {
      this.saveUserLocally(payload);
      return;
    }

    const request = this.toRegistrationRequest(payload);
    const request$ = this.editingUser
      ? this.api.updateUser(this.editingUser.id, request)
      : this.api.createUser(request);

    request$.subscribe({
      next: (user) => {
        this.upsertUser(user);
        this.showAutoStudentMessage(user.autoStudentCreatedCount);
        this.closeUserModal();
        this.cdr.markForCheck();
      },
      error: () => {
        this.enableMockData();
        this.saveUserLocally(payload);
      }
    });
  }

  protected requestStatusToggle(user: UserSummary): void {
    if (user.profile === 'Guardian' && user.isActive && user.householdMembers.length > 0) {
      this.pendingStatusUser = user;
      return;
    }

    this.toggleStatus(user);
  }

  protected confirmStatusToggle(): void {
    if (!this.pendingStatusUser) {
      return;
    }

    const user = this.pendingStatusUser;
    this.pendingStatusUser = null;
    this.toggleStatus(user);
  }

  private loadUsers(): void {
    this.api.getUsers().subscribe({
      next: (result) => {
        this.users = result.items;
        this.usingMockData = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.enableMockData();
      }
    });
  }

  private toggleStatus(user: UserSummary): void {
    if (this.usingMockData) {
      this.toggleStatusLocally(user);
      return;
    }

    this.api.updateUserStatus(user.id, !user.isActive).subscribe({
      next: (updatedUser) => {
        this.upsertUser(updatedUser);
        this.cdr.markForCheck();
      },
      error: () => {
        this.enableMockData();
        this.toggleStatusLocally(user);
      }
    });
  }

  private upsertUser(user: UserSummary): void {
    const existingIndex = this.users.findIndex((item) => item.id === user.id);
    this.users = existingIndex >= 0
      ? this.users.map((item) => item.id === user.id ? user : item)
      : [user, ...this.users];
  }

  private toRegistrationRequest(payload: UserModalPayload): UserRegistrationRequest {
    return {
      tenantId: this.currentTenantId(),
      name: payload.fullName,
      profile: payload.userType,
      fullAddress: payload.address,
      postalCode: payload.zipCode,
      docType: payload.docType,
      documentNumber: payload.docNumber,
      contactPhone: payload.phone,
      email: payload.email,
      birthDate: payload.birthDate || undefined,
      isStudent: payload.isStudent ?? false,
      householdMembers: payload.userType === 'Guardian'
        ? payload.householdMembers?.map((member) => ({
          userId: member.userId || undefined,
          name: member.name,
          birthDate: member.birthDate || undefined,
          docType: member.docType,
          documentNumber: member.docNumber,
          email: member.email
        })) ?? []
        : undefined,
      lessonTypes: payload.userType === 'Teacher' ? payload.lessonTypes ?? [] : undefined
    };
  }

  private showAutoStudentMessage(count: number): void {
    this.successMessage = count > 0
      ? this.i18n.translate('USER.AUTO_STUDENT_CREATED').replace('{count}', count.toString())
      : '';
  }

  private enableMockData(): void {
    if (!this.usingMockData) {
      this.usingMockData = true;
      this.users = this.users.length > 0 ? this.users : this.createMockUsers();
    }

    this.cdr.markForCheck();
  }

  private saveUserLocally(payload: UserModalPayload): void {
    const householdMembers = payload.userType === 'Guardian'
      ? this.syncLocalHouseholdMembers(payload)
      : [];
    const autoStudentCreatedCount = householdMembers.filter((member) => !payload.householdMembers?.some((input) => input.userId === member.userId)).length;
    const user: UserSummary = {
      id: this.editingUser?.id ?? this.createId(),
      name: payload.fullName,
      profile: payload.userType,
      fullAddress: payload.address,
      postalCode: payload.zipCode,
      docType: payload.docType,
      documentNumber: payload.docNumber,
      contactPhone: payload.phone,
      email: payload.email,
      birthDate: payload.birthDate || undefined,
      isActive: this.editingUser?.isActive ?? true,
      householdMembers,
      lessonTypes: payload.userType === 'Teacher' ? payload.lessonTypes ?? [] : [],
      autoStudentCreatedCount
    };

    this.upsertUser(user);
    this.showAutoStudentMessage(autoStudentCreatedCount);
    this.closeUserModal();
    this.cdr.markForCheck();
  }

  private syncLocalHouseholdMembers(payload: UserModalPayload): UserSummary['householdMembers'] {
    const previousMembers = this.editingUser?.householdMembers ?? [];
    const previousMemberIds = new Set(previousMembers.map((member) => member.userId));
    const retainedIds = new Set<string>();
    const householdMembers = (payload.householdMembers ?? []).map((member) => {
      const userId = member.userId || this.createId();
      retainedIds.add(userId);
      const existingStudent = this.users.find((user) => user.id === userId);
      const studentUser: UserSummary = {
        id: userId,
        name: member.name,
        profile: 'Student',
        fullAddress: payload.address,
        postalCode: payload.zipCode,
        docType: member.docType,
        documentNumber: member.docNumber,
        contactPhone: payload.phone,
        email: member.email,
        birthDate: member.birthDate || undefined,
        isActive: existingStudent?.isActive ?? true,
        householdMembers: [],
        lessonTypes: [],
        autoStudentCreatedCount: 0
      };

      this.upsertUser(studentUser);
      return {
        userId,
        name: member.name,
        birthDate: member.birthDate || undefined,
        docType: member.docType,
        documentNumber: member.docNumber,
        email: member.email,
        isActive: studentUser.isActive
      };
    });

    if (this.editingUser?.profile === 'Guardian') {
      for (const removedMemberId of previousMemberIds) {
        if (!retainedIds.has(removedMemberId)) {
          this.users = this.users.map((user) => user.id === removedMemberId ? { ...user, isActive: false } : user);
        }
      }
    }

    return householdMembers;
  }

  private toggleStatusLocally(user: UserSummary): void {
    const nextIsActive = !user.isActive;
    const linkedStudentIds = user.profile === 'Guardian' && !nextIsActive
      ? new Set(user.householdMembers.map((member) => member.userId))
      : new Set<string>();

    this.users = this.users.map((item) => {
      if (item.id === user.id) {
        return { ...item, isActive: nextIsActive };
      }

      if (linkedStudentIds.has(item.id)) {
        return { ...item, isActive: false };
      }

      return item;
    });
    this.cdr.markForCheck();
  }

  private createMockUsers(): UserSummary[] {
    const sofiaId = 'mock-student-sofia';
    const miguelId = 'mock-guardian-miguel';

    return [
      {
        id: 'mock-admin-clara',
        name: 'Clara Admin',
        profile: 'Admin',
        fullAddress: 'Rua da Escola 12, Lisboa',
        postalCode: '1000-001',
        docType: 'CC',
        documentNumber: 'CC-ADMIN-001',
        contactPhone: '+351 910 000 001',
        email: 'admin@oficinadosom.test',
        birthDate: '1988-03-12',
        isActive: true,
        householdMembers: [],
        lessonTypes: [],
        autoStudentCreatedCount: 0
      },
      {
        id: miguelId,
        name: 'Miguel Martins',
        profile: 'Guardian',
        fullAddress: 'Avenida Central 44, Lisboa',
        postalCode: '1050-010',
        docType: 'CC',
        documentNumber: 'CC-GUARDIAN-002',
        contactPhone: '+351 910 000 002',
        email: 'miguel@example.test',
        birthDate: '1982-07-21',
        isActive: true,
        householdMembers: [
          {
            userId: sofiaId,
            name: 'Sofia Martins',
            birthDate: '2014-04-12',
            docType: 'CC',
            documentNumber: 'CC-STUDENT-003',
            email: 'sofia@example.test',
            isActive: true
          }
        ],
        lessonTypes: [],
        autoStudentCreatedCount: 0
      },
      {
        id: sofiaId,
        name: 'Sofia Martins',
        profile: 'Student',
        fullAddress: 'Avenida Central 44, Lisboa',
        postalCode: '1050-010',
        docType: 'CC',
        documentNumber: 'CC-STUDENT-003',
        contactPhone: '+351 910 000 002',
        email: 'sofia@example.test',
        birthDate: '2014-04-12',
        isActive: true,
        householdMembers: [],
        lessonTypes: [],
        autoStudentCreatedCount: 0
      },
      {
        id: 'mock-teacher-ana',
        name: 'Ana Correia',
        profile: 'Teacher',
        fullAddress: 'Rua do Conservatorio 7, Lisboa',
        postalCode: '1200-120',
        docType: 'CC',
        documentNumber: 'CC-TEACHER-004',
        contactPhone: '+351 910 000 004',
        email: 'ana.teacher@example.test',
        birthDate: '1990-11-06',
        isActive: true,
        householdMembers: [],
        lessonTypes: ['Piano', 'Music Theory'],
        autoStudentCreatedCount: 0
      }
    ];
  }

  private createId(): string {
    return globalThis.crypto?.randomUUID?.() ?? `mock-${Date.now()}-${Math.random().toString(16).slice(2)}`;
  }

  private currentTenantId(): string {
    return localStorage.getItem('music-school-tenant-id') ?? '11111111-1111-1111-1111-111111111111';
  }
}
