import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ActionMessage, OrderDetail, OrderPayload, OrderSummary } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  getAll(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${this.baseUrl}/${id}`);
  }

  getByClient(clientId: string): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(`${this.baseUrl}/client/${clientId}`);
  }

  create(payload: OrderPayload): Observable<OrderDetail> {
    return this.http.post<OrderDetail>(this.baseUrl, payload);
  }

  update(id: string, payload: OrderPayload): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  validate(id: string): Observable<ActionMessage> {
    return this.http.post<ActionMessage>(`${this.baseUrl}/${id}/validate`, {});
  }

  cancel(id: string): Observable<ActionMessage> {
    return this.http.post<ActionMessage>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
