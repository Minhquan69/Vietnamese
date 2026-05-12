import {
  Component,
  OnDestroy,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  trigger,
  transition,
  style,
  animate,
} from '@angular/animations';
import { InteractiveQuizService } from '../../features/services/interactive-quiz.service';
import type {
  InteractiveQuizResultDto,
  PlayerQuestionDto,
  PlayerQuizPackageDto,
  QuizAttemptSummaryDto,
  QuizResponseItemDto,
} from '../../features/models/interactive-quiz.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-interactive-quiz-player',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './interactive-quiz-player.component.html',
  styleUrl: './interactive-quiz-player.component.scss',
  animations: [
    trigger('cardAnim', [
      transition('* => *', [
        style({ opacity: 0, transform: 'translateY(14px)' }),
        animate(
          '260ms cubic-bezier(0.22, 1, 0.36, 1)',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
    ]),
    trigger('feedbackAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.94)' }),
        animate(
          '200ms ease-out',
          style({ opacity: 1, transform: 'scale(1)' }),
        ),
      ]),
    ]),
  ],
})
export class InteractiveQuizPlayerComponent implements OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly quizApi = inject(InteractiveQuizService);

  readonly phase = signal<'loading' | 'ready' | 'results'>('loading');
  readonly pkg = signal<PlayerQuizPackageDto | null>(null);
  readonly index = signal(0);
  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly result = signal<InteractiveQuizResultDto | null>(null);
  readonly analytics = signal<QuizAttemptSummaryDto[]>([]);
  readonly showAnalytics = signal(false);

  readonly mcPick = signal<Record<number, number>>({});
  readonly fillValues = signal<Partial<Record<number, Record<string, string>>>>({});
  readonly reorderOrder = signal<Partial<Record<number, number[]>>>({});
  readonly dragMap = signal<Record<number, Record<string, number>>>({});

  readonly timerSeconds = signal<number | null>(null);
  private timerHandle: ReturnType<typeof setInterval> | null = null;
  private startedAt = 0;
  private totalSeconds = 0;

  readonly current = computed(() => {
    const p = this.pkg();
    const i = this.index();
    if (!p?.questions?.length) {
      return null;
    }
    return p.questions[i] ?? null;
  });

  readonly progressPercent = computed(() => {
    const p = this.pkg();
    const i = this.index();
    if (!p?.questions.length) {
      return 0;
    }
    return ((i + 1) / p.questions.length) * 100;
  });

  readonly timerLabel = computed(() => {
    const t = this.timerSeconds();
    if (t == null) {
      return '';
    }
    const m = Math.floor(t / 60);
    const s = t % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  });

  constructor() {
    const quizId = Number(this.route.snapshot.queryParamMap.get('quizId'));
    if (!quizId) {
      this.error.set('Missing quizId query parameter.');
      this.phase.set('ready');
      return;
    }
    this.load(quizId);
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  private load(quizId: number): void {
    this.phase.set('loading');
    this.quizApi.take(quizId).subscribe({
      next: (p) => {
        this.pkg.set(p);
        this.bootstrapState(p);
        this.phase.set('ready');
        this.startTimer(p);
        this.startedAt = Date.now();
      },
      error: () => {
        this.error.set('Could not load this quiz. Check that you are signed in and the quiz exists.');
        this.phase.set('ready');
      },
    });
  }

  private bootstrapState(p: PlayerQuizPackageDto): void {
    const mc: Record<number, number> = {};
    const fill: Record<number, Record<string, string>> = {};
    const ord: Record<number, number[]> = {};
    const drag: Record<number, Record<string, number>> = {};

    for (const q of p.questions) {
      switch (q.questionType) {
        case 'ReorderSentence': {
          const ids = [...q.answers.map((a) => a.answerId)];
          this.shuffle(ids);
          ord[q.questionId] = ids;
          break;
        }
        case 'DragDrop': {
          drag[q.questionId] = {};
          break;
        }
        case 'FillBlank': {
          fill[q.questionId] = {};
          for (const k of this.blankKeys(q.interactivePayload)) {
            fill[q.questionId][k] = '';
          }
          break;
        }
        default:
          break;
      }
    }

    this.mcPick.set(mc);
    this.fillValues.set(fill);
    this.reorderOrder.set(ord);
    this.dragMap.set(drag);
  }

  private shuffle<T>(arr: T[]): void {
    for (let i = arr.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [arr[i], arr[j]] = [arr[j], arr[i]];
    }
  }

  blankKeys(payload: string | null | undefined): string[] {
    if (!payload) {
      return [];
    }
    try {
      const o = JSON.parse(payload) as { blanks?: Array<{ key?: string }> };
      return Array.isArray(o.blanks)
        ? o.blanks.map((b) => b.key).filter((x): x is string => !!x?.length)
        : [];
    } catch {
      return [];
    }
  }

  dragSlotKeys(payload: string | null | undefined): string[] {
    if (!payload) {
      return [];
    }
    try {
      const o = JSON.parse(payload) as { slots?: string[] };
      return Array.isArray(o.slots) ? o.slots : [];
    } catch {
      return [];
    }
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

  imageUrl(url: string | null | undefined): string | null {
    if (!url?.trim()) {
      return null;
    }
    const u = url.trim();
    if (u.startsWith('http://') || u.startsWith('https://')) {
      return u;
    }
    return `${environment.apiOrigin}${u.startsWith('/') ? '' : '/'}${u}`;
  }

  private startTimer(p: PlayerQuizPackageDto): void {
    this.stopTimer();
    const lim = p.timeLimitMinutes;
    if (lim == null || lim <= 0) {
      this.timerSeconds.set(null);
      return;
    }
    this.totalSeconds = Math.round(lim * 60);
    this.timerSeconds.set(this.totalSeconds);
    this.timerHandle = setInterval(() => {
      this.timerSeconds.update((t) => {
        if (t == null || t <= 0) {
          return 0;
        }
        return t - 1;
      });
      const left = this.timerSeconds();
      if (left === 0) {
        this.stopTimer();
        this.finish();
      }
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerHandle) {
      clearInterval(this.timerHandle);
      this.timerHandle = null;
    }
  }

  pickMc(q: PlayerQuestionDto, answerId: number): void {
    this.mcPick.update((m) => ({ ...m, [q.questionId]: answerId }));
  }

  updateBlank(qId: number, key: string, value: string): void {
    this.fillValues.update((m) => {
      const row = { ...(m[qId] ?? {}) };
      row[key] = value;
      return { ...m, [qId]: row };
    });
  }

  onReorderDragStart(q: PlayerQuestionDto, aid: number, ev: DragEvent): void {
    ev.dataTransfer?.setData('text/plain', String(aid));
    if (ev.dataTransfer) {
      ev.dataTransfer.effectAllowed = 'move';
    }
  }

  onReorderDrop(q: PlayerQuestionDto, targetAid: number, ev: DragEvent): void {
    ev.preventDefault();
    const cur = [...(this.reorderOrder()[q.questionId] ?? [])];
    const fromAid = Number(ev.dataTransfer?.getData('text/plain'));
    if (!fromAid || fromAid === targetAid) {
      return;
    }
    const fromIx = cur.indexOf(fromAid);
    const toIx = cur.indexOf(targetAid);
    if (fromIx < 0 || toIx < 0) {
      return;
    }
    cur.splice(fromIx, 1);
    cur.splice(toIx, 0, fromAid);
    this.reorderOrder.update((m) => ({ ...m, [q.questionId]: cur }));
  }

  setDrag(q: PlayerQuestionDto, slot: string, answerId: number | null): void {
    this.dragMap.update((m) => {
      const row = { ...(m[q.questionId] ?? {}) };
      if (answerId == null) {
        delete row[slot];
      } else {
        row[slot] = answerId;
      }
      return { ...m, [q.questionId]: row };
    });
  }

  dragAnswerForSlot(q: PlayerQuestionDto, slot: string): number | undefined {
    return this.dragMap()[q.questionId]?.[slot];
  }

  poolAnswers(q: PlayerQuestionDto): PlayerQuestionDto['answers'] {
    const taken = new Set(Object.values(this.dragMap()[q.questionId] ?? {}));
    return q.answers.filter((a) => !taken.has(a.answerId));
  }

  getAnswerText(q: PlayerQuestionDto, answerId: number): string {
    return q.answers.find((a) => a.answerId === answerId)?.answerText ?? '';
  }

  readonly dragTap = signal<{ quizId: number; answerId: number } | null>(null);

  tapPoolAnswer(q: PlayerQuestionDto, answerId: number): void {
    this.dragTap.set({ quizId: q.questionId, answerId });
  }

  tapSlot(q: PlayerQuestionDto, slot: string): void {
    const p = this.dragTap();
    if (!p || p.quizId !== q.questionId) {
      return;
    }
    this.setDrag(q, slot, p.answerId);
    this.dragTap.set(null);
  }

  startPoolDrag(ev: DragEvent, answerId: number): void {
    ev.dataTransfer?.setData('answerId', String(answerId));
    if (ev.dataTransfer) {
      ev.dataTransfer.effectAllowed = 'copy';
    }
  }

  onSlotDragOver(ev: DragEvent): void {
    ev.preventDefault();
  }

  onSlotDrop(q: PlayerQuestionDto, slot: string, ev: DragEvent): void {
    ev.preventDefault();
    const raw = ev.dataTransfer?.getData('answerId');
    const id = raw ? Number(raw) : NaN;
    if (!Number.isFinite(id)) {
      return;
    }
    this.setDrag(q, slot, id);
  }

  next(): void {
    const p = this.pkg();
    if (!p) {
      return;
    }
    if (this.index() < p.questions.length - 1) {
      this.index.update((i) => i + 1);
    } else {
      this.finish();
    }
  }

  prev(): void {
    if (this.index() > 0) {
      this.index.update((i) => i - 1);
    }
  }

  finish(): void {
    const p = this.pkg();
    if (!p || this.submitting() || this.phase() === 'results') {
      return;
    }
    this.stopTimer();
    this.submitting.set(true);
    const responses = this.buildResponses(p);
    const duration = Math.max(
      0,
      Math.round((Date.now() - this.startedAt) / 1000),
    );
    this.quizApi
      .submit({ quizId: p.quizId, durationSeconds: duration, responses })
      .subscribe({
        next: (r) => {
          this.result.set(r);
          this.phase.set('results');
          this.submitting.set(false);
          this.loadAnalytics(p.quizId);
        },
        error: () => {
          this.submitting.set(false);
          this.error.set('Submission failed. Try again.');
        },
      });
  }

  private buildResponses(p: PlayerQuizPackageDto): QuizResponseItemDto[] {
    const list: QuizResponseItemDto[] = [];
    const mc = this.mcPick();
    const fill = this.fillValues();
    const ord = this.reorderOrder();
    const drag = this.dragMap();

    for (const q of p.questions) {
      switch (q.questionType) {
        case 'FillBlank':
          list.push({
            questionId: q.questionId,
            fillBlank: { ...(fill[q.questionId] ?? {}) },
          });
          break;
        case 'ReorderSentence':
          list.push({
            questionId: q.questionId,
            orderedAnswerIds: [...(ord[q.questionId] ?? [])],
          });
          break;
        case 'DragDrop':
          list.push({
            questionId: q.questionId,
            dragDrop: { ...(drag[q.questionId] ?? {}) },
          });
          break;
        case 'Listening':
        case 'MultipleChoice':
        default:
          list.push({
            questionId: q.questionId,
            answerId: mc[q.questionId] ?? null,
          });
          break;
      }
    }
    return list;
  }

  private loadAnalytics(quizId: number): void {
    this.quizApi.attempts(quizId).subscribe({
      next: (rows) => this.analytics.set(rows),
      error: () => {},
    });
  }

  toggleAnalytics(): void {
    this.showAnalytics.update((v) => !v);
  }

  retry(): void {
    const id = this.pkg()?.quizId;
    if (!id) {
      return;
    }
    this.result.set(null);
    this.phase.set('loading');
    this.index.set(0);
    this.error.set(null);
    this.load(id);
  }

  outcomeFor(questionId: number) {
    return this.result()?.items.find((i) => i.questionId === questionId);
  }

}
