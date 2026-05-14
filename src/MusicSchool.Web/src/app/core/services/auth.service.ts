import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

const authStorageKey = 'music-school-authenticated';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly loggedInSubject = new BehaviorSubject<boolean>(this.getInitialState());
  readonly loggedIn$ = this.loggedInSubject.asObservable();

  isLoggedIn(): boolean {
    return this.loggedInSubject.value;
  }

  login(email: string, password: string): boolean {
    const hasCredentials = email.trim().length > 0 && password.trim().length > 0;

    if (!hasCredentials) {
      return false;
    }

    this.saveSession();
    this.loggedInSubject.next(true);
    return true;
  }

  logout(): void {
    try {
      localStorage.removeItem(authStorageKey);
    } catch {
      // The session is still cleared in memory.
    }

    this.loggedInSubject.next(false);
  }

  private getInitialState(): boolean {
    try {
      return localStorage.getItem(authStorageKey) === 'true';
    } catch {
      return false;
    }
  }

  private saveSession(): void {
    try {
      localStorage.setItem(authStorageKey, 'true');
    } catch {
      // The current tab can still use the in-memory session.
    }
  }
}
