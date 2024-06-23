import { NgForOf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { WateringMethod } from "../watering-method";

@Component({
  selector: 'app-watering-method-list',
  standalone: true,
  imports: [
    NgForOf,
    RouterLink
  ],
  templateUrl: './watering-method-list.component.html',
  styleUrl: './watering-method-list.component.css'
})
export class WateringMethodListComponent implements OnInit {
  methods: WateringMethod[] = [];

  constructor(private plantKeeperService: PlantKeeperService) {
  }

  ngOnInit(): void {
    this.plantKeeperService.getWateringMethods().subscribe(methods => this.methods = methods);
  }
}
