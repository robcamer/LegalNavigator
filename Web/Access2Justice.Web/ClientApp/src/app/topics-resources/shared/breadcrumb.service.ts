import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Breadcrumb } from './breadcrumb';

@Injectable()

export class BreadCrumbService {
  siteHostName: string
  getBreadCrumbsUrl: string;
  constructor(private http: HttpClient) {
    this.siteHostName = "http://localhost:7150/";
    this.getBreadCrumbsUrl = this.siteHostName + "api/topics/getbreadcrumbs";
  }
  
  getBreadCrumbs(breadCrumbId: string): Observable<any> {
    return this.http.get(this.getBreadCrumbsUrl + '/' +  breadCrumbId);
  }
}
