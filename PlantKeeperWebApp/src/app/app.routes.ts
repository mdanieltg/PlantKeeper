import { Routes } from '@angular/router';
import { StartComponent } from "./start/start.component";
import { PlantListComponent } from "./plants/plant-list/plant-list.component";
import { PlantDetailsComponent } from "./plants/plant-details/plant-details.component";
import { KeeperListComponent } from "./keepers/keeper-list/keeper-list.component";
import { KeeperDetailsComponent } from "./keepers/keeper-details/keeper-details.component";
import { WateringMethodListComponent } from "./watering-methods/watering-method-list/watering-method-list.component";
import {
  WateringMethodDetailsComponent
} from "./watering-methods/watering-method-details/watering-method-details.component";

export const routes: Routes = [
  { path: 'keepers', component: KeeperListComponent, pathMatch: 'full' },
  { path: 'keepers/:id', component: KeeperDetailsComponent, pathMatch: 'full' },
  { path: 'plants', component: PlantListComponent, pathMatch: 'full' },
  { path: 'plants/:id', component: PlantDetailsComponent, pathMatch: 'full' },
  { path: 'watering-methods', component: WateringMethodListComponent, pathMatch: 'full' },
  { path: 'watering-methods/:id', component: WateringMethodDetailsComponent, pathMatch: 'full' },
  { path: '', component: StartComponent, pathMatch: 'full' },
];
