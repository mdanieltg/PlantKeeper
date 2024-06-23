import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Observable } from "rxjs";
import { environment } from "../environments/environment";
import { Keeper } from "./keepers/keeper";
import { Plant } from "./plants/plant";
import { WateringMethod } from "./watering-methods/watering-method";

@Injectable({
  providedIn: 'root'
})
export class PlantKeeperService {

  private baseUrl: string = `${environment.apiHost}/api`;
  private defaultHeaders = new HttpHeaders({ Accept: 'application/json' });

  constructor(private httpClient: HttpClient) {
  }

  getPlants(): Observable<Plant[]> {
    return this.httpClient.get<Plant[]>(`${this.baseUrl}/plants`, { headers: this.defaultHeaders });
  }

  getPlant(id: string): Observable<Plant> {
    return this.httpClient.get<Plant>(`${this.baseUrl}/plants/${id}`, { headers: this.defaultHeaders });
  }

  getKeepers(): Observable<Keeper[]> {
    return this.httpClient.get<Keeper[]>(`${this.baseUrl}/keepers`, { headers: this.defaultHeaders });
  }

  getKeeper(id: string): Observable<Keeper> {
    return this.httpClient.get<Keeper>(`${this.baseUrl}/keepers/${id}`, { headers: this.defaultHeaders });
  }

  getWateringMethods(): Observable<WateringMethod[]> {
    return this.httpClient.get<WateringMethod[]>(`${this.baseUrl}/watering-methods`, { headers: this.defaultHeaders });
  }

  getWateringMethod(id: string): Observable<WateringMethod> {
    return this.httpClient.get<WateringMethod>(`${this.baseUrl}/watering-methods/${id}`, { headers: this.defaultHeaders });
  }
}
