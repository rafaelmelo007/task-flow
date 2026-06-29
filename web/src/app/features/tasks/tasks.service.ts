import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTaskRequest, PagedResult, TaskDto, TaskStatus, UpdateTaskRequest } from '../../shared/models/task.model';

export interface TaskQueryParams {
  status?: TaskStatus;
  search?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tasks`;

  getAll(params: TaskQueryParams): Observable<PagedResult<TaskDto>> {
    let p = new HttpParams();
    if (params.status !== undefined && params.status !== null) p = p.set('status', params.status);
    if (params.search) p = p.set('search', params.search);
    if (params.page) p = p.set('page', params.page);
    if (params.pageSize) p = p.set('pageSize', params.pageSize);
    return this.http.get<PagedResult<TaskDto>>(this.base, { params: p });
  }

  getById(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.base}/${id}`);
  }

  create(req: CreateTaskRequest): Observable<TaskDto> {
    return this.http.post<TaskDto>(this.base, req);
  }

  update(id: string, req: UpdateTaskRequest): Observable<TaskDto> {
    return this.http.put<TaskDto>(`${this.base}/${id}`, req);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
