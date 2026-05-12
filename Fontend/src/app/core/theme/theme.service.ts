import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const STORAGE_KEY = 'vlp-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly doc = inject(DOCUMENT);

  /** Resolved theme applied to the document */
  readonly mode = signal<ThemeMode>('light');

  constructor() {
    this.apply(this.readInitial());
  }

  toggle(): void {
    this.apply(this.mode() === 'dark' ? 'light' : 'dark');
  }

  set(mode: ThemeMode): void {
    this.apply(mode);
  }

  private readInitial(): ThemeMode {
    if (typeof localStorage === 'undefined') {
      return 'light';
    }
    const stored = localStorage.getItem(STORAGE_KEY) as ThemeMode | null;
    if (stored === 'dark' || stored === 'light') {
      return stored;
    }
    if (typeof matchMedia !== 'undefined' && matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return 'light';
  }

  private apply(mode: ThemeMode): void {
    this.mode.set(mode);
    this.doc.documentElement.dataset['theme'] = mode;
    try {
      localStorage.setItem(STORAGE_KEY, mode);
    } catch {
      /* ignore quota / private mode */
    }
  }
}
