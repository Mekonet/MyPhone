import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UtilityService } from 'src/app/services/utility.service';

@Component({
  selector: 'app-search-result',
  templateUrl: './search-result.component.html',
  styleUrls: ['./search-result.component.css']
})
export class SearchResultComponent implements OnInit {
  searchResults: any[] = [];
  view: 'grid' | 'list' = 'list';
  sortby: 'default' | 'htl' | 'lth' = 'default';
  constructor(private utilityService: UtilityService,private router: Router) { }

  ngOnInit(): void {
     // נרשום ל-Subject של השירות כדי לעדכן את רשימת התוצאות בכל שינוי
     this.utilityService.searchResultsSubject.subscribe(
      (results: any[]) => {
        this.searchResults = results;
      }
    );

  }
  
  sortByPrice(sortKey: string){
    this.searchResults.sort((a, b) => {
      if (sortKey === 'default'){
        return a.id > b.id ? 1 : -1;
      }
      if (sortKey === 'htl'){
        return this.utilityService.applyDiscount(a.price, a.offer.discount) >
        this.utilityService.applyDiscount(b.price ,b.offer.discount)
        ? -1
        : 1;
      }
      if (sortKey =='lth'){
        
         return this.utilityService.applyDiscount(a.price, a.offer.discount) >
          this.utilityService.applyDiscount(b.price ,b.offer.discount)
          ? 1
          : -1;
        }
        (sortKey === 'htl' ? 1 : -1) * 
        (this.utilityService.applyDiscount(a.price, a.offer.discount) >
        this.utilityService.applyDiscount(b.price ,b.offer.discount)
        ? -1
        : 1);
       return 0; 
    });
  }
}
