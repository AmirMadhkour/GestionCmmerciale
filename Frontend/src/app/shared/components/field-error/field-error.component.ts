import { Component, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-field-error',
  templateUrl: './field-error.component.html'
})
export class FieldErrorComponent {
  readonly control = input.required<AbstractControl | null>();
  readonly label = input('Ce champ');

  get message(): string | null {
    const control = this.control();
    if (!control || !control.errors || !(control.touched || control.dirty)) {
      return null;
    }

    const errors = control.errors;

    if (errors['server']) {
      return errors['server'];
    }
    if (errors['required']) {
      return `${this.label()} est obligatoire.`;
    }
    if (errors['email']) {
      return 'L’adresse email est invalide.';
    }
    if (errors['minlength']) {
      return `${this.label()} doit contenir au moins ${errors['minlength'].requiredLength} caractères.`;
    }
    if (errors['maxlength']) {
      return `${this.label()} ne doit pas dépasser ${errors['maxlength'].requiredLength} caractères.`;
    }
    if (errors['min']) {
      return `${this.label()} doit être supérieur ou égal à ${errors['min'].min}.`;
    }
    if (errors['max']) {
      return `${this.label()} doit être inférieur ou égal à ${errors['max'].max}.`;
    }
    if (errors['pattern']) {
      return `${this.label()} a un format invalide.`;
    }
    return `${this.label()} est invalide.`;
  }
}
