import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

  // Auth (lazy-loaded)
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },

  // Main app shell
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/components/shell/shell.component').then(m => m.ShellComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'projects',
        loadComponent: () => import('./features/projects/project-list/project-list.component').then(m => m.ProjectListComponent)
      },
      {
        path: 'projects/:id',
        loadComponent: () => import('./features/projects/project-detail/project-detail.component').then(m => m.ProjectDetailComponent)
      },
      {
        path: 'projects/:projectId/board',
        loadComponent: () => import('./features/tasks/kanban-board/kanban-board.component').then(m => m.KanbanBoardComponent)
      }
    ]
  },

  { path: '**', redirectTo: 'dashboard' }
];
