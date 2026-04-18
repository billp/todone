import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  isDark = signal(localStorage.getItem('theme') === 'dark');

  constructor() {
    this.apply(this.isDark());
  }

  toggle(): void {
    const el = document.documentElement;
    el.classList.add('theme-transition');
    el.getBoundingClientRect(); // force reflow so transition rules are active before theme changes
    const next = !this.isDark();
    this.isDark.set(next);
    localStorage.setItem('theme', next ? 'dark' : 'light');
    this.apply(next);
    setTimeout(() => el.classList.remove('theme-transition'), 400);
  }

  private apply(dark: boolean): void {
    document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
  }
}
