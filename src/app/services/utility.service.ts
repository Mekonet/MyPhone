import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Cart, Payment, Product, User } from '../models/models';
import { Subject } from 'rxjs';
import { NavigationService } from './navigation.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class UtilityService {
  changeCart = new Subject();
  searchResultsSubject = new Subject<any[]>();

  constructor(private jwt: JwtHelperService, private navigationService: NavigationService ,private router: Router) { }
  
  applyDiscount(price: number, discount: number ): number {
    let finalPrice: number = price - price * (discount / 100);
    return finalPrice;
  }

  getUser(): User{
    let token = this.jwt.decodeToken()
    let user: User = {
      id: token.id,
      firstName: token.firstName,
      lastName: token.lastName,
      address: token.address,
      mobile: token.mobile,
      email: token.email,
      password: '',
      createdAt: token.createdAt,
      modifiedAt: token.modifiedAt,
      IsAdmin: token.IsAdmin === 1 ? true : false  
     };
    return user;
  }

  setUser(token: string) {
    localStorage.setItem('user', token);
  }

  isLoggedIn() {
    return localStorage.getItem('user') ? true : false;
  }

  logoutUser() {
    localStorage.removeItem('user');
    this.router.navigateByUrl('/home');
  }
  
  addToCart(product: Product) {
    let productid = product.id;
    let userid = this.getUser().id;

    this.navigationService.addToCart(userid, productid).subscribe((res) => {
      if(res.toString() === 'inserted') 
      this.changeCart.next(1);
    });
  }

  calculatePayment(cart: Cart, payment: Payment) {
    payment.totalAmount = 0;
    payment.amountPaid = 0;
    payment.amountReduced = 0;

    for (let cartitem of cart.cartItems) {
      payment.totalAmount += cartitem.product.price;

      payment.amountReduced +=
        cartitem.product.price -
        this.applyDiscount(
          cartitem.product.price,
          cartitem.product.offer.discount
        );

      payment.amountPaid += this.applyDiscount(
        cartitem.product.price,
        cartitem.product.offer.discount
      );
    }

    if (payment.amountPaid > 50000) payment.shipingCharges = 2000;
    else if (payment.amountPaid > 20000) payment.shipingCharges = 1000;
    else if (payment.amountPaid > 5000) payment.shipingCharges = 500;
    else payment.shipingCharges = 200;
  }
  
  calculatePricePaid(cart: Cart){
    let pricepaid =0;
    for(let cartitem of cart.cartItems){
      pricepaid += this.applyDiscount(
        cartitem.product.price, 
        cartitem.product.offer.discount
      );
    }
    return pricepaid;
  }
  removeFromCartAndUpdate(product: Product, cart: Cart ) {
    let productId = product.id;
    let userId = this.getUser().id;
    let cartItemId = 0;
    for(let cartitem of cart.cartItems){
      if (cartitem.product.id == productId)
      cartItemId = cartitem.id;
    }
    this.navigationService.removeProductFromCart(productId, cartItemId).subscribe((res) => {
      if (res.toString() === 'removed') {
        // Fetch the updated cart items after removing the product
        this.navigationService.getActiveCartOfUser(userId).subscribe((updatedCart: any) => {
          // Update the cart items in the provided cart object
          cart.cartItems = updatedCart.cartItems;
        });
      }
    });
  }
  passSearchResults(searchResults: any[]) {
    this.searchResultsSubject.next(searchResults);
    this.router.navigateByUrl('/search-result')
  }
searchProducts(query: string) {
  fetch(`https://localhost:5001/SearchProducts?query=${query}`)
    .then(res => res.json())
    .then(results => {
      // שולח את התוצאות ל-Subject כדי שהקומפוננטה תראה אותן
      this.passSearchResults(results);
    })
    .catch(err => console.error('Error fetching products:', err));
}

 
  
}