import { Injectable } from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import { Category, Order, Payment, PaymentMethod, Product, User } from '../models/models';
import { Observable, map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  baseUrl = "https://localhost:7096/api/Shopping/";

  constructor(private http: HttpClient) { }
    getCategoryList() {
      let url = this.baseUrl + 'GetCategoryList';
      return this.http.get<any[]>(url).pipe(
        map((categories)=>
          categories.map((category) => {
            let mappedCategory: Category = {
              id: category.id,
              category: category.category,
              subCategory: category.subCategory,
            };
            return mappedCategory;
          })
        )
      );
    }
    
    getProducts(category: string, subcategory: string, count: number) {
      return this.http.get<any[]>(this.baseUrl + 'GetProducts',{
        params: new HttpParams()
        .set('category',category)
        .set('subcategory',subcategory)
        .set('count',count),
      });
    }

    getProduct(id: number) {
      let url = this.baseUrl + 'GetProduct/'+ id;
      return this.http.get(url);
    }

    registerUser(user: User) {
      let url = this.baseUrl + 'RegisterUser';
      return this.http.post(url, user, {responseType: 'text'});
    }

    loginUser(email: string, password: string) {
      let url = this.baseUrl + 'LoginUser';
      return this.http.post(
        url,
        { Email: email, Password: password},
        { responseType: 'text' }
      );
    }

    submitReview(userid: number, productid: number, review: string) {
      let obj: any = {
        User:{
          Id: userid,
        },
        Product: {
          Id: productid,
        },
        Value: review,
      };

      let url = this.baseUrl + 'InsertReview';
      return this.http.post(url, obj, {responseType: 'text'});
    }

    getAllReviewsOfProduct(productId: number) {
      let url =this.baseUrl + 'GetProductReviews/' + productId;
      return this.http.get(url);
    }

    addToCart(userid: number, productid: number){
      let url= this.baseUrl + 'InsertCartItem/' + userid + '/' + productid;
      return this.http.post(url, null, {responseType: 'text'});
    }

    getActiveCartOfUser(userid: number) {
      let url = this.baseUrl + 'GetActiveCartOfUser/' + userid;
      return this.http.get(url);
    }

    getAllPreviousCartOfUser(userid: number)
    {
      let url = this.baseUrl + 'GetAllPreviousCartOfUser/' + userid;
      return this.http.get(url);
    }

    getPaymentMethods()
    {
      let url = this.baseUrl + 'GetPaymentMethods';
      return this.http.get<PaymentMethod[]>(url);
    }

    insertPayment(payment: Payment)
    {
      return this.http.post(this.baseUrl + 'InsertPayment', payment, {
        responseType: 'text',
      });
    }
    insertOrder(order: Order) {
      return this.http.post<Order>(this.baseUrl + 'InsertOrder', order);
    }
    
    removeProductFromCart(productId: number, cartItemId: number) {
      let url = this.baseUrl + 'RemoveProductFromCart/' + productId + '/' + cartItemId ;
      return this.http.delete(url, { responseType: 'text' });
    }
    
    submitProduct(product: Product): Observable<Product> {
      const url = `${this.baseUrl}UpdateProduct/${product.id}`;
      return this.http.put<Product>(url, product);
    }

    searchProducts(query: string): Observable<any[]> {
      const params = new HttpParams().set('query', query);
      return this.http.get<any[]>(`${this.baseUrl}SearchProducts`, { params });
    }

    addProduct(product: Product): Observable<Product> {
      return this.http.post<Product>(`${this.baseUrl}AddProduct`, product);
    }
    getAllOrders(): Observable<any[]> {
      return this.http.get<any[]>(`${this.baseUrl}GetAllOrders`);
    }
    getQuantityMessages(): Observable<string[]> {
      const url = `${this.baseUrl}quantity-messages`; // Adjust the URL as needed
      return this.http.get<string[]>(url);
    }
    updateProductQuantity(productId: number): Observable<any> {
      const url = `${this.baseUrl}UpdateProductQuantity/${productId}`;
      return this.http.put(url, {});
    }
    
    deleteProduct(productId: number): Observable<any> {
      const url = `${this.baseUrl}DeleteProduct/${productId}`;
      return this.http.delete(url);
    }
    
    addProductImage(productId: number, imageUrl: string): Observable<any> {
      const url = `${this.baseUrl}AddProductImage`;
      const body = { productId, imageUrl };
      return this.http.post(url, body);
    }
}
