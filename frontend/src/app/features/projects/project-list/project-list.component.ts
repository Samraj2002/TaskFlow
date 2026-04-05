import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProjectService } from '../../../core/services/api.services';
import { AuthService } from '../../../core/services/auth.service';
import { Project } from '../../../core/models/models';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule,
    MatCardModule, MatButtonModule, MatIconModule, MatDialogModule,
    MatInputModule, MatSnackBarModule, MatMenuModule, MatTooltipModule],
  templateUrl: './project-list.component.html',
  styleUrls:  ['./project-list.component.scss']
})
export class ProjectListComponent implements OnInit {
  projects    = signal<Project[]>([]);
  loading     = signal(true);
  showForm    = signal(false);
  editTarget  = signal<Project | null>(null);
  submitting  = signal(false);

  form = this.fb.group({
    title:       ['', [Validators.required, Validators.maxLength(200)]],
    description: ['']
  });

  constructor(
    private fb:         FormBuilder,
    private projectSvc: ProjectService,
    public  auth:       AuthService,
    private snack:      MatSnackBar
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.projectSvc.getAll().subscribe({
      next: res => { if (res.success) this.projects.set(res.data); this.loading.set(false); },
      error: ()  => this.loading.set(false)
    });
  }

  openCreate(): void {
    this.editTarget.set(null);
    this.form.reset();
    this.showForm.set(true);
  }

  openEdit(p: Project): void {
    this.editTarget.set(p);
    this.form.patchValue({ title: p.title, description: p.description });
    this.showForm.set(true);
  }

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    const dto = this.form.value as any;
    const call = this.editTarget()
      ? this.projectSvc.update(this.editTarget()!.id, dto)
      : this.projectSvc.create(dto);

    call.subscribe({
      next: res => {
        if (res.success) {
          this.snack.open(res.message, '', { duration: 3000 });
          this.showForm.set(false);
          this.load();
        }
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false)
    });
  }

  delete(p: Project): void {
    if (!confirm(`Delete "${p.title}"? This cannot be undone.`)) return;
    this.projectSvc.delete(p.id).subscribe(res => {
      if (res.success) {
        this.snack.open('Project deleted', '', { duration: 3000 });
        this.projects.update(list => list.filter(x => x.id !== p.id));
      }
    });
  }

  cancel(): void { this.showForm.set(false); this.form.reset(); }

  timeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const days = Math.floor(diff / 86400000);
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 30)  return `${days}d ago`;
    return new Date(dateStr).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }
}
