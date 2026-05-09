import { CommonModule } from '@angular/common';
import { BreakpointObserver } from '@angular/cdk/layout';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, LOCALE_ID, OnInit, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { map } from 'rxjs';
import {
  MusicSchoolApiService,
  TeacherScheduleOption,
  UserRegistrationRequest,
  UserSummary
} from './core/api/music-school-api.service';

const text = {
  'en-US': {
    appTitle: 'Music School',
    loginEyebrow: 'Private school portal',
    loginTitle: 'Sign in',
    loginSubtitle: 'Access lessons, payments, curriculum progress, and school operations.',
    loginEmailLabel: 'Email',
    loginPasswordLabel: 'Password',
    loginRememberLabel: 'Keep me signed in',
    loginAction: 'Sign in',
    loginError: 'Enter an email and password.',
    logoutAction: 'Sign out',
    primaryNavigationLabel: 'Primary navigation',
    openNavigationLabel: 'Open navigation',
    workspaceTitle: 'Operations workspace',
    roleSelectorLabel: 'Role',
    selectedRoleLabel: 'Selected role',
    languageSelectorLabel: 'Language',
    selectedLanguageLabel: 'Selected language',
    accountMenuLabel: 'Account menu',
    tenantContextLabel: 'Lisbon central school',
    dashboardHeading: 'School operations',
    dashboardIntro: 'Coordinate individual lessons, family billing, teacher resources, and student roadmap progress from one focused workspace.',
    uploadResourceAction: 'Upload resource',
    scheduleLessonAction: 'Schedule lesson',
    operationalMetricsLabel: 'Operational metrics',
    todayTabLabel: 'Today',
    schedulePanelTitle: 'Individual lessons',
    schedulePanelSubtitle: 'Fixed weekly 1-on-1 lessons in local teaching time.',
    scheduleFiltersLabel: 'Schedule filters',
    activeLessonsChip: 'Active',
    trialLessonsChip: 'Trial',
    lessonTimeColumn: 'Time',
    lessonStudentColumn: 'Student',
    lessonTeacherColumn: 'Teacher',
    lessonInstrumentColumn: 'Instrument',
    lessonStatusColumn: 'Status',
    quickScheduleTitle: 'Quick scheduling',
    studentFieldLabel: 'Student',
    teacherFieldLabel: 'Teacher',
    instrumentFieldLabel: 'Instrument',
    startTimeFieldLabel: 'Start time',
    createLessonAction: 'Create fixed lesson',
    familiesTabLabel: 'Families',
    familiesPanelTitle: 'Family groups',
    familiesPanelSubtitle: 'Guardians can manage and pay for multiple students.',
    addFamilyAction: 'Add family',
    familyGuardianColumn: 'Guardian',
    familyStudentsColumn: 'Students',
    familyPayerStatusColumn: 'Payer status',
    familyNextActionColumn: 'Next action',
    curriculumTabLabel: 'Curriculum',
    paymentsTabLabel: 'Payments',
    paymentsPanelTitle: 'Manual payment queue',
    paymentsPanelSubtitle: 'Track MBWay and bank transfer confirmations for Portugal.',
    registerPaymentAction: 'Register payment',
    paymentGuardianColumn: 'Guardian',
    paymentStudentColumn: 'Student',
    paymentMethodColumn: 'Method',
    paymentAmountColumn: 'Amount',
    paymentStatusColumn: 'Status',
    dashboardNavLabel: 'Dashboard',
    lessonsNavLabel: 'Lessons',
    familiesNavLabel: 'Families',
    usersNavLabel: 'Users',
    teacherRegisterNavLabel: 'Teacher Register',
    curriculumNavLabel: 'Curriculum',
    paymentsNavLabel: 'Payments',
    usersTabLabel: 'Users',
    usersPanelTitle: 'Website users',
    usersPanelSubtitle: 'Create, edit, deactivate, assign households, and link students to teacher slots.',
    addUserAction: 'Add user',
    editUserAction: 'Edit user',
    deactivateUserAction: 'Deactivate user',
    saveUserAction: 'Save user',
    userNameColumn: 'Name',
    userProfileColumn: 'Profile',
    userContactColumn: 'Contact',
    userAddressColumn: 'Address',
    userStatusColumn: 'Status',
    nameFieldLabel: 'Name',
    profileFieldLabel: 'Profile',
    fullAddressFieldLabel: 'Full address',
    postalCodeFieldLabel: 'Postal code',
    documentNumberFieldLabel: 'Document number',
    contactPhoneFieldLabel: 'Contact phone',
    emailFieldLabel: 'Email',
    householdFieldLabel: 'Household users',
    instrumentSearchFieldLabel: 'Instrument search',
    teacherScheduleOptionsLabel: 'Teacher schedule options',
    availableSlotStatus: 'Available',
    takenSlotStatus: 'Taken',
    activeUserStatus: 'Active',
    inactiveUserStatus: 'Inactive',
    teachersTabLabel: 'Teachers',
    teacherRegisterTitle: 'Teacher register',
    teacherRegisterSubtitle: 'Register teachers, update teaching details, inactivate profiles, and maintain weekly availability.',
    registerTeacherAction: 'Register teacher',
    updateTeacherAction: 'Update teacher',
    inactivateTeacherAction: 'Inactivate teacher',
    saveTeacherAction: 'Save teacher',
    cancelAction: 'Cancel',
    teacherNameColumn: 'Teacher',
    teacherEmailColumn: 'Email',
    teacherInstrumentsColumn: 'Instruments',
    teacherStatusColumn: 'Status',
    teacherSlotsColumn: 'Weekly slots',
    teacherNameFieldLabel: 'Teacher name',
    teacherEmailFieldLabel: 'Email',
    teacherInstrumentsFieldLabel: 'Instruments',
    teacherStatusFieldLabel: 'Status',
    activeStatus: 'Active',
    inactiveStatus: 'Inactive',
    scheduleTitle: 'Teacher schedule',
    scheduleSubtitle: 'Register fixed weekly availability before assigning individual lessons.',
    scheduleTeacherFieldLabel: 'Teacher',
    weekdayFieldLabel: 'Weekday',
    scheduleStartFieldLabel: 'Start time',
    scheduleDurationFieldLabel: 'Duration',
    scheduleRoomFieldLabel: 'Room',
    registerScheduleAction: 'Register schedule',
    scheduleTeacherColumn: 'Teacher',
    scheduleDayColumn: 'Day',
    scheduleTimeColumn: 'Time',
    scheduleDurationColumn: 'Duration',
    scheduleRoomColumn: 'Room',
    scheduleStatusColumn: 'Status',
    mondayLabel: 'Monday',
    tuesdayLabel: 'Tuesday',
    wednesdayLabel: 'Wednesday',
    thursdayLabel: 'Thursday',
    minutes45: '45 min',
    minutes60: '60 min',
    weeklyLessonsMetric: 'Weekly lessons',
    weeklyLessonsDetail: '+12 this month',
    activeFamiliesMetric: 'Active families',
    activeFamiliesDetail: '118 linked students',
    openPaymentsMetric: 'Open payments',
    openPaymentsDetail: 'MBWay and transfer',
    roadmapResourcesMetric: 'Roadmap resources',
    roadmapResourcesDetail: 'PDF and MP3 files',
    confirmedStatus: 'Confirmed',
    pendingStatus: 'Pending',
    primaryPayerStatus: 'Primary payer',
    paymentPendingStatus: 'Payment pending',
    sharedGuardianStatus: 'Shared guardian',
    sendReceiptAction: 'Send receipt',
    confirmMbwayAction: 'Confirm MBWay',
    reviewTransferAction: 'Review transfer',
    needsConfirmationStatus: 'Needs confirmation',
    receiptPendingStatus: 'Receipt pending',
    rhythmNodeTitle: 'Deep Dive 1: Rhythm foundations',
    chordNodeTitle: 'Deep Dive 2: Chord fluency',
    sightReadingNodeTitle: 'Deep Dive 3: Sight reading',
    resources8: '8 resources',
    resources6: '6 resources',
    resources11: '11 resources'
  },
  'en-GB': {
    appTitle: 'Music School',
    loginEyebrow: 'Private school portal',
    loginTitle: 'Sign in',
    loginSubtitle: 'Access lessons, payments, curriculum progress, and school operations.',
    loginEmailLabel: 'Email',
    loginPasswordLabel: 'Password',
    loginRememberLabel: 'Keep me signed in',
    loginAction: 'Sign in',
    loginError: 'Enter an email and password.',
    logoutAction: 'Sign out',
    primaryNavigationLabel: 'Primary navigation',
    openNavigationLabel: 'Open navigation',
    workspaceTitle: 'Operations workspace',
    roleSelectorLabel: 'Role',
    selectedRoleLabel: 'Selected role',
    languageSelectorLabel: 'Language',
    selectedLanguageLabel: 'Selected language',
    accountMenuLabel: 'Account menu',
    tenantContextLabel: 'Lisbon central school',
    dashboardHeading: 'School operations',
    dashboardIntro: 'Coordinate individual lessons, family billing, teacher resources, and student roadmap progress from one focused workspace.',
    uploadResourceAction: 'Upload resource',
    scheduleLessonAction: 'Schedule lesson',
    operationalMetricsLabel: 'Operational metrics',
    todayTabLabel: 'Today',
    schedulePanelTitle: 'Individual lessons',
    schedulePanelSubtitle: 'Fixed weekly 1-on-1 lessons in local teaching time.',
    scheduleFiltersLabel: 'Schedule filters',
    activeLessonsChip: 'Active',
    trialLessonsChip: 'Trial',
    lessonTimeColumn: 'Time',
    lessonStudentColumn: 'Student',
    lessonTeacherColumn: 'Teacher',
    lessonInstrumentColumn: 'Instrument',
    lessonStatusColumn: 'Status',
    quickScheduleTitle: 'Quick scheduling',
    studentFieldLabel: 'Student',
    teacherFieldLabel: 'Teacher',
    instrumentFieldLabel: 'Instrument',
    startTimeFieldLabel: 'Start time',
    createLessonAction: 'Create fixed lesson',
    familiesTabLabel: 'Families',
    familiesPanelTitle: 'Family groups',
    familiesPanelSubtitle: 'Guardians can manage and pay for multiple students.',
    addFamilyAction: 'Add family',
    familyGuardianColumn: 'Guardian',
    familyStudentsColumn: 'Students',
    familyPayerStatusColumn: 'Payer status',
    familyNextActionColumn: 'Next action',
    curriculumTabLabel: 'Curriculum',
    paymentsTabLabel: 'Payments',
    paymentsPanelTitle: 'Manual payment queue',
    paymentsPanelSubtitle: 'Track MBWay and bank transfer confirmations for Portugal.',
    registerPaymentAction: 'Register payment',
    paymentGuardianColumn: 'Guardian',
    paymentStudentColumn: 'Student',
    paymentMethodColumn: 'Method',
    paymentAmountColumn: 'Amount',
    paymentStatusColumn: 'Status',
    dashboardNavLabel: 'Dashboard',
    lessonsNavLabel: 'Lessons',
    familiesNavLabel: 'Families',
    usersNavLabel: 'Users',
    teacherRegisterNavLabel: 'Teacher Register',
    curriculumNavLabel: 'Curriculum',
    paymentsNavLabel: 'Payments',
    usersTabLabel: 'Users',
    usersPanelTitle: 'Website users',
    usersPanelSubtitle: 'Create, edit, deactivate, assign households, and link students to teacher slots.',
    addUserAction: 'Add user',
    editUserAction: 'Edit user',
    deactivateUserAction: 'Deactivate user',
    saveUserAction: 'Save user',
    userNameColumn: 'Name',
    userProfileColumn: 'Profile',
    userContactColumn: 'Contact',
    userAddressColumn: 'Address',
    userStatusColumn: 'Status',
    nameFieldLabel: 'Name',
    profileFieldLabel: 'Profile',
    fullAddressFieldLabel: 'Full address',
    postalCodeFieldLabel: 'Postal code',
    documentNumberFieldLabel: 'Document number',
    contactPhoneFieldLabel: 'Contact phone',
    emailFieldLabel: 'Email',
    householdFieldLabel: 'Household users',
    instrumentSearchFieldLabel: 'Instrument search',
    teacherScheduleOptionsLabel: 'Teacher schedule options',
    availableSlotStatus: 'Available',
    takenSlotStatus: 'Taken',
    activeUserStatus: 'Active',
    inactiveUserStatus: 'Inactive',
    teachersTabLabel: 'Teachers',
    teacherRegisterTitle: 'Teacher register',
    teacherRegisterSubtitle: 'Register teachers, update teaching details, inactivate profiles, and maintain weekly availability.',
    registerTeacherAction: 'Register teacher',
    updateTeacherAction: 'Update teacher',
    inactivateTeacherAction: 'Inactivate teacher',
    saveTeacherAction: 'Save teacher',
    cancelAction: 'Cancel',
    teacherNameColumn: 'Teacher',
    teacherEmailColumn: 'Email',
    teacherInstrumentsColumn: 'Instruments',
    teacherStatusColumn: 'Status',
    teacherSlotsColumn: 'Weekly slots',
    teacherNameFieldLabel: 'Teacher name',
    teacherEmailFieldLabel: 'Email',
    teacherInstrumentsFieldLabel: 'Instruments',
    teacherStatusFieldLabel: 'Status',
    activeStatus: 'Active',
    inactiveStatus: 'Inactive',
    scheduleTitle: 'Teacher schedule',
    scheduleSubtitle: 'Register fixed weekly availability before assigning individual lessons.',
    scheduleTeacherFieldLabel: 'Teacher',
    weekdayFieldLabel: 'Weekday',
    scheduleStartFieldLabel: 'Start time',
    scheduleDurationFieldLabel: 'Duration',
    scheduleRoomFieldLabel: 'Room',
    registerScheduleAction: 'Register schedule',
    scheduleTeacherColumn: 'Teacher',
    scheduleDayColumn: 'Day',
    scheduleTimeColumn: 'Time',
    scheduleDurationColumn: 'Duration',
    scheduleRoomColumn: 'Room',
    scheduleStatusColumn: 'Status',
    mondayLabel: 'Monday',
    tuesdayLabel: 'Tuesday',
    wednesdayLabel: 'Wednesday',
    thursdayLabel: 'Thursday',
    minutes45: '45 min',
    minutes60: '60 min',
    weeklyLessonsMetric: 'Weekly lessons',
    weeklyLessonsDetail: '+12 this month',
    activeFamiliesMetric: 'Active families',
    activeFamiliesDetail: '118 linked students',
    openPaymentsMetric: 'Open payments',
    openPaymentsDetail: 'MBWay and transfer',
    roadmapResourcesMetric: 'Roadmap resources',
    roadmapResourcesDetail: 'PDF and MP3 files',
    confirmedStatus: 'Confirmed',
    pendingStatus: 'Pending',
    primaryPayerStatus: 'Primary payer',
    paymentPendingStatus: 'Payment pending',
    sharedGuardianStatus: 'Shared guardian',
    sendReceiptAction: 'Send receipt',
    confirmMbwayAction: 'Confirm MBWay',
    reviewTransferAction: 'Review transfer',
    needsConfirmationStatus: 'Needs confirmation',
    receiptPendingStatus: 'Receipt pending',
    rhythmNodeTitle: 'Deep Dive 1: Rhythm foundations',
    chordNodeTitle: 'Deep Dive 2: Chord fluency',
    sightReadingNodeTitle: 'Deep Dive 3: Sight reading',
    resources8: '8 resources',
    resources6: '6 resources',
    resources11: '11 resources'
  },
  'pt-PT': {
    appTitle: 'Escola de Música',
    loginEyebrow: 'Portal privado da escola',
    loginTitle: 'Iniciar sessão',
    loginSubtitle: 'Aceda a aulas, pagamentos, progresso curricular e operações da escola.',
    loginEmailLabel: 'Email',
    loginPasswordLabel: 'Palavra-passe',
    loginRememberLabel: 'Manter sessão iniciada',
    loginAction: 'Entrar',
    loginError: 'Introduza email e palavra-passe.',
    logoutAction: 'Terminar sessão',
    primaryNavigationLabel: 'Navegação principal',
    openNavigationLabel: 'Abrir navegação',
    workspaceTitle: 'Área de operações',
    roleSelectorLabel: 'Função',
    selectedRoleLabel: 'Função selecionada',
    languageSelectorLabel: 'Idioma',
    selectedLanguageLabel: 'Idioma selecionado',
    accountMenuLabel: 'Menu da conta',
    tenantContextLabel: 'Escola central de Lisboa',
    dashboardHeading: 'Operações da escola',
    dashboardIntro: 'Coordene aulas individuais, faturação familiar, recursos dos professores e progresso dos alunos no roteiro a partir de um único espaço de trabalho.',
    uploadResourceAction: 'Carregar recurso',
    scheduleLessonAction: 'Agendar aula',
    operationalMetricsLabel: 'Métricas operacionais',
    todayTabLabel: 'Hoje',
    schedulePanelTitle: 'Aulas individuais',
    schedulePanelSubtitle: 'Aulas individuais semanais fixas no horário local.',
    scheduleFiltersLabel: 'Filtros de agenda',
    activeLessonsChip: 'Ativas',
    trialLessonsChip: 'Experimental',
    lessonTimeColumn: 'Hora',
    lessonStudentColumn: 'Aluno',
    lessonTeacherColumn: 'Professor',
    lessonInstrumentColumn: 'Instrumento',
    lessonStatusColumn: 'Estado',
    quickScheduleTitle: 'Agendamento rápido',
    studentFieldLabel: 'Aluno',
    teacherFieldLabel: 'Professor',
    instrumentFieldLabel: 'Instrumento',
    startTimeFieldLabel: 'Hora de início',
    createLessonAction: 'Criar aula fixa',
    familiesTabLabel: 'Famílias',
    familiesPanelTitle: 'Grupos familiares',
    familiesPanelSubtitle: 'Encarregados podem gerir e pagar por vários alunos.',
    addFamilyAction: 'Adicionar família',
    familyGuardianColumn: 'Encarregado',
    familyStudentsColumn: 'Alunos',
    familyPayerStatusColumn: 'Estado do pagador',
    familyNextActionColumn: 'Próxima ação',
    curriculumTabLabel: 'Currículo',
    paymentsTabLabel: 'Pagamentos',
    paymentsPanelTitle: 'Fila de pagamentos manuais',
    paymentsPanelSubtitle: 'Acompanhe confirmações de MBWay e transferência bancária em Portugal.',
    registerPaymentAction: 'Registar pagamento',
    paymentGuardianColumn: 'Encarregado',
    paymentStudentColumn: 'Aluno',
    paymentMethodColumn: 'Método',
    paymentAmountColumn: 'Valor',
    paymentStatusColumn: 'Estado',
    dashboardNavLabel: 'Painel',
    lessonsNavLabel: 'Aulas',
    familiesNavLabel: 'Famílias',
    usersNavLabel: 'Utilizadores',
    teacherRegisterNavLabel: 'Registo de professores',
    curriculumNavLabel: 'Currículo',
    paymentsNavLabel: 'Pagamentos',
    usersTabLabel: 'Utilizadores',
    usersPanelTitle: 'Utilizadores do website',
    usersPanelSubtitle: 'Crie, edite, desative, atribua agregados e associe alunos aos horários dos professores.',
    addUserAction: 'Adicionar utilizador',
    editUserAction: 'Editar utilizador',
    deactivateUserAction: 'Desativar utilizador',
    saveUserAction: 'Guardar utilizador',
    userNameColumn: 'Nome',
    userProfileColumn: 'Perfil',
    userContactColumn: 'Contacto',
    userAddressColumn: 'Morada',
    userStatusColumn: 'Estado',
    nameFieldLabel: 'Nome',
    profileFieldLabel: 'Perfil',
    fullAddressFieldLabel: 'Morada completa',
    postalCodeFieldLabel: 'Código postal',
    documentNumberFieldLabel: 'Número do documento',
    contactPhoneFieldLabel: 'Telefone de contacto',
    emailFieldLabel: 'Email',
    householdFieldLabel: 'Utilizadores do agregado',
    instrumentSearchFieldLabel: 'Pesquisa de instrumento',
    teacherScheduleOptionsLabel: 'Horários disponíveis',
    availableSlotStatus: 'Disponível',
    takenSlotStatus: 'Ocupado',
    activeUserStatus: 'Ativo',
    inactiveUserStatus: 'Inativo',
    teachersTabLabel: 'Professores',
    teacherRegisterTitle: 'Registo de professores',
    teacherRegisterSubtitle: 'Registe professores, atualize detalhes de ensino, inative perfis e mantenha a disponibilidade semanal.',
    registerTeacherAction: 'Registar professor',
    updateTeacherAction: 'Atualizar professor',
    inactivateTeacherAction: 'Inativar professor',
    saveTeacherAction: 'Guardar professor',
    cancelAction: 'Cancelar',
    teacherNameColumn: 'Professor',
    teacherEmailColumn: 'Email',
    teacherInstrumentsColumn: 'Instrumentos',
    teacherStatusColumn: 'Estado',
    teacherSlotsColumn: 'Horários semanais',
    teacherNameFieldLabel: 'Nome do professor',
    teacherEmailFieldLabel: 'Email',
    teacherInstrumentsFieldLabel: 'Instrumentos',
    teacherStatusFieldLabel: 'Estado',
    activeStatus: 'Ativo',
    inactiveStatus: 'Inativo',
    scheduleTitle: 'Horário do professor',
    scheduleSubtitle: 'Registe a disponibilidade semanal fixa antes de atribuir aulas individuais.',
    scheduleTeacherFieldLabel: 'Professor',
    weekdayFieldLabel: 'Dia da semana',
    scheduleStartFieldLabel: 'Hora de início',
    scheduleDurationFieldLabel: 'Duração',
    scheduleRoomFieldLabel: 'Sala',
    registerScheduleAction: 'Registar horário',
    scheduleTeacherColumn: 'Professor',
    scheduleDayColumn: 'Dia',
    scheduleTimeColumn: 'Hora',
    scheduleDurationColumn: 'Duração',
    scheduleRoomColumn: 'Sala',
    scheduleStatusColumn: 'Estado',
    mondayLabel: 'Segunda-feira',
    tuesdayLabel: 'Terça-feira',
    wednesdayLabel: 'Quarta-feira',
    thursdayLabel: 'Quinta-feira',
    minutes45: '45 min',
    minutes60: '60 min',
    weeklyLessonsMetric: 'Aulas semanais',
    weeklyLessonsDetail: '+12 este mês',
    activeFamiliesMetric: 'Famílias ativas',
    activeFamiliesDetail: '118 alunos associados',
    openPaymentsMetric: 'Pagamentos em aberto',
    openPaymentsDetail: 'MBWay e transferência',
    roadmapResourcesMetric: 'Recursos do roteiro',
    roadmapResourcesDetail: 'Ficheiros PDF e MP3',
    confirmedStatus: 'Confirmada',
    pendingStatus: 'Pendente',
    primaryPayerStatus: 'Pagador principal',
    paymentPendingStatus: 'Pagamento pendente',
    sharedGuardianStatus: 'Encarregado partilhado',
    sendReceiptAction: 'Enviar recibo',
    confirmMbwayAction: 'Confirmar MBWay',
    reviewTransferAction: 'Rever transferência',
    needsConfirmationStatus: 'Precisa de confirmação',
    receiptPendingStatus: 'Recibo pendente',
    rhythmNodeTitle: 'Aprofundamento 1: Bases rítmicas',
    chordNodeTitle: 'Aprofundamento 2: Fluência de acordes',
    sightReadingNodeTitle: 'Aprofundamento 3: Leitura à primeira vista',
    resources8: '8 recursos',
    resources6: '6 recursos',
    resources11: '11 recursos'
  },
  'pt-BR': {
    appTitle: 'Escola de Música',
    loginEyebrow: 'Portal privado da escola',
    loginTitle: 'Entrar',
    loginSubtitle: 'Acesse aulas, pagamentos, progresso curricular e operações da escola.',
    loginEmailLabel: 'Email',
    loginPasswordLabel: 'Senha',
    loginRememberLabel: 'Manter conectado',
    loginAction: 'Entrar',
    loginError: 'Informe email e senha.',
    logoutAction: 'Sair',
    primaryNavigationLabel: 'Navegação principal',
    openNavigationLabel: 'Abrir navegação',
    workspaceTitle: 'Área de operações',
    roleSelectorLabel: 'Perfil',
    selectedRoleLabel: 'Perfil selecionado',
    languageSelectorLabel: 'Idioma',
    selectedLanguageLabel: 'Idioma selecionado',
    accountMenuLabel: 'Menu da conta',
    tenantContextLabel: 'Escola central de Lisboa',
    dashboardHeading: 'Operações da escola',
    dashboardIntro: 'Coordene aulas individuais, cobrança familiar, recursos dos professores e progresso dos alunos no roteiro em um único espaço de trabalho.',
    uploadResourceAction: 'Enviar recurso',
    scheduleLessonAction: 'Agendar aula',
    operationalMetricsLabel: 'Métricas operacionais',
    todayTabLabel: 'Hoje',
    schedulePanelTitle: 'Aulas individuais',
    schedulePanelSubtitle: 'Aulas individuais semanais fixas no horário local.',
    scheduleFiltersLabel: 'Filtros de agenda',
    activeLessonsChip: 'Ativas',
    trialLessonsChip: 'Experimental',
    lessonTimeColumn: 'Horário',
    lessonStudentColumn: 'Aluno',
    lessonTeacherColumn: 'Professor',
    lessonInstrumentColumn: 'Instrumento',
    lessonStatusColumn: 'Status',
    quickScheduleTitle: 'Agendamento rápido',
    studentFieldLabel: 'Aluno',
    teacherFieldLabel: 'Professor',
    instrumentFieldLabel: 'Instrumento',
    startTimeFieldLabel: 'Horário de início',
    createLessonAction: 'Criar aula fixa',
    familiesTabLabel: 'Famílias',
    familiesPanelTitle: 'Grupos familiares',
    familiesPanelSubtitle: 'Responsáveis podem gerenciar e pagar por vários alunos.',
    addFamilyAction: 'Adicionar família',
    familyGuardianColumn: 'Responsável',
    familyStudentsColumn: 'Alunos',
    familyPayerStatusColumn: 'Status do pagador',
    familyNextActionColumn: 'Próxima ação',
    curriculumTabLabel: 'Currículo',
    paymentsTabLabel: 'Pagamentos',
    paymentsPanelTitle: 'Fila de pagamentos manuais',
    paymentsPanelSubtitle: 'Acompanhe confirmações de MBWay e transferência bancária em Portugal.',
    registerPaymentAction: 'Registrar pagamento',
    paymentGuardianColumn: 'Responsável',
    paymentStudentColumn: 'Aluno',
    paymentMethodColumn: 'Método',
    paymentAmountColumn: 'Valor',
    paymentStatusColumn: 'Status',
    dashboardNavLabel: 'Painel',
    lessonsNavLabel: 'Aulas',
    familiesNavLabel: 'Famílias',
    usersNavLabel: 'Usuários',
    teacherRegisterNavLabel: 'Cadastro de professores',
    curriculumNavLabel: 'Currículo',
    paymentsNavLabel: 'Pagamentos',
    usersTabLabel: 'Usuários',
    usersPanelTitle: 'Usuários do site',
    usersPanelSubtitle: 'Crie, edite, desative, atribua famílias e associe alunos aos horários dos professores.',
    addUserAction: 'Adicionar usuário',
    editUserAction: 'Editar usuário',
    deactivateUserAction: 'Desativar usuário',
    saveUserAction: 'Salvar usuário',
    userNameColumn: 'Nome',
    userProfileColumn: 'Perfil',
    userContactColumn: 'Contato',
    userAddressColumn: 'Endereço',
    userStatusColumn: 'Status',
    nameFieldLabel: 'Nome',
    profileFieldLabel: 'Perfil',
    fullAddressFieldLabel: 'Endereço completo',
    postalCodeFieldLabel: 'CEP',
    documentNumberFieldLabel: 'Número do documento',
    contactPhoneFieldLabel: 'Telefone de contato',
    emailFieldLabel: 'Email',
    householdFieldLabel: 'Usuários da família',
    instrumentSearchFieldLabel: 'Busca por instrumento',
    teacherScheduleOptionsLabel: 'Horários disponíveis',
    availableSlotStatus: 'Disponível',
    takenSlotStatus: 'Ocupado',
    activeUserStatus: 'Ativo',
    inactiveUserStatus: 'Inativo',
    teachersTabLabel: 'Professores',
    teacherRegisterTitle: 'Cadastro de professores',
    teacherRegisterSubtitle: 'Cadastre professores, atualize detalhes de ensino, inative perfis e mantenha a disponibilidade semanal.',
    registerTeacherAction: 'Cadastrar professor',
    updateTeacherAction: 'Atualizar professor',
    inactivateTeacherAction: 'Inativar professor',
    saveTeacherAction: 'Salvar professor',
    cancelAction: 'Cancelar',
    teacherNameColumn: 'Professor',
    teacherEmailColumn: 'Email',
    teacherInstrumentsColumn: 'Instrumentos',
    teacherStatusColumn: 'Status',
    teacherSlotsColumn: 'Horários semanais',
    teacherNameFieldLabel: 'Nome do professor',
    teacherEmailFieldLabel: 'Email',
    teacherInstrumentsFieldLabel: 'Instrumentos',
    teacherStatusFieldLabel: 'Status',
    activeStatus: 'Ativo',
    inactiveStatus: 'Inativo',
    scheduleTitle: 'Agenda do professor',
    scheduleSubtitle: 'Cadastre a disponibilidade semanal fixa antes de atribuir aulas individuais.',
    scheduleTeacherFieldLabel: 'Professor',
    weekdayFieldLabel: 'Dia da semana',
    scheduleStartFieldLabel: 'Horário de início',
    scheduleDurationFieldLabel: 'Duração',
    scheduleRoomFieldLabel: 'Sala',
    registerScheduleAction: 'Cadastrar horário',
    scheduleTeacherColumn: 'Professor',
    scheduleDayColumn: 'Dia',
    scheduleTimeColumn: 'Horário',
    scheduleDurationColumn: 'Duração',
    scheduleRoomColumn: 'Sala',
    scheduleStatusColumn: 'Status',
    mondayLabel: 'Segunda-feira',
    tuesdayLabel: 'Terça-feira',
    wednesdayLabel: 'Quarta-feira',
    thursdayLabel: 'Quinta-feira',
    minutes45: '45 min',
    minutes60: '60 min',
    weeklyLessonsMetric: 'Aulas semanais',
    weeklyLessonsDetail: '+12 neste mês',
    activeFamiliesMetric: 'Famílias ativas',
    activeFamiliesDetail: '118 alunos vinculados',
    openPaymentsMetric: 'Pagamentos em aberto',
    openPaymentsDetail: 'MBWay e transferência',
    roadmapResourcesMetric: 'Recursos do roteiro',
    roadmapResourcesDetail: 'Arquivos PDF e MP3',
    confirmedStatus: 'Confirmada',
    pendingStatus: 'Pendente',
    primaryPayerStatus: 'Pagador principal',
    paymentPendingStatus: 'Pagamento pendente',
    sharedGuardianStatus: 'Responsável compartilhado',
    sendReceiptAction: 'Enviar recibo',
    confirmMbwayAction: 'Confirmar MBWay',
    reviewTransferAction: 'Revisar transferência',
    needsConfirmationStatus: 'Precisa de confirmação',
    receiptPendingStatus: 'Recibo pendente',
    rhythmNodeTitle: 'Aprofundamento 1: Fundamentos rítmicos',
    chordNodeTitle: 'Aprofundamento 2: Fluência de acordes',
    sightReadingNodeTitle: 'Aprofundamento 3: Leitura à primeira vista',
    resources8: '8 recursos',
    resources6: '6 recursos',
    resources11: '11 recursos'
  },
  'es-ES': {
    appTitle: 'Escuela de Música',
    loginEyebrow: 'Portal privado de la escuela',
    loginTitle: 'Iniciar sesión',
    loginSubtitle: 'Accede a clases, pagos, progreso curricular y operaciones de la escuela.',
    loginEmailLabel: 'Email',
    loginPasswordLabel: 'Contraseña',
    loginRememberLabel: 'Mantener sesión iniciada',
    loginAction: 'Entrar',
    loginError: 'Introduce email y contraseña.',
    logoutAction: 'Cerrar sesión',
    primaryNavigationLabel: 'Navegación principal',
    openNavigationLabel: 'Abrir navegación',
    workspaceTitle: 'Área de operaciones',
    roleSelectorLabel: 'Rol',
    selectedRoleLabel: 'Rol seleccionado',
    languageSelectorLabel: 'Idioma',
    selectedLanguageLabel: 'Idioma seleccionado',
    accountMenuLabel: 'Menú de cuenta',
    tenantContextLabel: 'Escuela central de Lisboa',
    dashboardHeading: 'Operaciones de la escuela',
    dashboardIntro: 'Coordina clases individuales, facturación familiar, recursos docentes y progreso del alumnado en la hoja de ruta desde un único espacio de trabajo.',
    uploadResourceAction: 'Subir recurso',
    scheduleLessonAction: 'Programar clase',
    operationalMetricsLabel: 'Métricas operativas',
    todayTabLabel: 'Hoy',
    schedulePanelTitle: 'Clases individuales',
    schedulePanelSubtitle: 'Clases individuales semanales fijas en horario local.',
    scheduleFiltersLabel: 'Filtros de horario',
    activeLessonsChip: 'Activas',
    trialLessonsChip: 'Prueba',
    lessonTimeColumn: 'Hora',
    lessonStudentColumn: 'Estudiante',
    lessonTeacherColumn: 'Profesor',
    lessonInstrumentColumn: 'Instrumento',
    lessonStatusColumn: 'Estado',
    quickScheduleTitle: 'Programación rápida',
    studentFieldLabel: 'Estudiante',
    teacherFieldLabel: 'Profesor',
    instrumentFieldLabel: 'Instrumento',
    startTimeFieldLabel: 'Hora de inicio',
    createLessonAction: 'Crear clase fija',
    familiesTabLabel: 'Familias',
    familiesPanelTitle: 'Grupos familiares',
    familiesPanelSubtitle: 'Los tutores pueden gestionar y pagar por varios estudiantes.',
    addFamilyAction: 'Añadir familia',
    familyGuardianColumn: 'Tutor',
    familyStudentsColumn: 'Estudiantes',
    familyPayerStatusColumn: 'Estado del pagador',
    familyNextActionColumn: 'Siguiente acción',
    curriculumTabLabel: 'Currículo',
    paymentsTabLabel: 'Pagos',
    paymentsPanelTitle: 'Cola de pagos manuales',
    paymentsPanelSubtitle: 'Seguimiento de confirmaciones de MBWay y transferencia bancaria para Portugal.',
    registerPaymentAction: 'Registrar pago',
    paymentGuardianColumn: 'Tutor',
    paymentStudentColumn: 'Estudiante',
    paymentMethodColumn: 'Método',
    paymentAmountColumn: 'Importe',
    paymentStatusColumn: 'Estado',
    dashboardNavLabel: 'Panel',
    lessonsNavLabel: 'Clases',
    familiesNavLabel: 'Familias',
    usersNavLabel: 'Usuarios',
    teacherRegisterNavLabel: 'Registro de profesores',
    curriculumNavLabel: 'Currículo',
    paymentsNavLabel: 'Pagos',
    usersTabLabel: 'Usuarios',
    usersPanelTitle: 'Usuarios del sitio',
    usersPanelSubtitle: 'Crea, edita, desactiva, asigna hogares y vincula estudiantes con horarios docentes.',
    addUserAction: 'Añadir usuario',
    editUserAction: 'Editar usuario',
    deactivateUserAction: 'Desactivar usuario',
    saveUserAction: 'Guardar usuario',
    userNameColumn: 'Nombre',
    userProfileColumn: 'Perfil',
    userContactColumn: 'Contacto',
    userAddressColumn: 'Dirección',
    userStatusColumn: 'Estado',
    nameFieldLabel: 'Nombre',
    profileFieldLabel: 'Perfil',
    fullAddressFieldLabel: 'Dirección completa',
    postalCodeFieldLabel: 'Código postal',
    documentNumberFieldLabel: 'Número de documento',
    contactPhoneFieldLabel: 'Teléfono de contacto',
    emailFieldLabel: 'Email',
    householdFieldLabel: 'Usuarios del hogar',
    instrumentSearchFieldLabel: 'Búsqueda de instrumento',
    teacherScheduleOptionsLabel: 'Horarios disponibles',
    availableSlotStatus: 'Disponible',
    takenSlotStatus: 'Ocupado',
    activeUserStatus: 'Activo',
    inactiveUserStatus: 'Inactivo',
    teachersTabLabel: 'Profesores',
    teacherRegisterTitle: 'Registro de profesores',
    teacherRegisterSubtitle: 'Registra profesores, actualiza detalles docentes, desactiva perfiles y mantiene la disponibilidad semanal.',
    registerTeacherAction: 'Registrar profesor',
    updateTeacherAction: 'Actualizar profesor',
    inactivateTeacherAction: 'Desactivar profesor',
    saveTeacherAction: 'Guardar profesor',
    cancelAction: 'Cancelar',
    teacherNameColumn: 'Profesor',
    teacherEmailColumn: 'Email',
    teacherInstrumentsColumn: 'Instrumentos',
    teacherStatusColumn: 'Estado',
    teacherSlotsColumn: 'Horarios semanales',
    teacherNameFieldLabel: 'Nombre del profesor',
    teacherEmailFieldLabel: 'Email',
    teacherInstrumentsFieldLabel: 'Instrumentos',
    teacherStatusFieldLabel: 'Estado',
    activeStatus: 'Activo',
    inactiveStatus: 'Inactivo',
    scheduleTitle: 'Horario del profesor',
    scheduleSubtitle: 'Registra la disponibilidad semanal fija antes de asignar clases individuales.',
    scheduleTeacherFieldLabel: 'Profesor',
    weekdayFieldLabel: 'Día de la semana',
    scheduleStartFieldLabel: 'Hora de inicio',
    scheduleDurationFieldLabel: 'Duración',
    scheduleRoomFieldLabel: 'Aula',
    registerScheduleAction: 'Registrar horario',
    scheduleTeacherColumn: 'Profesor',
    scheduleDayColumn: 'Día',
    scheduleTimeColumn: 'Hora',
    scheduleDurationColumn: 'Duración',
    scheduleRoomColumn: 'Aula',
    scheduleStatusColumn: 'Estado',
    mondayLabel: 'Lunes',
    tuesdayLabel: 'Martes',
    wednesdayLabel: 'Miércoles',
    thursdayLabel: 'Jueves',
    minutes45: '45 min',
    minutes60: '60 min',
    weeklyLessonsMetric: 'Clases semanales',
    weeklyLessonsDetail: '+12 este mes',
    activeFamiliesMetric: 'Familias activas',
    activeFamiliesDetail: '118 estudiantes vinculados',
    openPaymentsMetric: 'Pagos abiertos',
    openPaymentsDetail: 'MBWay y transferencia',
    roadmapResourcesMetric: 'Recursos de la hoja de ruta',
    roadmapResourcesDetail: 'Archivos PDF y MP3',
    confirmedStatus: 'Confirmada',
    pendingStatus: 'Pendiente',
    primaryPayerStatus: 'Pagador principal',
    paymentPendingStatus: 'Pago pendiente',
    sharedGuardianStatus: 'Tutor compartido',
    sendReceiptAction: 'Enviar recibo',
    confirmMbwayAction: 'Confirmar MBWay',
    reviewTransferAction: 'Revisar transferencia',
    needsConfirmationStatus: 'Necesita confirmación',
    receiptPendingStatus: 'Recibo pendiente',
    rhythmNodeTitle: 'Profundización 1: Bases rítmicas',
    chordNodeTitle: 'Profundización 2: Fluidez de acordes',
    sightReadingNodeTitle: 'Profundización 3: Lectura a primera vista',
    resources8: '8 recursos',
    resources6: '6 recursos',
    resources11: '11 recursos'
  }
} as const;

