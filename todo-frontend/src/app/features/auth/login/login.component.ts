import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  error = signal('');
  loading = signal(false);

  submit(): void {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { username, password } = this.form.getRawValue();

    this.auth.login(username!, password!).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => this.router.navigate(['/todos']),
      error: (err) => {
        this.error.set(err.error || 'Invalid credentials.');
        this.loading.set(false);
      }
    });
  }
}
