// src/app/core/models/project.models.ts
export interface Project {
  id: string;
  title: string;
  description: string;
  creatorName: string;
  createdAt: string;
  memberCount: number;
  taskCount: number;
}

export interface CreateProjectDto {
  title: string;
  description: string;
}

export interface InviteMemberDto {
  email: string;
}

// src/app/core/models/task.models.ts
export type TaskPriority = 'Low' | 'Medium' | 'High';
export type TaskStatus   = 'Todo' | 'InProgress' | 'Done';

export interface Task {
  id: string;
  title: string;
  description: string;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate: string | null;
  isOverdue: boolean;
  projectId: string;
  projectTitle: string;
  assigneeId: string | null;
  assigneeName: string | null;
  assigneeInitials: string | null;
  position: number;
  commentCount: number;
  createdAt: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  priority: TaskPriority;
  dueDate: string | null;
  assigneeId: string | null;
}

export interface UpdateTaskDto extends CreateTaskDto {
  status: TaskStatus;
}

export interface MoveTaskDto {
  newStatus: TaskStatus;
  newPosition: number;
}

export interface Comment {
  id: string;
  content: string;
  authorName: string;
  authorInitials: string;
  createdAt: string;
}

export interface TaskFilters {
  status?: TaskStatus;
  priority?: TaskPriority;
  assigneeId?: string;
}

// src/app/core/models/dashboard.models.ts
export interface Dashboard {
  totalTasks: number;
  completedTasks: number;
  inProgressTasks: number;
  overdueTasks: number;
  completionPercent: number;
  projects: ProjectSummary[];
  myTasks: Task[];
  tasksByPriority: PriorityCount[];
}

export interface ProjectSummary {
  id: string;
  title: string;
  total: number;
  completed: number;
}

export interface PriorityCount {
  priority: string;
  count: number;
}

// Generic API response
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
