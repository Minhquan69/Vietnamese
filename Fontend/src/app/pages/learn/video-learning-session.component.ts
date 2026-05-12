import {
  AfterViewInit,
  Component,
  Input,
  OnDestroy,
  computed,
  effect,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { VideoLearningService } from '../../features/services/video-learning.service';
import { VocabularyLearningService } from '../../features/services/vocabulary-learning.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import type {
  ExtractedTokenDto,
  TranscriptCueDto,
  VideoLearningSessionDto,
} from '../../features/models/video-learning.model';
import type { VocabularyCardDto } from '../../features/models/vocabulary.model';
import { environment } from '../../../environments/environment';
import { firstValueFrom } from 'rxjs';

declare global {
  interface Window {
    YT?: { Player: new (el: string | HTMLElement, cfg: unknown) => YtPlayer };
    onYouTubeIframeAPIReady?: () => void;
  }
}

interface YtPlayer {
  getCurrentTime(): number;
  seekTo(seconds: number, allowSeekAhead: boolean): void;
  getPlayerState(): number;
  destroy(): void;
}

@Component({
  selector: 'app-video-learning-session',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './video-learning-session.component.html',
  styleUrl: './video-learning-session.component.scss',
})
export class VideoLearningSessionComponent implements AfterViewInit, OnDestroy {
  readonly videoLearning = inject(VideoLearningService);
  readonly vocabApi = inject(VocabularyLearningService);
  readonly authSession = inject(AuthSessionService);
  readonly sanitizer = inject(DomSanitizer);

  /** Bound from parent */
  @Input({ required: true }) youtubeId!: string;
  @Input() title = '';

  readonly mode = signal<'loading' | 'immersive' | 'fallback'>('loading');
  readonly session = signal<VideoLearningSessionDto | null>(null);
  readonly error = signal<string | null>(null);

  readonly currentTime = signal(0);
  readonly extractCache = signal<Record<number, ExtractedTokenDto[]>>({});
  readonly dictCard = signal<VocabularyCardDto | null>(null);
  readonly dictLoading = signal(false);
  readonly dictOpen = signal(false);
  readonly saving = signal(false);
  readonly linking = signal(false);

  private player: YtPlayer | null = null;
  private timeTimer: ReturnType<typeof setInterval> | null = null;
  private apiReady = false;

  readonly loggedIn = computed(() => !!this.authSession.getAccessToken());

  readonly activeCueIndex = computed(() => {
    const s = this.session();
    const t = this.currentTime();
    if (!s?.transcripts?.length) {
      return -1;
    }
    return s.transcripts.findIndex((c) => t >= c.startTime && t < c.endTime);
  });

  readonly activeCue = computed((): TranscriptCueDto | null => {
    const s = this.session();
    const i = this.activeCueIndex();
    if (!s || i < 0) {
      return null;
    }
    return s.transcripts[i] ?? null;
  });

  readonly fallbackEmbed = computed((): SafeResourceUrl | null => {
    const embed = `https://www.youtube.com/embed/${this.youtubeId}?rel=0`;
    return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
  });

  readonly playerDomId = computed(() => `vl-yt-${this.youtubeId.replace(/[^a-zA-Z0-9_-]/g, '_')}`);

  constructor() {
    effect(() => {
      const cue = this.activeCue();
      if (cue && this.mode() === 'immersive') {
        this.ensureExtract(cue.transcriptId);
      }
    });
  }

  ngOnDestroy(): void {
    this.stopTimer();
    try {
      this.player?.destroy();
    } catch {
      /* noop */
    }
    this.player = null;
  }

  ngAfterViewInit(): void {
    this.loadSession();
  }

  private loadSession(): void {
    if (!this.youtubeId?.trim()) {
      this.mode.set('fallback');
      return;
    }

    this.mode.set('loading');
    this.videoLearning.session(this.youtubeId.trim()).subscribe({
      next: (s) => {
        this.session.set(s);
        this.mode.set('immersive');
        setTimeout(() => this.bootstrapYoutube(), 80);
      },
      error: () => {
        this.mode.set('fallback');
        this.error.set('Video is not registered for enhanced learning — basic playback only.');
      },
    });
  }

  private bootstrapYoutube(): void {
    if (this.mode() !== 'immersive') {
      return;
    }

    const w = window as Window & { YT?: unknown };
    if (w.YT && (w.YT as { Player?: unknown }).Player) {
      this.createPlayer();
      return;
    }

    const prev = window.onYouTubeIframeAPIReady;
    window.onYouTubeIframeAPIReady = () => {
      prev?.();
      this.apiReady = true;
      this.createPlayer();
    };

    if (!document.querySelector('script[src="https://www.youtube.com/iframe_api"]')) {
      const tag = document.createElement('script');
      tag.src = 'https://www.youtube.com/iframe_api';
      document.body.appendChild(tag);
    }
  }

