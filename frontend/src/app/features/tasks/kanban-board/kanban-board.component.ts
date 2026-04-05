import { Component, OnInit, Input, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog } from '@angular/material/dialog';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TaskService } from '../../../core/services/api.services';
import { Task, TaskStatus, MoveTaskDto } from '../../../core/models/models';

interface KanbanColumn {
  id: TaskStatus;
  title: string;
  icon: string;
  color: string;
  tasks: Task[];
}

@Component({
  selector: 'app-kanban-board',
  standalone: true,
  imports: [CommonModule, DragDropModule, MatButtonModule, MatIconModule,
            MatChipsModule, MatMenuModule, MatBadgeModule, MatTooltipModule],
  templateUrl: './kanban-board.component.html',
  styleUrls: ['./kanban-board.component.scss']
})
export class KanbanBoardComponent implements OnInit {
  @Input() projectId!: string;

  loading = signal(true);
  columns = signal<KanbanColumn[]>([
    { id: 'Todo',       title: 'To Do',      icon: 'radio_button_unchecked', color: '#6b7280', tasks: [] },
    { id: 'InProgress', title: 'In Progress', icon: 'pending',               color: '#f59e0b', tasks: [] },
    { id: 'Done',       title: 'Done',        icon: 'check_circle',          color: '#10b981', tasks: [] }
  ]);

  columnIds = computed(() => this.columns().map(c => c.id));

  constructor(private taskService: TaskService, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading.set(true);
    this.taskService.getAll(this.projectId).subscribe({
      next: res => {
        if (res.success) {
          this.columns.update(cols => cols.map(col => ({
            ...col,
            tasks: res.data
              .filter(t => t.status === col.id)
              .sort((a, b) => a.position - b.position)
          })));
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onTaskDrop(event: CdkDragDrop<Task[]>, targetColumnId: TaskStatus): void {
    const task = event.item.data as Task;

    if (event.previousContainer === event.container) {
      // Same column — reorder
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Different column — move status
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }

    // Persist to API
    const dto: MoveTaskDto = {
      newStatus:   targetColumnId,
      newPosition: event.currentIndex
    };

    this.taskService.move(this.projectId, task.id, dto).subscribe();
  }

  getPriorityColor(priority: string): string {
    return { Low: '#10b981', Medium: '#f59e0b', High: '#ef4444' }[priority] ?? '#6b7280';
  }

  getPriorityIcon(priority: string): string {
    return { Low: 'arrow_downward', Medium: 'remove', High: 'arrow_upward' }[priority] ?? 'remove';
  }

  formatDueDate(date: string | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  openCreateTask(columnId: TaskStatus): void {
    // Opens the task dialog — wired up separately
  }

  deleteTask(task: Task, column: KanbanColumn): void {
    this.taskService.delete(this.projectId, task.id).subscribe(res => {
      if (res.success) {
        this.columns.update(cols =>
          cols.map(c => c.id === column.id
            ? { ...c, tasks: c.tasks.filter(t => t.id !== task.id) }
            : c)
        );
      }
    });
  }
}
