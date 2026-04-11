import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AssignTicketRequest, RegisterRequest,
  TicketDto, UpdateRoleRequest, UserDto
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  // Users
  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.apiUrl}/users`);
  }

  getAgents(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.apiUrl}/agents`);
  }

  createUser(req: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/users`, req);
  }

  updateRole(req: UpdateRoleRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/role`, req);
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${id}`);
  }

  // Tickets
  getAllTickets(): Observable<TicketDto[]> {
    return this.http.get<TicketDto[]>(`${this.apiUrl}/tickets`);
  }

  assignTicket(ticketId: string, req: AssignTicketRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/tickets/${ticketId}/assign`, req);
  }
}