type SupportedLocale = keyof typeof text;
type TranslationKey = keyof typeof text['en-US'];
type TranslationRecord = Record<TranslationKey, string>;
type UserRole = 'Admin' | 'Teacher' | 'Guardian' | 'Student';
type WorkspaceView = 'today' | 'families' | 'users' | 'teachers' | 'curriculum' | 'payments';

interface LanguageOption {
  readonly locale: SupportedLocale;
  readonly label: string;
  readonly flag: string;
}

interface NavigationItem {
  readonly icon: string;
  readonly labelKey: TranslationKey;
  readonly view: WorkspaceView;
  readonly roles: readonly UserRole[];
}

interface Metric {
  readonly labelKey: TranslationKey;
  readonly value: string;
  readonly detailKey: TranslationKey;
  readonly tone: 'blue' | 'green' | 'amber' | 'red';
  readonly roles: readonly UserRole[];
}

interface LessonRow {
  readonly time: string;
  readonly student: string;
  readonly teacher: string;
  readonly instrument: string;
  readonly statusKey: TranslationKey;
}

interface FamilyRow {
  readonly guardian: string;
  readonly students: readonly string[];
  readonly payerStatusKey: TranslationKey;
  readonly nextActionKey: TranslationKey;
}

interface CurriculumNode {
  readonly student: string;
  readonly teacher: string;
  readonly titleKey: TranslationKey;
  readonly instrument: string;
  readonly resource: string;
  readonly progress: number;
  readonly resourcesKey: TranslationKey;
}

