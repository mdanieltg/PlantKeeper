import { Component } from '@angular/core';
import { NgForOf } from "@angular/common";
import { Plant } from "../plant";

@Component({
  selector: 'app-plant-list',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './plant-list.component.html',
  styleUrl: './plant-list.component.css'
})
export class PlantListComponent {
  plants: Plant[] = [
    { id: '', name: 'Teléfono' },
    { id: '', name: 'Lavanda' },
    { id: '', name: 'Cuna de Moisés' },
    { id: '', name: 'Helecho' },
    { id: '', name: 'Espada' },
    { id: '', name: 'Citronela' },
    { id: '', name: 'Albaca' }
  ];
}
