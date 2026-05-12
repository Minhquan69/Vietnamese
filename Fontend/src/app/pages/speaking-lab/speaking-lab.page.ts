import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SpeakingService } from '../../features/services/speaking.service';
import type {
  SpeakingAnalytics,
  SpeakingAttemptSummary,
  SpeakingEvaluateResponse,
} from '../../features/models/speaking.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-speaking-lab-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './speaking-lab.page.html',
  styleUrl: './speaking-lab.page.scss',
})
export class SpeakingLabPageComponent implements OnInit, OnDestroy {
  private readonly speaking = inject(SpeakingService);

  @ViewChild('waveCanvas') waveCanvas?: ElementRef<HTMLCanvasElement>;

  readonly referenceText = signal('');
  readonly isRecording = signal(false);
  readonly evaluating = signal(false);
  readonly error = signal<string | null>(null);
  readonly lastResult = signal<SpeakingEvaluateResponse | null>(null);
  readonly analytics = signal<SpeakingAnalytics | null>(null);
  readonly historyRows = signal<SpeakingAttemptSummary[]>([]);
  readonly analyticsLoading = signal(false);

  private mediaStream: MediaStream | null = null;
  private audioContext: AudioContext | null = null;
  private analyser: AnalyserNode | null = null;
  private mediaRecorder: MediaRecorder | null = null;
  private recordedChunks: Blob[] = [];
  private rafId = 0;
  private recordStartedAt = 0;
  private timeDomainBuffer: Uint8Array | null = null;

  ngOnInit(): void {
    this.loadDashboard();
  }

  ngOnDestroy(): void {
    this.stopWaveformLoop();
    this.disposeRecording();
  }

  loadDashboard(): void {
    this.analyticsLoading.set(true);
    this.speaking.analytics().subscribe({
      next: (a) => {
        this.analytics.set(a);
        this.historyRows.set(a.recent ?? []);
        this.analyticsLoading.set(false);
      },
      error: () => this.analyticsLoading.set(false),
    });
  }

  startRecording(): void {
    if (this.isRecording() || this.evaluating()) {
      return;
    }

    this.error.set(null);
    void this.beginRecordingSession();
  }

  stopRecordingAndEvaluate(): void {
    if (!this.isRecording() || !this.mediaRecorder) {
      return;
    }

    this.mediaRecorder.stop();
  }

  audioPlaybackUrl(relative: string | null | undefined): string {
    if (!relative) {
      return '';
    }

    const path = relative.startsWith('/') ? relative : `/${relative}`;
    return `${environment.apiOrigin}${path}`;
  }

