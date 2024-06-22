import { Component, OnInit } from '@angular/core';
import { NgForOf } from "@angular/common";
import { Plant } from "../plant";
import { PlantKeeperService } from "../../plant-keeper.service";

@Component({
  selector: 'app-plant-list',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './plant-list.component.html',
  styleUrl: './plant-list.component.css'
})
export class PlantListComponent implements OnInit {

  plants: Plant[] = [];

  constructor(private plantKeeperService: PlantKeeperService) {
  }

  ngOnInit() {
    this.plantKeeperService.getPlants().subscribe(plants => this.plants = plants);
  }
}
