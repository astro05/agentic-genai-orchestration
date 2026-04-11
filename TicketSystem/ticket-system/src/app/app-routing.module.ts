import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserRole } from './core/models/models';
import { AuthGuard, RoleGuard } from './core/guards/guards';

import { LoginComponent }           from './features/auth/login/login.component';
import { RegisterComponent }        from './features/auth/register/register.component';
import { CustomerDashboardComponent } from './features/customer/dashboard/customer-dashboard.component';
import { CreateTicketComponent }    from './features/customer/create-ticket/create-ticket.component';
import { AgentDashboardComponent }  from './features/agent/dashboard/agent-dashboard.component';
import { AdminDashboardComponent }  from './features/admin/dashboard/admin-dashboard.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login',    component: LoginComponent    },
  { path: 'register', component: RegisterComponent },

  // Customer routes
  {
    path: 'customer',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.Customer] },
    children: [
      { path: '',              component: CustomerDashboardComponent },
      { path: 'create-ticket', component: CreateTicketComponent     }
    ]
  },

  // Agent routes
  {
    path: 'agent',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.Agent] },
    children: [
      { path: '', component: AgentDashboardComponent }
    ]
  },

  // Admin routes
  {
    path: 'admin',
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: [UserRole.Admin] },
    children: [
      { path: '', component: AdminDashboardComponent }
    ]
  },

  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}