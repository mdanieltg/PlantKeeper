import { TestBed } from '@angular/core/testing';

import { PlantKeeperService } from './plant-keeper.service';

describe('PlantKeeperServiceService', () => {
  let service: PlantKeeperService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PlantKeeperService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
