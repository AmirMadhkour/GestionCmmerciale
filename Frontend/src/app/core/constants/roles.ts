export enum Role {
  Admin = 'ADMIN',
  User = 'USER',
  Commercial = 'COMMERCIAL'
}

export type RoleInput = Role | keyof typeof Role;

export const ROLE_LABELS: Record<Role, string> = {
  [Role.Admin]: 'Administrateur',
  [Role.User]: 'Utilisateur',
  [Role.Commercial]: 'Commercial'
};

export function toRole(value: RoleInput | string): Role {
  if (value in Role) {
    return Role[value as keyof typeof Role];
  }
  return value as Role;
}