interface PaymentRow {
  readonly guardian: string;
  readonly student: string;
  readonly method: string;
  readonly amount: string;
  readonly statusKey: TranslationKey;
}

interface UserRow {
  readonly id: string;
  readonly name: string;
  readonly profile: UserRole;
  readonly fullAddress: string;
  readonly postalCode: string;
  readonly documentNumber: string;
  readonly contactPhone: string;
  readonly email: string;
  readonly isActive: boolean;
  readonly householdUserIds: readonly string[];
  readonly scheduleSlotId?: string;
}

interface UserDraft {
  id?: string;
  name: string;
  profile: UserRole;
  fullAddress: string;
  postalCode: string;
  documentNumber: string;
  contactPhone: string;
  email: string;
  householdUserIds: string[];
  instrumentSearch: string;
  scheduleSlotId?: string;
}

interface ScheduleSlotOption {
  readonly id: string;
  readonly teacherId: string;
  readonly instrumentId: string;
  readonly instrument: string;
  readonly teacher: string;
  readonly dayOfWeek: number;
  readonly dayKey: TranslationKey;
  readonly time: string;
  readonly durationMinutes: number;
  readonly durationKey: TranslationKey;
  readonly timeZoneId: string;
  readonly isTaken: boolean;
  readonly assignedStudent?: string;
}

