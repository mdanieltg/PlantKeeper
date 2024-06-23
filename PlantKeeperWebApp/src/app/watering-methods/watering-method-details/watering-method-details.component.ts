import { NgIf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { WateringMethod } from "../watering-method";

@Component({
  selector: 'app-watering-method-details',
  standalone: true,
  imports: [
    NgIf,
    RouterLink
  ],
  templateUrl: './watering-method-details.component.html',
  styleUrl: './watering-method-details.component.css'
})
export class WateringMethodDetailsComponent implements OnInit {
  pageTitle: string = 'Watering method details';
  method?: WateringMethod;

  constructor(private plantKeeperService: PlantKeeperService,
              private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    const id: string = this.route.snapshot.params['id'];
    this.plantKeeperService.getWateringMethod(id).subscribe(wateringMethods => {
      this.pageTitle = wateringMethods.name;
      this.method = wateringMethods;
    })
  }
}
