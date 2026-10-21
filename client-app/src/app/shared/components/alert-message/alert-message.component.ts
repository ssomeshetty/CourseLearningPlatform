import { Component, Input } from '@angular/core';

@Component({ selector: 'app-alert-message', standalone: true, template: '<p class="message" [class.error]="error" role="status">{{ message }}</p>', styles: [`.message{min-height:24px;margin:16px 0}.error{color:#a00}`] })
export class AlertMessageComponent { @Input() message = ''; @Input() error = false; }
