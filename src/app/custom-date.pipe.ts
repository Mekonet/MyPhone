import { DatePipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'customDate'
})
export class CustomDatePipe implements PipeTransform {
  transform(value: any, args?: any): any {
    // Create a new DatePipe with Hebrew locale
    const datePipe = new DatePipe('he-IL');
    
    // Format the date with the desired format
    return datePipe.transform(value, 'MMM d, y, HH:mm:ss');
  }
}
