// ── Enums ──────────────────────────────────────────────────────
export enum UserRole {
  Admin = 'Admin',
  Agent = 'Agent',
  Customer = 'Customer'
}

export enum TicketStatus {
  Open = 'Open',
  InProgress = 'InProgress',
  Resolved = 'Resolved'
}

export enum TicketPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export enum TicketCategory {
  AuthenticationIssue = 'AuthenticationIssue',
  BillingIssue = 'BillingIssue',
  TechnicalIssue = 'TechnicalIssue',
  PerformanceIssue = 'PerformanceIssue',
  FeatureRequest = 'FeatureRequest',
  GeneralInquiry = 'GeneralInquiry',
  AccountManagement = 'AccountManagement',
  NotificationIssue = 'NotificationIssue',
  DataIssue = 'DataIssue',
  SecurityIssue = 'SecurityIssue',
  UncategorizedIssue = 'UncategorizedIssue'
}

// ── Auth Models ────────────────────────────────────────────────
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: number; // 1 = Agent, 2 = Customer
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
}

export interface CurrentUser {
  userId: string;
  fullName: string;
  email: string;
  role: UserRole;
  token: string;
}

// ── User Models ────────────────────────────────────────────────
export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface UpdateRoleRequest {
  userId: string;
  newRole: number;
}

// ── Ticket Models ──────────────────────────────────────────────
export interface CreateTicketRequest {
  title: string;
  description: string;
}

export interface UpdateTicketStatusRequest {
  status: number; // 0=Open, 1=InProgress, 2=Resolved
}

export interface AssignTicketRequest {
  agentId: string;
}

export interface TicketDto {
  id: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category: string;
  createdByName: string;
  assignedToName?: string;
  createdAt: string;
  updatedAt: string;
}

// ── Helper Maps ────────────────────────────────────────────────
export const STATUS_LABELS: Record<string, string> = {
  Open: 'Open',
  InProgress: 'In Progress',
  Resolved: 'Resolved'
};

export const CATEGORY_LABELS: Record<string, string> = {
  AuthenticationIssue: 'Authentication Issue',
  BillingIssue: 'Billing Issue',
  TechnicalIssue: 'Technical Issue',
  PerformanceIssue: 'Performance Issue',
  FeatureRequest: 'Feature Request',
  GeneralInquiry: 'General Inquiry',
  AccountManagement: 'Account Management',
  NotificationIssue: 'Notification Issue',
  DataIssue: 'Data Issue',
  SecurityIssue: 'Security Issue',
  UncategorizedIssue: 'Uncategorized Issue'
};