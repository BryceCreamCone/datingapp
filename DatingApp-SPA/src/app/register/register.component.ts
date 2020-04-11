import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';
import { FormGroup, FormControl, Validators, ValidatorFn, ValidationErrors, FormBuilder } from '@angular/forms';
import { User } from '../_models/user';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  user: User;
  registerForm: FormGroup;
  passwordValidations: ValidatorFn[] = [
    Validators.required,
    Validators.minLength(8),
    Validators.maxLength(50)
  ];

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private router: Router,
    private formBuilder: FormBuilder) { }

  ngOnInit() {
    this.createForm();
  }

  createForm() {
    this.registerForm = this.formBuilder.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', this.passwordValidations],
      confirmPassword: ['']
    }, { validator: this.confirmPasswordValidator });
  }

  confirmPasswordValidator(fg: FormGroup) {
    return fg.get('password').value === fg.get('confirmPassword').value ?
      null : { mismatch: true };
  }

  checkFormFieldState(field: string): boolean {
    return this.registerForm.get(field).errors && this.registerForm.get(field).touched;
  }

  checkFormState(field?: string): boolean {
    if (field) { return this.registerForm.errors && this.registerForm.get(field).touched; }
    return this.registerForm.errors && this.registerForm.touched;
  }

  hasFieldError(field: string, error: string): boolean {
    return this.registerForm.get(field).hasError(error);
  }

  hasFormError(error: string): boolean {
    return this.registerForm.hasError(error);
  }

  findFieldError(field: string, key: string): object {
    const error = this.registerForm.get(field).errors[key];
    return error;
  }



  register() {
    if (this.registerForm.valid) {
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user)
        .subscribe(() => {
          this.alertify.success('Registration successful');
        }, error => this.alertify.error(error), () => {
          this.authService.login(this.user)
            .subscribe(next => {
              this.alertify.success('Automatically Logged In');
              this.router.navigate(['/members']);
            }, error => this.alertify.error(error));
        });
    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
