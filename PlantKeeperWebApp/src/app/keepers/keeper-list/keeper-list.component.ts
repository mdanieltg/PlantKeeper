import { Component } from '@angular/core';
import { Keeper } from "../keeper";
import { NgForOf } from "@angular/common";

@Component({
  selector: 'app-keeper-list',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './keeper-list.component.html',
  styleUrl: './keeper-list.component.css'
})
export class KeeperListComponent {
  keepers: Keeper[] = [
    { id: '', name: 'Daniel' },
    { id: '', name: 'Diego' },
    { id: '', name: 'Saul' },
    { id: '', name: 'Mo' },
  ]
}
