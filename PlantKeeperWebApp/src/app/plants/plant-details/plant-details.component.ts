import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from "@angular/router";
import { Plant } from "../plant";

@Component({
  selector: 'app-plant-details',
  standalone: true,
  imports: [],
  templateUrl: './plant-details.component.html',
  styleUrl: './plant-details.component.css'
})
export class PlantDetailsComponent implements OnInit {
  pageTitle = 'Plant Details';
  plant?: Plant;

  constructor(private route: ActivatedRoute,
              private router: Router) {
  }

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.plant = {
      id: id,
      name: "Plant",
      care: undefined
    }
  }
}
