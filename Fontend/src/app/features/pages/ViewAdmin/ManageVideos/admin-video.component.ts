import { VideoService } from '../../../services/video.service';
import { Video } from '../../../models/video.model';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-admin-video',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-video.component.html',
  styleUrls: ['./admin-video.component.css'],
})
export class AdminVideoComponent implements OnInit {
  videos: any[] = [];
  selectedVideo: any;

  page = 1;
  pageSize = 10;
  total = 0;
  status: number | null = null;

  youtubeId: string = '';
  searchYoutubeId: string = '';

  currentVideoUrl!: SafeResourceUrl;

  constructor(
    private videoService: VideoService,
    private sanitizer: DomSanitizer,
  ) {}

  ngOnInit(): void {
    this.loadVideos();
  }

  loadVideos(resetPage: boolean = false) {
    if (resetPage) {
      this.page = 1;
    }

    this.videoService
      .listVideo(this.status ?? undefined, this.page, this.pageSize)
      .subscribe((res: any) => {
        this.videos = res.data ? res.data : res;
        this.total = res.total;

        if (this.videos.length > 0) {
          this.selectVideo(this.videos[0]);
        }
      });
  }

  selectVideo(video: any) {
    this.selectedVideo = video;

    this.currentVideoUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
      'https://www.youtube.com/embed/' + video.youtubeId,
    );

    setTimeout(() => {
      const player = document.getElementById('videoPlayer');
      if (player) {
        player.scrollIntoView({ behavior: 'smooth' });
      }
    }, 100);
  }

  importVideo() {
    if (!this.youtubeId) {
      alert('Enter YouTube ID to import');
      return;
    }

    this.videoService.insertVideo(this.youtubeId).subscribe({
      next: () => {
        alert('Import successful');
        this.youtubeId = '';
        this.loadVideos();
      },
      error: () => {
        alert('Import failed');
      },
    });
  }

  searchVideoById() {
    if (!this.searchYoutubeId) {
      alert('Enter YouTube ID to search');
      return;
    }

    this.videoService.getVideoById(this.searchYoutubeId).subscribe({
      next: (video) => {
        this.videos = [video];
        this.selectVideo(video);
      },
      error: () => {
        alert('Video not found');
      },
    });
  }

  updateStatus(videoId: any, status: any) {
    if (videoId === undefined || videoId === null) {
      alert('videoId is undefined');
      console.log('Video object:', videoId);
      return;
    }

    const id = Number(videoId);
    const s = Number(status);
    this.videoService.updateVideo(id, s).subscribe({
      next: () => {
        alert('Update status successful');
        this.loadVideos();
      },
      error: (err) => {
        console.log(err);
        alert('Update failed');
      },
    });
  }

  nextPage() {
      this.page++;
      this.loadVideos();
  }

  prevPage() {
    if (this.page > 1) {
      this.page--;
      this.loadVideos();
    }
  }
  get totalPages() {
    return Math.ceil(this.total / this.pageSize);
  }
}
