import { NgForOf } from "@angular/common";
import { Component, OnInit } from '@angular/core';
import { RouterLink } from "@angular/router";
import { PlantKeeperService } from "../../plant-keeper.service";
import { Keeper } from "../keeper";

@Component({
  selector: 'app-keeper-list',
  standalone: true,
  imports: [
    NgForOf,
    RouterLink
  ],
  templateUrl: './keeper-list.component.html',
  styleUrl: './keeper-list.component.css'
})
export class KeeperListComponent implements OnInit {
  keepers: Keeper[] = [];

  constructor(private plantKeeperService: PlantKeeperService) {
  }

  ngOnInit(): void {
    this.plantKeeperService.getKeepers().subscribe(keepers => this.keepers = keepers);
  }
}
