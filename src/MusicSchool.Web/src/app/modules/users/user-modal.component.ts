import { ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { HouseholdMemberSummary, UserSummary } from '../../core/api/music-school-api.service';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';

type UserType = 'Admin' | 'Guardian' | 'Teacher' | 'Student';
type DocumentType = 'BI' | 'CC' | 'Residence' | 'Passport';

type HouseholdMemberForm = {
  userId: FormControl<string>;
  name: FormControl<string>;
  birthDate: FormControl<string>;
  docType: FormControl<DocumentType>;
  docNumber: FormControl<string>;
  email: FormControl<string>;
};

type HouseholdMemberPayload = {
  userId?: string;
  name: string;
  birthDate: string;
  docType: DocumentType;
  docNumber: string;
  email: string;
};

type UserModalForm = {
  userType: FormControl<UserType>;
  fullName: FormControl<string>;
  docType: FormControl<DocumentType>;
  docNumber: FormControl<string>;
  birthDate: FormControl<string>;
  email: FormControl<string>;
  phone: FormControl<string>;
  address: FormControl<string>;
  zipCode: FormControl<string>;
  isStudent?: FormControl<boolean>;
  householdMembers?: FormArray<FormGroup<HouseholdMemberForm>>;
  lessonTypeInput?: FormControl<string>;
  lessonTypes?: FormControl<string[]>;
};

export type UserModalPayload = {
  userType: UserType;
  fullName: string;
  docType: DocumentType;
  docNumber: string;
  birthDate: string;
  email: string;
  phone: string;
  address: string;
  zipCode: string;
  isStudent?: boolean;
  householdMembers?: HouseholdMemberPayload[];
  lessonTypeInput?: string;
  lessonTypes?: string[];
};

@Component({
  selector: 'app-user-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatIconModule, TranslatePipe],
  templateUrl: './user-modal.component.html',
  styleUrl: './user-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UserModalComponent {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  @Input() user: UserSummary | null = null;
  @Output() readonly cancelled = new EventEmitter<void>();
  @Output() readonly saved = new EventEmitter<UserModalPayload>();

  protected readonly userTypes: UserType[] = ['Admin', 'Guardian', 'Teacher', 'Student'];
  protected readonly docTypes: DocumentType[] = ['BI', 'CC', 'Residence', 'Passport'];
  protected readonly lessonCatalog = [
    'Piano',
    'Guitar',
    'Voice',
    'Drums',
    'Violin',
    'Music Theory',
    'Saxophone',
    'Music Production'
  ];

  protected filteredLessonTypes = this.lessonCatalog;
  protected showLessonOptions = false;

  readonly form: FormGroup<UserModalForm> = this.fb.group<UserModalForm>({
    userType: this.fb.nonNullable.control('Guardian', Validators.required),
    fullName: this.fb.nonNullable.control('', Validators.required),
    docType: this.fb.nonNullable.control('CC', Validators.required),
    docNumber: this.fb.nonNullable.control('', Validators.required),
    birthDate: this.fb.nonNullable.control('', Validators.required),
    email: this.fb.nonNullable.control('', [Validators.required, Validators.email]),
    phone: this.fb.nonNullable.control('', Validators.required),
    address: this.fb.nonNullable.control(''),
    zipCode: this.fb.nonNullable.control('')
  });

  constructor() {
    this.applyUserType(this.form.controls.userType.value);

    this.form.controls.userType.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((userType) => this.applyUserType(userType));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['user']) {
      this.populateForEdit();
    }
  }

  protected get householdMembers(): FormArray<FormGroup<HouseholdMemberForm>> | null {
    return this.form.controls.householdMembers ?? null;
  }

  protected get selectedLessonTypes(): string[] {
    return this.form.controls.lessonTypes?.value ?? [];
  }

  protected get lessonTypeInput(): FormControl<string> | null {
    return this.form.controls.lessonTypeInput ?? null;
  }

  protected get isGuardian(): boolean {
    return this.form.controls.userType.value === 'Guardian';
  }

  protected get isTeacher(): boolean {
    return this.form.controls.userType.value === 'Teacher';
  }

  protected get isEditMode(): boolean {
    return !!this.user;
  }

  protected addMember(member?: HouseholdMemberSummary): void {
    if (!this.form.controls.householdMembers) {
      return;
    }

    this.form.controls.householdMembers.push(this.createHouseholdMember(member));
  }

  protected removeMember(index: number): void {
    this.form.controls.householdMembers?.removeAt(index);
  }

  protected addLessonType(lessonType: string): void {
    const normalizedLessonType = lessonType.trim();
    const lessonTypesControl = this.form.controls.lessonTypes;
    const inputControl = this.form.controls.lessonTypeInput;

    if (!lessonTypesControl || !inputControl || !normalizedLessonType) {
      return;
    }

    const existingLessonTypes = lessonTypesControl.value;
    const catalogMatch = this.lessonCatalog.find(
      (option) => option.toLocaleLowerCase() === normalizedLessonType.toLocaleLowerCase()
    );
    const valueToAdd = catalogMatch ?? normalizedLessonType;

    if (!existingLessonTypes.some((option) => option.toLocaleLowerCase() === valueToAdd.toLocaleLowerCase())) {
      lessonTypesControl.setValue([...existingLessonTypes, valueToAdd]);
      lessonTypesControl.markAsDirty();
    }

    inputControl.setValue('');
    this.filteredLessonTypes = this.lessonCatalog.filter((option) => !lessonTypesControl.value.includes(option));
    this.showLessonOptions = false;
  }

  protected removeLessonType(lessonType: string): void {
    const lessonTypesControl = this.form.controls.lessonTypes;

    if (!lessonTypesControl) {
      return;
    }

    lessonTypesControl.setValue(lessonTypesControl.value.filter((option) => option !== lessonType));
    lessonTypesControl.markAsDirty();
    this.updateFilteredLessons(this.form.controls.lessonTypeInput?.value ?? '');
  }

  protected commitLessonInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.addLessonType(input.value);
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saved.emit(this.form.getRawValue() as UserModalPayload);
  }

  protected cancel(): void {
    this.cancelled.emit();
  }

  protected fieldHasError(controlName: keyof UserModalForm, errorName: string): boolean {
    const control = this.form.controls[controlName];
    return !!control?.hasError(errorName) && (control.touched || control.dirty);
  }

  protected memberFieldHasError(index: number, controlName: keyof HouseholdMemberForm, errorName: string): boolean {
    const control = this.form.controls.householdMembers?.at(index).controls[controlName];
    return !!control?.hasError(errorName) && (control.touched || control.dirty);
  }

  private applyUserType(userType: UserType): void {
    if (userType === 'Guardian') {
      this.addGuardianControls();
    } else {
      this.removeGuardianControls();
    }

    if (userType === 'Teacher') {
      this.addTeacherControls();
    } else {
      this.removeTeacherControls();
    }
  }

  private addGuardianControls(): void {
    if (!this.form.controls.isStudent) {
      this.form.addControl('isStudent', this.fb.nonNullable.control(false));
    }

    if (!this.form.controls.householdMembers) {
      this.form.addControl('householdMembers', this.fb.array<FormGroup<HouseholdMemberForm>>([]));
      this.addMember();
    }
  }

  private removeGuardianControls(): void {
    if (this.form.controls.isStudent) {
      this.form.removeControl('isStudent');
    }

    if (this.form.controls.householdMembers) {
      this.form.removeControl('householdMembers');
    }
  }

  private addTeacherControls(): void {
    if (!this.form.controls.lessonTypes) {
      this.form.addControl('lessonTypes', this.fb.nonNullable.control<string[]>([], Validators.required));
    }

    if (!this.form.controls.lessonTypeInput) {
      const lessonInput = this.fb.nonNullable.control('');
      this.form.addControl('lessonTypeInput', lessonInput);
      lessonInput.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((searchTerm) => this.updateFilteredLessons(searchTerm));
    }

    this.updateFilteredLessons('');
  }

  private removeTeacherControls(): void {
    if (this.form.controls.lessonTypeInput) {
      this.form.removeControl('lessonTypeInput');
    }

    if (this.form.controls.lessonTypes) {
      this.form.removeControl('lessonTypes');
    }

    this.showLessonOptions = false;
  }

  private populateForEdit(): void {
    if (!this.user) {
      return;
    }

    this.form.patchValue({
      userType: this.user.profile,
      fullName: this.user.name,
      docType: this.toDocumentType(this.user.docType),
      docNumber: this.user.documentNumber,
      birthDate: this.user.birthDate ?? '',
      email: this.user.email,
      phone: this.user.contactPhone,
      address: this.user.fullAddress,
      zipCode: this.user.postalCode
    });

    this.applyUserType(this.user.profile);

    if (this.user.profile === 'Guardian' && this.form.controls.householdMembers) {
      this.form.controls.householdMembers.clear();
      for (const member of this.user.householdMembers ?? []) {
        this.addMember(member);
      }
    }

    if (this.user.profile === 'Teacher' && this.form.controls.lessonTypes) {
      this.form.controls.lessonTypes.setValue(this.user.lessonTypes ?? []);
      this.updateFilteredLessons(this.form.controls.lessonTypeInput?.value ?? '');
    }
  }

  private createHouseholdMember(member?: HouseholdMemberSummary): FormGroup<HouseholdMemberForm> {
    return this.fb.group<HouseholdMemberForm>({
      userId: this.fb.nonNullable.control(member?.userId ?? ''),
      name: this.fb.nonNullable.control(member?.name ?? '', Validators.required),
      birthDate: this.fb.nonNullable.control(member?.birthDate ?? '', Validators.required),
      docType: this.fb.nonNullable.control(this.toDocumentType(member?.docType ?? 'CC'), Validators.required),
      docNumber: this.fb.nonNullable.control(member?.documentNumber ?? '', Validators.required),
      email: this.fb.nonNullable.control(member?.email ?? '', [Validators.required, Validators.email])
    });
  }

  private toDocumentType(value: string): DocumentType {
    return this.docTypes.includes(value as DocumentType) ? value as DocumentType : 'CC';
  }

  private updateFilteredLessons(searchTerm: string): void {
    const selectedLessons = new Set(this.form.controls.lessonTypes?.value ?? []);
    const normalizedSearchTerm = searchTerm.trim().toLocaleLowerCase();

    this.filteredLessonTypes = this.lessonCatalog.filter((lessonType) => {
      const isSelected = selectedLessons.has(lessonType);
      const matchesSearch = !normalizedSearchTerm || lessonType.toLocaleLowerCase().includes(normalizedSearchTerm);
      return !isSelected && matchesSearch;
    });
  }
}
