import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';

// Shared
import { NavbarComponent } from './shared/components/navbar/navbar.component';

// Auth
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';

// Customer
import { CustomerDashboardComponent } from './features/customer/dashboard/customer-dashboard.component';
import { CreateTicketComponent } from './features/customer/create-ticket/create-ticket.component';

// Agent
import { AgentDashboardComponent } from './features/agent/dashboard/agent-dashboard.component';

// Admin
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    LoginComponent,
    RegisterComponent,
    CustomerDashboardComponent,
    CreateTicketComponent,
    AgentDashboardComponent,
    AdminDashboardComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule 
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}