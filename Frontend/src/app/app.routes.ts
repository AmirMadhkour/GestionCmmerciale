import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    title: 'Connexion · Gestion Commerciale',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'orders' },
      {
        path: 'orders',
        loadChildren: () => import('./features/orders/orders.routes').then((m) => m.ORDERS_ROUTES)
      },
      {
        path: 'clients',
        loadChildren: () => import('./features/clients/clients.routes').then((m) => m.CLIENTS_ROUTES)
      },
      {
        path: 'products',
        loadChildren: () => import('./features/products/products.routes').then((m) => m.PRODUCTS_ROUTES)
      },
      {
        path: 'forbidden',
        title: 'Accès refusé · Gestion Commerciale',
        loadComponent: () => import('./shared/components/forbidden/forbidden.component').then((m) => m.ForbiddenComponent)
      },
      {
        path: '**',
        title: 'Page introuvable · Gestion Commerciale',
        loadComponent: () => import('./shared/components/not-found/not-found.component').then((m) => m.NotFoundComponent)
      }
    ]
  }
];
