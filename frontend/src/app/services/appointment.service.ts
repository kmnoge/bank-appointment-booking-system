import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Branch { id: number; code: string; name: string; address: string; }
export interface BookingRequest { fullName: string; email: string; phoneNumber: string; bankBranchId: number; serviceType: string; appointmentAt: string; idempotencyKey: string; }
export interface BookingConfirmation { id: string; reference: string; message: string; fullName: string; branch: string; serviceType: string; appointmentAt: string; status: string; }
export interface AdminAppointment { id: string; reference: string; fullName: string; email: string; phoneNumber: string; bankBranchId: number; branch: string; serviceType: string; appointmentAt: string; status: string; createdAt: string; }
@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly http = inject(HttpClient);
  getBranches(): Observable<Branch[]> { return this.http.get<Branch[]>(`${environment.apiBaseUrl}/appointments/branches`); }
  book(request: BookingRequest): Observable<BookingConfirmation> { return this.http.post<BookingConfirmation>(`${environment.apiBaseUrl}/appointments`, request); }
  getAdminAppointments(apiKey: string, filters: { branchId?: number | null; date?: string; status?: string; search?: string }): Observable<AdminAppointment[]> {
    const params: Record<string, string | number> = {};
    if (filters.branchId) params['branchId'] = filters.branchId;
    if (filters.date) params['date'] = filters.date;
    if (filters.status) params['status'] = filters.status;
    if (filters.search?.trim()) params['search'] = filters.search.trim();
    return this.http.get<AdminAppointment[]>(`${environment.apiBaseUrl}/admin/appointments`, { headers: { 'X-Branch-Admin-Key': apiKey }, params });
  }
  cancelAdminAppointment(apiKey: string, id: string): Observable<void> {
    return this.http.post<void>(`${environment.apiBaseUrl}/admin/appointments/${id}/cancel`, {}, { headers: { 'X-Branch-Admin-Key': apiKey } });
  }
}
