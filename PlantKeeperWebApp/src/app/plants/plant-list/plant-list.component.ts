import { NgForOf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { Plant } from "../plant";

@Component({
  selector: 'app-plant-list',
  standalone: true,
  imports: [
    NgForOf,
    RouterLink
  ],
  templateUrl: './plant-list.component.html',
  styleUrl: './plant-list.component.css'
})
export class PlantListComponent implements OnInit {
  plants: Plant[] = [];

  constructor(private plantKeeperService: PlantKeeperService) {
  }

  ngOnInit(): void {
    this.plantKeeperService.getPlants().subscribe(plants => this.plants = plants);
  }
}
