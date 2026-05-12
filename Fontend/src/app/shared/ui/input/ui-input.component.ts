import { Component, Input, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';

@Component({
  selector: 'ui-input',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './ui-input.component.html',
  styleUrl: './ui-input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UiInputComponent),
      multi: true,
    },
  ],
})
export class UiInputComponent implements ControlValueAccessor {
  private static idSeq = 0;

  @Input() label = '';
  @Input() hint = '';
  @Input() error = '';
  @Input() type: 'text' | 'email' | 'password' | 'number' | 'search' = 'text';
  @Input() placeholder = '';
  @Input() autocomplete = '';
  @Input() id = `ui-inp-${UiInputComponent.idSeq++}`;
  @Input() disabled = false;

  value = '';
  touched = false;

  private onChange: (v: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInput(ev: Event): void {
    const v = (ev.target as HTMLInputElement).value;
    this.value = v;
    this.onChange(v);
  }

  onBlur(): void {
    this.touched = true;
    this.onTouched();
  }
}