interface TeacherRow {
  readonly name: string;
  readonly email: string;
  readonly instruments: string;
  readonly statusKey: TranslationKey;
  readonly weeklySlots: number;
}

interface TeacherScheduleRow {
  readonly teacher: string;
  readonly dayKey: TranslationKey;
  readonly time: string;
  readonly durationKey: TranslationKey;
  readonly room: string;
  readonly statusKey: TranslationKey;
}

interface EditableTeacher {
  name: string;
  email: string;
  instruments: string;
  statusKey: TranslationKey;
  weeklySlots: number;
}

interface EditableTeacherSchedule {
  teacher: string;
  dayKey: TranslationKey;
  time: string;
  durationKey: TranslationKey;
  room: string;
  statusKey: TranslationKey;
}

interface TeacherEditorDialogData {
  readonly mode: 'create' | 'update';
  readonly teacher: EditableTeacher;
  readonly schedules: EditableTeacherSchedule[];
  readonly canManageTeacherProfile: boolean;
  readonly t: TranslationRecord;
}

interface TeacherEditorDialogResult {
  readonly teacher: TeacherRow;
  readonly schedules: TeacherScheduleRow[];
}

@Component({
  selector: 'app-teacher-editor-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule
  ],
  template: `
    <h2 mat-dialog-title>{{ canManageTeacherProfile ? (data.mode === 'create' ? t.registerTeacherAction : t.updateTeacherAction) : t.scheduleTitle }}</h2>

    <mat-dialog-content class="teacher-dialog-content">
      @if (canManageTeacherProfile) {
        <section class="dialog-section">
          <h3>{{ t.teacherRegisterTitle }}</h3>
          <div class="dialog-form-grid">
            <mat-form-field appearance="outline">
              <mat-label>{{ t.teacherNameFieldLabel }}</mat-label>
              <input matInput [(ngModel)]="teacher.name">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>{{ t.teacherEmailFieldLabel }}</mat-label>
              <input matInput type="email" [(ngModel)]="teacher.email">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>{{ t.teacherInstrumentsFieldLabel }}</mat-label>
              <input matInput [(ngModel)]="teacher.instruments">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>{{ t.teacherStatusFieldLabel }}</mat-label>
              <mat-select [(ngModel)]="teacher.statusKey">
                <mat-option value="activeStatus">{{ t.activeStatus }}</mat-option>
                <mat-option value="inactiveStatus">{{ t.inactiveStatus }}</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
        </section>
      }

      <section class="dialog-section">
        <h3>{{ t.scheduleTitle }}</h3>
        <p>{{ t.scheduleSubtitle }}</p>
        <div class="dialog-form-grid schedule-dialog-grid">
          <mat-form-field appearance="outline">
            <mat-label>{{ t.weekdayFieldLabel }}</mat-label>
            <mat-select [(ngModel)]="scheduleDraft.dayKey">
              <mat-option value="mondayLabel">{{ t.mondayLabel }}</mat-option>
              <mat-option value="tuesdayLabel">{{ t.tuesdayLabel }}</mat-option>
              <mat-option value="wednesdayLabel">{{ t.wednesdayLabel }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>{{ t.scheduleStartFieldLabel }}</mat-label>
            <input matInput type="time" [(ngModel)]="scheduleDraft.time">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>{{ t.scheduleDurationFieldLabel }}</mat-label>
            <mat-select [(ngModel)]="scheduleDraft.durationKey">
              <mat-option value="minutes45">{{ t.minutes45 }}</mat-option>
              <mat-option value="minutes60">{{ t.minutes60 }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>{{ t.scheduleRoomFieldLabel }}</mat-label>
            <input matInput [(ngModel)]="scheduleDraft.room">
          </mat-form-field>
        </div>

        <button mat-stroked-button type="button" (click)="addSchedule()">
          <mat-icon aria-hidden="true">event_available</mat-icon>
          <span>{{ t.registerScheduleAction }}</span>
        </button>

        <table mat-table [dataSource]="scheduleRows" class="nested-table">
          <ng-container matColumnDef="day">
            <th mat-header-cell *matHeaderCellDef>{{ t.scheduleDayColumn }}</th>
            <td mat-cell *matCellDef="let schedule">{{ translate(schedule.dayKey) }}</td>
          </ng-container>
          <ng-container matColumnDef="time">
            <th mat-header-cell *matHeaderCellDef>{{ t.scheduleTimeColumn }}</th>
            <td mat-cell *matCellDef="let schedule">{{ schedule.time }}</td>
          </ng-container>
          <ng-container matColumnDef="duration">
            <th mat-header-cell *matHeaderCellDef>{{ t.scheduleDurationColumn }}</th>
            <td mat-cell *matCellDef="let schedule">{{ translate(schedule.durationKey) }}</td>
          </ng-container>
          <ng-container matColumnDef="room">
            <th mat-header-cell *matHeaderCellDef>{{ t.scheduleRoomColumn }}</th>
            <td mat-cell *matCellDef="let schedule">{{ schedule.room }}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>{{ t.scheduleStatusColumn }}</th>
            <td mat-cell *matCellDef="let schedule">{{ translate(schedule.statusKey) }}</td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="scheduleColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: scheduleColumns;"></tr>
        </table>
      </section>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="dialogRef.close()">{{ t.cancelAction }}</button>
      @if (canManageTeacherProfile) {
        <button mat-stroked-button type="button" class="danger-action" (click)="inactivateTeacher()">
          <mat-icon aria-hidden="true">person_off</mat-icon>
          <span>{{ t.inactivateTeacherAction }}</span>
        </button>
      }
      <button mat-flat-button type="button" (click)="save()">
        <mat-icon aria-hidden="true">save</mat-icon>
        <span>{{ t.saveTeacherAction }}</span>
      </button>
    </mat-dialog-actions>
  `
})
export class TeacherEditorDialogComponent {
  readonly data = inject<TeacherEditorDialogData>(MAT_DIALOG_DATA);
  readonly dialogRef = inject<MatDialogRef<TeacherEditorDialogComponent, TeacherEditorDialogResult>>(MatDialogRef);
  readonly t = this.data.t;
  readonly canManageTeacherProfile = this.data.canManageTeacherProfile;
  readonly scheduleColumns = ['day', 'time', 'duration', 'room', 'status'];
  teacher: EditableTeacher = { ...this.data.teacher };
  scheduleRows: EditableTeacherSchedule[] = this.data.schedules.map((schedule) => ({ ...schedule }));
  scheduleDraft: EditableTeacherSchedule = {
    teacher: this.teacher.name,
    dayKey: 'mondayLabel',
    time: '15:30',
    durationKey: 'minutes45',
    room: 'Studio 1',
    statusKey: 'activeStatus'
  };

