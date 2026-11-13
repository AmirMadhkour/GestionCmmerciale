import { Routes } from '@angular/router';

export const ORDERS_ROUTES: Routes = [
  {
    path: '',
    title: 'Commandes · Gestion Commerciale',
    loadComponent: () => import('./order-list/order-list.component').then((m) => m.OrderListComponent)
  },
  {
    path: 'new',
    title: 'Nouvelle commande · Gestion Commerciale',
    loadComponent: () => import('./order-form/order-form.component').then((m) => m.OrderFormComponent)
  },
  {
    path: ':id',
    title: 'Détail de la commande · Gestion Commerciale',
    loadComponent: () => import('./order-detail/order-detail.component').then((m) => m.OrderDetailComponent)
  },
  {
    path: ':id/edit',
    title: 'Modifier la commande · Gestion Commerciale',
    loadComponent: () => import('./order-form/order-form.component').then((m) => m.OrderFormComponent)
  }
];
