import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AdminAppointment, AppointmentService, Branch } from '../services/appointment.service';

@Component({
  selector: 'app-branch-admin', standalone: true, imports: [CommonModule, FormsModule],
  template: `
  <section class="admin-panel" aria-labelledby="admin-title">
    <div class="admin-heading"><div><p class="eyebrow">STAFF WORKSPACE</p><h2 id="admin-title">Branch appointment management</h2><p class="subtle">View, search, filter and cancel appointments.</p></div><span class="admin-badge">Branch admin</span></div>
    <div *ngIf="!authenticated" class="login-box">
      <label>Branch administrator access key<input type="password" [(ngModel)]="keyInput" autocomplete="current-password" placeholder="Enter your admin access key" (keyup.enter)="connect()"></label>
      <button class="admin-button" type="button" (click)="connect()" [disabled]="busy || !keyInput.trim()">{{ busy ? 'Connecting…' : 'Access dashboard' }}</button>
      <p class="admin-error" *ngIf="errorMessage" role="alert">{{ errorMessage }}</p>
      <p class="subtle">Use the administrator key configured by your system operator. This starter uses a shared key; replace it with bank-approved staff authentication before production.</p>
    </div>
    <ng-container *ngIf="authenticated">
      <div class="admin-toolbar"><div class="admin-filters">
        <label>Branch<select [(ngModel)]="branchId"><option [ngValue]="null">All branches</option><option *ngFor="let b of branches" [ngValue]="b.id">{{ b.name }}</option></select></label>
        <label>Appointment date<input type="date" [(ngModel)]="dateFilter"></label>
        <label>Status<select [(ngModel)]="statusFilter"><option value="">All statuses</option><option value="Confirmed">Confirmed</option><option value="Cancelled">Cancelled</option></select></label>
        <label class="search-filter">Search<input [(ngModel)]="searchFilter" placeholder="Name, email, phone or reference" (keyup.enter)="load()"></label>
      </div><div class="admin-actions"><button class="admin-button" type="button" (click)="load()" [disabled]="busy">{{ busy ? 'Loading…' : 'Apply filters' }}</button><button class="admin-button secondary" type="button" (click)="clearFilters()">Clear</button><button class="admin-button secondary" type="button" (click)="logout()">Sign out</button></div></div>
      <p class="admin-error" *ngIf="errorMessage" role="alert">{{ errorMessage }}</p><p class="admin-success" *ngIf="successMessage" role="status">{{ successMessage }}</p>
      <div class="admin-summary"><strong>{{ appointments.length }}</strong> appointment(s) returned <span>Maximum 500 records per query</span></div>
      <div class="table-wrap"><table><thead><tr><th>Appointment</th><th>Customer</th><th>Branch & service</th><th>Status</th><th>Action</th></tr></thead><tbody>
        <tr *ngFor="let a of appointments"><td><strong>{{ a.appointmentAt | date:'EEE, d MMM y' }}</strong><small>{{ a.appointmentAt | date:'HH:mm' }} · {{ a.reference }}</small></td><td><strong>{{ a.fullName }}</strong><small>{{ a.email }}</small><small>{{ a.phoneNumber }}</small></td><td><strong>{{ a.branch }}</strong><small>{{ a.serviceType }}</small></td><td><span class="status" [class.cancelled]="a.status === 'Cancelled'">{{ a.status }}</span></td><td><button *ngIf="a.status === 'Confirmed'" class="cancel-button" type="button" (click)="cancel(a)" [disabled]="busy">Cancel</button><span *ngIf="a.status === 'Cancelled'" class="muted">No action</span></td></tr>
        <tr *ngIf="!appointments.length && !busy"><td colspan="5" class="empty-state">No appointments match these filters.</td></tr>
      </tbody></table></div>
    </ng-container>
  </section>`,
  styles: [`.admin-panel{margin:28px 0;background:#fff;border:1px solid #dce6ef;border-radius:17px;padding:28px;box-shadow:0 8px 28px #17395808}.admin-heading{display:flex;justify-content:space-between;gap:15px;align-items:center;margin-bottom:24px}.admin-heading h2{font:700 22px Manrope;margin:0 0 6px}.subtle,.muted{color:#718397;font-size:12px}.subtle{line-height:1.6}.admin-badge{background:#eaf3fa;color:#16517a;border-radius:30px;padding:8px 12px;font-size:11px;font-weight:700;white-space:nowrap}.login-box{max-width:520px;background:#f6f9fc;padding:20px;border-radius:12px}.login-box label,.admin-filters label{display:flex;flex-direction:column;gap:8px;font-size:12px;font-weight:700;color:#344c65}.login-box input{margin-bottom:12px}.admin-button,.cancel-button{border:0;border-radius:8px;background:#104c77;color:white;padding:11px 15px;font-size:12px;font-weight:700;cursor:pointer}.admin-button:disabled,.cancel-button:disabled{opacity:.55;cursor:wait}.admin-button.secondary{background:#edf3f8;color:#244c6c}.admin-toolbar{display:flex;justify-content:space-between;gap:15px;align-items:end;flex-wrap:wrap}.admin-filters{display:grid;grid-template-columns:repeat(3,minmax(130px,1fr));gap:12px;flex:1}.search-filter{grid-column:1/-1}.admin-actions{display:flex;gap:8px;flex-wrap:wrap}.admin-summary{display:flex;align-items:center;gap:7px;color:#526b81;font-size:12px;margin:22px 0 12px}.admin-summary strong{font-size:18px;color:#123d61}.admin-summary span{margin-left:auto;color:#8493a2;font-size:10px}.table-wrap{width:100%;overflow:auto;border:1px solid #e5ebf1;border-radius:10px}table{border-collapse:collapse;width:100%;min-width:720px;text-align:left}th{background:#f5f8fb;color:#63788d;text-transform:uppercase;letter-spacing:.7px;font-size:10px;padding:13px}td{padding:15px 13px;border-top:1px solid #edf1f5;font-size:12px;color:#334d65}td strong,td small{display:block}td small{margin-top:5px;color:#718397;font-size:11px}.status{display:inline-block;background:#e0f6e9;color:#177448;padding:6px 9px;border-radius:30px;font-size:10px;font-weight:700}.status.cancelled{background:#fff0ef;color:#a13c37}.cancel-button{background:#fff0ef;color:#a13c37;padding:8px 11px}.empty-state{text-align:center;padding:35px;color:#7b8c9e}.admin-error,.admin-success{padding:10px 12px;border-radius:8px;font-size:12px}.admin-error{color:#9b2929;background:#fff0f0}.admin-success{color:#176c48;background:#e8f8ef}@media(max-width:750px){.admin-panel{padding:18px}.admin-toolbar{align-items:stretch}.admin-filters{grid-template-columns:1fr 1fr}.admin-actions{width:100%}.admin-heading h2{font-size:18px}}@media(max-width:450px){.admin-filters{grid-template-columns:1fr}.search-filter{grid-column:auto}.admin-summary{flex-wrap:wrap}.admin-summary span{width:100%;margin-left:0}}`]
})
export class BranchAdminComponent implements OnInit {
  private readonly service = inject(AppointmentService);
  keyInput = ''; apiKey = ''; authenticated = false; busy = false;
  branches: Branch[] = []; appointments: AdminAppointment[] = []; branchId: number | null = null;
  dateFilter = ''; statusFilter = ''; searchFilter = ''; errorMessage = ''; successMessage = '';
  ngOnInit(): void { this.service.getBranches().subscribe({ next: x => this.branches = x, error: () => this.errorMessage = 'Could not load the branch list.' }); }
  connect(): void { const key = this.keyInput.trim(); if (!key || this.busy) return; this.apiKey = key; this.errorMessage = ''; this.load(true); }
  load(connecting = false): void {
    if (!this.apiKey || this.busy) return; this.busy = true; this.errorMessage = ''; this.successMessage = '';
    this.service.getAdminAppointments(this.apiKey, { branchId: this.branchId, date: this.dateFilter, status: this.statusFilter, search: this.searchFilter }).subscribe({
      next: rows => { this.appointments = rows; this.authenticated = true; this.busy = false; this.keyInput = ''; },
      error: (e: HttpErrorResponse) => { this.busy = false; this.authenticated = false; this.apiKey = ''; this.errorMessage = e.status === 401 ? 'Access denied. Check the administrator key or ask your system operator.' : 'Unable to load appointments. Please try again.'; }
    });
  }
  cancel(a: AdminAppointment): void {
    if (this.busy || !window.confirm(`Cancel appointment ${a.reference} for ${a.fullName}?`)) return;
    this.busy = true; this.errorMessage = ''; this.successMessage = '';
    this.service.cancelAdminAppointment(this.apiKey, a.id).subscribe({ next: () => { this.busy = false; this.successMessage = `Appointment ${a.reference} has been cancelled.`; this.load(); }, error: () => { this.busy = false; this.errorMessage = 'Could not cancel this appointment. Refresh and try again.'; } });
  }
  clearFilters(): void { this.branchId = null; this.dateFilter = ''; this.statusFilter = ''; this.searchFilter = ''; this.load(); }
  logout(): void { this.apiKey = ''; this.keyInput = ''; this.authenticated = false; this.appointments = []; this.errorMessage = ''; this.successMessage = ''; }
}
