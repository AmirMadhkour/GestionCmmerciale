import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Client, clientDisplayName } from '../../../core/models/client.model';
import { ClientService } from '../../../core/services/client.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { HasRoleDirective } from '../../../shared/directives/has-role.directive';

@Component({
  selector: 'app-client-list',
  imports: [DatePipe, RouterLink, PageHeaderComponent, LoadingSpinnerComponent, ConfirmDialogComponent, HasRoleDirective],
  templateUrl: './client-list.component.html'
})
export class ClientListComponent implements OnInit {
  private readonly clientService = inject(ClientService);
  private readonly notifier = inject(NotificationService);

  readonly displayName = clientDisplayName;
  readonly clients = signal<Client[]>([]);
  readonly loading = signal(true);
  readonly search = signal('');
  readonly pendingDelete = signal<Client | null>(null);
  readonly deleting = signal(false);

  readonly filtered = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) {
      return this.clients();
    }
    return this.clients().filter((client) =>
      [client.lastName, client.firstNameOrCompany, client.email, client.phone].some((value) =>
        value.toLowerCase().includes(term)
      )
    );
  });

  readonly deleteMessage = computed(() => {
    const client = this.pendingDelete();
    return client ? `Voulez-vous vraiment supprimer le client « ${clientDisplayName(client)} » ? Cette action est irréversible.` : '';
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.clientService.getAll().subscribe({
      next: (clients) => {
        this.clients.set(clients);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onSearch(event: Event): void {
    this.search.set((event.target as HTMLInputElement).value);
  }

  askDelete(client: Client): void {
    this.pendingDelete.set(client);
  }

  cancelDelete(): void {
    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const client = this.pendingDelete();
    if (!client) {
      return;
    }
    this.deleting.set(true);
    this.clientService.delete(client.id).subscribe({
      next: () => {
        this.clients.update((clients) => clients.filter((item) => item.id !== client.id));
        this.notifier.success('Client supprimé avec succès.');
        this.deleting.set(false);
        this.pendingDelete.set(null);
      },
      error: () => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
      }
    });
  }
}
