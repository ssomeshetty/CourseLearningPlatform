import { Component, EventEmitter, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { errorMessage } from '../../core/services/error-message';
import { Notice } from '../../core/models/notice';
import { Session } from '../../core/models/platform.models';

@Component({ selector: 'app-auth', standalone: true, imports: [FormsModule], templateUrl: './auth.component.html', styles: [` .auth-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,300px),1fr));gap:18px;max-width:850px;margin:auto}.panel{border:1px solid #ddd;border-radius:8px;padding:22px;background:#fff}.panel h2{margin-bottom:18px}.panel label{display:grid;gap:7px;margin:14px 0;font-size:.9rem;font-weight:600}.panel button{margin-top:4px}@media(max-width:600px){.panel{padding:18px}}`] })
export class AuthComponent {
  private readonly auth = inject(AuthService);
  @Output() signedIn = new EventEmitter<Session>();
  @Output() notice = new EventEmitter<Notice>();
  signin = { email: '', password: '' };
  signup = { name: '', email: '', employeeCode: '', password: '' };

  async signIn(): Promise<void> {
    try { const session = await this.auth.signIn(this.signin.email, this.signin.password); this.auth.saveSession(session); this.signedIn.emit(session); this.notice.emit({ text: 'Signed in.' }); }
    catch (error) { this.notice.emit({ text: errorMessage(error), error: true }); }
  }

  async signUp(): Promise<void> {
    try { const session = await this.auth.signUp(this.signup); this.auth.saveSession(session); this.signedIn.emit(session); this.notice.emit({ text: 'Account created.' }); }
    catch (error) { this.notice.emit({ text: errorMessage(error), error: true }); }
  }
}

