import { LOW_STOCK_THRESHOLD } from '../../core/constants/app.constants';

export function stockBadgeClass(quantity: number): string {
  if (quantity <= 0) {
    return 'bg-red-50 text-red-700 ring-red-200';
  }
  if (quantity <= LOW_STOCK_THRESHOLD) {
    return 'bg-amber-50 text-amber-700 ring-amber-200';
  }
  return 'bg-emerald-50 text-emerald-700 ring-emerald-200';
}

export function stockLabel(quantity: number): string {
  if (quantity <= 0) {
    return 'Rupture';
  }
  if (quantity <= LOW_STOCK_THRESHOLD) {
    return 'Stock faible';
  }
  return 'En stock';
}
