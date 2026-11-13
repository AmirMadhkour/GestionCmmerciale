import { Routes } from '@angular/router';
import { Role } from '../../core/constants/roles';
import { roleGuard } from '../../core/guards/role.guard';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    title: 'Produits · Gestion Commerciale',
    loadComponent: () => import('./product-list/product-list.component').then((m) => m.ProductListComponent)
  },
  {
    path: 'new',
    title: 'Nouveau produit · Gestion Commerciale',
    canActivate: [roleGuard],
    data: { roles: [Role.Admin] },
    loadComponent: () => import('./product-form/product-form.component').then((m) => m.ProductFormComponent)
  },
  {
    path: ':id',
    title: 'Fiche produit · Gestion Commerciale',
    loadComponent: () => import('./product-detail/product-detail.component').then((m) => m.ProductDetailComponent)
  },
  {
    path: ':id/edit',
    title: 'Modifier le produit · Gestion Commerciale',
    canActivate: [roleGuard],
    data: { roles: [Role.Admin] },
    loadComponent: () => import('./product-form/product-form.component').then((m) => m.ProductFormComponent)
  }
];
