import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Course } from '../../../core/models/platform.models';

@Component({ selector: 'app-course-card', standalone: true, imports: [CommonModule], template: `
  <article class="course-card"><img [src]="course.imageUrl" [alt]="course.title" (error)="hideImage($event)"><h3>{{ course.title }}</h3><p>{{ course.instructor }} · {{ course.hours }} hours · &dollar;{{ course.cost }}</p>
    <div class="actions"><button class="secondary" *ngIf="showDetails" (click)="view.emit(course.id)">Details</button><button *ngIf="signedIn && !isAdmin" (click)="enroll.emit(course.id)">Enroll</button>
      <ng-container *ngIf="isAdmin"><button (click)="edit.emit(course)">Edit</button><button class="secondary" (click)="remove.emit(course.id)">Delete</button></ng-container><button class="secondary" *ngIf="showAnalytics" (click)="analytics.emit(course.id)">View analytics</button>
    </div>
  </article>`, styles: [` .course-card{height:100%;border:1px solid #ddd;border-radius:8px;padding:14px;background:#fff;display:flex;flex-direction:column;transition:box-shadow .15s,transform .15s}.course-card:hover{box-shadow:0 5px 18px #00000012;transform:translateY(-2px)}.course-card img{width:100%;height:170px;object-fit:cover;background:#eee;border-radius:5px;margin-bottom:14px}.course-card h3{margin-bottom:7px}.course-card p{color:#555;font-size:.9rem;line-height:1.5;margin-bottom:10px}.actions{display:flex;gap:8px;flex-wrap:wrap;margin-top:auto;padding-top:10px}.actions button{font-size:.86rem;padding:8px 10px}@media(max-width:500px){.course-card img{height:190px}}`] })
export class CourseCardComponent {
  @Input({ required: true }) course!: Course;
  @Input() isAdmin = false;
  @Input() signedIn = false;
  @Input() showDetails = true;
  @Input() showAnalytics = false;
  @Output() view = new EventEmitter<number>();
  @Output() enroll = new EventEmitter<number>();
  @Output() edit = new EventEmitter<Course>();
  @Output() remove = new EventEmitter<number>();
  @Output() analytics = new EventEmitter<number>();
  hideImage(event: Event): void { (event.target as HTMLImageElement).style.display = 'none'; }
}




