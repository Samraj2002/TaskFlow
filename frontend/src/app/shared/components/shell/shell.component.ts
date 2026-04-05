import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, RouterOutlet,
            MatSidenavModule, MatToolbarModule, MatListModule,
            MatIconModule, MatButtonModule, MatMenuModule, MatTooltipModule],
  template: `
    <mat-sidenav-container class="shell">
      <!-- Sidenav -->
      <mat-sidenav mode="side" [opened]="sidenavOpen()" class="sidenav">
        <div class="sidenav-header">
          <mat-icon class="brand-icon">task_alt</mat-icon>
          <span class="brand-name">TaskFlow</span>
        </div>

        <mat-nav-list class="nav-list">
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>
          <a mat-list-item routerLink="/projects" routerLinkActive="active-link">
            <mat-icon matListItemIcon>folder</mat-icon>
            <span matListItemTitle>Projects</span>
          </a>
        </mat-nav-list>

        <!-- User info at bottom -->
        <div class="sidenav-footer">
          <div class="user-avatar">{{ auth.currentUser()?.initials }}</div>
          <div class="user-info">
            <span class="user-name">{{ auth.currentUser()?.name }}</span>
            <span class="user-role">{{ auth.currentUser()?.role }}</span>
          </div>
          <button mat-icon-button [matMenuTriggerFor]="userMenu">
            <mat-icon>more_vert</mat-icon>
          </button>
          <mat-menu #userMenu>
            <button mat-menu-item (click)="auth.logout()">
              <mat-icon>logout</mat-icon> Sign out
            </button>
          </mat-menu>
        </div>
      </mat-sidenav>

      <!-- Main content -->
      <mat-sidenav-content class="main-content">
        <mat-toolbar class="toolbar">
          <button mat-icon-button (click)="toggleSidenav()">
            <mat-icon>menu</mat-icon>
          </button>
          <span class="toolbar-spacer"></span>
          <button mat-icon-button matTooltip="Notifications">
            <mat-icon>notifications_none</mat-icon>
          </button>
        </mat-toolbar>

        <div class="content-area">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .shell { height: 100vh; }

    .sidenav {
      width: 240px;
      background: #1e1b4b;
      color: #e0e7ff;
      display: flex;
      flex-direction: column;
    }

    .sidenav-header {
      display: flex; align-items: center; gap: 10px;
      padding: 20px 16px 12px;
      font-size: 18px; font-weight: 700; color: #fff;
      border-bottom: 1px solid rgba(255,255,255,0.1);
      .brand-icon { color: #818cf8; font-size: 26px; }
    }

    .nav-list {
      flex: 1; padding-top: 8px;
      a { color: rgba(255,255,255,0.7); border-radius: 8px; margin: 2px 8px;
          &:hover { background: rgba(255,255,255,0.08); color: #fff; }
          &.active-link { background: rgba(99,102,241,0.4); color: #fff; } }
    }

    .sidenav-footer {
      display: flex; align-items: center; gap: 10px;
      padding: 16px;
      border-top: 1px solid rgba(255,255,255,0.1);
    }

    .user-avatar {
      width: 32px; height: 32px; border-radius: 50%;
      background: linear-gradient(135deg, #818cf8, #6366f1);
      color: #fff; display: flex; align-items: center; justify-content: center;
      font-size: 12px; font-weight: 700; flex-shrink: 0;
    }

    .user-info { flex: 1; min-width: 0;
      .user-name { display: block; font-size: 13px; font-weight: 600;
                   color: #fff; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      .user-role { display: block; font-size: 11px; color: rgba(255,255,255,0.5); }
    }

    .toolbar { background: #fff; border-bottom: 1px solid #e2e8f0; box-shadow: none; }
    .toolbar-spacer { flex: 1; }

    .main-content { display: flex; flex-direction: column; height: 100%; }
    .content-area { flex: 1; overflow-y: auto; }
  `]
})
export class ShellComponent {
  sidenavOpen = signal(true);
  constructor(public auth: AuthService) {}
  toggleSidenav() { this.sidenavOpen.update(v => !v); }
}