  private createPlayer(): void {
    const w = window as Window & { YT?: { Player: new (el: string, cfg: unknown) => YtPlayer } };
    if (!w.YT?.Player) {
      return;
    }

    const id = this.playerDomId();
    const el = document.getElementById(id);
    if (!el) {
      return;
    }

    this.player?.destroy();
    this.player = new w.YT.Player(id, {
      videoId: this.youtubeId,
      playerVars: {
        playsinline: 1,
        rel: 0,
        modestbranding: 1,
      },
      events: {
        onReady: () => this.startTimer(),
      },
    });
  }

  private startTimer(): void {
    this.stopTimer();
    this.timeTimer = setInterval(() => {
      try {
        const st = this.player?.getPlayerState?.();
        const playing = st === 1;
        if (playing && this.player) {
          this.currentTime.set(this.player.getCurrentTime());
        }
      } catch {
        /* noop */
      }
    }, 200);
  }

  private stopTimer(): void {
    if (this.timeTimer) {
      clearInterval(this.timeTimer);
      this.timeTimer = null;
    }
  }

  private ensureExtract(transcriptId: number): void {
    const cache = this.extractCache();
    if (cache[transcriptId]) {
      return;
    }

    this.videoLearning.extract(this.youtubeId, transcriptId).subscribe({
      next: (res) => {
        this.extractCache.update((m) => ({ ...m, [transcriptId]: res.tokens }));
      },
      error: () => {},
    });
  }

  tokensForCue(cue: TranscriptCueDto | null): ExtractedTokenDto[] {
    if (!cue) {
      return [];
    }
    return this.extractCache()[cue.transcriptId] ?? [];
  }

  seekToCue(c: TranscriptCueDto): void {
    try {
      this.player?.seekTo(c.startTime + 0.05, true);
      this.currentTime.set(c.startTime);
    } catch {
      /* noop */
    }
  }

  async openToken(t: ExtractedTokenDto): Promise<void> {
    this.dictOpen.set(true);
    this.dictLoading.set(true);
    this.dictCard.set(null);

    try {
      if (t.vocabularyId != null) {
        const card = await firstValueFrom(this.vocabApi.getById(t.vocabularyId));
        this.dictCard.set(card ?? null);
      } else {
        const res = await firstValueFrom(this.vocabApi.search(t.text, 1, 5));
        const hit = res?.items?.[0];
        this.dictCard.set(hit ?? null);
      }
    } catch {
      this.dictCard.set(null);
    } finally {
      this.dictLoading.set(false);
    }
  }

  closeDict(): void {
    this.dictOpen.set(false);
    this.dictCard.set(null);
  }

  resolveAudio(url: string | null | undefined): string | null {
    if (!url?.trim()) {
      return null;
    }
    const u = url.trim();
    if (u.startsWith('http://') || u.startsWith('https://')) {
      return u;
    }
    return `${environment.apiOrigin}${u.startsWith('/') ? '' : '/'}${u}`;
  }

  saveWord(card: VocabularyCardDto, cue: TranscriptCueDto | null): void {
    if (!this.loggedIn()) {
      return;
    }
    this.saving.set(true);
    this.vocabApi.setSaved(card.vocabularyId, true).subscribe({
      next: () => {
        this.saving.set(false);
        if (cue) {
          this.linkVideoEntry(card.vocabularyId, cue);
        }
      },
      error: () => this.saving.set(false),
    });
  }

  private linkVideoEntry(vocabularyId: number, cue: TranscriptCueDto): void {
    if (!this.loggedIn()) {
      return;
    }
    this.linking.set(true);
    this.videoLearning
      .link({
        youtubeId: this.youtubeId,
        vocabularyId,
        transcriptId: cue.transcriptId,
        contextSnippet: cue.sentence.slice(0, 280),
      })
      .subscribe({
        next: () => this.linking.set(false),
        error: () => this.linking.set(false),
      });
  }

  sidebarCueActive(c: TranscriptCueDto): boolean {
    const i = this.activeCueIndex();
    const s = this.session();
    if (!s || i < 0) {
      return false;
    }
    return s.transcripts[i]?.transcriptId === c.transcriptId;
  }

  openChip(card: VocabularyCardDto): void {
    void this.openToken({
      text: card.word,
      startIndex: 0,
      length: card.word.length,
      vocabularyId: card.vocabularyId,
    });
  }
}
