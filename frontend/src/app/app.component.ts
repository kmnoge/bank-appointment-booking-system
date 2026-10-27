import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AppointmentService, BookingConfirmation, Branch } from './services/appointment.service';
import { BranchAdminComponent } from './components/branch-admin.component';

@Component({ selector: 'app-root', standalone: true, imports: [CommonModule, ReactiveFormsModule, BranchAdminComponent], template: `
  <main class="page-shell"><header class="topbar"><div class="brand-mark" aria-hidden="true">C</div><div><span class="eyebrow">CAPITEC BANKING SERVICES</span><h1>Branch appointments</h1></div><button type="button" class="admin-toggle" (click)="showAdmin = !showAdmin">{{ showAdmin ? 'Hide branch admin' : 'Branch admin' }}</button><span class="secure-label">Secure booking</span></header>
  <section class="hero"><div><p class="eyebrow light">YOUR TIME MATTERS</p><h2>Bank on better: Client stories that<br>inspire us.</h2><p>Choose your branch, tell us what you need, and reserve a convenient time with our team.</p></div><div class="hero-art" aria-hidden="true"><div class="art-ring ring-one"></div><div class="art-ring ring-two"></div><div class="art-card">✓</div></div></section>

  <section class="content-grid"><div class="form-panel"><div class="section-heading"><span class="step">01</span><div><h2>Book an appointment</h2><p>Fields marked * are required.</p></div></div>
  <div *ngIf="errorMessage" class="alert error" role="alert">{{ errorMessage }}</div>
  
  <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
    <div class="field-grid"><label>Full name *<input formControlName="fullName" autocomplete="name" placeholder="e.g. Alex Mokoena"><small *ngIf="invalid('fullName')">Enter your full name (at least 2 characters).</small></label>
    <label>Email address *<input type="email" formControlName="email" autocomplete="email" placeholder="you@example.com"><small *ngIf="invalid('email')">Enter a valid email address.</small></label>
    <label>Phone number *<input type="tel" formControlName="phoneNumber" autocomplete="tel" placeholder="e.g. 082 123 4567"><small *ngIf="invalid('phoneNumber')">Enter a valid phone number.</small></label>
    <label>Branch *<select formControlName="bankBranchId"><option [ngValue]="null" disabled>Select a branch</option><option *ngFor="let branch of branches" [ngValue]="branch.id">{{ branch.name }} — {{ branch.address }}</option></select><small *ngIf="invalid('bankBranchId')">Select a branch.</small></label>
    <label>Service required *<select formControlName="serviceType"><option value="" disabled>Select a service</option><option>Account services</option><option>Card services</option><option>Loan consultation</option><option>Home loan consultation</option><option>Business banking</option><option>General enquiry</option></select><small *ngIf="invalid('serviceType')">Select a service.</small></label>
    <label>Appointment date *<input type="date" formControlName="appointmentDate" [min]="minDate"><small *ngIf="invalid('appointmentDate')">Choose a date within the next 90 days.</small></label>
    <label class="full-width">Available time *<select formControlName="appointmentTime"><option value="" disabled>Select a 30-minute slot</option><option *ngFor="let slot of slots" [value]="slot">{{ slot }}</option></select><small>Monday–Friday, 09:00–15:30 (South African time).</small></label></div>
    <p class="privacy-note">Your details will be used to manage this appointment. Please do not enter your PIN, password, or full card details.</p>
    <button class="submit-button" type="submit" [disabled]="submitting || loadingBranches">{{ submitting ? 'Booking appointment…' : 'Confirm appointment' }} <span aria-hidden="true">→</span></button>
  </form></div>
  <aside class="side-panel"><div class="side-icon">◷</div><h3>Before you book</h3><ul><li>Appointments last 30 minutes.</li><li>Bookings must be made at least 30 minutes ahead.</li><li>Bring your ID and any relevant documents.</li><li>You’ll receive a confirmation reference after booking.</li></ul><div class="help-card"><strong>Need assistance?</strong><p>Contact your selected branch if you need to change your appointment.</p></div></aside></section>
  <section *ngIf="confirmation" class="confirmation" role="status" aria-live="polite"><div class="success-icon">✓</div><div><p class="eyebrow">BOOKING CONFIRMED</p><h2>{{ confirmation.message }}</h2><p><strong>Reference:</strong> {{ confirmation.reference }}</p><p>{{ confirmation.branch }} · {{ confirmation.serviceType }}</p><p>{{ confirmation.appointmentAt | date:'EEEE, d MMMM y, HH:mm' }}</p><button type="button" class="secondary-button" (click)="reset()">Book another appointment</button></div></section>
  <app-branch-admin *ngIf="showAdmin"></app-branch-admin>
  <footer>© {{ currentYear }} Bank Branch Appointments <span>Available Monday–Friday · 09:00–16:00</span></footer></main>` })
