import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-inputs',
  imports: [CommonModule],
  templateUrl: './inputs.html',
  styleUrl: './inputs.css',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => Inputs),
      multi: true,
    },
  ],
})
export class Inputs implements ControlValueAccessor {
  @Input() type: 'text' | 'email' | 'password' | 'select' | 'checkbox' | 'radio' = 'text';
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() icon: string = '';
  @Input() required: boolean = false;
  @Input() options: { label: string; value: any }[] = [];
  @Input() hasError: boolean = false;
  @Input() errorMessage: string = '';

  value: any = '';
  mostrarPassword: boolean = false;

  onChange = (val: any) => {};
  onTouched = () => {};

  writeValue(val: any) {
    this.value = val;
  }
  
  registerOnChange(fn: any) {
    this.onChange = fn;
  }
  
  registerOnTouched(fn: any) {
    this.onTouched = fn;
  }

  onInput(event: any) {
    if (this.type === 'checkbox') {
      this.value = event.target.checked;
    } else if (this.type === 'radio') {
      this.value = event.target.value;
    } else {
      this.value = event.target.value;
    }
    this.onChange(this.value);
    this.onTouched();
  }

  togglePassword() {
    this.mostrarPassword = !this.mostrarPassword;
  }
}