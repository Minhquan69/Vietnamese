import { Injectable } from '@angular/core';
import { HttpClient,HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchResult } from '../models/search-result.model';
import { Video } from '../models/video.model';
@Injectable({
  providedIn: 'root',
})
export class VideoService {
  private apiUrl = 'http://localhost:5108/api/videos';

  constructor(private http: HttpClient) {}

  searchVideo(keyword: string): Observable<SearchResult[]> {
    return this.http.get<SearchResult[]>(
      `${this.apiUrl}/searchVideo?keyword=${encodeURIComponent(keyword)}`,
    );
  }

  listVideo(
    status?: number,
    page: number = 1,
    pageSize: number = 10,
  ): Observable<any> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (status !== undefined) {
      params = params.set('status', status);
    }

    return this.http.get<any>(`${this.apiUrl}/listVideo`, { params });
  }
  insertVideo(youtubeId: string): Observable<any> {
    const token = localStorage.getItem('token');

    return this.http.post(
      `${this.apiUrl}/insertVideo`,
      { youtubeId: youtubeId },
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
        responseType: 'text', // QUAN TRỌNG
      },
    );
  }
  updateVideo(videoId: number, status: number): Observable<any> {
    const token = localStorage.getItem('token');

    const url = `${this.apiUrl}/updateVideo?videoId=${videoId}&status=${status}`;

    return this.http.put(
      url,
      {},
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
        responseType: 'text',
      },
    );
  }
  getVideoById(id: string): Observable<Video> {
    return this.http.get<Video>(`${this.apiUrl}/${id}`);
  }
}
