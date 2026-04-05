import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, Project, CreateProjectDto,
  Task, CreateTaskDto, UpdateTaskDto, MoveTaskDto,
  Comment, TaskFilters, Dashboard, InviteMemberDto
} from '../models/models';

// ── ProjectService ────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class ProjectService {
  private API = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Project[]>> {
    return this.http.get<ApiResponse<Project[]>>(this.API);
  }

  getById(id: string): Observable<ApiResponse<Project>> {
    return this.http.get<ApiResponse<Project>>(`${this.API}/${id}`);
  }

  create(dto: CreateProjectDto): Observable<ApiResponse<Project>> {
    return this.http.post<ApiResponse<Project>>(this.API, dto);
  }

  update(id: string, dto: CreateProjectDto): Observable<ApiResponse<Project>> {
    return this.http.put<ApiResponse<Project>>(`${this.API}/${id}`, dto);
  }

  delete(id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.API}/${id}`);
  }

  inviteMember(projectId: string, dto: InviteMemberDto): Observable<ApiResponse<null>> {
    return this.http.post<ApiResponse<null>>(`${this.API}/${projectId}/members`, dto);
  }

  removeMember(projectId: string, memberId: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.API}/${projectId}/members/${memberId}`);
  }
}

// ── TaskService ───────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class TaskService {
  private apiUrl = (projectId: string) =>
    `${environment.apiUrl}/projects/${projectId}/tasks`;

  constructor(private http: HttpClient) {}

  getAll(projectId: string, filters: TaskFilters = {}): Observable<ApiResponse<Task[]>> {
    let params = new HttpParams();
    if (filters.status)     params = params.set('status',     filters.status);
    if (filters.priority)   params = params.set('priority',   filters.priority);
    if (filters.assigneeId) params = params.set('assigneeId', filters.assigneeId);
    return this.http.get<ApiResponse<Task[]>>(this.apiUrl(projectId), { params });
  }

  getById(projectId: string, id: string): Observable<ApiResponse<Task>> {
    return this.http.get<ApiResponse<Task>>(`${this.apiUrl(projectId)}/${id}`);
  }

  create(projectId: string, dto: CreateTaskDto): Observable<ApiResponse<Task>> {
    return this.http.post<ApiResponse<Task>>(this.apiUrl(projectId), dto);
  }

  update(projectId: string, id: string, dto: UpdateTaskDto): Observable<ApiResponse<Task>> {
    return this.http.put<ApiResponse<Task>>(`${this.apiUrl(projectId)}/${id}`, dto);
  }

  move(projectId: string, id: string, dto: MoveTaskDto): Observable<ApiResponse<Task>> {
    return this.http.patch<ApiResponse<Task>>(`${this.apiUrl(projectId)}/${id}/move`, dto);
  }

  delete(projectId: string, id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.apiUrl(projectId)}/${id}`);
  }

  getComments(projectId: string, taskId: string): Observable<ApiResponse<Comment[]>> {
    return this.http.get<ApiResponse<Comment[]>>(`${this.apiUrl(projectId)}/${taskId}/comments`);
  }

  addComment(projectId: string, taskId: string, content: string): Observable<ApiResponse<Comment>> {
    return this.http.post<ApiResponse<Comment>>(
      `${this.apiUrl(projectId)}/${taskId}/comments`, { content });
  }
}

// ── DashboardService ──────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  get(): Observable<ApiResponse<Dashboard>> {
    return this.http.get<ApiResponse<Dashboard>>(`${environment.apiUrl}/dashboard`);
  }
}
