import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Plant } from "./plants/plant";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class PlantKeeperService {

  constructor(private httpClient: HttpClient) {
  }

  private baseUrl: string = 'http://localhost:5033/api';
  private defaultHeaders = new HttpHeaders({ Accept: 'application/json' });

  getPlants(): Observable<Plant[]> {
    return this.httpClient.get<Plant[]>(`${this.baseUrl}/plants`, { headers: this.defaultHeaders });
  }
}
