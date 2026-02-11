import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Order } from 'src/app/models/models';
import { NavigationService } from 'src/app/services/navigation.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  orders: Order[] = [];
  quantityMessages: string[] = []; // Array to store messages related to product quantity

  constructor(private navigationService: NavigationService) { }

  ngOnInit(): void {
    this.getAllOrders();
    this.getQuantityMessages(); // Fetch messages related to product quantity
  }

  getAllOrders(): void {
    this.navigationService.getAllOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
      },
      error: (error) => {
        console.error('Error fetching orders:', error);
        // Handle error, e.g., show an error message to the user
      }
    });
  }

  getQuantityMessages(): void {
    // Assuming you have a method in NavigationService to fetch quantity messages
    this.navigationService.getQuantityMessages().subscribe({
      next: (messages) => {
        this.quantityMessages = messages;
      },
      error: (error) => {
        console.error('Error fetching quantity messages:', error);
        // Handle error, e.g., show an error message to the user
      }
    });
  }
}