import { Routes } from '@angular/router';
import { Role } from '../../core/constants/roles';
import { roleGuard } from '../../core/guards/role.guard';

export const CLIENTS_ROUTES: Routes = [
  {
    path: '',
    title: 'Clients · Gestion Commerciale',
    loadComponent: () => import('./client-list/client-list.component').then((m) => m.ClientListComponent)
  },
  {
    path: 'new',
    title: 'Nouveau client · Gestion Commerciale',
    canActivate: [roleGuard],
    data: { roles: [Role.Admin] },
    loadComponent: () => import('./client-form/client-form.component').then((m) => m.ClientFormComponent)
  },
  {
    path: ':id',
    title: 'Fiche client · Gestion Commerciale',
    loadComponent: () => import('./client-detail/client-detail.component').then((m) => m.ClientDetailComponent)
  },
  {
    path: ':id/edit',
    title: 'Modifier le client · Gestion Commerciale',
    canActivate: [roleGuard],
    data: { roles: [Role.Admin] },
    loadComponent: () => import('./client-form/client-form.component').then((m) => m.ClientFormComponent)
  }
];
