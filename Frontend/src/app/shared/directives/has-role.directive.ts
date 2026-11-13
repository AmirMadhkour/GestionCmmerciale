import { Directive, TemplateRef, ViewContainerRef, effect, inject, input } from '@angular/core';
import { RoleInput } from '../../core/constants/roles';
import { AuthService } from '../../core/services/auth.service';

@Directive({ selector: '[appHasRole]' })
export class HasRoleDirective {
  private readonly templateRef = inject<TemplateRef<unknown>>(TemplateRef);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly auth = inject(AuthService);
  private hasView = false;

  readonly appHasRole = input.required<RoleInput | RoleInput[]>();

  constructor() {
    effect(() => {
      const allowed = this.auth.hasRole(this.appHasRole());

      if (allowed && !this.hasView) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.hasView = true;
      } else if (!allowed && this.hasView) {
        this.viewContainer.clear();
        this.hasView = false;
      }
    });
  }
}