export class AppComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AppointmentService);
  showAdmin = false;
  branches: Branch[] = []; loadingBranches = true; submitting = false; errorMessage = ''; confirmation: BookingConfirmation | null = null;
  readonly currentYear = new Date().getFullYear();
  readonly minDate = this.localDateString(new Date());
  readonly slots = Array.from({ length: 14 }, (_, i) => { const minutes = 9 * 60 + i * 30; return `${String(Math.floor(minutes / 60)).padStart(2, '0')}:${String(minutes % 60).padStart(2, '0')}`; }).filter(time => time !== '16:00');
  readonly form = this.fb.group({ fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(120)]], email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]], phoneNumber: ['', [Validators.required, Validators.pattern(/^[+()\d\s-]{7,30}$/)]], bankBranchId: [null as number | null, Validators.required], serviceType: ['', Validators.required], appointmentDate: ['', Validators.required], appointmentTime: ['', Validators.required] });
  ngOnInit(): void { this.service.getBranches().subscribe({ next: branches => { this.branches = branches; this.loadingBranches = false; }, error: () => { this.loadingBranches = false; this.errorMessage = 'We could not load branches. Please refresh the page or try again later.'; } }); }
  invalid(name: string): boolean { const control = this.form.get(name); return !!control && control.invalid && (control.touched || control.dirty); }
  submit(): void {
    this.errorMessage = ''; this.form.markAllAsTouched(); if (this.form.invalid || this.submitting) return;
    const value = this.form.getRawValue(); if (!value.bankBranchId || !value.appointmentDate || !value.appointmentTime) return;
    const local = new Date(`${value.appointmentDate}T${value.appointmentTime}:00`); const now = new Date();
    if (local.getTime() < now.getTime() + 30 * 60 * 1000 || local.getTime() > now.getTime() + 90 * 24 * 60 * 60 * 1000) { this.errorMessage = 'Choose a time at least 30 minutes from now and within the next 90 days.'; return; }
    if ([0, 6].includes(local.getDay())) { this.errorMessage = 'Please choose a weekday.'; return; }
    this.submitting = true;
    this.service.book({ fullName: value.fullName!.trim(), email: value.email!.trim(), phoneNumber: value.phoneNumber!.trim(), bankBranchId: value.bankBranchId, serviceType: value.serviceType!, appointmentAt: `${value.appointmentDate}T${value.appointmentTime}:00`, idempotencyKey: this.createIdempotencyKey() }).subscribe({ next: result => { this.confirmation = result; this.submitting = false; window.scrollTo({ top: 0, behavior: 'smooth' }); }, error: (error: HttpErrorResponse) => { this.submitting = false; this.errorMessage = error.status === 409 ? (error.error?.detail ?? 'That time is no longer available. Choose another slot.') : error.error?.detail ?? 'We could not complete your booking. Please try again.'; } });
  }
  reset(): void { this.confirmation = null; this.form.reset(); this.errorMessage = ''; }
  private createIdempotencyKey(): string { return typeof crypto !== 'undefined' && 'randomUUID' in crypto ? crypto.randomUUID() : `${Date.now()}-${Math.random().toString(36).slice(2)}`; }
  private localDateString(date: Date): string { const year = date.getFullYear(); const month = String(date.getMonth() + 1).padStart(2, '0'); const day = String(date.getDate()).padStart(2, '0'); return `${year}-${month}-${day}`; }
}
