import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { NavigationService } from 'src/app/services/navigation.service';
import { UtilityService } from 'src/app/services/utility.service';
import { HeaderComponent } from '../header/header.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  message= "";
  classname = '';

  constructor(private fb: FormBuilder,
     private navigationService: NavigationService,
     private utilityService: UtilityService,
     private router: Router ) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email:['',[Validators.required, Validators.email]],
      pwd: [
        '',
        [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(15),
      ],
    ]
    });
  }
  login() {
    this.navigationService
    .loginUser(this.Email.value, this.PWD.value)
    .subscribe((res: any) => {
      if(res.toString() !== 'invalid'){
        this.message = 'התחברתה בהצלחה'; // הודעת התחבר בהצלחה
        this.utilityService.setUser(res.toString());
        this.classname = "text-success";
      }
      else{
        this.message = 'משתמש או סיסמא לא נכונים!';
        this.classname = "text-danger";
      }
    });
  }
  
  get Email(): FormControl {
    return this.loginForm.get('email') as FormControl;
  }
  get PWD(): FormControl {
    return this.loginForm.get('pwd') as FormControl;
  }
}
