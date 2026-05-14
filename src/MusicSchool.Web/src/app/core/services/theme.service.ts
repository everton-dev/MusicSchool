import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

type ThemeMode = 'light' | 'dark';

const themeStorageKey = 'music-school-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  readonly mode = signal<ThemeMode>(this.getInitialMode());

  constructor() {
    this.applyTheme(this.mode());
  }

  toggleTheme(): void {
    const nextMode: ThemeMode = this.mode() === 'dark' ? 'light' : 'dark';
    this.mode.set(nextMode);
    this.saveMode(nextMode);
    this.applyTheme(nextMode);
  }

  private getInitialMode(): ThemeMode {
    try {
      return localStorage.getItem(themeStorageKey) === 'dark' ? 'dark' : 'light';
    } catch {
      return 'light';
    }
  }

  private applyTheme(mode: ThemeMode): void {
    const isDark = mode === 'dark';
    this.document.documentElement.classList.toggle('dark-theme', isDark);
    this.document.body.classList.toggle('dark-theme', isDark);
    this.document.documentElement.style.colorScheme = isDark ? 'dark' : 'light';
  }

  private saveMode(mode: ThemeMode): void {
    try {
      localStorage.setItem(themeStorageKey, mode);
    } catch {
      // Storage can be blocked by browser settings.
    }
  }
}
