export interface Client {
  id: string;
  lastName: string;
  firstNameOrCompany: string;
  email: string;
  phone: string;
  address: string[];
  createdAt: string;
}

export interface ClientPayload {
  lastName: string;
  firstNameOrCompany: string;
  email: string;
  phone: string;
  address: string[];
}

export function clientDisplayName(client: Pick<Client, 'lastName' | 'firstNameOrCompany'>): string {
  return `${client.firstNameOrCompany} ${client.lastName}`.trim();
}
