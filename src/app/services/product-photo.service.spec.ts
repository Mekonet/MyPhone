import { TestBed } from '@angular/core/testing';

import { ProductPhotoService } from './product-photo.service';

describe('ProductPhotoService', () => {
  let service: ProductPhotoService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductPhotoService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