  private async beginRecordingSession(): Promise<void> {
    try {
      if (!navigator.mediaDevices?.getUserMedia) {
        this.error.set('Trình duyệt không hỗ trợ ghi âm.');
        return;
      }

      const stream = await navigator.mediaDevices.getUserMedia({
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
        },
      });

      this.mediaStream = stream;
      const AudioCtx = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
      const ctx = new AudioCtx();
      this.audioContext = ctx;
      await ctx.resume();

      const source = ctx.createMediaStreamSource(stream);
      const analyser = ctx.createAnalyser();
      analyser.fftSize = 256;
      analyser.smoothingTimeConstant = 0.65;
      source.connect(analyser);
      this.analyser = analyser;
      this.timeDomainBuffer = new Uint8Array(analyser.fftSize);

      const mime = this.pickRecorderMimeType();
      this.recordedChunks = [];
      const recorder = mime
        ? new MediaRecorder(stream, { mimeType: mime })
        : new MediaRecorder(stream);
      this.mediaRecorder = recorder;

      recorder.ondataavailable = (ev) => {
        if (ev.data.size > 0) {
          this.recordedChunks.push(ev.data);
        }
      };

      recorder.onstop = () => {
        const durationMs = Math.max(0, Date.now() - this.recordStartedAt);
        this.isRecording.set(false);
        this.stopWaveformLoop();
        const blob = new Blob(this.recordedChunks, {
          type: recorder.mimeType || mime || 'audio/webm',
        });
        this.disposeRecording();
        void this.uploadRecording(blob, durationMs);
      };

      this.recordStartedAt = Date.now();
      recorder.start(200);
      this.isRecording.set(true);
      this.startWaveformLoop();
    } catch {
      this.error.set('Không thể bật micro. Hãy cấp quyền và thử lại.');
      this.disposeRecording();
      this.isRecording.set(false);
    }
  }

  private pickRecorderMimeType(): string {
    const candidates = ['audio/webm;codecs=opus', 'audio/webm', 'audio/ogg;codecs=opus'];
    for (const c of candidates) {
      if (typeof MediaRecorder !== 'undefined' && MediaRecorder.isTypeSupported(c)) {
        return c;
      }
    }

    return '';
  }

  private startWaveformLoop(): void {
    const tick = () => {
      this.drawWaveform();
      if (this.isRecording()) {
        this.rafId = requestAnimationFrame(tick);
      }
    };

    this.rafId = requestAnimationFrame(tick);
  }

  private stopWaveformLoop(): void {
    if (this.rafId) {
      cancelAnimationFrame(this.rafId);
      this.rafId = 0;
    }
  }

  private drawWaveform(): void {
    const canvas = this.waveCanvas?.nativeElement;
    const analyser = this.analyser;
    if (!canvas || !analyser || !this.timeDomainBuffer) {
      return;
    }

    const ctx2d = canvas.getContext('2d');
    if (!ctx2d) {
      return;
    }

    const w = canvas.width;
    const h = canvas.height;
    analyser.getByteTimeDomainData(this.timeDomainBuffer);

    ctx2d.clearRect(0, 0, w, h);
    ctx2d.fillStyle = 'rgba(79, 70, 229, 0.08)';
    ctx2d.fillRect(0, 0, w, h);

    ctx2d.lineWidth = 2;
    ctx2d.strokeStyle = 'rgba(79, 70, 229, 0.85)';
    ctx2d.beginPath();

    const slice = w / this.timeDomainBuffer.length;
    let x = 0;
    for (let i = 0; i < this.timeDomainBuffer.length; i++) {
      const v = this.timeDomainBuffer[i] / 128.0;
      const y = (v * h) / 2;
      if (i === 0) {
        ctx2d.moveTo(x, y);
      } else {
        ctx2d.lineTo(x, y);
      }

      x += slice;
    }

    ctx2d.stroke();

    ctx2d.fillStyle = 'rgba(15, 23, 42, 0.35)';
    const bars = 48;
    const step = Math.floor(this.timeDomainBuffer.length / bars);
    const barW = w / bars - 2;
    for (let b = 0; b < bars; b++) {
      const idx = b * step;
      const amp = Math.abs((this.timeDomainBuffer[idx] ?? 128) - 128) / 128;
      const bh = Math.max(4, amp * h * 0.85);
      ctx2d.fillRect(b * (barW + 2) + 1, h - bh, barW, bh);
    }
  }

  private disposeRecording(): void {
    this.mediaRecorder = null;
    this.analyser = null;
    this.timeDomainBuffer = null;
    if (this.mediaStream) {
      this.mediaStream.getTracks().forEach((t) => t.stop());
      this.mediaStream = null;
    }

    if (this.audioContext) {
      void this.audioContext.close();
      this.audioContext = null;
    }
  }

  private async uploadRecording(blob: Blob, durationMs: number): Promise<void> {
    this.evaluating.set(true);
    this.error.set(null);

    const ext = blob.type.includes('ogg') ? '.ogg' : '.webm';
    const file = new File([blob], `recording${ext}`, { type: blob.type || 'audio/webm' });
    const fd = new FormData();
    fd.append('audio', file);
    const ref = this.referenceText().trim();
    if (ref) {
      fd.append('referenceText', ref);
    }

    fd.append('durationMs', String(durationMs));

    this.speaking.evaluate(fd).subscribe({
      next: (res) => {
        this.lastResult.set(res);
        this.evaluating.set(false);
        this.loadDashboard();
      },
      error: (err) => {
        const msg =
          err?.error?.message ??
          err?.error?.errors?.[0] ??
          (typeof err?.message === 'string' ? err.message : null) ??
          'Không gửi được bản ghi. Thử lại.';
        this.error.set(String(msg));
        this.evaluating.set(false);
      },
    });
  }
}
