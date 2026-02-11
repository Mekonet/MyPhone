import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { timer } from 'rxjs';
import { Cart, Order, Payment, PaymentMethod } from 'src/app/models/models';
import { NavigationService } from 'src/app/services/navigation.service';
import { UtilityService } from 'src/app/services/utility.service';

@Component({
  selector: 'app-order',
  templateUrl: './order.component.html',
  styleUrls: ['./order.component.css']
})
export class OrderComponent implements OnInit {
  selectedPaymentMethodName ='a';
  selectedPaymentMethod = new FormControl('0');

  address = '';
  mobileNumber = '';
  displaySpinner = false;
  message = "";
  classname = '';

  paymentMethods: PaymentMethod[] = [];
  //עגלת משתמש
  usersCart: Cart = {
    id:0,
    user: this.utilityService.getUser(),
    cartItems: [],
    ordered: false,
    orderedOn: '',
  };
  //פרטי תשלום
  usersPaymentInfo: Payment = {
    id: 0,
    user: this.utilityService.getUser(),
    paymentMethod:{
      id: 0,
      type: '',
      provider: '',
      available: false,
      reason: '',
    },
    totalAmount : 0,
    shipingCharges: 0,
    amountReduced: 0,
    amountPaid: 0,
    createdAt: '',
  };
  constructor(
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private router: Router
  ) { }

  ngOnInit(): void {
    //מקבל את פעולת תשלום
    this.navigationService.getPaymentMethods().subscribe((res) => {
      this.paymentMethods = res;
    });

    this.selectedPaymentMethod.valueChanges.subscribe((res:any) => {
      if (res === '0')
       this.selectedPaymentMethodName = '';
      else
       this.selectedPaymentMethodName = res.toString();
    });

    //עגלה
    this.navigationService
      .getActiveCartOfUser(this.utilityService.getUser().id)
      .subscribe((res: any) => {
        this.usersCart = res;
        this.utilityService.calculatePayment(res, this.usersPaymentInfo);
      });

    //כתובת וטלפון
    this.address = this.utilityService.getUser().address;
    this.mobileNumber = this.utilityService.getUser().mobile;
  }
  //אמצעי תשלום
  getPaymentMethod(id: string){
    let x = this.paymentMethods.find((v) => v.id === parseInt(id));
    return x?.type + ' - ' + x?.provider;
  }
  //בצע הזמנה
  placeOrder(){
    this.displaySpinner = true;
    let isPaymentSuccessful = this.payMoney();

    if (!isPaymentSuccessful){
      this.displaySpinner = false;
      this.message = '!תשלום נכשל!, לא בוצע חיוב';
      this.classname = "text-danger";
      return;
    }
    let step =0;
    let count = timer(0, 3000).subscribe((res) => {
      ++step;
      if(step ===1){
        this.message = 'משלם..';
        this.classname = "text-success";
      }
      if(step === 2){
        this.message = 'תשלום בוצע בהצלחה';
        this.storeOrder();
      }
      if(step === 3){
        this.message = 'הזמנה בוצעה בהצלחה';
        this.displaySpinner = false;
      }
      if(step === 4){
        this.router.navigateByUrl('/home');
        count.unsubscribe();
      }
    });
  }
  //שולם
  payMoney(){
    return true;
  }
  storeOrder(){
    let payment: Payment;
    let pmid = 0;
    if(this.selectedPaymentMethod.value)
      pmid = parseInt(this.selectedPaymentMethod.value);

    payment = {
      id: 0,
      paymentMethod: {
        id: pmid,
        type: '',
        provider: '',
        available: false,
        reason: '',
      },
      user: this.utilityService.getUser(),
      totalAmount: this.usersPaymentInfo.totalAmount,
      shipingCharges: this.usersPaymentInfo.shipingCharges,
      amountReduced: this.usersPaymentInfo.amountReduced,
      amountPaid: this.usersPaymentInfo.amountPaid,
      createdAt: '',
    };
    for (const cartItem of this.usersCart.cartItems) {
      this.navigationService.updateProductQuantity(cartItem.product.id)
        .subscribe(
          (response) => {
            console.log(`Product ${cartItem.product.id} quantity updated successfully`, response);
          },
          (error) => {
            console.error(`Failed to update product ${cartItem.product.id} quantity:`, error);
          }
        );
    }
    this.navigationService
      .insertPayment(payment)
      .subscribe((paymentResponse: any) => {
        payment.id = parseInt(paymentResponse);
        let order: Order = {
          id: 0,
          user: this.utilityService.getUser(),
          cart: this.usersCart,
          payment: payment,
          createdAt: '',
        };
        this.navigationService.insertOrder(order).subscribe(
          (orderResponse: any) => {
            this.utilityService.changeCart.next(0);
          },
          (error: any) => {
            console.error("Failed to insert order:", error);
            // Handle error here
          }
        );
      }, (error: any) => {
        console.error("Failed to insert payment:", error);
        // Handle error here
      });
      
  }
  
}
