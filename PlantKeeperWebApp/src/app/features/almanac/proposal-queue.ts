import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ApiClient } from '../../core/api-client';
import { AlmanacChangeProposalDto, AlmanacProposalStatus } from '../../core/models';
import { ApiFailure, toApiFailure } from '../../core/problem-details';
import { EmptyState } from '../../shared/ui/empty-state';
import { LoadState } from '../../shared/ui/load-state';
import { PageHeader } from '../../shared/ui/page-header';

type Filter = AlmanacProposalStatus | 'All';

@Component({
  selector: 'app-proposal-queue',
  imports: [EmptyState, LoadState, PageHeader],
  templateUrl: './proposal-queue.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProposalQueue {
  private readonly api = inject(ApiClient);

  protected readonly filters: readonly Filter[] = ['Pending', 'Applied', 'Rejected', 'All'];
  protected readonly filter = signal<Filter>('Pending');

  // Reactive params: changing the filter refetches on its own.
  protected readonly proposals = this.api.listResource<AlmanacChangeProposalDto>(
    'almanac-proposals',
    () => (this.filter() === 'All' ? {} : { status: this.filter() }),
  );

  protected readonly pending = computed(
    () => this.proposals.value().filter((proposal) => proposal.status === 'Pending').length,
  );

  protected readonly busy = signal<string | null>(null);
  protected readonly failure = signal<ApiFailure | null>(null);

  /** The proposed body, pretty-printed. A reviewer has to see what they are approving. */
  protected state(proposal: AlmanacChangeProposalDto): string {
    return proposal.proposedState === null || proposal.proposedState === undefined
      ? '(no body — this proposes a deletion)'
      : JSON.stringify(proposal.proposedState, null, 2);
  }

  protected async approve(proposal: AlmanacChangeProposalDto): Promise<void> {
    await this.decide(proposal, 'approve');
  }

  protected async reject(proposal: AlmanacChangeProposalDto): Promise<void> {
    await this.decide(proposal, 'reject');
  }

  private async decide(proposal: AlmanacChangeProposalDto, decision: string): Promise<void> {
    this.busy.set(proposal.id);
    this.failure.set(null);

    try {
      await this.api.create(`almanac-proposals/${proposal.id}/${decision}`, { note: null });
    } catch (error) {
      // A refusal is still a decision - the proposal moves to Rejected either way - so the
      // list is reloaded below rather than only on success.
      this.failure.set(toApiFailure(error));
    } finally {
      this.proposals.reload();
      this.busy.set(null);
    }
  }
}