  addSchedule(): void {
    this.scheduleRows = [
      ...this.scheduleRows,
      {
        ...this.scheduleDraft,
        teacher: this.teacher.name || this.t.teacherNameColumn
      }
    ];
    this.teacher.weeklySlots = this.scheduleRows.length;
  }

  inactivateTeacher(): void {
    this.teacher.statusKey = 'inactiveStatus';
  }

  save(): void {
    const teacherName = this.teacher.name || this.t.teacherNameColumn;
    const teacherEmail = this.teacher.email || `${teacherName.toLowerCase().replaceAll(' ', '.')}@musicschool.test`;
    const schedules = this.scheduleRows.map((schedule) => ({
      ...schedule,
      teacher: teacherName
    }));

    this.dialogRef.close({
      teacher: {
        ...this.teacher,
        name: teacherName,
        email: teacherEmail,
        weeklySlots: schedules.length
      },
      schedules
    });
  }

  translate(key: TranslationKey): string {
    return this.t[key];
  }
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatProgressBarModule,
    MatSelectModule,
    MatSidenavModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule
  ],
  template: `
    @if (!isAuthenticated) {
      <main class="login-page">
        <div class="login-language-row">
          <div class="language-flags" role="group" [attr.aria-label]="t.languageSelectorLabel">
            @for (language of languages; track language.locale) {
              <button
                mat-icon-button
                type="button"
                class="flag-button"
                [class.selected]="selectedLanguage === language.locale"
                [attr.aria-label]="language.label"
                [attr.aria-pressed]="selectedLanguage === language.locale"
                (click)="changeLanguage(language.locale)">
                <span aria-hidden="true">{{ language.flag }}</span>
              </button>
            }
          </div>
        </div>

        <section class="login-shell" aria-labelledby="login-title">
          <article class="login-brand-panel">
            <div class="brand-mark" aria-hidden="true">
              <mat-icon>music_note</mat-icon>
            </div>
            <p class="eyebrow">{{ t.loginEyebrow }}</p>
            <h1 id="login-title">{{ t.appTitle }}</h1>
            <p>{{ t.loginSubtitle }}</p>
            <div class="staff-lines" aria-hidden="true">
              <span></span>
              <span></span>
              <span></span>
              <span></span>
              <span></span>
            </div>
          </article>

          <form class="login-card" (ngSubmit)="login()">
            <div>
              <p class="eyebrow">{{ t.loginAction }}</p>
              <h2>{{ t.loginTitle }}</h2>
            </div>

            <mat-form-field appearance="outline">
              <mat-label>{{ t.loginEmailLabel }}</mat-label>
              <input matInput type="email" name="email" autocomplete="email" [(ngModel)]="loginEmail">
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>{{ t.loginPasswordLabel }}</mat-label>
              <input matInput [type]="showPassword ? 'text' : 'password'" name="password" autocomplete="current-password" [(ngModel)]="loginPassword">
              <button mat-icon-button matSuffix type="button" (click)="showPassword = !showPassword" [attr.aria-label]="t.loginPasswordLabel">
                <mat-icon aria-hidden="true">{{ showPassword ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
            </mat-form-field>

            <label class="remember-row">
              <input type="checkbox" name="remember" [(ngModel)]="rememberLogin">
              <span>{{ t.loginRememberLabel }}</span>
            </label>

            @if (loginErrorVisible) {
              <p class="login-error">{{ t.loginError }}</p>
            }

            <button mat-flat-button type="submit" class="wide-action">
              <mat-icon aria-hidden="true">login</mat-icon>
              <span>{{ t.loginAction }}</span>
            </button>
          </form>
        </section>
      </main>
    } @else {
    <mat-sidenav-container class="app-frame" [hasBackdrop]="isCompactNavigation()">
      <mat-sidenav
        #drawer
        class="side-nav"
        [mode]="isCompactNavigation() ? 'over' : 'side'"
        [opened]="!isCompactNavigation()"
        [fixedInViewport]="isCompactNavigation()">
        <div class="brand">
          <mat-icon aria-hidden="true">music_note</mat-icon>
          <span>{{ t.appTitle }}</span>
        </div>

        <nav class="nav-shell" [attr.aria-label]="t.primaryNavigationLabel">
          @for (item of visibleNavigationItems; track item.labelKey) {
            <button type="button" class="nav-button" [class.active]="activeNavigationKey === item.labelKey" (click)="selectNavigationItem(item); closeNavigation(drawer)">
              <span class="nav-icon"><mat-icon aria-hidden="true">{{ item.icon }}</mat-icon></span>
              <span class="nav-label">{{ translate(item.labelKey) }}</span>
            </button>
          }
        </nav>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar class="top-bar">
          <button mat-icon-button type="button" class="menu-trigger" [attr.aria-label]="t.openNavigationLabel" (click)="drawer.toggle()">
            <mat-icon aria-hidden="true">menu</mat-icon>
          </button>
          <span class="toolbar-title">{{ t.workspaceTitle }}</span>
          <span class="toolbar-spacer"></span>

          <mat-form-field appearance="outline" subscriptSizing="dynamic" class="role-field">
            <mat-label>{{ t.roleSelectorLabel }}</mat-label>
            <mat-select [ngModel]="selectedRole" (ngModelChange)="changeRole($event)" [attr.aria-label]="t.selectedRoleLabel">
              @for (role of roles; track role) {
                <mat-option [value]="role">{{ role }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <div class="language-flags" role="group" [attr.aria-label]="t.languageSelectorLabel">
            @for (language of languages; track language.locale) {
              <button
                mat-icon-button
                type="button"
                class="flag-button"
                [class.selected]="selectedLanguage === language.locale"
                [attr.aria-label]="language.label"
                [attr.aria-pressed]="selectedLanguage === language.locale"
                (click)="changeLanguage(language.locale)">
                <span aria-hidden="true">{{ language.flag }}</span>
              </button>
            }
          </div>

          <button mat-icon-button type="button" [attr.aria-label]="t.logoutAction" (click)="logout()">
            <mat-icon aria-hidden="true">logout</mat-icon>
          </button>
        </mat-toolbar>

        <main class="workspace">
          <section class="page-heading" aria-labelledby="dashboard-title">
            <div>
              <p class="eyebrow">{{ t.tenantContextLabel }}</p>
              <h1 id="dashboard-title">{{ t.dashboardHeading }}</h1>
              <p class="intro">{{ t.dashboardIntro }}</p>
            </div>

            @if (canUploadResources || canScheduleLessons) {
              <div class="heading-actions">
                @if (canUploadResources) {
                  <button mat-stroked-button type="button">
                    <mat-icon aria-hidden="true">upload_file</mat-icon>
                    <span>{{ t.uploadResourceAction }}</span>
                  </button>
                }
                @if (canScheduleLessons) {
                  <button mat-flat-button type="button">
                    <mat-icon aria-hidden="true">event_available</mat-icon>
                    <span>{{ t.scheduleLessonAction }}</span>
                  </button>
                }
              </div>
            }
          </section>

          <section class="metrics-grid" [attr.aria-label]="t.operationalMetricsLabel">
            @for (metric of visibleMetrics; track metric.labelKey) {
              <article class="metric-tile" [class]="'metric-tile tone-' + metric.tone">
                <span>{{ translate(metric.labelKey) }}</span>
                <strong>{{ metric.value }}</strong>
                <small>{{ translate(metric.detailKey) }}</small>
              </article>
            }
          </section>

          <mat-tab-group mat-stretch-tabs="false" class="content-tabs" [selectedIndex]="selectedTabIndex" (selectedIndexChange)="selectTabIndex($event)">
            @if (canSeeView('today')) {
              <mat-tab [label]="t.todayTabLabel">
              <section class="content-grid">
                <article class="panel schedule-panel">
                  <div class="panel-header">
                    <div>
                      <h2>{{ t.schedulePanelTitle }}</h2>
                      <p>{{ t.schedulePanelSubtitle }}</p>
                    </div>
                    <mat-chip-set [attr.aria-label]="t.scheduleFiltersLabel">
                      <mat-chip highlighted>{{ t.activeLessonsChip }}</mat-chip>
                      <mat-chip>{{ t.trialLessonsChip }}</mat-chip>
                    </mat-chip-set>
                  </div>

                  <table mat-table [dataSource]="visibleLessonRows">
                    <ng-container matColumnDef="time">
                      <th mat-header-cell *matHeaderCellDef>{{ t.lessonTimeColumn }}</th>
                      <td mat-cell *matCellDef="let lesson">{{ lesson.time }}</td>
                    </ng-container>
                    <ng-container matColumnDef="student">
                      <th mat-header-cell *matHeaderCellDef>{{ t.lessonStudentColumn }}</th>
                      <td mat-cell *matCellDef="let lesson">{{ lesson.student }}</td>
                    </ng-container>
                    <ng-container matColumnDef="teacher">
                      <th mat-header-cell *matHeaderCellDef>{{ t.lessonTeacherColumn }}</th>
                      <td mat-cell *matCellDef="let lesson">{{ lesson.teacher }}</td>
                    </ng-container>
                    <ng-container matColumnDef="instrument">
                      <th mat-header-cell *matHeaderCellDef>{{ t.lessonInstrumentColumn }}</th>
                      <td mat-cell *matCellDef="let lesson">{{ lesson.instrument }}</td>
                    </ng-container>
                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>{{ t.lessonStatusColumn }}</th>
                      <td mat-cell *matCellDef="let lesson"><span class="status-pill">{{ translate(lesson.statusKey) }}</span></td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="lessonColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: lessonColumns;"></tr>
                  </table>
                </article>

                @if (canScheduleLessons) {
                  <article class="panel quick-panel">
                    <h2>{{ t.quickScheduleTitle }}</h2>
                    <div class="form-grid">
                      <mat-form-field appearance="outline">
                        <mat-label>{{ t.studentFieldLabel }}</mat-label>
                        <input matInput value="Sofia Martins">
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>{{ t.teacherFieldLabel }}</mat-label>
                        <input matInput value="Maria Silva">
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>{{ t.instrumentFieldLabel }}</mat-label>
                        <mat-select value="Piano">
                          <mat-option value="Piano">Piano</mat-option>
                          <mat-option value="Guitar">Guitar</mat-option>
                          <mat-option value="Violin">Violin</mat-option>
                        </mat-select>
                      </mat-form-field>
                      <mat-form-field appearance="outline">
                        <mat-label>{{ t.startTimeFieldLabel }}</mat-label>
                        <input matInput type="time" value="16:30">
                      </mat-form-field>
                    </div>
                    <button mat-flat-button type="button" class="wide-action">
                      <mat-icon aria-hidden="true">add_task</mat-icon>
                      <span>{{ t.createLessonAction }}</span>
                    </button>
                  </article>
                }
              </section>
              </mat-tab>
            }

            @if (canSeeView('families')) {
              <mat-tab [label]="t.familiesTabLabel">
              <section class="panel full-panel">
                <div class="panel-header">
                  <div>
                    <h2>{{ t.familiesPanelTitle }}</h2>
                    <p>{{ t.familiesPanelSubtitle }}</p>
                  </div>
                  @if (canManageFamilies) {
                    <button mat-stroked-button type="button">
                      <mat-icon aria-hidden="true">group_add</mat-icon>
                      <span>{{ t.addFamilyAction }}</span>
                    </button>
                  }
                </div>

                <table mat-table [dataSource]="visibleFamilyRows">
                  <ng-container matColumnDef="guardian">
                    <th mat-header-cell *matHeaderCellDef>{{ t.familyGuardianColumn }}</th>
                    <td mat-cell *matCellDef="let family">{{ family.guardian }}</td>
                  </ng-container>
                  <ng-container matColumnDef="students">
                    <th mat-header-cell *matHeaderCellDef>{{ t.familyStudentsColumn }}</th>
                    <td mat-cell *matCellDef="let family">
                      <span class="student-chip-list">
                        @for (student of family.students; track student) {
                          <span class="student-token">{{ student }}</span>
                        }
                      </span>
                    </td>
                  </ng-container>
                  <ng-container matColumnDef="payerStatus">
                    <th mat-header-cell *matHeaderCellDef>{{ t.familyPayerStatusColumn }}</th>
                    <td mat-cell *matCellDef="let family">{{ translate(family.payerStatusKey) }}</td>
                  </ng-container>
                  <ng-container matColumnDef="nextAction">
                    <th mat-header-cell *matHeaderCellDef>{{ t.familyNextActionColumn }}</th>
                    <td mat-cell *matCellDef="let family">{{ translate(family.nextActionKey) }}</td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="familyColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: familyColumns;"></tr>
                </table>
              </section>
              </mat-tab>
            }

            @if (canSeeView('users')) {
              <mat-tab [label]="t.usersTabLabel">
              <section class="users-layout">
                <article class="panel user-editor-panel">
                  <div class="panel-header">
                    <div>
                      <h2>{{ t.usersPanelTitle }}</h2>
                      <p>{{ t.usersPanelSubtitle }}</p>
                    </div>
                    <button mat-stroked-button type="button" (click)="startNewUser()">
                      <mat-icon aria-hidden="true">person_add</mat-icon>
                      <span>{{ t.addUserAction }}</span>
                    </button>
                  </div>

                  <div class="dialog-form-grid">
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.nameFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.name">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.profileFieldLabel }}</mat-label>
                      <mat-select [(ngModel)]="userDraft.profile">
                        @for (role of roles; track role) {
                          <mat-option [value]="role">{{ role }}</mat-option>
                        }
                      </mat-select>
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.fullAddressFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.fullAddress">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.postalCodeFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.postalCode">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.documentNumberFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.documentNumber">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.contactPhoneFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.contactPhone">
                    </mat-form-field>
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.emailFieldLabel }}</mat-label>
                      <input matInput type="email" [(ngModel)]="userDraft.email">
                    </mat-form-field>
                    @if (userDraft.profile === 'Guardian') {
                      <mat-form-field appearance="outline">
                        <mat-label>{{ t.householdFieldLabel }}</mat-label>
                        <mat-select multiple [(ngModel)]="userDraft.householdUserIds">
                          @for (candidate of householdCandidateRows; track candidate.id) {
                            <mat-option [value]="candidate.id">{{ candidate.name }} - {{ candidate.profile }}</mat-option>
                          }
                        </mat-select>
                      </mat-form-field>
                    }
                  </div>

                  <section class="schedule-picker">
                    <mat-form-field appearance="outline">
                      <mat-label>{{ t.instrumentSearchFieldLabel }}</mat-label>
                      <input matInput [(ngModel)]="userDraft.instrumentSearch" (ngModelChange)="loadScheduleOptions()">
                    </mat-form-field>
                    <div>
                      <h3>{{ t.teacherScheduleOptionsLabel }}</h3>
                      <div class="slot-grid">
                        @for (slot of filteredScheduleSlotOptions; track slot.id) {
                          <button
                            type="button"
                            class="slot-option"
                            [class.selected]="userDraft.scheduleSlotId === slot.id"
                            [class.taken]="slot.isTaken"
                            [disabled]="slot.isTaken"
                            (click)="selectScheduleSlot(slot)">
                            <strong>{{ slot.instrument }} - {{ slot.teacher }}</strong>
                            <span>{{ translate(slot.dayKey) }} {{ slot.time }} - {{ translate(slot.durationKey) }}</span>
                            <small>{{ slot.isTaken ? t.takenSlotStatus + ': ' + slot.assignedStudent : t.availableSlotStatus }}</small>
                          </button>
                        }
                      </div>
                    </div>
                  </section>

                  <button mat-flat-button type="button" class="wide-action" (click)="saveUser()">
                    <mat-icon aria-hidden="true">save</mat-icon>
                    <span>{{ t.saveUserAction }}</span>
                  </button>
                </article>

                <article class="panel full-panel">
                  <table mat-table [dataSource]="userRows">
                    <ng-container matColumnDef="name">
                      <th mat-header-cell *matHeaderCellDef>{{ t.userNameColumn }}</th>
                      <td mat-cell *matCellDef="let user">
                        <button mat-button type="button" (click)="editUser(user)">{{ user.name }}</button>
                      </td>
                    </ng-container>
                    <ng-container matColumnDef="profile">
                      <th mat-header-cell *matHeaderCellDef>{{ t.userProfileColumn }}</th>
                      <td mat-cell *matCellDef="let user">{{ user.profile }}</td>
                    </ng-container>
                    <ng-container matColumnDef="contact">
                      <th mat-header-cell *matHeaderCellDef>{{ t.userContactColumn }}</th>
                      <td mat-cell *matCellDef="let user">{{ user.email }}<br>{{ user.contactPhone }}</td>
                    </ng-container>
                    <ng-container matColumnDef="address">
                      <th mat-header-cell *matHeaderCellDef>{{ t.userAddressColumn }}</th>
                      <td mat-cell *matCellDef="let user">{{ user.fullAddress }}<br>{{ user.postalCode }}</td>
                    </ng-container>
                    <ng-container matColumnDef="status">
                      <th mat-header-cell *matHeaderCellDef>{{ t.userStatusColumn }}</th>
                      <td mat-cell *matCellDef="let user">
                        <span class="status-pill" [class.warning]="!user.isActive">
                          {{ user.isActive ? t.activeUserStatus : t.inactiveUserStatus }}
                        </span>
                        <button mat-icon-button type="button" [attr.aria-label]="t.deactivateUserAction" (click)="deactivateUser(user)">
                          <mat-icon aria-hidden="true">person_off</mat-icon>
                        </button>
                      </td>
                    </ng-container>

                    <tr mat-header-row *matHeaderRowDef="userColumns"></tr>
                    <tr mat-row *matRowDef="let row; columns: userColumns;"></tr>
                  </table>
                </article>
              </section>
              </mat-tab>
            }

            @if (canSeeView('teachers')) {
              <mat-tab [label]="t.teachersTabLabel">
              <section class="teacher-list-layout">
                <article class="panel teacher-directory-panel">
                  <div class="panel-header">
                    <div>
                      <h2>{{ selectedRole === 'Teacher' ? t.scheduleTitle : t.teacherRegisterTitle }}</h2>
                      <p>{{ selectedRole === 'Teacher' ? t.scheduleSubtitle : t.teacherRegisterSubtitle }}</p>
                    </div>
                    @if (canManageTeachers) {
                      <button mat-flat-button type="button" (click)="openTeacherDialog()">
                        <mat-icon aria-hidden="true">person_add</mat-icon>
                        <span>{{ t.registerTeacherAction }}</span>
                      </button>
                    }
                  </div>

                  <div class="teacher-list">
                    @for (teacher of visibleTeacherRows; track teacher.email) {
                      <button type="button" class="teacher-list-item" (click)="openTeacherDialog(teacher)">
                        <span class="teacher-avatar" aria-hidden="true">{{ teacher.name.slice(0, 1) }}</span>
                        <span class="teacher-summary">
                          <strong>{{ teacher.name }}</strong>
                          <span>{{ teacher.email }}</span>
                        </span>
                        <span class="teacher-meta">
                          <span>{{ teacher.instruments }}</span>
                          <span>{{ teacher.weeklySlots }} {{ t.teacherSlotsColumn }}</span>
                        </span>
                        <span class="status-pill" [class.warning]="teacher.statusKey === 'inactiveStatus'">
                          {{ translate(teacher.statusKey) }}
                        </span>
                      </button>
                    }
                  </div>
                </article>
              </section>
              </mat-tab>
            }

            @if (canSeeView('curriculum')) {
              <mat-tab [label]="t.curriculumTabLabel">
              <section class="roadmap-list">
                @for (node of visibleCurriculumNodes; track node.student + node.titleKey) {
                  <article class="panel roadmap-item">
                    <div class="roadmap-copy">
                      <h2>{{ translate(node.titleKey) }}</h2>
                      <p>{{ node.student }} - {{ node.instrument }} - {{ translate(node.resourcesKey) }}</p>
                      <small>{{ node.teacher }} - {{ node.resource }}</small>
                    </div>
                    <div class="progress-block">
                      <span>{{ node.progress }}%</span>
                      <mat-progress-bar mode="determinate" [value]="node.progress"></mat-progress-bar>
                    </div>
                  </article>
                }
              </section>
              </mat-tab>
            }

            @if (canSeeView('payments')) {
              <mat-tab [label]="t.paymentsTabLabel">
              <section class="panel full-panel">
                <div class="panel-header">
                  <div>
                    <h2>{{ t.paymentsPanelTitle }}</h2>
                    <p>{{ t.paymentsPanelSubtitle }}</p>
                  </div>
                  @if (canManagePayments) {
                    <button mat-flat-button type="button">
                      <mat-icon aria-hidden="true">receipt_long</mat-icon>
                      <span>{{ t.registerPaymentAction }}</span>
                    </button>
                  }
                </div>

                <table mat-table [dataSource]="visiblePaymentRows">
                  <ng-container matColumnDef="guardian">
                    <th mat-header-cell *matHeaderCellDef>{{ t.paymentGuardianColumn }}</th>
                    <td mat-cell *matCellDef="let payment">{{ payment.guardian }}</td>
                  </ng-container>
                  <ng-container matColumnDef="student">
                    <th mat-header-cell *matHeaderCellDef>{{ t.paymentStudentColumn }}</th>
                    <td mat-cell *matCellDef="let payment">{{ payment.student }}</td>
                  </ng-container>
                  <ng-container matColumnDef="method">
                    <th mat-header-cell *matHeaderCellDef>{{ t.paymentMethodColumn }}</th>
                    <td mat-cell *matCellDef="let payment">{{ payment.method }}</td>
                  </ng-container>
                  <ng-container matColumnDef="amount">
                    <th mat-header-cell *matHeaderCellDef>{{ t.paymentAmountColumn }}</th>
                    <td mat-cell *matCellDef="let payment">{{ payment.amount }}</td>
                  </ng-container>
                  <ng-container matColumnDef="status">
                    <th mat-header-cell *matHeaderCellDef>{{ t.paymentStatusColumn }}</th>
                    <td mat-cell *matCellDef="let payment"><span class="status-pill warning">{{ translate(payment.statusKey) }}</span></td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="paymentColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: paymentColumns;"></tr>
                </table>
              </section>
              </mat-tab>
            }
          </mat-tab-group>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
    }
  `,
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly localeId = inject(LOCALE_ID);
  private readonly dialog = inject(MatDialog);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly api = inject(MusicSchoolApiService);
  readonly isCompactNavigation = toSignal(
    this.breakpointObserver.observe('(max-width: 900px)').pipe(map((state) => state.matches)),
    { initialValue: false }
  );

  readonly languages: LanguageOption[] = [
    { locale: 'en-US', label: 'English (US)', flag: '🇺🇸' },
    { locale: 'pt-PT', label: 'Português (PT)', flag: '🇵🇹' },
    { locale: 'pt-BR', label: 'Português (BR)', flag: '🇧🇷' },
    { locale: 'es-ES', label: 'Español (ES)', flag: '🇪🇸' }
  ];

  selectedLanguage = this.getInitialLocale();
  t = text[this.selectedLanguage];
  selectedTabIndex = 0;
  selectedView: WorkspaceView = 'today';
  activeNavigationKey: TranslationKey = 'dashboardNavLabel';
  private readonly tenantId = localStorage.getItem('music-school-tenant-id') ?? '11111111-1111-1111-1111-111111111111';
  isAuthenticated = localStorage.getItem('music-school-authenticated') === 'true';
  loginEmail = '';
  loginPassword = '';
  rememberLogin = true;
  showPassword = false;
  loginErrorVisible = false;

  readonly navigationItems: NavigationItem[] = [
    { icon: 'dashboard', labelKey: 'dashboardNavLabel', view: 'today', roles: ['Admin'] },
    { icon: 'event', labelKey: 'lessonsNavLabel', view: 'today', roles: ['Admin', 'Teacher', 'Guardian', 'Student'] },
    { icon: 'family_restroom', labelKey: 'familiesNavLabel', view: 'families', roles: ['Admin', 'Guardian'] },
    { icon: 'manage_accounts', labelKey: 'usersNavLabel', view: 'users', roles: ['Admin'] },
    { icon: 'co_present', labelKey: 'teacherRegisterNavLabel', view: 'teachers', roles: ['Admin'] },
    { icon: 'event_note', labelKey: 'scheduleTitle', view: 'teachers', roles: ['Teacher'] },
    { icon: 'library_music', labelKey: 'curriculumNavLabel', view: 'curriculum', roles: ['Admin', 'Teacher', 'Guardian', 'Student'] },
    { icon: 'payments', labelKey: 'paymentsNavLabel', view: 'payments', roles: ['Admin', 'Guardian'] }
  ];

  readonly roles: UserRole[] = ['Admin', 'Teacher', 'Guardian', 'Student'];
  selectedRole: UserRole = 'Admin';
  readonly currentGuardianName = 'Miguel Martins';
  readonly currentTeacherName = 'Maria Silva';
  readonly currentStudentName = 'Sofia Martins';

  readonly metrics: Metric[] = [
    { labelKey: 'weeklyLessonsMetric', value: '128', detailKey: 'weeklyLessonsDetail', tone: 'blue', roles: ['Admin', 'Teacher', 'Guardian', 'Student'] },
    { labelKey: 'activeFamiliesMetric', value: '74', detailKey: 'activeFamiliesDetail', tone: 'green', roles: ['Admin', 'Guardian'] },
    { labelKey: 'openPaymentsMetric', value: '16', detailKey: 'openPaymentsDetail', tone: 'amber', roles: ['Admin', 'Guardian'] },
    { labelKey: 'roadmapResourcesMetric', value: '342', detailKey: 'roadmapResourcesDetail', tone: 'red', roles: ['Admin', 'Teacher', 'Guardian', 'Student'] }
  ];

  readonly lessonColumns = ['time', 'student', 'teacher', 'instrument', 'status'];
  readonly lessonRows: LessonRow[] = [
    { time: '15:30', student: 'Sofia Martins', teacher: 'Maria Silva', instrument: 'Piano', statusKey: 'confirmedStatus' },
    { time: '16:30', student: 'Leo Costa', teacher: 'Rui Santos', instrument: 'Guitar', statusKey: 'confirmedStatus' },
    { time: '17:15', student: 'Ana Pereira', teacher: 'Carla Lopes', instrument: 'Violin', statusKey: 'pendingStatus' },
    { time: '18:00', student: 'Ines Martins', teacher: 'Maria Silva', instrument: 'Voice', statusKey: 'confirmedStatus' },
    { time: '19:00', student: 'Miguel Martins', teacher: 'Rui Santos', instrument: 'Guitar', statusKey: 'confirmedStatus' }
  ];

  readonly familyColumns = ['guardian', 'students', 'payerStatus', 'nextAction'];
  readonly familyRows: FamilyRow[] = [
    { guardian: 'Miguel Martins', students: ['Miguel Martins', 'Sofia Martins', 'Ines Martins'], payerStatusKey: 'primaryPayerStatus', nextActionKey: 'sendReceiptAction' },
    { guardian: 'Beatriz Costa', students: ['Leo Costa'], payerStatusKey: 'paymentPendingStatus', nextActionKey: 'confirmMbwayAction' },
    { guardian: 'Paulo Pereira', students: ['Ana Pereira', 'Tiago Pereira'], payerStatusKey: 'sharedGuardianStatus', nextActionKey: 'reviewTransferAction' }
  ];

  readonly userColumns = ['name', 'profile', 'contact', 'address', 'status'];
  userRows: UserRow[] = [
    {
      id: 'admin-1',
      name: 'Clara Admin',
      profile: 'Admin',
      fullAddress: 'Rua da Escola 12, Lisboa',
      postalCode: '1000-001',
      documentNumber: 'ID-10001',
      contactPhone: '+351 910 000 001',
      email: 'admin@musicschool.test',
      isActive: true,
      householdUserIds: []
    },
    {
      id: 'guardian-1',
      name: 'Miguel Martins',
      profile: 'Guardian',
      fullAddress: 'Avenida Central 44, Lisboa',
      postalCode: '1050-010',
      documentNumber: 'ID-20001',
      contactPhone: '+351 910 000 002',
      email: 'miguel.martins@example.test',
      isActive: true,
      householdUserIds: ['student-1', 'student-2'],
      scheduleSlotId: 'guitar-rui-thu-1800'
    },
    {
      id: 'student-1',
      name: 'Sofia Martins',
      profile: 'Student',
      fullAddress: 'Avenida Central 44, Lisboa',
      postalCode: '1050-010',
      documentNumber: 'ID-30001',
      contactPhone: '+351 910 000 003',
      email: 'sofia.martins@example.test',
      isActive: true,
      householdUserIds: [],
      scheduleSlotId: 'piano-maria-mon-1530'
    },
    {
      id: 'student-2',
      name: 'Ines Martins',
      profile: 'Student',
      fullAddress: 'Avenida Central 44, Lisboa',
      postalCode: '1050-010',
      documentNumber: 'ID-30002',
      contactPhone: '+351 910 000 004',
      email: 'ines.martins@example.test',
      isActive: true,
      householdUserIds: [],
      scheduleSlotId: 'voice-maria-mon-1800'
    }
  ];

  userDraft: UserDraft = this.createEmptyUserDraft();

  scheduleSlotOptions: ScheduleSlotOption[] = [
    { id: 'piano-maria-mon-1530', teacherId: '', instrumentId: '', instrument: 'Piano', teacher: 'Maria Silva', dayOfWeek: 1, dayKey: 'mondayLabel', time: '15:30', durationMinutes: 45, durationKey: 'minutes45', timeZoneId: 'Europe/Lisbon', isTaken: true, assignedStudent: 'Sofia Martins' },
    { id: 'piano-maria-wed-1700', teacherId: '', instrumentId: '', instrument: 'Piano', teacher: 'Maria Silva', dayOfWeek: 3, dayKey: 'wednesdayLabel', time: '17:00', durationMinutes: 60, durationKey: 'minutes60', timeZoneId: 'Europe/Lisbon', isTaken: false },
    { id: 'guitar-rui-tue-1630', teacherId: '', instrumentId: '', instrument: 'Guitar', teacher: 'Rui Santos', dayOfWeek: 2, dayKey: 'tuesdayLabel', time: '16:30', durationMinutes: 45, durationKey: 'minutes45', timeZoneId: 'Europe/Lisbon', isTaken: true, assignedStudent: 'Leo Costa' },
    { id: 'guitar-rui-thu-1800', teacherId: '', instrumentId: '', instrument: 'Guitar', teacher: 'Rui Santos', dayOfWeek: 2, dayKey: 'tuesdayLabel', time: '18:00', durationMinutes: 45, durationKey: 'minutes45', timeZoneId: 'Europe/Lisbon', isTaken: false },
    { id: 'violin-carla-wed-1700', teacherId: '', instrumentId: '', instrument: 'Violin', teacher: 'Carla Lopes', dayOfWeek: 3, dayKey: 'wednesdayLabel', time: '17:00', durationMinutes: 60, durationKey: 'minutes60', timeZoneId: 'Europe/Lisbon', isTaken: true, assignedStudent: 'Ana Pereira' }
  ];

  teacherRows: TeacherRow[] = [
    { name: 'Maria Silva', email: 'maria.silva@musicschool.test', instruments: 'Piano, Voice', statusKey: 'activeStatus', weeklySlots: 18 },
    { name: 'Rui Santos', email: 'rui.santos@musicschool.test', instruments: 'Guitar, Bass', statusKey: 'activeStatus', weeklySlots: 14 },
    { name: 'Carla Lopes', email: 'carla.lopes@musicschool.test', instruments: 'Violin', statusKey: 'inactiveStatus', weeklySlots: 0 }
  ];

  teacherScheduleRows: TeacherScheduleRow[] = [
    { teacher: 'Maria Silva', dayKey: 'mondayLabel', time: '15:30', durationKey: 'minutes45', room: 'Studio 2', statusKey: 'activeStatus' },
    { teacher: 'Maria Silva', dayKey: 'wednesdayLabel', time: '17:00', durationKey: 'minutes60', room: 'Studio 1', statusKey: 'activeStatus' },
    { teacher: 'Rui Santos', dayKey: 'tuesdayLabel', time: '16:30', durationKey: 'minutes45', room: 'Studio 3', statusKey: 'activeStatus' }
  ];

  readonly curriculumNodes: CurriculumNode[] = [
    { student: 'Sofia Martins', teacher: 'Maria Silva', titleKey: 'rhythmNodeTitle', instrument: 'Piano', resource: 'Lesson notes and fingering PDF', progress: 68, resourcesKey: 'resources8' },
    { student: 'Leo Costa', teacher: 'Rui Santos', titleKey: 'chordNodeTitle', instrument: 'Guitar', resource: 'Backing track MP3', progress: 42, resourcesKey: 'resources6' },
    { student: 'Ana Pereira', teacher: 'Carla Lopes', titleKey: 'sightReadingNodeTitle', instrument: 'Violin', resource: 'Etude scan and bowing notes', progress: 54, resourcesKey: 'resources11' },
    { student: 'Ines Martins', teacher: 'Maria Silva', titleKey: 'sightReadingNodeTitle', instrument: 'Voice', resource: 'Breathing warm-up audio', progress: 36, resourcesKey: 'resources6' },
    { student: 'Miguel Martins', teacher: 'Rui Santos', titleKey: 'chordNodeTitle', instrument: 'Guitar', resource: 'Practice loop MP3', progress: 24, resourcesKey: 'resources8' }
  ];

  readonly paymentColumns = ['guardian', 'student', 'method', 'amount', 'status'];
  readonly paymentRows: PaymentRow[] = [
    { guardian: 'Beatriz Costa', student: 'Leo Costa', method: 'MBWay', amount: 'EUR 90.00', statusKey: 'needsConfirmationStatus' },
    { guardian: 'Paulo Pereira', student: 'Ana Pereira', method: 'Bank transfer', amount: 'EUR 120.00', statusKey: 'needsConfirmationStatus' },
    { guardian: 'Miguel Martins', student: 'Sofia Martins', method: 'Bank transfer', amount: 'EUR 180.00', statusKey: 'receiptPendingStatus' }
  ];

  get visibleNavigationItems(): NavigationItem[] {
    return this.navigationItems.filter((item) => this.canUseRole(item.roles));
  }

  get visibleTabViews(): WorkspaceView[] {
    return [...new Set(this.visibleNavigationItems.map((item) => item.view))];
  }

  get visibleMetrics(): Metric[] {
    return this.metrics.filter((metric) => this.canUseRole(metric.roles));
  }

  get visibleLessonRows(): LessonRow[] {
    if (this.selectedRole === 'Guardian') {
      return this.lessonRows.filter((lesson) => this.guardianStudentNames.includes(lesson.student));
    }

    if (this.selectedRole === 'Teacher') {
      return this.lessonRows.filter((lesson) => lesson.teacher === this.currentTeacherName);
    }

    if (this.selectedRole === 'Student') {
      return this.lessonRows.filter((lesson) => lesson.student === this.currentStudentName);
    }

    return this.lessonRows;
  }

  get visibleFamilyRows(): FamilyRow[] {
    return this.selectedRole === 'Guardian'
      ? this.familyRows.filter((family) => family.guardian === this.currentGuardianName)
      : this.familyRows;
  }

  get visibleTeacherRows(): TeacherRow[] {
    return this.selectedRole === 'Teacher'
      ? this.teacherRows.filter((teacher) => teacher.name === this.currentTeacherName)
      : this.teacherRows;
  }

  get visibleCurriculumNodes(): CurriculumNode[] {
    if (this.selectedRole === 'Guardian') {
      return this.curriculumNodes.filter((node) => this.guardianStudentNames.includes(node.student));
    }

    if (this.selectedRole === 'Teacher') {
      return this.curriculumNodes.filter((node) => node.teacher === this.currentTeacherName);
    }

    if (this.selectedRole === 'Student') {
      return this.curriculumNodes.filter((node) => node.student === this.currentStudentName);
    }

    return this.curriculumNodes;
  }

  get visiblePaymentRows(): PaymentRow[] {
    return this.selectedRole === 'Guardian'
      ? this.paymentRows.filter((payment) => payment.guardian === this.currentGuardianName)
      : this.paymentRows;
  }

  get householdCandidateRows(): UserRow[] {
    return this.userRows.filter((user) => user.id !== this.userDraft.id && user.isActive);
  }

  get filteredScheduleSlotOptions(): ScheduleSlotOption[] {
    const query = this.userDraft.instrumentSearch.trim().toLowerCase();
    return query
      ? this.scheduleSlotOptions.filter((slot) => slot.instrument.toLowerCase().includes(query))
      : this.scheduleSlotOptions;
  }

  get canScheduleLessons(): boolean {
    return this.selectedRole === 'Admin';
  }

  get canUploadResources(): boolean {
    return this.selectedRole === 'Admin' || this.selectedRole === 'Teacher';
  }

  get canManageFamilies(): boolean {
    return this.selectedRole === 'Admin';
  }

  get canManagePayments(): boolean {
    return this.selectedRole === 'Admin';
  }

  get canManageTeachers(): boolean {
    return this.selectedRole === 'Admin';
  }

  get canManageUsers(): boolean {
    return this.selectedRole === 'Admin';
  }

  ngOnInit(): void {
    if (this.isAuthenticated) {
      this.loadUsers();
    }
  }

  login(): void {
    if (!this.loginEmail.trim() || !this.loginPassword.trim()) {
      this.loginErrorVisible = true;
      return;
    }

    this.isAuthenticated = true;
    this.loginErrorVisible = false;

    if (this.rememberLogin) {
      localStorage.setItem('music-school-authenticated', 'true');
    } else {
      localStorage.removeItem('music-school-authenticated');
    }

    this.loadUsers();
  }

  logout(): void {
    this.isAuthenticated = false;
    this.loginPassword = '';
    localStorage.removeItem('music-school-authenticated');
  }

  changeLanguage(locale: SupportedLocale): void {
    this.selectedLanguage = this.resolveSupportedLocale(locale);
    this.t = text[this.selectedLanguage];
    localStorage.setItem('music-school-locale', this.selectedLanguage);
    document.documentElement.lang = this.selectedLanguage;
    window.history.replaceState(null, '', this.buildLocalizedPath(this.selectedLanguage));
  }

  translate(key: TranslationKey): string {
    return this.t[key];
  }

  changeRole(role: UserRole): void {
    this.selectedRole = role;

    if (!this.canSeeView(this.selectedView)) {
      this.selectNavigationItem(this.visibleNavigationItems[0]);
      return;
    }

    this.syncSelectedTabIndex();
    this.activeNavigationKey = this.resolveActiveNavigationKey(this.selectedView);
  }

  selectNavigationItem(item: NavigationItem): void {
    this.selectedView = item.view;
    this.activeNavigationKey = item.labelKey;
    this.syncSelectedTabIndex();
  }

  closeNavigation(drawer: MatSidenav): void {
    if (this.isCompactNavigation()) {
      drawer.close();
    }
  }

  selectTabIndex(index: number): void {
    const view = this.visibleTabViews[index] ?? this.visibleTabViews[0];

    if (!view) {
      return;
    }

    this.selectedView = view;
    this.selectedTabIndex = index;
    this.activeNavigationKey = this.resolveActiveNavigationKey(view);
  }

  canSeeView(view: WorkspaceView): boolean {
    return this.visibleTabViews.includes(view);
  }

  startNewUser(): void {
    this.userDraft = this.createEmptyUserDraft();
  }

  editUser(user: UserRow): void {
    this.userDraft = {
      id: user.id,
      name: user.name,
      profile: user.profile,
      fullAddress: user.fullAddress,
      postalCode: user.postalCode,
      documentNumber: user.documentNumber,
      contactPhone: user.contactPhone,
      email: user.email,
      householdUserIds: [...user.householdUserIds],
      instrumentSearch: this.scheduleSlotOptions.find((slot) => slot.id === user.scheduleSlotId)?.instrument ?? '',
      scheduleSlotId: user.scheduleSlotId
    };
  }

  saveUser(): void {
    const request = this.toUserRegistrationRequest();
    const save$ = this.userDraft.id
      ? this.api.updateUser(this.userDraft.id, request)
      : this.api.createUser(request);

    save$.subscribe({
      next: (user) => {
        this.upsertUser(this.fromUserSummary(user));
        this.userDraft = this.createEmptyUserDraft();
        this.loadUsers();
        this.changeDetector.markForCheck();
      },
      error: () => {
        this.changeDetector.markForCheck();
      }
    });
  }

  deactivateUser(user: UserRow): void {
    this.api.deactivateUser(user.id).subscribe({
      next: (updatedUser) => {
        this.upsertUser(this.fromUserSummary(updatedUser));
        this.changeDetector.markForCheck();
      },
      error: () => {
        this.changeDetector.markForCheck();
      }
    });
  }

  selectScheduleSlot(slot: ScheduleSlotOption): void {
    if (slot.isTaken) {
      return;
    }

    this.userDraft.scheduleSlotId = slot.id;
    this.userDraft.instrumentSearch = slot.instrument;
  }

  loadScheduleOptions(): void {
    const query = this.userDraft.instrumentSearch.trim();
    if (query.length < 2) {
      return;
    }

    this.api.getTeacherScheduleOptions(query).subscribe({
      next: (options) => {
        this.scheduleSlotOptions = options.map((option) => this.fromTeacherScheduleOption(option));
        this.changeDetector.markForCheck();
      },
      error: () => {
        this.changeDetector.markForCheck();
      }
    });
  }

  openTeacherDialog(teacher?: TeacherRow): void {
    if (!this.canManageTeachers && teacher?.name !== this.currentTeacherName) {
      return;
    }

    const editableTeacher: EditableTeacher = teacher
      ? { ...teacher }
      : {
          name: '',
          email: '',
          instruments: '',
          statusKey: 'activeStatus',
          weeklySlots: 0
        };

    const schedules = teacher
      ? this.teacherScheduleRows
          .filter((schedule) => schedule.teacher === teacher.name)
          .map((schedule) => ({ ...schedule }))
      : [];

    const dialogRef = this.dialog.open<TeacherEditorDialogComponent, TeacherEditorDialogData, TeacherEditorDialogResult>(
      TeacherEditorDialogComponent,
      {
        width: '920px',
        maxWidth: 'calc(100vw - 32px)',
        maxHeight: 'calc(100dvh - 32px)',
        data: {
          mode: teacher ? 'update' : 'create',
          teacher: editableTeacher,
          schedules,
          canManageTeacherProfile: this.canManageTeachers,
          t: this.t
        }
      });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      const previousName = teacher?.name;
      const previousEmail = teacher?.email;
      const otherTeachers = this.teacherRows.filter((item) => item.email !== previousEmail);
      this.teacherRows = [...otherTeachers, result.teacher].sort((left, right) => left.name.localeCompare(right.name));

      if (previousName) {
        this.teacherScheduleRows = this.teacherScheduleRows.filter((schedule) => schedule.teacher !== previousName);
      }

      this.teacherScheduleRows = [
        ...this.teacherScheduleRows,
        ...result.schedules
      ];
      this.changeDetector.markForCheck();
    });
  }

  private getInitialLocale(): SupportedLocale {
    const storedLocale = localStorage.getItem('music-school-locale');
    const pathLocale = window.location.pathname.split('/').filter(Boolean)[0];
    return this.resolveSupportedLocale(storedLocale ?? pathLocale ?? this.localeId);
  }

  private resolveSupportedLocale(locale: string): SupportedLocale {
    return this.languages.some((language) => language.locale === locale) ? locale as SupportedLocale : 'en-US';
  }

  private canUseRole(roles: readonly UserRole[]): boolean {
    return roles.includes(this.selectedRole);
  }

  private loadUsers(): void {
    this.api.getUsers(1, 100).subscribe({
      next: (page) => {
        this.userRows = page.items.map((user) => this.fromUserSummary(user));
        this.changeDetector.markForCheck();
      },
      error: () => {
        this.changeDetector.markForCheck();
      }
    });
  }

  private upsertUser(user: UserRow): void {
    this.userRows = [
      ...this.userRows.filter((item) => item.id !== user.id),
      user
    ].sort((left, right) => left.name.localeCompare(right.name));
  }

  private toUserRegistrationRequest(): UserRegistrationRequest {
    const selectedSlot = this.scheduleSlotOptions.find((slot) => slot.id === this.userDraft.scheduleSlotId);
    const scheduleSelection = selectedSlot && selectedSlot.teacherId && selectedSlot.instrumentId
      ? {
          teacherId: selectedSlot.teacherId,
          instrumentId: selectedSlot.instrumentId,
          dayOfWeek: selectedSlot.dayOfWeek,
          startTime: selectedSlot.time,
          durationMinutes: selectedSlot.durationMinutes,
          timeZoneId: selectedSlot.timeZoneId
        }
      : undefined;

    return {
      tenantId: this.tenantId,
      name: this.userDraft.name.trim(),
      profile: this.userDraft.profile,
      fullAddress: this.userDraft.fullAddress.trim(),
      postalCode: this.userDraft.postalCode.trim(),
      documentNumber: this.userDraft.documentNumber.trim(),
      contactPhone: this.userDraft.contactPhone.trim(),
      email: this.userDraft.email.trim().toLowerCase(),
      householdUserIds: this.userDraft.profile === 'Guardian' ? this.userDraft.householdUserIds : [],
      scheduleSelection
    };
  }

  private fromUserSummary(user: UserSummary): UserRow {
    return {
      id: user.id,
      name: user.name,
      profile: user.profile,
      fullAddress: user.fullAddress,
      postalCode: user.postalCode,
      documentNumber: user.documentNumber,
      contactPhone: user.contactPhone,
      email: user.email,
      isActive: user.isActive,
      householdUserIds: []
    };
  }

  private fromTeacherScheduleOption(option: TeacherScheduleOption): ScheduleSlotOption {
    return {
      id: [
        option.instrumentId,
        option.teacherId,
        option.dayOfWeek,
        option.startTime
      ].join(':'),
      teacherId: option.teacherId,
      instrumentId: option.instrumentId,
      instrument: option.instrumentName,
      teacher: option.teacherName,
      dayOfWeek: option.dayOfWeek,
      dayKey: this.dayKey(option.dayOfWeek),
      time: option.startTime,
      durationMinutes: option.durationMinutes,
      durationKey: option.durationMinutes === 60 ? 'minutes60' : 'minutes45',
      timeZoneId: option.timeZoneId,
      isTaken: option.isTaken,
      assignedStudent: option.assignedStudentName
    };
  }

  private dayKey(dayOfWeek: number): TranslationKey {
    switch (dayOfWeek) {
      case 1:
        return 'mondayLabel';
      case 2:
        return 'tuesdayLabel';
      case 3:
        return 'wednesdayLabel';
      case 4:
        return 'thursdayLabel';
      default:
        return 'mondayLabel';
    }
  }

  private createEmptyUserDraft(): UserDraft {
    return {
      name: '',
      profile: 'Student',
      fullAddress: '',
      postalCode: '',
      documentNumber: '',
      contactPhone: '',
      email: '',
      householdUserIds: [],
      instrumentSearch: ''
    };
  }

  private get guardianStudentNames(): readonly string[] {
    return this.familyRows.find((family) => family.guardian === this.currentGuardianName)?.students ?? [];
  }

  private syncSelectedTabIndex(): void {
    this.selectedTabIndex = Math.max(0, this.visibleTabViews.indexOf(this.selectedView));
  }

  private resolveActiveNavigationKey(view: WorkspaceView): TranslationKey {
    return this.visibleNavigationItems.find((item) => item.view === view)?.labelKey ?? this.visibleNavigationItems[0]?.labelKey ?? 'dashboardNavLabel';
  }

  private buildLocalizedPath(locale: SupportedLocale): string {
    const supportedLocales = new Set<string>(this.languages.map((language) => language.locale));
    const pathParts = window.location.pathname.split('/').filter(Boolean);

    if (pathParts.length > 0 && supportedLocales.has(pathParts[0])) {
      pathParts[0] = locale;
    } else {
      pathParts.unshift(locale);
    }

    return `/${pathParts.join('/')}${window.location.search}${window.location.hash}`;
  }
}
