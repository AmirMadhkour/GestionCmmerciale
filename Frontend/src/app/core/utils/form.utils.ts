import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { ApiError } from '../models/api-error.model';

const SEGMENT_PATTERN = /^([^[\]]+)(?:\[(\d+)\])?$/;

export function isControlInvalid(control: AbstractControl | null | undefined): boolean {
  return !!control && control.invalid && (control.touched || control.dirty);
}

export function findControlByServerKey(root: AbstractControl, key: string): AbstractControl | null {
  let current: AbstractControl | null = root;

  for (const segment of key.split('.')) {
    const match = SEGMENT_PATTERN.exec(segment);
    if (!match || !(current instanceof FormGroup)) {
      return null;
    }

    const group: FormGroup = current;
    const controlName = Object.keys(group.controls).find((name) => name.toLowerCase() === match[1].toLowerCase());
    if (!controlName) {
      return null;
    }

    current = group.controls[controlName];

    if (match[2] !== undefined) {
      if (!(current instanceof FormArray)) {
        return null;
      }
      current = current.at(Number(match[2])) ?? null;
    }
  }

  return current;
}

export function applyServerErrors(form: FormGroup, error: ApiError): string[] {
  const unmatched: string[] = [];

  for (const [key, messages] of Object.entries(error.errors ?? {})) {
    const message = messages[0];
    if (!message) {
      continue;
    }

    const control = findControlByServerKey(form, key);
    if (control) {
      control.setErrors({ ...(control.errors ?? {}), server: message });
      control.markAsTouched();
    } else {
      unmatched.push(message);
    }
  }

  return unmatched;
}
