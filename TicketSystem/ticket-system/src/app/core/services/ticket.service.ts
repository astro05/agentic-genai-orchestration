import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AddTicketMessageRequest,
  CreateTicketRequest,
  ReplyAssistResponseDto,
  TicketDto,
  TicketMessageDto,
  UpdateTicketNotesRequest,
  UpdateTicketStatusRequest
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly apiUrl = `${environment.apiUrl}/ticket`;

  constructor(private http: HttpClient) {}

  // Customer
  createTicket(req: CreateTicketRequest): Observable<TicketDto> {
    return this.http.post<TicketDto>(this.apiUrl, req);
  }

  getMyTickets(): Observable<TicketDto[]> {
    return this.http.get<TicketDto[]>(`${this.apiUrl}/my`);
  }

  // Agent
  getAssignedTickets(): Observable<TicketDto[]> {
    return this.http.get<TicketDto[]>(`${this.apiUrl}/assigned`);
  }

  updateStatus(id: string, req: UpdateTicketStatusRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/status`, req);
  }

  updateAgentNotes(id: string, req: UpdateTicketNotesRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/notes`, req);
  }

  addTicketMessage(id: string, req: AddTicketMessageRequest): Observable<TicketMessageDto> {
    return this.http.post<TicketMessageDto>(`${this.apiUrl}/${id}/messages`, req);
  }

  /** AI + knowledge base suggested reply (agent, assigned ticket only). */
  getReplyAssist(ticketId: string): Observable<ReplyAssistResponseDto> {
    return this.http.get<ReplyAssistResponseDto>(`${this.apiUrl}/${ticketId}/reply-assist`);
  }
}