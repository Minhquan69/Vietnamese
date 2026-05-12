import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VocabularyLearningService } from '../../features/services/vocabulary-learning.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import type {
  UserVocabularyCardDto,
  VocabularyCardDto,
  VocabularyStatsDto,
} from '../../features/models/vocabulary.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-vocabulary-hub',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './vocabulary-hub.component.html',
  styleUrl: './vocabulary-hub.component.scss',
})
export class VocabularyHubComponent implements OnInit {
  private readonly vocab = inject(VocabularyLearningService);
  readonly authSession = inject(AuthSessionService);

  readonly tab = signal<'browse' | 'study' | 'saved'>('browse');
  readonly searchQuery = signal('');
  readonly items = signal<VocabularyCardDto[]>([]);
  readonly total = signal(0);
  readonly browseLoading = signal(false);
  readonly selected = signal<VocabularyCardDto | null>(null);
  /** Present when signed in; reflects saved/mastery for dictionary modal */
  readonly modalUserState = signal<{
    saved: boolean;
    masteryScore: number;
    familiarity: number;
  } | null>(null);
  readonly modalUserLoading = signal(false);

  readonly deck = signal<UserVocabularyCardDto[]>([]);
  readonly deckIndex = signal(0);
  readonly flipped = signal(false);
  readonly studyLoading = signal(false);
  readonly reviewing = signal(false);
  private touchStartX = 0;

  readonly stats = signal<VocabularyStatsDto | null>(null);
  readonly savedItems = signal<UserVocabularyCardDto[]>([]);
  readonly savedLoading = signal(false);

  readonly loggedIn = computed(() => !!this.authSession.getAccessToken());

  readonly currentCard = computed(() => {
    const d = this.deck();
    const i = this.deckIndex();
    return d[i] ?? null;
  });

  ngOnInit(): void {
    this.loadBrowse();
    if (this.loggedIn()) {
      this.refreshStats();
    }
  }

  setTab(t: 'browse' | 'study' | 'saved'): void {
    this.tab.set(t);
    if (t === 'saved' && this.loggedIn()) {
      this.loadSaved();
    }
    if (t === 'study' && this.loggedIn() && this.deck().length === 0) {
      this.loadDeck();
    }
  }

  loadBrowse(): void {
    this.browseLoading.set(true);
    this.vocab.search(this.searchQuery(), 1, 48).subscribe({
      next: (res) => {
        this.items.set(res.items);
        this.total.set(res.total);
        this.browseLoading.set(false);
      },
      error: () => this.browseLoading.set(false),
    });
  }

  onSearchSubmit(): void {
    this.loadBrowse();
  }

  openDictionary(card: VocabularyCardDto): void {
    this.selected.set(card);
    this.modalUserState.set(null);
    if (!this.loggedIn()) {
      this.modalUserLoading.set(false);
      return;
    }
    this.modalUserLoading.set(true);
    this.vocab.getMine(card.vocabularyId).subscribe({
      next: (u) => {
        this.modalUserState.set({
          saved: u.saved,
          masteryScore: u.masteryScore,
          familiarity: u.familiarity,
        });
        this.modalUserLoading.set(false);
      },
      error: () => {
        this.modalUserState.set({
          saved: false,
          masteryScore: 0,
          familiarity: 0,
        });
        this.modalUserLoading.set(false);
      },
    });
  }

  closeDictionary(): void {
    this.selected.set(null);
    this.modalUserState.set(null);
    this.modalUserLoading.set(false);
  }

  refreshStats(): void {
    if (!this.loggedIn()) {
      return;
    }
    this.vocab.stats().subscribe({
      next: (s) => this.stats.set(s),
      error: () => {},
    });
  }

  toggleSave(card: VocabularyCardDto, saved: boolean): void {
    if (!this.loggedIn()) {
      return;
    }
    this.vocab.setSaved(card.vocabularyId, saved).subscribe({
      next: () => {
        this.refreshStats();
        if (this.selected()?.vocabularyId === card.vocabularyId) {
          this.modalUserState.update((s) =>
            s ? { ...s, saved } : { saved, masteryScore: 0, familiarity: 0 },
          );
        }
        if (this.tab() === 'saved') {
          this.loadSaved();
        }
      },
    });
  }

  modalToggleSave(): void {
    const card = this.selected();
    const st = this.modalUserState();
    if (!card || this.modalUserLoading() || st === null) {
      return;
    }
    this.toggleSave(card, !st.saved);
  }

  loadSaved(): void {
    this.savedLoading.set(true);
    this.vocab.savedList().subscribe({
      next: (list) => {
        this.savedItems.set(list);
        this.savedLoading.set(false);
      },
      error: () => this.savedLoading.set(false),
    });
  }

  loadDeck(): void {
    this.studyLoading.set(true);
    this.vocab.getDeck(24).subscribe({
      next: (cards) => {
        this.deck.set(cards);
        this.deckIndex.set(0);
        this.flipped.set(false);
        this.studyLoading.set(false);
      },
      error: () => this.studyLoading.set(false),
    });
  }

  toggleFlip(): void {
    this.flipped.update((v) => !v);
  }

  playAudio(url: string | null | undefined): void {
    const resolved = this.resolveAudioUrl(url);
    if (!resolved) {
      return;
    }
    const a = new Audio(resolved);
    void a.play().catch(() => {});
  }

  resolveAudioUrl(url: string | null | undefined): string | null {
    if (!url?.trim()) {
      return null;
    }
    const u = url.trim();
    if (u.startsWith('http://') || u.startsWith('https://')) {
      return u;
    }
    return `${environment.apiOrigin}${u.startsWith('/') ? '' : '/'}${u}`;
  }

  grade(g: 'again' | 'hard' | 'good' | 'easy'): void {
    const c = this.currentCard();
    if (!c || this.reviewing()) {
      return;
    }
    this.reviewing.set(true);
    this.vocab.review(c.vocabularyId, g).subscribe({
      next: () => {
        this.reviewing.set(false);
        this.flipped.set(false);
        const next = this.deckIndex() + 1;
        if (next >= this.deck().length) {
          this.loadDeck();
          return;
        }
        this.deckIndex.set(next);
        this.refreshStats();
      },
      error: () => this.reviewing.set(false),
    });
  }

  onCardTouchStart(ev: TouchEvent): void {
    if (this.tab() !== 'study' || !ev.changedTouches?.length) {
      return;
    }
    this.touchStartX = ev.changedTouches[0].clientX;
  }

  onCardTouchEnd(ev: TouchEvent): void {
    if (this.tab() !== 'study' || !ev.changedTouches?.length || !this.currentCard()) {
      return;
    }
    const dx = ev.changedTouches[0].clientX - this.touchStartX;
    if (Math.abs(dx) < 56) {
      return;
    }
    if (dx < 0) {
      this.grade('again');
    } else {
      this.grade('easy');
    }
  }

  familiarityLabel(n: number): string {
    switch (n) {
      case 0:
        return 'New';
      case 1:
        return 'Learning';
      case 2:
        return 'Familiar';
      default:
        return 'Mastered';
    }
  }
}
