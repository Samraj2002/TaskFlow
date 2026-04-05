import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';
import { Chart, ArcElement, BarElement, CategoryScale, LinearScale, Tooltip, Legend, DoughnutController, BarController } from 'chart.js';
Chart.register(ArcElement, BarElement, CategoryScale, LinearScale, Tooltip, Legend, DoughnutController, BarController);
import { DashboardService } from '../../core/services/api.services';
import { AuthService } from '../../core/services/auth.service';
import { Dashboard } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatIconModule,
            MatButtonModule, MatProgressBarModule, MatChipsModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  loading   = signal(true);
  dashboard = signal<Dashboard | null>(null);

  priorityChartData   = signal<ChartData<'doughnut'> | null>(null);
  projectChartData    = signal<ChartData<'bar'>      | null>(null);

  chartOptions: ChartOptions = {
    responsive: true,
    plugins: { legend: { position: 'bottom' } }
  };

  barOptions: ChartOptions<'bar'> = {
    responsive: true,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } }
  };

  constructor(
    public  auth: AuthService,
    private dashboardSvc: DashboardService
  ) {}

  getFirstName(): string {
    const name = this.auth.currentUser()?.name;
    if (!name) return 'there';
    return name.split(' ')[0] ?? 'there';
  }

  ngOnInit(): void {
    this.dashboardSvc.get().subscribe({
      next: res => {
        if (res.success) {
          this.dashboard.set(res.data);
          this.buildCharts(res.data);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private buildCharts(data: Dashboard): void {
    // Doughnut — tasks by priority
    const colorMap: Record<string, string> = {
      High: '#ef4444', Medium: '#f59e0b', Low: '#10b981'
    };
    this.priorityChartData.set({
      labels:   data.tasksByPriority.map(p => p.priority),
      datasets: [{
        data:            data.tasksByPriority.map(p => p.count),
        backgroundColor: data.tasksByPriority.map(p => colorMap[p.priority] ?? '#6b7280')
      }]
    });

    // Bar — project progress
    this.projectChartData.set({
      labels:   data.projects.map(p => p.title),
      datasets: [
        { label: 'Total',     data: data.projects.map(p => p.total),     backgroundColor: '#e0e7ff' },
        { label: 'Completed', data: data.projects.map(p => p.completed), backgroundColor: '#6366f1' }
      ]
    });
  }

  getPriorityColor(p: string): string {
    return { High: 'warn', Medium: 'accent', Low: 'primary' }[p] ?? 'primary';
  }
}
