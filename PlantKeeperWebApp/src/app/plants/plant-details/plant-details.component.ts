import { NgIf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { Plant } from "../plant";

@Component({
  selector: 'app-plant-details',
  standalone: true,
  imports: [
    NgIf,
    RouterLink
  ],
  templateUrl: './plant-details.component.html',
  styleUrl: './plant-details.component.css'
})
export class PlantDetailsComponent implements OnInit {
  pageTitle: string = 'Plant details';
  plant?: Plant;

  constructor(private plantKeeperService: PlantKeeperService,
              private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    const id: string = this.route.snapshot.params['id'];
    this.plantKeeperService.getPlant(id).subscribe(plant => {
      this.pageTitle = plant.name;
      this.plant = plant;
    });
  }
}
