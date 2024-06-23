import { NgIf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { Keeper } from "../keeper";

@Component({
  selector: 'app-keeper-details',
  standalone: true,
  imports: [
    NgIf,
    RouterLink
  ],
  templateUrl: './keeper-details.component.html',
  styleUrl: './keeper-details.component.css'
})
export class KeeperDetailsComponent implements OnInit {
  pageTitle: string = 'Keeper details';
  keeper?: Keeper;

  constructor(private plantKeeperService: PlantKeeperService,
              private route: ActivatedRoute) {
  }

  ngOnInit(): void {
    const id: string = this.route.snapshot.params['id'];
    this.plantKeeperService.getKeeper(id).subscribe(keeper => {
      this.pageTitle = keeper.name;
      this.keeper = keeper;
    })
  }
}
