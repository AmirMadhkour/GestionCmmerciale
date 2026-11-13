import { VAT_RATE } from '../constants/app.constants';

export function roundAmount(amount: number): number {
  return Math.round(amount * 100) / 100;
}

export function vatAmount(amountHt: number): number {
  return roundAmount(amountHt * VAT_RATE);
}

export function applyVat(amountHt: number): number {
  return roundAmount(amountHt * (1 + VAT_RATE));
}
