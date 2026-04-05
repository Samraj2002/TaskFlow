import { Component, OnInit, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProjectService } from '../../../core/services/api.services';
import { KanbanBoardComponent } from '../../tasks/kanban-board/kanban-board.component';
import { Project } from '../../../core/models/models';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule,
    MatTabsModule, MatCardModule, MatButtonModule, MatIconModule,
    MatInputModule, MatSnackBarModule, MatChipsModule, MatTooltipModule,
    KanbanBoardComponent],
  templateUrl: './project-detail.component.html',
  styleUrls:  ['./project-detail.component.scss']
})
export class ProjectDetailComponent implements OnInit {
  @Input() id!: string;   // from withComponentInputBinding()

  project   = signal<Project | null>(null);
  loading   = signal(true);
  inviting  = signal(false);
  showInvite = signal(false);

  inviteForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  constructor(
    private fb:         FormBuilder,
    private projectSvc: ProjectService,
    private snack:      MatSnackBar
  ) {}

  ngOnInit(): void {
    this.projectSvc.getById(this.id).subscribe({
      next: res => { if (res.success) this.project.set(res.data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  inviteMember(): void {
    if (this.inviteForm.invalid) return;
    this.inviting.set(true);
    this.projectSvc.inviteMember(this.id, { email: this.inviteForm.value.email! }).subscribe({
      next: res => {
        this.snack.open(res.message, '', { duration: 3000 });
        this.inviteForm.reset();
        this.showInvite.set(false);
        this.inviting.set(false);
      },
      error: err => {
        this.snack.open(err.error?.message ?? 'Failed to invite', 'Close', { duration: 4000 });
        this.inviting.set(false);
      }
    });
  }
}
