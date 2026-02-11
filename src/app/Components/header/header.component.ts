import { Component, ElementRef, OnInit, Type, ViewChild, ViewContainerRef } from '@angular/core';
import { Category, NavigationItem, Product } from 'src/app/models/models';
import { LoginComponent } from '../login/login.component';
import { RegisterComponent } from '../register/register.component';
import { NavigationService } from 'src/app/services/navigation.service';
import { UtilityService } from 'src/app/services/utility.service';
import { Subscription } from 'rxjs';
import { NavigationStart, Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl:'./header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {
  @ViewChild('modalTitle') modalTitle!: ElementRef;
  @ViewChild('container',{ read: ViewContainerRef, static:true})
  container!: ViewContainerRef;
  cartItems: number = 0;
  navigationList: NavigationItem [] = [];
  isModalOpen: boolean = false;
  modalSubscription: Subscription | undefined;
  searchQuery: string = '';
  searchResults: Product[] = [];

  constructor(
    private navigationService: NavigationService,
    public utilityService: UtilityService,
    private router: Router
    ) { }

  ngOnInit(): void {  
    // מקבל את רשימת המוצרים 
    this.navigationService
    .getCategoryList()
    .subscribe((list: Category[])=>{
      for (let item of list){
        let present = false;
        for(let navItem of this.navigationList){
          if(navItem.category === item.category){
            navItem.subcategories.push(item.subCategory);
            present = true;
          }
        }
        if(!present){
          this.navigationList.push({
            category: item.category,
            subcategories: [item.subCategory],
          });
        }
      }
    });

    //עגלה
    if (this.utilityService.isLoggedIn()) {
      this.navigationService
        .getActiveCartOfUser(this.utilityService.getUser().id)
        .subscribe((res: any) => {
          this.cartItems = res.cartItems.length;
        });
    }
    
    this.utilityService.changeCart.subscribe((res: any) => {
      if (parseInt(res) === 0) this.cartItems = 0;
      else this.cartItems += parseInt(res);
    });
    
    this.router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        if (this.router.url !== '/search') {
          this.clearSearch();
        }
      }
    });
  }

  openModal(name: string){
    this.container.clear();
    let componentType!: Type<any>;
    if(name === 'login'){
    componentType = LoginComponent;
    this.modalTitle.nativeElement.textContent = 'הכנס פרטי משתמש';
    }
    if(name === 'register'){
    componentType = RegisterComponent;
    this.modalTitle.nativeElement.textContent = 'מלא פרטי נרשם';
    }
    this.container.createComponent(componentType);
  }
  // פונקציה זו תופסת את תוצאות החיפוש ומעבירה אותן לשירות UtilityService
  searchProducts(): void {
    if (this.searchQuery.trim() !== '') {
      // Call the service method to perform the search
      this.navigationService.searchProducts(this.searchQuery.trim()).subscribe(
        (results: any[]) => {
          // Pass the search results to the utility service
          this.utilityService.passSearchResults(results);
        },
        (error: any) => {
          console.error('Error occurred during search:', error);
        }
      );
    }
  }
  clearSearch(): void {
    this.searchQuery = ''; // Clear the search query
    this.searchResults = []; // Reset the search results array
  }
  addProduct() {
    this.router.navigateByUrl('/add-product');
  }
  
}
