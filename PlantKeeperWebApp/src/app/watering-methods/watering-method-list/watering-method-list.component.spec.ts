import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WateringMethodListComponent } from './watering-method-list.component';

describe('WateringMethodListComponent', () => {
  let component: WateringMethodListComponent;
  let fixture: ComponentFixture<WateringMethodListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WateringMethodListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WateringMethodListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
