import { Component, signal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  loading = signal(false);
  error   = signal('');

  form = this.fb.group({
    name:     ['', [Validators.required, Validators.minLength(2)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {}

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');

    this.auth.register(this.form.value as any).subscribe({
      next: res => {
        if (res.success) this.router.navigate(['/dashboard']);
        else this.error.set(res.message);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err.error?.message ?? 'Registration failed.');
        this.loading.set(false);
      }
    });
  }
}
