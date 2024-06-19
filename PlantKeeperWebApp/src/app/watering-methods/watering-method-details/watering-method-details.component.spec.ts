import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WateringMethodDetailsComponent } from './watering-method-details.component';

describe('WateringMethodDetailsComponent', () => {
  let component: WateringMethodDetailsComponent;
  let fixture: ComponentFixture<WateringMethodDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WateringMethodDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WateringMethodDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
