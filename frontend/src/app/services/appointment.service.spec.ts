import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AppointmentService } from './appointment.service';

describe('AppointmentService', () => {
  let service: AppointmentService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(AppointmentService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  it('loads branches from the API', () => {
    service.getBranches().subscribe(branches => expect(branches.length).toBe(1));
    const request = http.expectOne('/api/appointments/branches');
    expect(request.request.method).toBe('GET');
    request.flush([{ id: 1, code: 'JHB-CBD', name: 'Johannesburg CBD Branch', address: 'Johannesburg CBD, Gauteng' }]);
  });
});
