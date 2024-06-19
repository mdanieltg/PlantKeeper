import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KeeperListComponent } from './keeper-list.component';

describe('KeeperListComponent', () => {
  let component: KeeperListComponent;
  let fixture: ComponentFixture<KeeperListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KeeperListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KeeperListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
