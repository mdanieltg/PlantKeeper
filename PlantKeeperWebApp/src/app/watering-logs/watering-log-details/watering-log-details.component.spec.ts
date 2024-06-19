import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WateringLogDetailsComponent } from './watering-log-details.component';

describe('WateringLogDetailsComponent', () => {
  let component: WateringLogDetailsComponent;
  let fixture: ComponentFixture<WateringLogDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WateringLogDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WateringLogDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
