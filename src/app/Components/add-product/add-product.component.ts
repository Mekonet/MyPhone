import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavigationService } from 'src/app/services/navigation.service';

@Component({
  selector: 'app-add-product',
  templateUrl: './add-product.component.html',
  styleUrls: ['./add-product.component.css']
})
export class AddProductComponent implements OnInit {
  productForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private navigationService: NavigationService) {
    this.productForm = this.formBuilder.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]],
      quantity: ['', [Validators.required, Validators.min(0)]],
      imageName: ['']
    });
  }
  ngOnInit(): void {
    
  }
  onSubmit(): void {
    if (this.productForm.valid) {
      const formData = this.productForm.value;
      this.navigationService.addProduct(formData).subscribe(
        (response) => {
          console.log('Product added successfully:', response);
          this.productForm.reset();
        },
        (error) => {
          console.error('Error adding product:', error);
          console.log('Form validity:', this.productForm.valid);

        }
      );
    }
  }

}
