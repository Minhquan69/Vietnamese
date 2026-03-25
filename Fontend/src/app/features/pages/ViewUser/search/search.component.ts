import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VideoService } from '../../../services/video.service';
import { SearchResult } from '../../../models/search-result.model';

declare var YT: any;

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
})
export class SearchComponent implements OnInit {
  keyword = '';
  searchkeyword = '';
  hasSearched = false;
  noResult = false;

  results: SearchResult[] = [];
  index = 0;

  videoId = '';
  currentTime = 0;

  subtitle = '';
  highlightedSubtitle = '';

  isPlaying = true;

  player: any;

  constructor(private videoService: VideoService) {}

  ngOnInit() {}

  onSearchClick() {
    this.startSearch();
  }

  startSearch() {
    if (!this.keyword.trim()) return;

    // Reset state để Angular render lại
    this.hasSearched = false;
    this.noResult = false;
    this.videoId = '';
    this.highlightedSubtitle = '';

    // Delay 1 tick để Angular update UI
    setTimeout(() => {
      this.searchkeyword = this.keyword;
      this.hasSearched = true;

      this.videoService.searchVideo(this.keyword).subscribe((res) => {
        if (!res || res.length === 0) {
          this.noResult = true;
          this.results = [];
          return;
        }

        this.noResult = false;
        this.results = res;
        this.index = 0;

        const video = res[0];

        this.videoId = video.youtubeId;
        this.subtitle = video.subtitle;
        this.currentTime = Math.floor(video.startTime);

        this.loadVideo();
        this.highlightSubtitle();
      });
    }, 0);
  }

  loadVideo() {
    setTimeout(() => {
      if (this.player) {
        this.player.destroy();
      }

      this.player = new YT.Player('ytplayer', {
        videoId: this.videoId,

        playerVars: {
          start: this.currentTime,
          autoplay: 1,
          mute: 1,
          rel: 0,
        },
      });
    }, 100);
  }

  highlightSubtitle() {
    if (!this.subtitle) return;

    const escaped = this.searchkeyword.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

    this.highlightedSubtitle = this.subtitle.replace(
      new RegExp(escaped, 'gi'),
      `<span class="highlight">$&</span>`,
    );
  }

  

  previousVideo() {
    if (this.index <= 0) return;

    this.index--;

    const video = this.results[this.index];

    this.videoId = video.youtubeId;
    this.subtitle = video.subtitle;
    this.currentTime = Math.floor(video.startTime);

    this.loadVideo();

    this.highlightSubtitle();
  }

  nextVideo() {
    if (this.index >= this.results.length - 1) return;

    this.index++;

    const video = this.results[this.index];

    this.videoId = video.youtubeId;
    this.subtitle = video.subtitle;
    this.currentTime = Math.floor(video.startTime);

    this.loadVideo();

    this.highlightSubtitle();
  }

  back5() {
    if (!this.player) return;

    const t = this.player.getCurrentTime();

    this.player.seekTo(Math.max(0, t - 5), true);
  }

  forward5() {
    if (!this.player) return;

    const t = this.player.getCurrentTime();

    this.player.seekTo(t + 5, true);
  }

  replay() {
    if (!this.player) return;

    this.player.seekTo(this.currentTime, true);
  }

  pause() {
    if (!this.player) return;

    if (this.player.getPlayerState() === 1) {
      this.player.pauseVideo();
      this.isPlaying = false;
    } else {
      this.player.playVideo();
      this.isPlaying = true;
    }
  }
}
