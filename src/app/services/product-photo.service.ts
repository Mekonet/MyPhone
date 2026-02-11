import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductPhotoService {
getProducts(): Observable<any> {
    // כאן אתה משתמש ב-proxy, אין צורך לציין פורט
    return this.http.get('/api/products');
  }
  constructor(private http: HttpClient) { }

  
  
}