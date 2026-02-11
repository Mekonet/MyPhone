import { Component, OnInit } from '@angular/core';
import { Cart, CartItem, Payment } from 'src/app/models/models';
import { NavigationService } from 'src/app/services/navigation.service';
import { UtilityService } from 'src/app/services/utility.service';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})

export class CartComponent implements OnInit {

  usersCart: Cart = {
    id: 0,
    user: this.utilityService.getUser(),
    cartItems: [],
    ordered: false,
    orderedOn: '',
  };

  usersPaymentInfo: Payment = {
    id: 0,
    user: this.utilityService.getUser(),
    paymentMethod: {
      id: 0,
      type: '',
      provider: '',
      available: false,
      reason: '',
    },
    totalAmount: 0,
    shipingCharges: 0,
    amountReduced: 0,
    amountPaid: 0,
    createdAt: '',
  };
  usersPreviousCarts: Cart[] = [];
  
  constructor(
    public utilityService: UtilityService,
    private navigationService: NavigationService
  ) { }

  ngOnInit(): void {
    // עגלה
    this.navigationService
      .getActiveCartOfUser(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersCart = res;
        // מחשב מחיר
        this.utilityService.calculatePayment(
          this.usersCart,
          this.usersPaymentInfo
        );
      });

    // מציג עגלות קודמות
    this.navigationService
      .getAllPreviousCartOfUser(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersPreviousCarts = res;
      });
  }
  //מחיקת מוצר מהעגלה
  removeProductFromCart(productId: number, cartItemId: number): void {
    this.navigationService.removeProductFromCart(productId, cartItemId)
      .subscribe((res: any) => {
        // מעדכן את המוצרים בעגלה
        this.navigationService.getActiveCartOfUser(this.utilityService.getUser().id)
          .subscribe((cart: any) => {
            this.usersCart = cart;  
            this.utilityService.changeCart.next(-1);
            // מחשב מחיר מחדש
            this.utilityService.calculatePayment(
              this.usersCart,
              this.usersPaymentInfo
            );
          });
      });
      
  }
   

}