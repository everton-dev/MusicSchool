import { Routes } from '@angular/router';
import { TeachersComponent } from './teachers.component';
import { TeacherScheduleComponent } from './teacher-schedule.component';

export const TEACHERS_ROUTES: Routes = [
  {
    path: '',
    component: TeachersComponent
  },
  {
    path: 'schedule/:id',
    component: TeacherScheduleComponent
  }
];
