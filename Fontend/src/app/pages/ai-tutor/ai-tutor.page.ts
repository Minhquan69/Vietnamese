import {
  AfterViewChecked,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AiTutorService } from '../../features/services/ai-tutor.service';
import { AI_TUTOR_SCENARIOS } from '../../features/models/ai-tutor.model';

type ChatLine = { role: 'user' | 'assistant'; content: string };

@Component({
  selector: 'app-ai-tutor-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './ai-tutor.page.html',
  styleUrl: './ai-tutor.page.scss',
})
export class AiTutorPageComponent implements OnInit, AfterViewChecked {
  private readonly tutor = inject(AiTutorService);

  @ViewChild('scrollArea') scrollArea?: ElementRef<HTMLElement>;

  readonly scenarios = AI_TUTOR_SCENARIOS;
  readonly conversationId = signal<number | null>(null);
  readonly scenarioKey = signal<string>('daily');
  readonly lines = signal<ChatLine[]>([]);
  readonly suggestions = signal<string[]>([]);
  readonly draft = signal('');
  readonly sending = signal(false);
  readonly error = signal<string | null>(null);
  readonly sidebarOpen = signal(false);
  readonly history = signal<{ conversationId: number; title: string; updatedUtc: string }[]>([]);
  readonly historyLoading = signal(false);

  private shouldScroll = false;

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToEnd();
      this.shouldScroll = false;
    }
  }

  ngOnInit(): void {
    this.loadHistory();
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((v) => !v);
  }

  loadHistory(): void {
    this.historyLoading.set(true);
    this.tutor.conversations().subscribe({
      next: (rows) => {
        this.history.set(
          rows.map((r) => ({
            conversationId: r.conversationId,
            title: r.title,
            updatedUtc: r.updatedUtc,
          })),
        );
        this.historyLoading.set(false);
      },
      error: () => this.historyLoading.set(false),
    });
  }

  newChat(): void {
    this.conversationId.set(null);
    this.lines.set([]);
    this.suggestions.set([]);
    this.draft.set('');
    this.scenarioKey.set('daily');
    this.sidebarOpen.set(false);
  }

  pickScenario(key: string): void {
    if (this.conversationId() != null) {
      return;
    }
    this.scenarioKey.set(key);
  }

  applyChip(text: string): void {
    this.draft.set(text);
  }

  openConversation(id: number): void {
    this.tutor.messages(id).subscribe({
      next: (msgs) => {
        this.conversationId.set(id);
        this.lines.set(
          msgs.map((m) => ({
            role: m.role === 'assistant' ? 'assistant' : 'user',
            content: m.content,
          })) as ChatLine[],
        );
        this.suggestions.set([]);
        this.sidebarOpen.set(false);
        this.shouldScroll = true;
      },
    });
  }

  send(): void {
    const text = this.draft().trim();
    if (!text || this.sending()) {
      return;
    }

    this.error.set(null);
    this.sending.set(true);
    this.suggestions.set([]);
    this.lines.update((L) => [...L, { role: 'user', content: text }]);
    this.draft.set('');
    this.shouldScroll = true;

    this.tutor
      .chat({
        conversationId: this.conversationId(),
        message: text,
        scenarioKey: this.conversationId() == null ? this.scenarioKey() : null,
      })
      .subscribe({
        next: (res) => {
          this.conversationId.set(res.conversationId);
          this.lines.update((L) => [...L, { role: 'assistant', content: res.assistantMessage }]);
          this.suggestions.set(res.suggestions ?? []);
          this.sending.set(false);
          this.shouldScroll = true;
          this.loadHistory();
        },
        error: (err) => {
          this.sending.set(false);
          const body = err?.error as { message?: string; Message?: string } | undefined;
          const msg =
            body?.message ??
            body?.Message ??
            (typeof err?.error === 'string' ? err.error : null) ??
            'Could not reach the tutor. Try again.';
          this.error.set(msg);
        },
      });
  }

  onKeydown(ev: KeyboardEvent): void {
    if (ev.key === 'Enter' && !ev.shiftKey) {
      ev.preventDefault();
      this.send();
    }
  }

  private scrollToEnd(): void {
    const el = this.scrollArea?.nativeElement;
    if (el) {
      el.scrollTop = el.scrollHeight;
    }
  }
}
