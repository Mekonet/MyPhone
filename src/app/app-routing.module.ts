import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './Components/home/home.component';
import { ProductsComponent } from './Components/products/products.component';
import { ProductDetailsComponent } from './Components/product-details/product-details.component';
import { CartComponent } from './Components/cart/cart.component';
import { PageNotFoundComponent } from './Components/page-not-found/page-not-found.component';
import { OrderComponent } from './Components/order/order.component';
import { SearchResultComponent } from './Components/search-result/search-result.component';
import { AddProductComponent } from './Components/add-product/add-product.component';
import { MessagesComponent } from './Components/messages/messages.component';

const routes: Routes = [
  {path:'home', component: HomeComponent},
  {path:'products',component:ProductsComponent},
  {path:'product-details',component:ProductDetailsComponent},
  {path:'cart',component:CartComponent},
  {path:'orders',component:OrderComponent},
  {path:'add-product',component:AddProductComponent},
  {path:'search-result',component:SearchResultComponent},
  {path:'messages',component:MessagesComponent},
  {path:'',redirectTo:'/home', pathMatch: 'full'},
  {path:'**',component:PageNotFoundComponent},

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
